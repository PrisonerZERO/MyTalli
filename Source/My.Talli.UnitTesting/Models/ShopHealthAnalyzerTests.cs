namespace My.Talli.UnitTesting.Models;

using My.Talli.Web.Models;

using MODELS = My.Talli.Domain.Models;

/// <summary>Tests</summary>
public class ShopHealthAnalyzerTests
{
    #region <Methods>

    [Fact]
    public void Classify_PausedShop_ReturnsPaused()
    {
        var shop = BuildShop(isEnabled: false, consecutiveFailures: 0, lastSync: DateTime.UtcNow);

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, DateTime.UtcNow);

        Assert.Equal(ShopHealth.Paused, health);
    }

    [Fact]
    public void Classify_FailingShop_ReturnsFailing()
    {
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 1, lastSync: DateTime.UtcNow);

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, DateTime.UtcNow);

        Assert.Equal(ShopHealth.Failing, health);
    }

    [Fact]
    public void Classify_StaleShop_ReturnsStale()
    {
        var now = DateTime.UtcNow;
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 0, lastSync: now.AddDays(-3));

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, now);

        Assert.Equal(ShopHealth.Stale, health);
    }

    [Fact]
    public void Classify_RecentlySynced_ReturnsHealthy()
    {
        var now = DateTime.UtcNow;
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 0, lastSync: now.AddHours(-2));

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, now);

        Assert.Equal(ShopHealth.Healthy, health);
    }

    [Fact]
    public void Classify_NeverSynced_ReturnsHealthy()
    {
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 0, lastSync: null);

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, DateTime.UtcNow);

        Assert.Equal(ShopHealth.Healthy, health);
    }

    [Fact]
    public void Classify_FailingTakesPriorityOverStale()
    {
        var now = DateTime.UtcNow;
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 5, lastSync: now.AddDays(-7));

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, now);

        Assert.Equal(ShopHealth.Failing, health);
    }

    [Fact]
    public void Classify_StatusPending_ReturnsSyncPending()
    {
        // Brand-new shop or just-reconnected: Status = "Pending", failure state cleared.
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 0, lastSync: null, status: "Pending");

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, DateTime.UtcNow);

        Assert.Equal(ShopHealth.SyncPending, health);
    }

    [Fact]
    public void Classify_StatusPendingWithOldLastSync_ReturnsSyncPendingNotStale()
    {
        // Reconnect scenario: ConnectXxxCommand reset failures + cleared error + set Status=Pending,
        // but LastSyncDateTime still points at the old sync from before the failure cycle. Should
        // classify as SyncPending (forward-looking), NOT Stale (backward-looking).
        var now = DateTime.UtcNow;
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 0, lastSync: now.AddDays(-8), status: "Pending");

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, now);

        Assert.Equal(ShopHealth.SyncPending, health);
    }

    [Fact]
    public void Classify_FailingTakesPriorityOverPending()
    {
        // If the worker fired AFTER reconnect and failed, we want to surface the new failure
        // (not the stale "Pending" status from the reconnect).
        var shop = BuildShop(isEnabled: true, consecutiveFailures: 1, lastSync: null, status: "Pending");

        var health = ShopHealthAnalyzer.ClassifyHealth(shop, DateTime.UtcNow);

        Assert.Equal(ShopHealth.Failing, health);
    }

    [Theory]
    [InlineData("An error occurred during a cryptographic operation", "can't be read")]
    [InlineData("DataProtection: failed to unprotect", "can't be read")]
    [InlineData("invalid_grant: token expired", "expired")]
    [InlineData("invalid_token", "expired")]
    [InlineData("HTTP 401 Unauthorized", "Access denied")]
    [InlineData("HTTP 403 Forbidden", "revoked access")]
    [InlineData("HTTP 429 Too Many Requests", "Rate limited")]
    [InlineData("HTTP 503 Service Unavailable", "temporarily unavailable")]
    [InlineData("Connection timeout after 30s", "temporarily unavailable")]
    public void Translate_KnownPattern_ProducesFriendlyMessage(string raw, string expectedFragment)
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Failing, "Etsy", raw, DateTime.UtcNow, DateTime.UtcNow);

        Assert.Contains(expectedFragment, message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Translate_UnknownError_ReturnsGenericMessage()
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Failing, "Gumroad", "Some weird unmatched message", DateTime.UtcNow, DateTime.UtcNow);

        Assert.Contains("Try reconnecting", message);
    }

    [Fact]
    public void Translate_NullError_ReturnsGenericMessage()
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Failing, "Etsy", null, DateTime.UtcNow, DateTime.UtcNow);

        Assert.Contains("Try reconnecting", message);
    }

    [Fact]
    public void Translate_HealthyShop_ReturnsEmpty()
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Healthy, "Etsy", null, DateTime.UtcNow, DateTime.UtcNow);

        Assert.Equal(string.Empty, message);
    }

    [Fact]
    public void Translate_PausedShop_MentionsResume()
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Paused, "Etsy", null, DateTime.UtcNow, DateTime.UtcNow);

        Assert.Contains("Resume", message);
    }

    [Fact]
    public void Translate_StaleShop_NeverSynced_TellsUserItsAboutToStart()
    {
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Stale, "Etsy", null, lastSyncDateTime: null, DateTime.UtcNow);

        Assert.Contains("first one", message);
    }

    [Fact]
    public void Translate_StaleShop_ShortGap_SaysWillRetry()
    {
        var now = DateTime.UtcNow;
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Stale, "Etsy", null, now.AddHours(-37), now);

        Assert.Contains("retry automatically", message);
    }

    [Fact]
    public void Translate_StaleShop_LongGap_SuggestsReconnect()
    {
        var now = DateTime.UtcNow;
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.Stale, "Etsy", null, now.AddDays(-5), now);

        Assert.Contains("reconnecting", message);
    }

    [Fact]
    public void Translate_SyncPending_WithPriorSync_ShowsReconnectedMessage()
    {
        // Reconnect scenario — old LastSyncDateTime exists, copy says "Reconnected"
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.SyncPending, "Etsy", null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        Assert.Contains("Reconnected", message);
        Assert.Contains("5 minutes", message);
        Assert.DoesNotContain("Try reconnecting", message);
    }

    [Fact]
    public void Translate_SyncPending_NoPriorSync_ShowsFirstSyncMessage()
    {
        // Brand-new shop scenario — never synced, copy doesn't say "Reconnected"
        var message = ShopHealthAnalyzer.ToFriendlyMessage(ShopHealth.SyncPending, "Etsy", null, lastSyncDateTime: null, DateTime.UtcNow);

        Assert.Contains("First sync", message);
        Assert.DoesNotContain("Reconnected", message);
    }

    [Fact]
    public void HealthLabel_KnownValues_ReturnExpectedStrings()
    {
        Assert.Equal("Healthy", ShopHealthAnalyzer.ToHealthLabel(ShopHealth.Healthy));
        Assert.Equal("Sync Upcoming", ShopHealthAnalyzer.ToHealthLabel(ShopHealth.SyncPending));
        Assert.Equal("Sync Delayed", ShopHealthAnalyzer.ToHealthLabel(ShopHealth.Stale));
        Assert.Equal("Reconnect Needed", ShopHealthAnalyzer.ToHealthLabel(ShopHealth.Failing));
        Assert.Equal("Paused", ShopHealthAnalyzer.ToHealthLabel(ShopHealth.Paused));
    }

    private static MODELS.ShopConnection BuildShop(bool isEnabled, int consecutiveFailures, DateTime? lastSync, string status = "")
    {
        return new MODELS.ShopConnection
        {
            ConsecutiveFailures = consecutiveFailures,
            IsActive = true,
            IsEnabled = isEnabled,
            LastSyncDateTime = lastSync,
            NextSyncDateTime = DateTime.UtcNow.AddHours(1),
            Status = status
        };
    }

    #endregion
}
