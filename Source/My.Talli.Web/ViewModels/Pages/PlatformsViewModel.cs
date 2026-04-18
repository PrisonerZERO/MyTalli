namespace My.Talli.Web.ViewModels.Pages;

using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;
using System.Security.Claims;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class PlatformsViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter { get; set; } = default!;

	#endregion

	#region <Properties>

	public int AvailableCount => Platforms.Count(p => !p.IsConnected);

	public List<PlatformItem> AvailablePlatforms => Platforms.Where(p => !p.IsConnected).ToList();

	public int ConnectedCount => Platforms.Count(p => p.IsConnected);

	public List<PlatformItem> ConnectedPlatforms => Platforms.Where(p => p.IsConnected).ToList();

	public bool IsLoading { get; private set; } = true;

	public int TotalTransactions => Platforms.Where(p => p.IsConnected).Sum(p => p.TransactionCount);


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
		{
			IsLoading = false;
			return;
		}

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
		{
			IsLoading = false;
			return;
		}

		await LoadPlatformsAsync(userId);
		IsLoading = false;
	}


	#endregion

	#region <Methods>

	private static List<PlatformItem> GetPlatformCatalog()
	{
		return
		[
			new PlatformItem
			{
				BrandColor = "#635bff",
				Description = "Charges, refunds, subscriptions & payouts",
				Icon = "stripe",
				Name = "Stripe",
				Subtitle = "Payment processing",
			},
			new PlatformItem
			{
				BrandColor = "#f56400",
				Description = "Shop receipts, transactions & payments",
				Icon = "etsy",
				Name = "Etsy",
				Subtitle = "Handmade marketplace",
			},
			new PlatformItem
			{
				BrandColor = "#ff90e8",
				Description = "Sales, products & subscriber revenue",
				Icon = "gumroad",
				Name = "Gumroad",
				Subtitle = "Digital products",
			},
			new PlatformItem
			{
				BrandColor = "#003087",
				Description = "Transactions, invoices & balance",
				Icon = "paypal",
				Name = "PayPal",
				Subtitle = "Payment service",
			},
			new PlatformItem
			{
				BrandColor = "#96bf48",
				Description = "Orders, products & revenue",
				Icon = "shopify",
				Name = "Shopify",
				Subtitle = "E-commerce platform",
			},
		];
	}

	private async Task LoadPlatformsAsync(long userId)
	{
		// Query real data
		var connections = await PlatformConnectionAdapter.FindAsync(p => p.UserId == userId);
		var shops = await ShopConnectionAdapter.FindAsync(s => s.UserId == userId);
		var revenues = await RevenueAdapter.FindAsync(r => r.UserId == userId);

		var connectionsByPlatform = connections.ToDictionary(c => c.Platform, StringComparer.OrdinalIgnoreCase);
		var shopsByConnectionId = shops.GroupBy(s => s.PlatformConnectionId).ToDictionary(g => g.Key, g => g.First());
		var transactionCounts = revenues.GroupBy(r => r.Platform, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

		// Merge catalog with real data
		var catalog = GetPlatformCatalog();

		foreach (var item in catalog)
		{
			if (connectionsByPlatform.TryGetValue(item.Name, out var connection))
			{
				item.IsConnected = true;
				item.ConnectionStatus = connection.ConnectionStatus;

				if (shopsByConnectionId.TryGetValue(connection.Id, out var shop))
				{
					item.IsEnabled = shop.IsEnabled;
					item.LastErrorMessage = shop.LastErrorMessage;
					item.LastSyncLabel = ToSyncLabel(shop);
					item.SyncStatus = shop.Status;
				}
			}

			if (transactionCounts.TryGetValue(item.Name, out var count))
				item.TransactionCount = count;
		}

		Platforms = catalog;
	}

	private static string ToSyncLabel(MODELS.ShopConnection shop)
	{
		if (shop.LastSyncDateTime == null)
			return "Never synced";

		var elapsed = DateTime.UtcNow - shop.LastSyncDateTime.Value;

		return elapsed.TotalMinutes < 1 ? "Synced just now"
			: elapsed.TotalMinutes < 60 ? $"Synced {(int)elapsed.TotalMinutes} min ago"
			: elapsed.TotalHours < 24 ? $"Synced {(int)elapsed.TotalHours} hours ago"
			: $"Synced {(int)elapsed.TotalDays} days ago";
	}

	private List<PlatformItem> Platforms { get; set; } = [];

	#endregion
}
