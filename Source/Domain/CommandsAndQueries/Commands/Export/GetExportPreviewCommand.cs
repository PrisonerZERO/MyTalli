namespace My.Talli.Domain.Commands.Export;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetExportPreviewCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Expense, ENTITIES.Expense> _expenseAdapter;
	private readonly RepositoryAdapterAsync<Payout, ENTITIES.Payout> _payoutAdapter;
	private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;


	#endregion

	#region <Constructors>

	public GetExportPreviewCommand(
		RepositoryAdapterAsync<Expense, ENTITIES.Expense> expenseAdapter,
		RepositoryAdapterAsync<Payout, ENTITIES.Payout> payoutAdapter,
		RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter)
	{
		_expenseAdapter = expenseAdapter;
		_payoutAdapter = payoutAdapter;
		_revenueAdapter = revenueAdapter;
	}


	#endregion

	#region <Methods>

	public async Task<ExportPreview> ExecuteAsync(long userId, DateTime fromUtc, DateTime toUtc)
	{
		var revenues = await _revenueAdapter.FindAsync(r => r.UserId == userId && r.TransactionDate >= fromUtc && r.TransactionDate <= toUtc);
		var expenses = await _expenseAdapter.FindAsync(e => e.UserId == userId && e.ExpenseDate >= fromUtc && e.ExpenseDate <= toUtc);
		var payouts = await _payoutAdapter.FindAsync(p => p.UserId == userId && p.PayoutDate >= fromUtc && p.PayoutDate <= toUtc);

		return new ExportPreview
		{
			ExpenseRowCount = expenses.Count(),
			FromDateUtc = fromUtc,
			PayoutRowCount = payouts.Count(),
			RevenueRowCount = revenues.Count(),
			ToDateUtc = toUtc
		};
	}


	#endregion
}
