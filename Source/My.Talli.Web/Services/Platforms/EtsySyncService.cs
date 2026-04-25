namespace My.Talli.Web.Services.Platforms;

using Domain.Commands.Platforms;
using Domain.Components;
using Domain.enums;
using Domain.extensions;
using Domain.Models;

/// <summary>Service</summary>
public class EtsySyncService : IPlatformSyncService
{
    #region <Constants>

    private const int InitialBackfillDays = 90;
    private const int PageLimit = 100;
    private static readonly TimeSpan AccessTokenSafetyMargin = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PerPageDelay = TimeSpan.FromMilliseconds(200);

    private static readonly Dictionary<string, EtsyLedgerTypes> KnownLedgerTypes =
        Enum.GetValues<EtsyLedgerTypes>().ToDictionary(t => t.ToStringValue(), t => t, StringComparer.OrdinalIgnoreCase);

    #endregion

    #region <Variables>

    private readonly IEtsyApiClient _etsyService;
    private readonly EtsyTokenRefresher _etsyTokenRefresher;
    private readonly ILogger<EtsySyncService> _logger;
    private readonly RefreshShopTokensCommand _refreshShopTokensCommand;
    private readonly UpsertEtsyExpenseCommand _upsertEtsyExpenseCommand;
    private readonly UpsertEtsyPayoutCommand _upsertEtsyPayoutCommand;
    private readonly UpsertEtsyRevenueCommand _upsertEtsyRevenueCommand;

    #endregion

    #region <Constructors>

    public EtsySyncService(IEtsyApiClient etsyService, EtsyTokenRefresher etsyTokenRefresher, ILogger<EtsySyncService> logger, RefreshShopTokensCommand refreshShopTokensCommand, UpsertEtsyExpenseCommand upsertEtsyExpenseCommand, UpsertEtsyPayoutCommand upsertEtsyPayoutCommand, UpsertEtsyRevenueCommand upsertEtsyRevenueCommand)
    {
        _etsyService = etsyService;
        _etsyTokenRefresher = etsyTokenRefresher;
        _logger = logger;
        _refreshShopTokensCommand = refreshShopTokensCommand;
        _upsertEtsyExpenseCommand = upsertEtsyExpenseCommand;
        _upsertEtsyPayoutCommand = upsertEtsyPayoutCommand;
        _upsertEtsyRevenueCommand = upsertEtsyRevenueCommand;
    }

    #endregion

    #region <Properties>

    public string Platform => "Etsy";

    #endregion

    #region <Methods>

    public async Task<PlatformSyncResult> SyncShopAsync(ShopConnection shop, CancellationToken cancellationToken)
    {
        var result = new PlatformSyncResult();

        var accessToken = await EnsureFreshAccessTokenAsync(shop, cancellationToken);
        var minCreated = BuildMinCreatedTimestamp(shop);
        var shopId = long.Parse(shop.PlatformShopId);

        await SyncReceiptsAsync(shop, shopId, accessToken, minCreated, result, cancellationToken);
        await SyncLedgerAsync(shop, shopId, accessToken, minCreated, result, cancellationToken);

        return result;
    }

