namespace My.Talli.UnitTesting.Commands.Admin;

using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class WriteHeartbeatTickCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_NewSource_InsertsRowWithLastTickAt()
    {
        var builder = new AdminBuilder();
        var before = DateTime.UtcNow;

        var result = await builder.WriteHeartbeatTick.ExecuteAsync("AdminHealthWorker", expectedIntervalSeconds: 60);

        var after = DateTime.UtcNow;
        Assert.Equal("AdminHealthWorker", result.HeartbeatSource);
        Assert.Equal(60, result.ExpectedIntervalSeconds);
        Assert.InRange(result.LastTickAt, before, after);
        Assert.Null(result.Metadata);
        Assert.Single(await builder.HeartbeatAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_ExistingSource_UpdatesLastTickAtWithoutInserting()
    {
        var builder = new AdminBuilder();
        var first = await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300);
        await Task.Delay(10);

        var second = await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300);

        Assert.Equal(first.Id, second.Id);
        Assert.True(second.LastTickAt >= first.LastTickAt);
        Assert.Single(await builder.HeartbeatAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_DifferentSources_InsertsSeparateRows()
    {
        var builder = new AdminBuilder();

        await builder.WriteHeartbeatTick.ExecuteAsync("AdminHealthWorker", 60);
        await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300);
        await builder.WriteHeartbeatTick.ExecuteAsync("TokenRefreshWorker", 21600);

        var all = (await builder.HeartbeatAdapter.GetAllAsync()).ToList();
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task Execute_MetadataPassedThrough()
    {
        var builder = new AdminBuilder();

        var result = await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300, metadata: "{\"shopsProcessed\":5}");

        Assert.Equal("{\"shopsProcessed\":5}", result.Metadata);
    }

    [Fact]
    public async Task Execute_ExistingSource_OverridesMetadataAndInterval()
    {
        var builder = new AdminBuilder();
        await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300, metadata: "{\"shopsProcessed\":5}");

        var second = await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 600, metadata: "{\"shopsProcessed\":7}");

        Assert.Equal(600, second.ExpectedIntervalSeconds);
        Assert.Equal("{\"shopsProcessed\":7}", second.Metadata);
    }

    [Fact]
    public async Task Execute_ExistingSource_NullMetadata_OverridesPriorMetadata()
    {
        var builder = new AdminBuilder();
        await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300, metadata: "{\"shopsProcessed\":5}");

        var second = await builder.WriteHeartbeatTick.ExecuteAsync("ShopSyncWorker", 300);

        Assert.Null(second.Metadata);
    }

    #endregion
}
