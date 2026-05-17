namespace My.Talli.Web.Commands.Notifications;

using Domain.Components.Tokens;
using Domain.Models;
using Domain.Notifications.Emails;
using Domain.Repositories;
using Web.Services.Email;

using ENTITIES = My.Talli.Domain.Entities;

/// <summary>Command</summary>
public class SendSubscriptionExpiredEmailCommand
{
	#region <Variables>

	private readonly IEmailService _emailService;
	private readonly ILogger<SendSubscriptionExpiredEmailCommand> _logger;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
	private readonly UnsubscribeTokenService _tokenService;

	#endregion

	#region <Constructors>

	public SendSubscriptionExpiredEmailCommand(
		IEmailService emailService,
		ILogger<SendSubscriptionExpiredEmailCommand> logger,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter,
		UnsubscribeTokenService tokenService)
	{
		_emailService = emailService;
		_logger = logger;
		_subscriptionAdapter = subscriptionAdapter;
		_tokenService = tokenService;
	}

	#endregion

	#region <Methods>

	/// <summary>Worker entry point — caller already has the Subscription loaded and passes planName + endDate directly.</summary>
	public async Task ExecuteAsync(string email, string firstName, string planName, DateTime endDate, long userId)
	{
		try
		{
			var notification = new SubscriptionExpiredEmailNotification();
			var unsubscribeToken = _tokenService.GenerateToken(userId);
			var notificationPayload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = endDate.ToString("MMMM d, yyyy"),
				FirstName = firstName,
				PlanName = planName,
				UnsubscribeToken = unsubscribeToken
			};
			var notificationArgument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload> { Payload = notificationPayload };
			var smtp = notification.Build(notificationArgument);

			smtp.To = [email];
			await _emailService.SendAsync(smtp);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send subscription expired email for user {UserId}", userId);
		}
	}

	/// <summary>Admin-resend entry point — looks up the user's most recent Pro Subscription (ProductId 1 or 2) and uses its EndDate. Returns false if the user has no Pro sub on record.</summary>
	public async Task<bool> ExecuteAsync(string email, string firstName, long userId)
	{
		var proSubscriptions = await _subscriptionAdapter.FindAsync(s =>
			s.UserId == userId && (s.ProductId == 1 || s.ProductId == 2));
		var subscription = proSubscriptions.OrderByDescending(s => s.EndDate).FirstOrDefault();

		if (subscription is null)
		{
			_logger.LogWarning("No Pro subscription found for user {UserId} — skipping subscription expired email", userId);
			return false;
		}

		var planName = subscription.ProductId == 2 ? "Pro Yearly" : "Pro Monthly";
		await ExecuteAsync(email, firstName, planName, subscription.EndDate, userId);
		return true;
	}

	#endregion
}
