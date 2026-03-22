namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Subscription : DefaultModel
{
	#region <Properties>

	public DateTime? CancelledDate { get; set; }

	public DateTime EndDate { get; set; }

	public long OrderItemId { get; set; }

	public long ProductId { get; set; }

	public DateTime RenewalDate { get; set; }

	public DateTime StartDate { get; set; }

	public string Status { get; set; } = string.Empty;

	public long UserId { get; set; }


	#endregion
}
