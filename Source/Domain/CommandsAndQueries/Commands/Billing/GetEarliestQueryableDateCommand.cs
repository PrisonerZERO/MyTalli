namespace My.Talli.Domain.Commands.Billing;

/// <summary>Command</summary>
public class GetEarliestQueryableDateCommand
{
	#region <Constants>

	public const int FreeTierHistoryDays = 30;

	#endregion

	#region <Variables>

	private readonly IsProSubscriberCommand _isProSubscriber;

	#endregion

	#region <Constructors>

	public GetEarliestQueryableDateCommand(IsProSubscriberCommand isProSubscriber)
	{
		_isProSubscriber = isProSubscriber;
	}

	#endregion

	#region <Methods>

	/// <summary>Returns the earliest date the user is allowed to query historical Revenue/Expense/Payout data. Returns null for Pro users (no floor — full history). Returns UtcNow - 30 days for free users (matches the "30-day history" promise in the pricing copy). Callers should clamp their StartDate filter to max(requestedStart, floor) before querying.</summary>
	public async Task<DateTime?> ExecuteAsync(long userId)
	{
		if (await _isProSubscriber.ExecuteAsync(userId)) return null;

		return DateTime.UtcNow.Date.AddDays(-FreeTierHistoryDays);
	}

	#endregion
}
