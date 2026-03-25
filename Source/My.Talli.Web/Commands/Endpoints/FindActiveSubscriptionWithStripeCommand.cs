namespace My.Talli.Web.Commands.Endpoints;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class FindActiveSubscriptionWithStripeCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
    private readonly RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> _subscriptionStripeAdapter;

    #endregion

    #region <Constructors>

    public FindActiveSubscriptionWithStripeCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter, RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> subscriptionStripeAdapter)
    {
        _subscriptionAdapter = subscriptionAdapter;
        _subscriptionStripeAdapter = subscriptionStripeAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<(Subscription?, SubscriptionStripe?)> ExecuteAsync(long userId)
    {
        var subscription = (await _subscriptionAdapter.FindAsync(x => x.UserId == userId && (x.Status == "Active" || x.Status == "Cancelling"))).FirstOrDefault();
        if (subscription is null)
            return (null, null);

        var stripeRecord = await _subscriptionStripeAdapter.GetByIdAsync(subscription.Id);
        return (subscription, stripeRecord);
    }

    #endregion
}
