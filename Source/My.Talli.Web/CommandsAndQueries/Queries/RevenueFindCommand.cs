namespace My.Talli.Web.Queries;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

using MODELS = Domain.Models;

/// <summary>Command</summary>
public class RevenueFindCommand
{
	#region <Variables>

	private readonly TalliDbContext _dbContext;

	#endregion

	#region <Constructors>

	public RevenueFindCommand(TalliDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	#endregion

	#region <Methods>

	public IQueryable<MODELS.Revenue> Query(RevenueFilter filter)
	{
		var query = _dbContext.Revenues.AsNoTracking().Where(r => r.UserId == filter.UserId);

		if (filter.StartDate.HasValue)
			query = query.Where(r => r.TransactionDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			query = query.Where(r => r.TransactionDate < filter.EndDateExclusive.Value);

		if (!string.IsNullOrEmpty(filter.Platform))
			query = query.Where(r => r.Platform == filter.Platform);

		if (filter.ShopConnectionId.HasValue)
			query = query.Where(r => r.ShopConnectionId == filter.ShopConnectionId.Value);

		var projected = query.Select(r => new MODELS.Revenue
		{
			Currency = r.Currency,
			Description = r.Description,
			FeeAmount = r.FeeAmount,
			GrossAmount = r.GrossAmount,
			Id = r.Id,
			IsDeleted = r.IsDeleted,
			IsDisputed = r.IsDisputed,
			IsRefunded = r.IsRefunded,
			IsVisible = r.IsVisible,
			NetAmount = r.NetAmount,
			Platform = r.Platform,
			PlatformTransactionId = r.PlatformTransactionId,
			ShopConnectionId = r.ShopConnectionId,
			TransactionDate = r.TransactionDate,
			UserId = r.UserId,
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

	private static IQueryable<MODELS.Revenue> ApplySort(IQueryable<MODELS.Revenue> source, RevenueFilter filter)
	{
		return filter.SortColumn switch
		{
			"Amount" => filter.SortDescending ? source.OrderByDescending(r => r.NetAmount) : source.OrderBy(r => r.NetAmount),
			"Description" => filter.SortDescending ? source.OrderByDescending(r => r.Description) : source.OrderBy(r => r.Description),
			"Platform" => filter.SortDescending ? source.OrderByDescending(r => r.Platform) : source.OrderBy(r => r.Platform),
			_ => filter.SortDescending ? source.OrderByDescending(r => r.TransactionDate) : source.OrderBy(r => r.TransactionDate),
		};
	}

	#endregion
}
