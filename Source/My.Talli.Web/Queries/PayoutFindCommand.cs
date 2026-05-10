namespace My.Talli.Web.Queries;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

using MODELS = Domain.Models;

/// <summary>Command</summary>
public class PayoutFindCommand
{
	#region <Variables>

	private readonly TalliDbContext _dbContext;

	#endregion

	#region <Constructors>

	public PayoutFindCommand(TalliDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	#endregion

	#region <Methods>

	public IQueryable<MODELS.Payout> Query(PayoutFilter filter)
	{
		var query = _dbContext.Payouts.AsNoTracking().Where(p => p.UserId == filter.UserId);

		if (filter.StartDate.HasValue)
			query = query.Where(p => p.PayoutDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			query = query.Where(p => p.PayoutDate < filter.EndDateExclusive.Value);

		if (!string.IsNullOrEmpty(filter.Platform))
			query = query.Where(p => p.Platform == filter.Platform);

		if (filter.ShopConnectionId.HasValue)
			query = query.Where(p => p.ShopConnectionId == filter.ShopConnectionId.Value);

		var projected = query.Select(p => new MODELS.Payout
		{
			Amount = p.Amount,
			CreatedOn = p.CreatedOnDateTime,
			Currency = p.Currency,
			ExpectedArrivalDate = p.ExpectedArrivalDate,
			Id = p.Id,
			IsDeleted = p.IsDeleted,
			IsVisible = p.IsVisible,
			PayoutDate = p.PayoutDate,
			Platform = p.Platform,
			PlatformPayoutId = p.PlatformPayoutId,
			ShopConnectionId = p.ShopConnectionId,
			Status = p.Status,
			UserId = p.UserId,
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

	private static IQueryable<MODELS.Payout> ApplySort(IQueryable<MODELS.Payout> source, PayoutFilter filter)
	{
		return filter.SortColumn switch
		{
			"Amount" => filter.SortDescending ? source.OrderByDescending(p => p.Amount) : source.OrderBy(p => p.Amount),
			"ExpectedArrival" => filter.SortDescending ? source.OrderByDescending(p => p.ExpectedArrivalDate) : source.OrderBy(p => p.ExpectedArrivalDate),
			"Platform" => filter.SortDescending ? source.OrderByDescending(p => p.Platform) : source.OrderBy(p => p.Platform),
			"Status" => filter.SortDescending ? source.OrderByDescending(p => p.Status) : source.OrderBy(p => p.Status),
			_ => filter.SortDescending ? source.OrderByDescending(p => p.PayoutDate) : source.OrderBy(p => p.PayoutDate),
		};
	}

	#endregion
}
