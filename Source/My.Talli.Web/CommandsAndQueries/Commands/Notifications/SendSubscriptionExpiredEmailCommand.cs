namespace My.Talli.Web.Commands.Notifications;

using Domain.Components.Tokens;
using Domain.Notifications.Emails;
using Web.Services.Email;

/// <summary>Command</summary>
public class SendSubscriptionExpiredEmailCommand
{
	#region <Variables>

	private readonly IEmailService _emailService;
	private readonly ILogger<SendSubscriptionExpiredEmailCommand> _logger;
	private readonly UnsubscribeTokenService _tokenService;

	#endregion

	#region <Constructors>

	public SendSubscriptionExpiredEmailCommand(IEmailService emailService, ILogger<SendSubscriptionExpiredEmailCommand> logger, UnsubscribeTokenService tokenService)
	{
		_emailService = emailService;
		_logger = logger;
		_tokenService = tokenService;
	}

	#endregion

	#region <Methods>

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

	#endregion
}
