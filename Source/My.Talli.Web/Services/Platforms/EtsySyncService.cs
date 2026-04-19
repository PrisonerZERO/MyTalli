namespace My.Talli.Web.Services.Platforms;

using Domain.Commands.Platforms;
using Domain.Components;
using Domain.Models;

/// <summary>Service</summary>
public class EtsySyncService : IPlatformSyncService
{
    #region <Constants>

    private const int PageLimit = 100;
    private const int InitialBackfillDays = 90;
    private static readonly TimeSpan AccessTokenSafetyMargin = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PerPageDelay = TimeSpan.FromMilliseconds(200);

    #endregion

    #region <Variables>

    private readonly EtsyService _etsyService;
    private readonly EtsyTokenRefresher _etsyTokenRefresher;
    private readonly ILogger<EtsySyncService> _logger;
    private readonly RefreshShopTokensCommand _refreshShopTokensCommand;
    private readonly UpsertEtsyRevenueCommand _upsertEtsyRevenueCommand;

    #endregion

    #region <Constructors>

    public EtsySyncService(EtsyService etsyService, EtsyTokenRefresher etsyTokenRefresher, ILogger<EtsySyncService> logger, RefreshShopTokensCommand refreshShopTokensCommand, UpsertEtsyRevenueCommand upsertEtsyRevenueCommand)
    {
        _etsyService = etsyService;
        _etsyTokenRefresher = etsyTokenRefresher;
        _logger = logger;
        _refreshShopTokensCommand = refreshShopTokensCommand;
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

        var offset = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var page = await _etsyService.GetReceiptsAsync(shopId, accessToken, minCreated, PageLimit, offset, cancellationToken);
            result.PagesFetched++;
            result.ReceiptsProcessed += page.Results.Count;

            if (page.Results.Count == 0)
                break;

            var inputs = BuildInputs(page.Results);
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

        return result;
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

    private static List<EtsyRevenueInput> BuildInputs(IEnumerable<EtsyReceipt> receipts)
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

    #endregion
}
