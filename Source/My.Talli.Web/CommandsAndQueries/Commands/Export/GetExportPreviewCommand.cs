namespace My.Talli.Web.Commands.Export;

using Domain.Data.EntityFramework;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>Command</summary>
public class GetExportPreviewCommand
{
	#region <Variables>

	private readonly IDbContextFactory<TalliDbContext> _dbContextFactory;

	#endregion

	#region <Constructors>

	public GetExportPreviewCommand(IDbContextFactory<TalliDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	#endregion

	#region <Methods>

	/// <summary>Returns row counts for Revenue / Expense / Payout in the given date range. Each count runs on its OWN DbContext (via IDbContextFactory) so all three queries execute in parallel via Task.WhenAll — they don't contend on the scoped TalliDbContext's ConcurrencyLock. Cuts wall-clock time from 3× round-trips to 1× round-trip.</summary>
	public async Task<ExportPreview> ExecuteAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		var revenueTask = CountRevenueAsync(userId, fromUtc, toUtc);
		var expenseTask = CountExpenseAsync(userId, fromUtc, toUtc);
		var payoutTask = CountPayoutAsync(userId, fromUtc, toUtc);

		await Task.WhenAll(revenueTask, expenseTask, payoutTask);

		return new ExportPreview
		{
			ExpenseRowCount = expenseTask.Result,
			FromDateUtc = fromUtc,
			PayoutRowCount = payoutTask.Result,
			RevenueRowCount = revenueTask.Result,
			ToDateUtc = toUtc
		};
	}

	private async Task<int> CountRevenueAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		await using var ctx = await _dbContextFactory.CreateDbContextAsync();
		return await ctx.Revenues.AsNoTracking().CountAsync(r => r.UserId == userId && r.TransactionDate >= fromUtc && r.TransactionDate <= toUtc);
	}

	private async Task<int> CountExpenseAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		await using var ctx = await _dbContextFactory.CreateDbContextAsync();
		return await ctx.Expenses.AsNoTracking().CountAsync(e => e.UserId == userId && e.ExpenseDate >= fromUtc && e.ExpenseDate <= toUtc);
	}

	private async Task<int> CountPayoutAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		await using var ctx = await _dbContextFactory.CreateDbContextAsync();
		return await ctx.Payouts.AsNoTracking().CountAsync(p => p.UserId == userId && p.PayoutDate >= fromUtc && p.PayoutDate <= toUtc);
	}

	#endregion
}
