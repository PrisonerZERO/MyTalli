namespace My.Talli.Web.Queries;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Models;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class ManualEntryFindCommand
{
	#region <Variables>

	private readonly TalliDbContext _dbContext;

	#endregion

	#region <Constructors>

	public ManualEntryFindCommand(TalliDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	#endregion

	#region <Methods>

	public IQueryable<ManualEntryItem> Query(ManualEntryFilter filter)
	{
		var joined = from r in _dbContext.Revenues.AsNoTracking()
					 join m in _dbContext.RevenueManuals.AsNoTracking() on r.Id equals m.Id into manualGroup
					 from m in manualGroup.DefaultIfEmpty()
					 where r.UserId == filter.UserId
						   && r.Platform == "Manual"
						   && r.ShopConnectionId == filter.ShopConnectionId
					 select new JoinedRow { Revenue = r, Manual = m };

		if (filter.StartDate.HasValue)
			joined = joined.Where(x => x.Revenue.TransactionDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			joined = joined.Where(x => x.Revenue.TransactionDate < filter.EndDateExclusive.Value);

		var sorted = ApplySort(joined, filter);

		return sorted.Select(x => new ManualEntryItem
		{
			Category = x.Manual != null ? x.Manual.Category : "Other",
			Currency = x.Revenue.Currency,
			Description = x.Revenue.Description,
			FeeAmount = x.Revenue.FeeAmount,
			GrossAmount = x.Revenue.GrossAmount,
			Id = x.Revenue.Id,
			NetAmount = x.Revenue.NetAmount,
			Notes = x.Manual != null ? x.Manual.Notes : null,
			Quantity = x.Manual != null ? x.Manual.Quantity : 1,
			TransactionDate = x.Revenue.TransactionDate,
		});
	}

	public IQueryable<decimal> QueryGrossAmount(ManualEntryFilter filter)
	{
		var query = _dbContext.Revenues.AsNoTracking()
			.Where(r => r.UserId == filter.UserId && r.Platform == "Manual" && r.ShopConnectionId == filter.ShopConnectionId);

		if (filter.StartDate.HasValue)
			query = query.Where(r => r.TransactionDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			query = query.Where(r => r.TransactionDate < filter.EndDateExclusive.Value);

		return query.Select(r => r.GrossAmount);
	}

	public IQueryable<decimal> QueryNetAmount(ManualEntryFilter filter)
	{
		var query = _dbContext.Revenues.AsNoTracking()
			.Where(r => r.UserId == filter.UserId && r.Platform == "Manual" && r.ShopConnectionId == filter.ShopConnectionId);

		if (filter.StartDate.HasValue)
			query = query.Where(r => r.TransactionDate >= filter.StartDate.Value);

		if (filter.EndDateExclusive.HasValue)
			query = query.Where(r => r.TransactionDate < filter.EndDateExclusive.Value);

		return query.Select(r => r.NetAmount);
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

	private static IQueryable<JoinedRow> ApplySort(IQueryable<JoinedRow> source, ManualEntryFilter filter)
	{
		return filter.SortColumn switch
		{
			"Description" => filter.SortDescending ? source.OrderByDescending(x => x.Revenue.Description) : source.OrderBy(x => x.Revenue.Description),
			"Category" => filter.SortDescending ? source.OrderByDescending(x => x.Manual != null ? x.Manual.Category : "") : source.OrderBy(x => x.Manual != null ? x.Manual.Category : ""),
			"Quantity" => filter.SortDescending ? source.OrderByDescending(x => x.Manual != null ? x.Manual.Quantity : 1) : source.OrderBy(x => x.Manual != null ? x.Manual.Quantity : 1),
			"UnitPrice" => filter.SortDescending
				? source.OrderByDescending(x => x.Manual != null && x.Manual.Quantity > 0 ? x.Revenue.GrossAmount / x.Manual.Quantity : x.Revenue.GrossAmount)
				: source.OrderBy(x => x.Manual != null && x.Manual.Quantity > 0 ? x.Revenue.GrossAmount / x.Manual.Quantity : x.Revenue.GrossAmount),
			"FeeAmount" => filter.SortDescending ? source.OrderByDescending(x => x.Revenue.FeeAmount) : source.OrderBy(x => x.Revenue.FeeAmount),
			"NetAmount" => filter.SortDescending ? source.OrderByDescending(x => x.Revenue.NetAmount) : source.OrderBy(x => x.Revenue.NetAmount),
			_ => filter.SortDescending ? source.OrderByDescending(x => x.Revenue.TransactionDate) : source.OrderBy(x => x.Revenue.TransactionDate),
		};
	}

	#endregion

	#region <Nested>

	private class JoinedRow
	{
		public ENTITIES.Revenue Revenue { get; set; } = null!;
		public ENTITIES.RevenueManual? Manual { get; set; }
	}

	#endregion
}
