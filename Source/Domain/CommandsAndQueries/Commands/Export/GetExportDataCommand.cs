namespace My.Talli.Domain.Commands.Export;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetExportDataCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Expense, ENTITIES.Expense> _expenseAdapter;
	private readonly RepositoryAdapterAsync<Payout, ENTITIES.Payout> _payoutAdapter;
	private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;


	#endregion

	#region <Constructors>

	public GetExportDataCommand(
		RepositoryAdapterAsync<Expense, ENTITIES.Expense> expenseAdapter,
		RepositoryAdapterAsync<Payout, ENTITIES.Payout> payoutAdapter,
		RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter,
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
	{
		_expenseAdapter = expenseAdapter;
		_payoutAdapter = payoutAdapter;
		_revenueAdapter = revenueAdapter;
		_shopConnectionAdapter = shopConnectionAdapter;
	}


	#endregion

	#region <Methods>

	public async Task<List<ExportRevenueRow>> GetRevenueAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		var rows = (await _revenueAdapter.FindAsync(r => r.UserId == userId && r.TransactionDate >= fromUtc && r.TransactionDate <= toUtc)).ToList();
		var shopNames = await BuildShopNameMapAsync(userId);

		return rows
			.OrderBy(r => r.TransactionDate)
			.Select(r => new ExportRevenueRow
			{
				Currency = r.Currency,
				Description = r.Description,
				FeeAmount = r.FeeAmount,
				GrossAmount = r.GrossAmount,
				IsDisputed = r.IsDisputed,
				IsRefunded = r.IsRefunded,
				NetAmount = r.NetAmount,
				Platform = r.Platform,
				PlatformTransactionId = r.PlatformTransactionId,
				ShopName = ResolveShopName(shopNames, r.ShopConnectionId),
				TransactionDate = r.TransactionDate
			})
			.ToList();
	}

	public async Task<List<ExportExpenseRow>> GetExpensesAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		var rows = (await _expenseAdapter.FindAsync(e => e.UserId == userId && e.ExpenseDate >= fromUtc && e.ExpenseDate <= toUtc)).ToList();
		var shopNames = await BuildShopNameMapAsync(userId);

		return rows
			.OrderBy(e => e.ExpenseDate)
			.Select(e => new ExportExpenseRow
			{
				Amount = e.Amount,
				Category = e.Category,
				Currency = e.Currency,
				Description = e.Description,
				ExpenseDate = e.ExpenseDate,
				Platform = e.Platform,
				PlatformTransactionId = e.PlatformTransactionId,
				ShopName = ResolveShopName(shopNames, e.ShopConnectionId)
			})
			.ToList();
	}

	public async Task<List<ExportPayoutRow>> GetPayoutsAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		var rows = (await _payoutAdapter.FindAsync(p => p.UserId == userId && p.PayoutDate >= fromUtc && p.PayoutDate <= toUtc)).ToList();
		var shopNames = await BuildShopNameMapAsync(userId);

		return rows
			.OrderBy(p => p.PayoutDate)
			.Select(p => new ExportPayoutRow
			{
				Amount = p.Amount,
				Currency = p.Currency,
				ExpectedArrivalDate = p.ExpectedArrivalDate,
				PayoutDate = p.PayoutDate,
				Platform = p.Platform,
				PlatformPayoutId = p.PlatformPayoutId,
				ShopName = ResolveShopName(shopNames, p.ShopConnectionId),
				Status = p.Status
			})
			.ToList();
	}

	private async Task<Dictionary<long, string>> BuildShopNameMapAsync(long userId)
	{
		var shops = await _shopConnectionAdapter.FindAsync(s => s.UserId == userId);
		return shops.ToDictionary(s => s.Id, s => s.ShopName);
	}

	private static string ResolveShopName(Dictionary<long, string> shopNames, long? shopConnectionId)
	{
		if (!shopConnectionId.HasValue)
			return string.Empty;

		return shopNames.TryGetValue(shopConnectionId.Value, out var name) ? name : string.Empty;
	}


	#endregion
}
