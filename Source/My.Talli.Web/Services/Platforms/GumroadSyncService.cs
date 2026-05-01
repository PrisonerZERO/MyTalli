namespace My.Talli.Web.Services.Platforms;

using Domain.Commands.Platforms;
using Domain.Components;
using Domain.Components.Tokens;
using Domain.Models;

/// <summary>Service</summary>
public class GumroadSyncService : IPlatformSyncService
{
    #region <Constants>

    private const int InitialBackfillDays = 90;
    private const int MaxPagesPerSync = 50;
    private static readonly TimeSpan PerPageDelay = TimeSpan.FromMilliseconds(200);

    #endregion

    #region <Variables>

    private readonly IGumroadApiClient _gumroadService;
    private readonly ILogger<GumroadSyncService> _logger;
    private readonly IShopTokenProtector _tokenProtector;
    private readonly UpsertGumroadRevenueCommand _upsertGumroadRevenueCommand;

    #endregion

    #region <Constructors>

    public GumroadSyncService(IGumroadApiClient gumroadService, ILogger<GumroadSyncService> logger, IShopTokenProtector tokenProtector, UpsertGumroadRevenueCommand upsertGumroadRevenueCommand)
    {
        _gumroadService = gumroadService;
        _logger = logger;
        _tokenProtector = tokenProtector;
        _upsertGumroadRevenueCommand = upsertGumroadRevenueCommand;
    }

    #endregion

    #region <Properties>

    public string Platform => "Gumroad";

    #endregion

    #region <Methods>

    public async Task<PlatformSyncResult> SyncShopAsync(ShopConnection shop, CancellationToken cancellationToken)
    {
        var result = new PlatformSyncResult();
        var accessToken = _tokenProtector.Unprotect(shop.AccessToken);
        var after = shop.LastSyncDateTime ?? DateTime.UtcNow.AddDays(-InitialBackfillDays);

        string? pageKey = null;
        var pagesFetched = 0;

        while (!cancellationToken.IsCancellationRequested && pagesFetched < MaxPagesPerSync)
        {
            var page = await _gumroadService.GetSalesAsync(accessToken, after, pageKey, cancellationToken);
            result.PagesFetched++;
            result.ReceiptsProcessed += page.Sales.Count;
            pagesFetched++;

            if (page.Sales.Count == 0)
                break;

            var inputs = BuildRevenueInputs(page.Sales);
            if (inputs.Count > 0)
            {
                var inserted = await _upsertGumroadRevenueCommand.ExecuteAsync(shop.UserId, shop.Id, inputs);
                result.NewRevenueRowCount += inserted;

                var mostRecent = inputs.Max(i => i.TransactionDate);
                if (result.MostRecentTransactionDate is null || mostRecent > result.MostRecentTransactionDate)
                    result.MostRecentTransactionDate = mostRecent;
            }

            if (string.IsNullOrEmpty(page.NextPageKey))
                break;

            pageKey = page.NextPageKey;

            try { await Task.Delay(PerPageDelay, cancellationToken); }
            catch (TaskCanceledException) { break; }
        }

        if (pagesFetched >= MaxPagesPerSync)
            _logger.LogWarning("Gumroad sync hit page cap ({Cap}) for shop {ShopId}; remaining sales will be picked up on next sync.", MaxPagesPerSync, shop.Id);

        return result;
    }

    private static List<GumroadRevenueInput> BuildRevenueInputs(IEnumerable<GumroadSale> sales)
    {
        var inputs = new List<GumroadRevenueInput>();

        foreach (var sale in sales)
        {
            if (string.IsNullOrEmpty(sale.Id))
                continue;

            var gross = sale.PriceCents / 100m;

            inputs.Add(new GumroadRevenueInput
            {
                Currency = string.IsNullOrEmpty(sale.Currency) ? "usd" : sale.Currency,
                Description = sale.ProductName ?? string.Empty,
                FeeAmount = 0m,
                GrossAmount = gross,
                IsDisputed = sale.Disputed,
                IsRefunded = sale.Refunded,
                LicenseKey = sale.LicenseKey,
                NetAmount = gross,
                SaleId = sale.Id,
                TransactionDate = DateTime.SpecifyKind(sale.CreatedAt, DateTimeKind.Utc)
            });
        }

        return inputs;
    }

    #endregion
}
