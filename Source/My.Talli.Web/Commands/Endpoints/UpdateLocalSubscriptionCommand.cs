namespace My.Talli.Web.Commands.Endpoints;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpdateLocalSubscriptionCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<Product, ENTITIES.Product> _productAdapter;
    private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
    private readonly RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> _subscriptionStripeAdapter;

    #endregion

    #region <Constructors>

    public UpdateLocalSubscriptionCommand(RepositoryAdapterAsync<Product, ENTITIES.Product> productAdapter, RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter, RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> subscriptionStripeAdapter)
    {
        _productAdapter = productAdapter;
        _subscriptionAdapter = subscriptionAdapter;
        _subscriptionStripeAdapter = subscriptionStripeAdapter;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(Subscription subscription, SubscriptionStripe stripeRecord, string plan, string newPriceId)
    {
        var newProductName = plan == "yearly" ? "Pro Yearly" : "Pro Monthly";
        var newProduct = (await _productAdapter.FindAsync(x => x.ProductName == newProductName)).FirstOrDefault();

        // TRANSACTION
        await EnforcedTransactionScope.ExecuteAsync(async () =>
        {
            if (newProduct is not null)
            {
                subscription.ProductId = newProduct.Id;
                await _subscriptionAdapter.UpdateAsync(subscription);
            }

            stripeRecord.StripePriceId = newPriceId;
            await _subscriptionStripeAdapter.UpdateAsync(stripeRecord);
        });
    }

    #endregion
}
