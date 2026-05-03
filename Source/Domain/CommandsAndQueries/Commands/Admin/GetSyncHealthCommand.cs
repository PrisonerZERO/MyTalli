namespace My.Talli.Domain.Commands.Admin;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetSyncHealthCommand
{
	#region <Constants>

	private const int ProblemShopRowCap = 25;
	private const int StaleSecondsMultiplier = 2;
	private static readonly TimeSpan StaleShopGracePeriod = TimeSpan.FromHours(36);

	private static readonly string[] ExpectedWorkerSources =
	[
		"AdminHealthWorker",
		"ShopSyncWorker",
		"TokenRefreshWorker"
	];

	private static readonly string[] AvailablePlatforms =
	[
		"Etsy",
		"Gumroad",
		"Stripe"
	];


	#endregion

	#region <Variables>

	private readonly RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> _heartbeatAdapter;
	private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
	private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;


	#endregion

	#region <Constructors>

	public GetSyncHealthCommand(
		RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> heartbeatAdapter,
		RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter,
		RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter,
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
	{
		_heartbeatAdapter = heartbeatAdapter;
		_platformConnectionAdapter = platformConnectionAdapter;
		_revenueAdapter = revenueAdapter;
		_shopConnectionAdapter = shopConnectionAdapter;
	}


	#endregion

	#region <Methods>

	public async Task<SyncHealthReport> ExecuteAsync()
	{
		var now = DateTime.UtcNow;

		var heartbeats = (await _heartbeatAdapter.GetAllAsync()).ToList();
		var platformConnections = (await _platformConnectionAdapter.GetAllAsync()).ToList();
		var shops = (await _shopConnectionAdapter.GetAllAsync()).ToList();
		var revenues = (await _revenueAdapter.GetAllAsync()).ToList();

		return new SyncHealthReport
		{
			GeneratedAt = now,
			Workers = BuildWorkerStatuses(heartbeats, now),
			Platforms = BuildPlatformStatuses(platformConnections, shops, revenues, now),
			ProblemShops = BuildProblemShops(shops, platformConnections, now)
		};
	}

	private static List<WorkerHealthStatus> BuildWorkerStatuses(List<Heartbeat> heartbeats, DateTime now)
	{
		var byName = heartbeats.ToDictionary(h => h.HeartbeatSource, h => h, StringComparer.OrdinalIgnoreCase);
		var statuses = new List<WorkerHealthStatus>();

		foreach (var source in ExpectedWorkerSources)
		{
			if (!byName.TryGetValue(source, out var heartbeat))
			{
				statuses.Add(new WorkerHealthStatus
				{
					Source = source,
					HasTicked = false,
					IsStale = true
				});
				continue;
			}

			var sinceLast = now - heartbeat.LastTickAt;
			var staleThreshold = TimeSpan.FromSeconds(heartbeat.ExpectedIntervalSeconds * StaleSecondsMultiplier);

			statuses.Add(new WorkerHealthStatus
			{
				ExpectedIntervalSeconds = heartbeat.ExpectedIntervalSeconds,
				HasTicked = true,
				IsStale = sinceLast > staleThreshold,
				LastTickAt = heartbeat.LastTickAt,
				Metadata = heartbeat.Metadata,
				SecondsSinceLastTick = sinceLast,
				Source = source
			});
		}

		return statuses;
	}

	private static List<PlatformHealthStatus> BuildPlatformStatuses(List<PlatformConnection> connections, List<ShopConnection> shops, List<Revenue> revenues, DateTime now)
	{
		var statuses = new List<PlatformHealthStatus>();

		var revenueByPlatform = revenues
			.GroupBy(r => r.Platform, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => new
			{
				Total = g.Sum(r => r.NetAmount),
				Count = g.Count(),
				MostRecent = g.Max(r => r.TransactionDate)
			}, StringComparer.OrdinalIgnoreCase);

		var shopsByPlatformId = shops.ToLookup(s => s.PlatformConnectionId);
		var connectionsByPlatform = connections
			.GroupBy(c => c.Platform, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

		foreach (var platform in AvailablePlatforms)
		{
			var platformConnections = connectionsByPlatform.TryGetValue(platform, out var c) ? c : [];
			var platformShops = platformConnections.SelectMany(pc => shopsByPlatformId[pc.Id]).ToList();
			var failingShops = platformShops.Count(s => s.ConsecutiveFailures > 0);
			var staleShops = platformShops.Count(s => IsStaleShop(s, now));
			var lastSyncAt = platformShops
				.Where(s => s.LastSyncDateTime.HasValue)
				.Select(s => s.LastSyncDateTime!.Value)
				.DefaultIfEmpty()
				.Max();

			var revenueStats = revenueByPlatform.TryGetValue(platform, out var stats) ? stats : null;

			statuses.Add(new PlatformHealthStatus
			{
				FailingShops = failingShops,
				IsAvailable = true,
				LastSyncAt = lastSyncAt == default ? null : lastSyncAt,
				MostRecentRevenueAt = revenueStats?.MostRecent,
				Platform = platform,
				RevenueRowCount = revenueStats?.Count ?? 0,
				RevenueTotal = revenueStats?.Total ?? 0m,
				StaleShops = staleShops,
				TotalShops = platformShops.Count
			});
		}

		return statuses;
	}

	private static List<ShopHealthIssue> BuildProblemShops(List<ShopConnection> shops, List<PlatformConnection> connections, DateTime now)
	{
		var platformById = connections.ToDictionary(c => c.Id, c => c.Platform);

		var issues = shops
			.Where(s => s.IsActive && s.IsEnabled)
			.Select(s => new
			{
				Shop = s,
				Platform = platformById.TryGetValue(s.PlatformConnectionId, out var p) ? p : "Unknown",
				IsFailing = s.ConsecutiveFailures > 0,
				IsStale = IsStaleShop(s, now)
			})
			.Where(x => x.IsFailing || x.IsStale)
			.OrderByDescending(x => x.Shop.ConsecutiveFailures)
			.ThenBy(x => x.Shop.LastSyncDateTime ?? DateTime.MinValue)
			.Take(ProblemShopRowCap)
			.Select(x => new ShopHealthIssue
			{
				ConsecutiveFailures = x.Shop.ConsecutiveFailures,
				IssueKind = x.IsFailing ? (x.IsStale ? "Failing + Stale" : "Failing") : "Stale",
				LastErrorMessage = x.Shop.LastErrorMessage,
				LastSyncDateTime = x.Shop.LastSyncDateTime,
				NextSyncDateTime = x.Shop.NextSyncDateTime,
				Platform = x.Platform,
				ShopConnectionId = x.Shop.Id,
				ShopName = string.IsNullOrWhiteSpace(x.Shop.ShopName) ? $"Shop {x.Shop.Id}" : x.Shop.ShopName,
				UserId = x.Shop.UserId
			})
			.ToList();

		return issues;
	}

	private static bool IsStaleShop(ShopConnection shop, DateTime now)
	{
		if (!shop.IsActive || !shop.IsEnabled)
			return false;

		if (!shop.LastSyncDateTime.HasValue)
			return false; // never-synced is a different signal — not "stale"

		return now - shop.LastSyncDateTime.Value > StaleShopGracePeriod;
	}


	#endregion
}
