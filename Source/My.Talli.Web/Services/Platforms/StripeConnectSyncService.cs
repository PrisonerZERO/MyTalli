namespace My.Talli.Web.Services.Platforms;

using Domain.Commands.Platforms;
using Domain.Models;
using Stripe;

using STRIPE = Stripe;

/// <summary>Service</summary>
public class StripeConnectSyncService : IPlatformSyncService
{
    #region <Constants>

    private const int InitialBackfillDays = 90;
    private const int MaxPagesPerSync = 50;
    private const int PageSize = 100;
    private static readonly TimeSpan PerPageDelay = TimeSpan.FromMilliseconds(200);

    #endregion

    #region <Variables>

    private readonly ILogger<StripeConnectSyncService> _logger;
    private readonly IStripeConnectApiClient _stripeConnect;
    private readonly UpsertStripePayoutCommand _upsertStripePayoutCommand;
    private readonly UpsertStripeRevenueCommand _upsertStripeRevenueCommand;

    #endregion

    #region <Constructors>

    public StripeConnectSyncService(ILogger<StripeConnectSyncService> logger, IStripeConnectApiClient stripeConnect, UpsertStripePayoutCommand upsertStripePayoutCommand, UpsertStripeRevenueCommand upsertStripeRevenueCommand)
    {
        _logger = logger;
        _stripeConnect = stripeConnect;
        _upsertStripePayoutCommand = upsertStripePayoutCommand;
        _upsertStripeRevenueCommand = upsertStripeRevenueCommand;
    }

    #endregion

    #region <Properties>

    public string Platform => "Stripe";

    #endregion

    #region <Methods>

    public async Task<PlatformSyncResult> SyncShopAsync(ShopConnection shop, CancellationToken cancellationToken)
    {
        var result = new PlatformSyncResult();
        var accountId = shop.PlatformAccountId;
        var createdAfter = shop.LastSyncDateTime ?? DateTime.UtcNow.AddDays(-InitialBackfillDays);

        await SyncChargesAsync(shop, accountId, createdAfter, result, cancellationToken);
        await SyncPayoutsAsync(shop, accountId, createdAfter, result, cancellationToken);

        return result;
    }

    private async Task SyncChargesAsync(ShopConnection shop, string accountId, DateTime createdAfter, PlatformSyncResult result, CancellationToken cancellationToken)
    {
        string? startingAfter = null;
        var pagesFetched = 0;

        while (!cancellationToken.IsCancellationRequested && pagesFetched < MaxPagesPerSync)
        {
            var page = await _stripeConnect.ListChargesAsync(accountId, createdAfter, startingAfter, PageSize, cancellationToken);
            result.PagesFetched++;
            result.ReceiptsProcessed += page.Data.Count;
            pagesFetched++;

            if (page.Data.Count == 0)
                break;

            var inputs = BuildRevenueInputs(page.Data);
            if (inputs.Count > 0)
            {
                var inserted = await _upsertStripeRevenueCommand.ExecuteAsync(shop.UserId, shop.Id, inputs);
                result.NewRevenueRowCount += inserted;

                var mostRecent = inputs.Max(i => i.TransactionDate);
                if (result.MostRecentTransactionDate is null || mostRecent > result.MostRecentTransactionDate)
                    result.MostRecentTransactionDate = mostRecent;
            }

            if (!page.HasMore)
                break;

            startingAfter = page.Data[^1].Id;

            try { await Task.Delay(PerPageDelay, cancellationToken); }
            catch (TaskCanceledException) { break; }
        }

        if (pagesFetched >= MaxPagesPerSync)
            _logger.LogWarning("Stripe charge sync hit page cap ({Cap}) for shop {ShopId}; remaining charges will be picked up on next sync.", MaxPagesPerSync, shop.Id);
    }

