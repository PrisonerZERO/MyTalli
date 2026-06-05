namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class SwapShopAccountCommand
{
    #region <Constants>

    private const string PendingStatus = "Pending";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Expense, ENTITIES.Expense> _expenseAdapter;
    private readonly RepositoryAdapterAsync<Payout, ENTITIES.Payout> _payoutAdapter;
    private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
    private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> _shopConnectionEtsyAdapter;

    #endregion

    #region <Constructors>

    public SwapShopAccountCommand(
        RepositoryAdapterAsync<Expense, ENTITIES.Expense> expenseAdapter,
        RepositoryAdapterAsync<Payout, ENTITIES.Payout> payoutAdapter,
        RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter,
        RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter,
        RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter,
        RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> shopConnectionEtsyAdapter)
    {
        _expenseAdapter = expenseAdapter;
        _payoutAdapter = payoutAdapter;
        _platformConnectionAdapter = platformConnectionAdapter;
        _revenueAdapter = revenueAdapter;
        _shopConnectionAdapter = shopConnectionAdapter;
        _shopConnectionEtsyAdapter = shopConnectionEtsyAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<SwapShopAccountResult> ExecuteAsync(SwapShopAccountInput input)
    {
        // IDOR ownership check — the shop must exist and belong to this user.
        var shop = (await _shopConnectionAdapter.FindAsync(s =>
            s.Id == input.ShopConnectionId &&
            s.UserId == input.UserId)).FirstOrDefault();

        if (shop is null)
            return new SwapShopAccountResult { Found = false, Swapped = false };

        // Defense-in-depth alongside the reconnect endpoint — confirm the shop is on the platform the caller claims.
        var connection = await _platformConnectionAdapter.GetByIdAsync(shop.PlatformConnectionId);
        if (connection is null || !string.Equals(connection.Platform, input.Platform, StringComparison.OrdinalIgnoreCase))
            return new SwapShopAccountResult { Found = false, Swapped = false };

        var isSameAccount = string.Equals(shop.PlatformShopId, input.PlatformShopId, StringComparison.Ordinal);

        // Re-point identity + tokens and reset the sync lifecycle. On a same-account reconnect this is just a
        // token refresh; on a different-account reconnect it also points the slot at the new account.
        shop.AccessToken = input.AccessToken;
        shop.ConsecutiveFailures = 0;
        shop.IsActive = true;
        shop.LastErrorMessage = null;
        shop.NextSyncDateTime = DateTime.UtcNow;
        shop.PlatformAccountId = input.PlatformAccountId;
        shop.PlatformShopId = input.PlatformShopId;
        shop.RefreshToken = input.RefreshToken;
        shop.RefreshTokenExpiryDateTime = input.RefreshTokenExpiryDateTime;
        shop.ShopName = input.ShopName;
        shop.Status = PendingStatus;
        shop.TokenExpiryDateTime = input.TokenExpiryDateTime;

        if (isSameAccount)
        {
            // Same account re-authorized — keep the shop's existing synced data and last-sync timestamp.
            await _shopConnectionAdapter.UpdateAsync(shop);
            await UpsertEtsyDetailAsync(shop.Id, input.EtsyDetail);
            return new SwapShopAccountResult { Found = true, Swapped = false };
        }

        // Different account — the slot now tracks a new account, so the previously-synced revenue / expense /
        // payout data belongs to the wrong account. It's disposable wrong-account junk, not user-meaningful
        // data, so hard-delete it (the per-platform subtables cascade-delete via their FK) and re-sync fresh.
        shop.LastSyncDateTime = null;
        await _shopConnectionAdapter.UpdateAsync(shop);
        await UpsertEtsyDetailAsync(shop.Id, input.EtsyDetail);

        await DeleteShopDataAsync(input.ShopConnectionId);

        return new SwapShopAccountResult { Found = true, Swapped = true };
    }

    private async Task UpsertEtsyDetailAsync(long shopConnectionId, SwapShopEtsyDetail? detail)
    {
        if (detail is null)
            return;

        var existing = await _shopConnectionEtsyAdapter.GetByIdAsync(shopConnectionId);

        if (existing is null)
        {
            await _shopConnectionEtsyAdapter.InsertAsync(new ShopConnectionEtsy
            {
                CountryCode = detail.CountryCode,
                Id = shopConnectionId,
                IsVacationMode = detail.IsVacationMode,
                ShopCurrency = detail.ShopCurrency,
                ShopUrl = detail.ShopUrl
            });
            return;
        }

        existing.CountryCode = detail.CountryCode;
        existing.IsVacationMode = detail.IsVacationMode;
        existing.ShopCurrency = detail.ShopCurrency;
        existing.ShopUrl = detail.ShopUrl;
        await _shopConnectionEtsyAdapter.UpdateAsync(existing);
    }

    private async Task DeleteShopDataAsync(long shopConnectionId)
    {
        // Base-row deletes cascade to the per-platform subtables (FK_RevenueStripe_Revenue etc. are OnDelete Cascade).
        var revenues = (await _revenueAdapter.FindAsync(r => r.ShopConnectionId == shopConnectionId)).ToList();
        if (revenues.Count > 0)
            await _revenueAdapter.DeleteRangeAsync(revenues);

        var expenses = (await _expenseAdapter.FindAsync(e => e.ShopConnectionId == shopConnectionId)).ToList();
        if (expenses.Count > 0)
            await _expenseAdapter.DeleteRangeAsync(expenses);

        var payouts = (await _payoutAdapter.FindAsync(p => p.ShopConnectionId == shopConnectionId)).ToList();
        if (payouts.Count > 0)
            await _payoutAdapter.DeleteRangeAsync(payouts);
    }

    #endregion
}
