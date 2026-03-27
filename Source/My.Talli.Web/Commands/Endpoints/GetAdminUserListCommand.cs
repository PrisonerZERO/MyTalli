namespace My.Talli.Web.Commands.Endpoints;

using Domain.Data.EntityFramework;
using Domain.Framework;
using Domain.Models;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetAdminUserListCommand
{
    #region <Variables>

    private readonly TalliDbContext _dbContext;
    private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

    #endregion

    #region <Constructors>

    public GetAdminUserListCommand(TalliDbContext dbContext, RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
    {
        _dbContext = dbContext;
        _subscriptionAdapter = subscriptionAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<List<AdminUserListItem>> ExecuteAsync()
    {
        // Users with emails from the view
        var authenticatedUsers = await _dbContext.AuthenticatedUsers
            .OrderBy(u => u.Id)
            .ToListAsync();

        // Active subscription user IDs (Active or Cancelling = still has Pro access)
        var activeSubscriptions = await _subscriptionAdapter.FindAsync(s =>
            s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling);
        var activeUserIds = new HashSet<long>(activeSubscriptions.Select(s => s.UserId));

        return authenticatedUsers.Select(u => new AdminUserListItem
        {
            DisplayName = u.DisplayName,
            Email = u.EmailAddress,
            FirstName = u.FirstName,
            HasActiveSubscription = activeUserIds.Contains(u.Id),
            PreferredProvider = u.PreferredProvider,
            UserId = u.Id
        }).ToList();
    }

    #endregion
}
