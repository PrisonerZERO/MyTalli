namespace My.Talli.Web.Queries;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

using MODELS = Domain.Models;

/// <summary>Command</summary>
public class ExpenseFindCommand
{
	#region <Variables>

	private readonly TalliDbContext _dbContext;

	#endregion

	#region <Constructors>

	public ExpenseFindCommand(TalliDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	#endregion

	#region <Methods>

	public IQueryable<MODELS.Expense> Query(ExpenseFilter filter)
	{
		var query = _dbContext.Expenses.AsNoTracking().Where(e => e.UserId == filter.UserId);

		if (filter.StartDate.HasValue)
			query = query.Where(e => e.ExpenseDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			query = query.Where(e => e.ExpenseDate < filter.EndDateExclusive.Value);

		if (!string.IsNullOrEmpty(filter.Platform))
			query = query.Where(e => e.Platform == filter.Platform);

		if (filter.ShopConnectionId.HasValue)
			query = query.Where(e => e.ShopConnectionId == filter.ShopConnectionId.Value);

		var projected = query.Select(e => new MODELS.Expense
		{
			Amount = e.Amount,
			Category = e.Category,
			CreatedOn = e.CreatedOnDateTime,
			Currency = e.Currency,
			Description = e.Description,
			ExpenseDate = e.ExpenseDate,
			Id = e.Id,
			IsDeleted = e.IsDeleted,
			IsVisible = e.IsVisible,
			Platform = e.Platform,
			PlatformTransactionId = e.PlatformTransactionId,
			ShopConnectionId = e.ShopConnectionId,
			UserId = e.UserId,
		});

		return ApplySort(projected, filter);
	}

	public async Task<T> RunAsync<T>(Func<Task<T>> materializer)
	{
		await _dbContext.ConcurrencyLock.WaitAsync();

		try
		{
			return await materializer();
		}
		finally
		{
			_dbContext.ConcurrencyLock.Release();
		}
	}

	private static IQueryable<MODELS.Expense> ApplySort(IQueryable<MODELS.Expense> source, ExpenseFilter filter)
	{
		return filter.SortColumn switch
		{
			"Amount" => filter.SortDescending ? source.OrderByDescending(e => e.Amount) : source.OrderBy(e => e.Amount),
			"Category" => filter.SortDescending ? source.OrderByDescending(e => e.Category) : source.OrderBy(e => e.Category),
			"Description" => filter.SortDescending ? source.OrderByDescending(e => e.Description) : source.OrderBy(e => e.Description),
			"Platform" => filter.SortDescending ? source.OrderByDescending(e => e.Platform) : source.OrderBy(e => e.Platform),
			_ => filter.SortDescending ? source.OrderByDescending(e => e.ExpenseDate) : source.OrderBy(e => e.ExpenseDate),
		};
	}

	#endregion
}
