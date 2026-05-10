namespace My.Talli.Web.Queries;

/// <summary>Filter</summary>
public class RevenueFilter
{
	#region <Properties>

	public DateTime? EndDateExclusive { get; init; }

	public string? Platform { get; init; }

	public long? ShopConnectionId { get; init; }

	public string SortColumn { get; init; } = "TransactionDate";

	public bool SortDescending { get; init; } = true;

	public DateTime? StartDate { get; init; }

	public long UserId { get; init; }

	#endregion
}