    private async Task SyncReceiptsAsync(ShopConnection shop, long shopId, string accessToken, long minCreated, PlatformSyncResult result, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var page = await _etsyService.GetReceiptsAsync(shopId, accessToken, minCreated, PageLimit, offset, cancellationToken);
            result.PagesFetched++;
            result.ReceiptsProcessed += page.Results.Count;

            if (page.Results.Count == 0)
                break;

            var inputs = BuildRevenueInputs(page.Results);
            if (inputs.Count > 0)
            {
                var inserted = await _upsertEtsyRevenueCommand.ExecuteAsync(shop.UserId, shop.Id, inputs);
                result.NewRevenueRowCount += inserted;

                var mostRecent = inputs.Max(i => i.TransactionDate);
                if (result.MostRecentTransactionDate is null || mostRecent > result.MostRecentTransactionDate)
                    result.MostRecentTransactionDate = mostRecent;
            }

            if (page.Results.Count < PageLimit)
                break;

            offset += PageLimit;

            try { await Task.Delay(PerPageDelay, cancellationToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task SyncLedgerAsync(ShopConnection shop, long shopId, string accessToken, long minCreated, PlatformSyncResult result, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var page = await _etsyService.GetLedgerEntriesAsync(shopId, accessToken, minCreated, PageLimit, offset, cancellationToken);
            result.LedgerPagesFetched++;
            result.LedgerEntriesProcessed += page.Results.Count;

            if (page.Results.Count == 0)
                break;

            var classified = ClassifyLedger(page.Results, shopId);

            if (classified.ExpenseInputs.Count > 0)
            {
                var inserted = await _upsertEtsyExpenseCommand.ExecuteAsync(shop.UserId, shop.Id, classified.ExpenseInputs);
                result.NewExpenseRowCount += inserted;
            }

            if (classified.PayoutInputs.Count > 0)
            {
                var inserted = await _upsertEtsyPayoutCommand.ExecuteAsync(shop.UserId, shop.Id, classified.PayoutInputs);
                result.NewPayoutRowCount += inserted;
            }

            if (page.Results.Count < PageLimit)
                break;

            offset += PageLimit;

            try { await Task.Delay(PerPageDelay, cancellationToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task<string> EnsureFreshAccessTokenAsync(ShopConnection shop, CancellationToken cancellationToken)
    {
        var needsRefresh = shop.TokenExpiryDateTime is null ||
                           shop.TokenExpiryDateTime <= DateTime.UtcNow.Add(AccessTokenSafetyMargin);

        if (!needsRefresh)
            return shop.AccessToken;

        if (string.IsNullOrEmpty(shop.RefreshToken))
            throw new InvalidOperationException($"Shop {shop.Id} has no refresh token — cannot sync.");

        _logger.LogInformation("Refreshing Etsy access token inline for shop {ShopId} before sync.", shop.Id);

        var refreshed = await _etsyTokenRefresher.RefreshAsync(shop.RefreshToken, cancellationToken);
        await _refreshShopTokensCommand.ExecuteAsync(shop.Id, refreshed.AccessToken, refreshed.AccessTokenExpiryDateTime, refreshed.RefreshToken, refreshed.RefreshTokenExpiryDateTime);

        shop.AccessToken = refreshed.AccessToken;
        shop.RefreshToken = refreshed.RefreshToken ?? shop.RefreshToken;
        shop.TokenExpiryDateTime = refreshed.AccessTokenExpiryDateTime;
        shop.RefreshTokenExpiryDateTime = refreshed.RefreshTokenExpiryDateTime;

        return refreshed.AccessToken;
    }

    private static long BuildMinCreatedTimestamp(ShopConnection shop)
    {
        var floor = shop.LastSyncDateTime ?? DateTime.UtcNow.AddDays(-InitialBackfillDays);
        return new DateTimeOffset(DateTime.SpecifyKind(floor, DateTimeKind.Utc)).ToUnixTimeSeconds();
    }

    private static List<EtsyRevenueInput> BuildRevenueInputs(IEnumerable<EtsyReceipt> receipts)
    {
        var inputs = new List<EtsyRevenueInput>();

        foreach (var receipt in receipts)
        {
            if (!receipt.IsPaid)
                continue;

            var receiptDate = DateTimeOffset.FromUnixTimeSeconds(receipt.CreateTimestamp).UtcDateTime;

            foreach (var txn in receipt.Transactions)
            {
                if (txn.Price is null)
                    continue;

                var txnDate = txn.PaidTimestamp.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(txn.PaidTimestamp.Value).UtcDateTime
                    : receiptDate;

                var unitPrice = txn.Price.ToDecimal();
                var gross = unitPrice * txn.Quantity;

                inputs.Add(new EtsyRevenueInput
                {
                    Currency = txn.Price.CurrencyCode,
                    Description = txn.Title ?? string.Empty,
                    FeeAmount = 0m,
                    GrossAmount = gross,
                    ListingId = txn.ListingId,
                    NetAmount = gross,
                    ReceiptId = receipt.ReceiptId,
                    ShopCurrency = txn.Price.CurrencyCode,
                    TransactionDate = txnDate,
                    TransactionId = txn.TransactionId
                });
            }
        }

        return inputs;
    }

    private LedgerClassification ClassifyLedger(IEnumerable<EtsyLedgerEntry> entries, long shopId)
    {
        var expenseInputs = new List<EtsyExpenseInput>();
        var payoutInputs = new List<EtsyPayoutInput>();

        foreach (var entry in entries)
        {
            if (!TryParseLedgerType(entry.LedgerEntryType, out var type))
            {
                _logger.LogWarning("Etsy ledger entry type not recognized for shop {ShopId}: {Type}", shopId, entry.LedgerEntryType ?? "(null)");
                continue;
            }

            switch (type)
            {
                case EtsyLedgerTypes.Sale:
                case EtsyLedgerTypes.Refund:
                    continue; // Handled by receipts

                case EtsyLedgerTypes.Payment:
                case EtsyLedgerTypes.Disbursement:
                case EtsyLedgerTypes.Withdrawal:
                    payoutInputs.Add(BuildPayoutInput(entry));
                    continue;

                case EtsyLedgerTypes.ListingFee:
                case EtsyLedgerTypes.TransactionFee:
                case EtsyLedgerTypes.ProcessingFee:
                case EtsyLedgerTypes.PromotedListingFee:
                case EtsyLedgerTypes.MarketingFee:
                case EtsyLedgerTypes.SubscriptionFee:
                case EtsyLedgerTypes.PostageLabel:
                case EtsyLedgerTypes.ShippingLabel:
                case EtsyLedgerTypes.Tax:
                case EtsyLedgerTypes.Vat:
                    expenseInputs.Add(BuildExpenseInput(entry, type));
                    continue;
            }
        }

        return new LedgerClassification(expenseInputs, payoutInputs);
    }

    private static bool TryParseLedgerType(string? entryType, out EtsyLedgerTypes type)
    {
        if (!string.IsNullOrEmpty(entryType) && KnownLedgerTypes.TryGetValue(entryType, out type))
            return true;

        type = default;
        return false;
    }

    private static EtsyExpenseInput BuildExpenseInput(EtsyLedgerEntry entry, EtsyLedgerTypes type)
    {
        return new EtsyExpenseInput
        {
            Amount = Math.Abs(entry.Amount),
            Category = MapExpenseCategory(type).ToStringValue(),
            Currency = entry.Currency,
            Description = entry.Description ?? entry.LedgerEntryType ?? string.Empty,
            ExpenseDate = DateTimeOffset.FromUnixTimeSeconds(entry.CreateDate).UtcDateTime,
            LedgerEntryId = entry.EntryId
        };
    }

    private static EtsyPayoutInput BuildPayoutInput(EtsyLedgerEntry entry)
    {
        return new EtsyPayoutInput
        {
            Amount = Math.Abs(entry.Amount),
            Currency = entry.Currency,
            LedgerEntryId = entry.EntryId,
            PayoutDate = DateTimeOffset.FromUnixTimeSeconds(entry.CreateDate).UtcDateTime,
            ShopCurrency = entry.Currency,
            Status = "Paid"
        };
    }

    private static ExpenseCategory MapExpenseCategory(EtsyLedgerTypes type) => type switch
    {
        EtsyLedgerTypes.ListingFee => ExpenseCategory.ListingFee,
        EtsyLedgerTypes.PromotedListingFee => ExpenseCategory.AdFee,
        EtsyLedgerTypes.MarketingFee => ExpenseCategory.AdFee,
        EtsyLedgerTypes.SubscriptionFee => ExpenseCategory.SubscriptionFee,
        EtsyLedgerTypes.TransactionFee => ExpenseCategory.ProcessingFee,
        EtsyLedgerTypes.ProcessingFee => ExpenseCategory.ProcessingFee,
        EtsyLedgerTypes.PostageLabel => ExpenseCategory.ShippingLabel,
        EtsyLedgerTypes.ShippingLabel => ExpenseCategory.ShippingLabel,
        _ => ExpenseCategory.Other
    };

    private record LedgerClassification(List<EtsyExpenseInput> ExpenseInputs, List<EtsyPayoutInput> PayoutInputs);

    #endregion
}