    private async Task SyncPayoutsAsync(ShopConnection shop, string accountId, DateTime createdAfter, PlatformSyncResult result, CancellationToken cancellationToken)
    {
        string? startingAfter = null;
        var pagesFetched = 0;

        while (!cancellationToken.IsCancellationRequested && pagesFetched < MaxPagesPerSync)
        {
            var page = await _stripeConnect.ListPayoutsAsync(accountId, createdAfter, startingAfter, PageSize, cancellationToken);
            result.LedgerPagesFetched++;
            result.LedgerEntriesProcessed += page.Data.Count;
            pagesFetched++;

            if (page.Data.Count == 0)
                break;

            var inputs = BuildPayoutInputs(page.Data);
            if (inputs.Count > 0)
            {
                var inserted = await _upsertStripePayoutCommand.ExecuteAsync(shop.UserId, shop.Id, inputs);
                result.NewPayoutRowCount += inserted;
            }

            if (!page.HasMore)
                break;

            startingAfter = page.Data[^1].Id;

            try { await Task.Delay(PerPageDelay, cancellationToken); }
            catch (TaskCanceledException) { break; }
        }

        if (pagesFetched >= MaxPagesPerSync)
            _logger.LogWarning("Stripe payout sync hit page cap ({Cap}) for shop {ShopId}; remaining payouts will be picked up on next sync.", MaxPagesPerSync, shop.Id);
    }

    private static List<StripeRevenueInput> BuildRevenueInputs(IEnumerable<Charge> charges)
    {
        var inputs = new List<StripeRevenueInput>();

        foreach (var charge in charges)
        {
            if (string.IsNullOrEmpty(charge.Id))
                continue;

            // Stripe amounts are in the smallest unit (cents for USD). Convert to decimal.
            var gross = charge.Amount / 100m;
            var fee = charge.BalanceTransaction?.Fee / 100m ?? 0m;
            var net = charge.BalanceTransaction?.Net / 100m ?? gross - fee;

            // Use the BalanceTransaction id as the canonical dedup key — every settled charge has one,
            // and it's also what appears in payout reconciliations.
            var balanceTxnId = charge.BalanceTransactionId ?? charge.Id;

            inputs.Add(new StripeRevenueInput
            {
                BalanceTransactionId = balanceTxnId,
                Currency = string.IsNullOrEmpty(charge.Currency) ? "usd" : charge.Currency,
                Description = charge.Description ?? string.Empty,
                ExchangeRate = charge.BalanceTransaction?.ExchangeRate,
                FeeAmount = fee,
                GrossAmount = gross,
                IsDisputed = charge.Disputed,
                IsRefunded = charge.Refunded,
                NetAmount = net,
                PaymentMethod = ResolvePaymentMethod(charge),
                RiskScore = (int?)charge.Outcome?.RiskScore,
                TransactionDate = DateTime.SpecifyKind(charge.Created, DateTimeKind.Utc)
            });
        }

        return inputs;
    }

    private static List<StripePayoutInput> BuildPayoutInputs(IEnumerable<STRIPE.Payout> payouts)
    {
        var inputs = new List<StripePayoutInput>();

        foreach (var payout in payouts)
        {
            if (string.IsNullOrEmpty(payout.Id))
                continue;

            inputs.Add(new StripePayoutInput
            {
                Amount = payout.Amount / 100m,
                Currency = string.IsNullOrEmpty(payout.Currency) ? "usd" : payout.Currency,
                ExpectedArrivalDate = payout.ArrivalDate == default ? null : DateTime.SpecifyKind(payout.ArrivalDate, DateTimeKind.Utc),
                PayoutDate = DateTime.SpecifyKind(payout.Created, DateTimeKind.Utc),
                PayoutMethod = string.IsNullOrEmpty(payout.Method) ? "standard" : payout.Method,
                StatementDescriptor = payout.StatementDescriptor,
                Status = string.IsNullOrEmpty(payout.Status) ? "Pending" : payout.Status,
                StripePayoutId = payout.Id
            });
        }

        return inputs;
    }

    private static string ResolvePaymentMethod(Charge charge)
    {
        // PaymentMethodDetails.Type is the canonical short string (card, bank_transfer, etc.)
        // Fallback chain handles older Stripe.net shapes and missing data.
        if (charge.PaymentMethodDetails is not null && !string.IsNullOrEmpty(charge.PaymentMethodDetails.Type))
            return charge.PaymentMethodDetails.Type;

        if (!string.IsNullOrEmpty(charge.PaymentMethod))
            return charge.PaymentMethod;

        return string.Empty;
    }

    #endregion
}
