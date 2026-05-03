namespace My.Talli.Web.Models;

using MODELS = Domain.Models;

/// <summary>Helper</summary>
public static class ShopHealthAnalyzer
{
	#region <Constants>

	private static readonly TimeSpan StaleGracePeriod = TimeSpan.FromHours(36);

	#endregion

	#region <Methods>

	public static ShopHealth ClassifyHealth(MODELS.ShopConnection shop, DateTime now)
	{
		if (!shop.IsEnabled)
			return ShopHealth.Paused;

		if (shop.ConsecutiveFailures > 0)
			return ShopHealth.Failing;

		// "Pending" status is set by ConnectXxxCommand on both first-time connect and reconnect, and
		// stays Pending until ShopSyncWorker fires (within ~5 min). Show this as a forward-looking
		// "sync upcoming" state — NOT as Stale — even if LastSyncDateTime is old.
		if (string.Equals(shop.Status, "Pending", StringComparison.OrdinalIgnoreCase))
			return ShopHealth.SyncPending;

		if (shop.LastSyncDateTime.HasValue && now - shop.LastSyncDateTime.Value > StaleGracePeriod)
			return ShopHealth.Stale;

		return ShopHealth.Healthy;
	}

	/// <summary>
	/// Translates a raw worker error message into a one-sentence, user-friendly string suitable for
	/// display on the Platforms page. Falls back to a generic "try reconnecting" message when no
	/// pattern matches. Never surfaces the raw exception or stack trace.
	/// </summary>
	public static string ToFriendlyMessage(ShopHealth health, string platform, string? rawError, DateTime? lastSyncDateTime, DateTime now, string status = "")
	{
		switch (health)
		{
			case ShopHealth.Healthy:
				return string.Empty;

			case ShopHealth.SyncPending:
				// Differentiate first-ever connect from reconnect by whether a prior sync exists
				return lastSyncDateTime.HasValue
					? "Reconnected — first sync will start within 5 minutes."
					: "First sync will start within 5 minutes.";

			case ShopHealth.Paused:
				return "Sync is paused. Click Resume to start syncing again.";

			case ShopHealth.Stale:
			{
				if (!lastSyncDateTime.HasValue)
					return "No sync has run yet. The first one will start shortly.";

				var hours = (int)(now - lastSyncDateTime.Value).TotalHours;
				return hours < 48
					? $"Sync is delayed. Will retry automatically."
					: $"No sync in {hours / 24} days. Try reconnecting if this continues.";
			}

			case ShopHealth.Failing:
				return TranslateError(rawError, platform);

			default:
				return string.Empty;
		}
	}

	private static string TranslateError(string? rawError, string platform)
	{
		if (string.IsNullOrWhiteSpace(rawError))
			return "Sync failed. Try reconnecting to refresh the connection.";

		var lower = rawError.ToLowerInvariant();

		if (lower.Contains("cryptographic") || lower.Contains("unprotect"))
			return "Stored authorization can't be read. Reconnect to refresh it.";

		if (lower.Contains("expired") || lower.Contains("invalid_grant") || lower.Contains("invalid_token"))
			return "Your authorization expired. Click Reconnect to re-authorize.";

		if (lower.Contains("401") || lower.Contains("unauthorized"))
			return "Access denied. Click Reconnect to re-authorize.";

		if (lower.Contains("403") || lower.Contains("forbidden") || lower.Contains("revoked"))
			return $"{platform} revoked access. Click Reconnect to re-authorize.";

		if (lower.Contains("429") || lower.Contains("rate"))
			return "Rate limited — sync will retry automatically.";

		if (lower.Contains("503") || lower.Contains("502") || lower.Contains("504") || lower.Contains("timeout"))
			return $"{platform} is temporarily unavailable. Will retry automatically.";

		if (lower.Contains("500"))
			return $"{platform} returned a server error. Will retry automatically.";

		return "Sync failed. Try reconnecting to refresh the connection.";
	}

	public static string ToHealthLabel(ShopHealth health) => health switch
	{
		ShopHealth.Healthy => "Healthy",
		ShopHealth.SyncPending => "Sync Upcoming",
		ShopHealth.Stale => "Sync Delayed",
		ShopHealth.Failing => "Reconnect Needed",
		ShopHealth.Paused => "Paused",
		_ => "Unknown"
	};


	#endregion
}
