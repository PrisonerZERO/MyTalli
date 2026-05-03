namespace My.Talli.UnitTesting.Commands.Admin;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class GetSyncHealthCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyDatabase_ReturnsAllExpectedSourcesAsNeverTicked()
    {
        var builder = new AdminBuilder();

        var report = await builder.GetSyncHealth.ExecuteAsync();

        Assert.Equal(3, report.Workers.Count);
        Assert.All(report.Workers, w =>
        {
            Assert.False(w.HasTicked);
            Assert.True(w.IsStale);
        });
    }

    [Fact]
    public async Task Execute_EmptyDatabase_AllPlatformsReturnZeroes()
    {
        var builder = new AdminBuilder();

        var report = await builder.GetSyncHealth.ExecuteAsync();

        Assert.Equal(3, report.Platforms.Count);
        Assert.All(report.Platforms, p =>
        {
            Assert.Equal(0, p.TotalShops);
            Assert.Equal(0, p.RevenueRowCount);
            Assert.Equal(0m, p.RevenueTotal);
            Assert.Equal(0, p.FailingShops);
            Assert.Equal(0, p.StaleShops);
        });
    }

    [Fact]
    public async Task Execute_NoProblemShops_ProblemShopsListIsEmpty()
    {
        var builder = new AdminBuilder();

        var report = await builder.GetSyncHealth.ExecuteAsync();

        Assert.Empty(report.ProblemShops);
    }

    [Fact]
    public async Task Execute_HeartbeatRowExists_WorkerReportedAsHealthy()
    {
        var builder = new AdminBuilder();
        await builder.HeartbeatAdapter.InsertAsync(new Heartbeat
        {
            HeartbeatSource = "AdminHealthWorker",
            ExpectedIntervalSeconds = 60,
            LastTickAt = DateTime.UtcNow.AddSeconds(-30)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var worker = report.Workers.Single(w => w.Source == "AdminHealthWorker");
        Assert.True(worker.HasTicked);
        Assert.False(worker.IsStale);
    }

    [Fact]
    public async Task Execute_HeartbeatStale_WorkerReportedAsStale()
    {
        var builder = new AdminBuilder();
        await builder.HeartbeatAdapter.InsertAsync(new Heartbeat
        {
            HeartbeatSource = "AdminHealthWorker",
            ExpectedIntervalSeconds = 60,
            LastTickAt = DateTime.UtcNow.AddMinutes(-10) // way past 2× expected interval
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var worker = report.Workers.Single(w => w.Source == "AdminHealthWorker");
        Assert.True(worker.HasTicked);
        Assert.True(worker.IsStale);
    }

    [Fact]
    public async Task Execute_RevenueRowsExist_PlatformTotalsAggregated()
    {
        var builder = new AdminBuilder();
        await builder.RevenueAdapter.InsertAsync(new Revenue
        {
            Platform = "Etsy",
            NetAmount = 100m,
            UserId = 1,
            TransactionDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        await builder.RevenueAdapter.InsertAsync(new Revenue
        {
            Platform = "Etsy",
            NetAmount = 50m,
            UserId = 1,
            TransactionDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc)
        });
        await builder.RevenueAdapter.InsertAsync(new Revenue
        {
            Platform = "Stripe",
            NetAmount = 200m,
            UserId = 1,
            TransactionDate = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var etsy = report.Platforms.Single(p => p.Platform == "Etsy");
        Assert.Equal(150m, etsy.RevenueTotal);
        Assert.Equal(2, etsy.RevenueRowCount);
        Assert.Equal(new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), etsy.MostRecentRevenueAt);

        var stripe = report.Platforms.Single(p => p.Platform == "Stripe");
        Assert.Equal(200m, stripe.RevenueTotal);
        Assert.Equal(1, stripe.RevenueRowCount);
    }

    [Fact]
    public async Task Execute_FailingShop_ListedInProblemShops()
    {
        var builder = new AdminBuilder();
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            Platform = "Etsy",
            UserId = 1,
            ConnectionStatus = "Active"
        });
        await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            PlatformConnectionId = connection.Id,
            UserId = 1,
            ShopName = "BrokenShop",
            ConsecutiveFailures = 3,
            LastErrorMessage = "Etsy API returned 500",
            IsActive = true,
            IsEnabled = true,
            AccessToken = "tok",
            PlatformAccountId = "acct-1",
            PlatformShopId = "shop-1",
            Status = "Failed",
            NextSyncDateTime = DateTime.UtcNow.AddHours(1)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var problem = Assert.Single(report.ProblemShops);
        Assert.Equal("BrokenShop", problem.ShopName);
        Assert.Equal("Etsy", problem.Platform);
        Assert.Equal(3, problem.ConsecutiveFailures);
        Assert.Equal("Failing", problem.IssueKind);
        Assert.Equal("Etsy API returned 500", problem.LastErrorMessage);

        var etsy = report.Platforms.Single(p => p.Platform == "Etsy");
        Assert.Equal(1, etsy.TotalShops);
        Assert.Equal(1, etsy.FailingShops);
    }

    [Fact]
    public async Task Execute_StaleShop_ListedAsStaleNotFailing()
    {
        var builder = new AdminBuilder();
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            Platform = "Stripe",
            UserId = 1,
            ConnectionStatus = "Active"
        });
        await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            PlatformConnectionId = connection.Id,
            UserId = 1,
            ShopName = "StaleShop",
            ConsecutiveFailures = 0,
            LastSyncDateTime = DateTime.UtcNow.AddDays(-3), // way past the 36h grace period
            IsActive = true,
            IsEnabled = true,
            AccessToken = "tok",
            PlatformAccountId = "acct-1",
            PlatformShopId = "shop-1",
            Status = "Completed",
            NextSyncDateTime = DateTime.UtcNow.AddHours(1)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var problem = Assert.Single(report.ProblemShops);
        Assert.Equal("Stale", problem.IssueKind);
        Assert.Equal(0, problem.ConsecutiveFailures);

        var stripe = report.Platforms.Single(p => p.Platform == "Stripe");
        Assert.Equal(1, stripe.StaleShops);
        Assert.Equal(0, stripe.FailingShops);
    }

    [Fact]
    public async Task Execute_NeverSyncedShop_NotMarkedAsStale()
    {
        var builder = new AdminBuilder();
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            Platform = "Gumroad",
            UserId = 1,
            ConnectionStatus = "Active"
        });
        await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            PlatformConnectionId = connection.Id,
            UserId = 1,
            ShopName = "NewShop",
            ConsecutiveFailures = 0,
            LastSyncDateTime = null, // never synced — should NOT be classified as stale
            IsActive = true,
            IsEnabled = true,
            AccessToken = "tok",
            PlatformAccountId = "acct-1",
            PlatformShopId = "shop-1",
            Status = "Pending",
            NextSyncDateTime = DateTime.UtcNow
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        Assert.Empty(report.ProblemShops);

        var gumroad = report.Platforms.Single(p => p.Platform == "Gumroad");
        Assert.Equal(1, gumroad.TotalShops);
        Assert.Equal(0, gumroad.StaleShops);
    }

    [Fact]
    public async Task Execute_DisabledShop_NotListedAsProblem()
    {
        var builder = new AdminBuilder();
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            Platform = "Etsy",
            UserId = 1,
            ConnectionStatus = "Active"
        });
        await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            PlatformConnectionId = connection.Id,
            UserId = 1,
            ShopName = "PausedShop",
            ConsecutiveFailures = 5, // would normally count
            IsActive = true,
            IsEnabled = false, // user paused — exclude from problem list
            AccessToken = "tok",
            PlatformAccountId = "acct-1",
            PlatformShopId = "shop-1",
            Status = "Paused",
            NextSyncDateTime = DateTime.UtcNow.AddYears(1)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        Assert.Empty(report.ProblemShops);
    }

    [Fact]
    public async Task Execute_FailingAndStale_LabeledFailingPlusStale()
    {
        var builder = new AdminBuilder();
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            Platform = "Etsy",
            UserId = 1,
            ConnectionStatus = "Active"
        });
        await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            PlatformConnectionId = connection.Id,
            UserId = 1,
            ShopName = "DoubleTroubleShop",
            ConsecutiveFailures = 4,
            LastSyncDateTime = DateTime.UtcNow.AddDays(-5),
            IsActive = true,
            IsEnabled = true,
            AccessToken = "tok",
            PlatformAccountId = "acct-1",
            PlatformShopId = "shop-1",
            Status = "Failed",
            NextSyncDateTime = DateTime.UtcNow.AddHours(1)
        });

        var report = await builder.GetSyncHealth.ExecuteAsync();

        var problem = Assert.Single(report.ProblemShops);
        Assert.Equal("Failing + Stale", problem.IssueKind);
    }

    #endregion
}
