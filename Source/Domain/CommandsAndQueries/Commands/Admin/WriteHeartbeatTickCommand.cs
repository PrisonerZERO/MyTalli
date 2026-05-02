namespace My.Talli.Domain.Commands.Admin;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class WriteHeartbeatTickCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> _heartbeatAdapter;

    #endregion

    #region <Constructors>

    public WriteHeartbeatTickCommand(RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> heartbeatAdapter)
    {
        _heartbeatAdapter = heartbeatAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<Heartbeat> ExecuteAsync(string heartbeatSource, int expectedIntervalSeconds, string? metadata = null)
    {
        var now = DateTime.UtcNow;
        var existing = (await _heartbeatAdapter.FindAsync(h => h.HeartbeatSource == heartbeatSource)).FirstOrDefault();

        if (existing is not null)
        {
            existing.ExpectedIntervalSeconds = expectedIntervalSeconds;
            existing.LastTickAt = now;
            existing.Metadata = metadata;
            return await _heartbeatAdapter.UpdateAsync(existing);
        }

        return await _heartbeatAdapter.InsertAsync(new Heartbeat
        {
            ExpectedIntervalSeconds = expectedIntervalSeconds,
            HeartbeatSource = heartbeatSource,
            LastTickAt = now,
            Metadata = metadata
        });
    }

    #endregion
}
