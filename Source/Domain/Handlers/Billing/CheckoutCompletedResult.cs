namespace My.Talli.Domain.Handlers.Billing;

/// <summary>Result</summary>
public class CheckoutCompletedResult
{
	#region <Properties>

	public string Amount { get; set; } = string.Empty;

	public string CardLastFour { get; set; } = string.Empty;

	public string Plan { get; set; } = string.Empty;

	public string RenewalDate { get; set; } = string.Empty;

	public string UserEmail { get; set; } = string.Empty;

	public string UserFirstName { get; set; } = string.Empty;

	public long UserId { get; set; }


	#endregion
}
