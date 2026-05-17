namespace My.Talli.Web.Commands.Billing;

using Domain.Commands.Billing;
using Domain.Models;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Web.Commands.Notifications;

using ENTITIES = My.Talli.Domain.Entities;

/// <summary>Command</summary>
public class NotifyExpiredSubscribersCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> _appleAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> _googleAdapter;
	private readonly ILogger<NotifyExpiredSubscribersCommand> _logger;
	private readonly MarkExpirationEmailSentCommand _markEmailSent;
	private readonly RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> _microsoftAdapter;
	private readonly SendSubscriptionExpiredEmailCommand _sendEmail;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
	private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;

	#endregion

	#region <Constructors>

	public NotifyExpiredSubscribersCommand(
		RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> appleAdapter,
		RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> googleAdapter,
		ILogger<NotifyExpiredSubscribersCommand> logger,
		MarkExpirationEmailSentCommand markEmailSent,
		RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> microsoftAdapter,
		SendSubscriptionExpiredEmailCommand sendEmail,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter,
		RepositoryAdapterAsync<User, ENTITIES.User> userAdapter)
	{
		_appleAdapter = appleAdapter;
		_googleAdapter = googleAdapter;
		_logger = logger;
		_markEmailSent = markEmailSent;
		_microsoftAdapter = microsoftAdapter;
		_sendEmail = sendEmail;
		_subscriptionAdapter = subscriptionAdapter;
		_userAdapter = userAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;

		var due = (await _subscriptionAdapter.FindAsync(s =>
			(s.ProductId == 1 || s.ProductId == 2)
			&& s.EndDate < now
			&& s.ExpirationEmailSentAt == null))
			.ToList();

		if (due.Count == 0)
			return 0;

		var sentCount = 0;
		foreach (var subscription in due)
		{
			if (cancellationToken.IsCancellationRequested)
				return sentCount;

			var user = await _userAdapter.GetByIdAsync(subscription.UserId);
			if (user is null)
			{
				_logger.LogWarning("Skipping expiration email for Subscription {SubscriptionId}: no User row found for UserId {UserId}.", subscription.Id, subscription.UserId);
				continue;
			}

			var email = await ResolveEmailAsync(user);
			if (string.IsNullOrWhiteSpace(email))
			{
				_logger.LogWarning("Skipping expiration email for Subscription {SubscriptionId} (user {UserId}): could not resolve email from PreferredProvider {PreferredProvider}.", subscription.Id, user.Id, user.PreferredProvider);
				continue;
			}

			var planName = subscription.ProductId == 2 ? "Pro Yearly" : "Pro Monthly";

			await _sendEmail.ExecuteAsync(email, user.FirstName, planName, subscription.EndDate, user.Id);
			await _markEmailSent.ExecuteAsync(subscription.Id);
			sentCount++;
		}

		return sentCount;
	}

	private async Task<string?> ResolveEmailAsync(User user)
	{
		switch (user.PreferredProvider)
		{
			case "Google":
				return (await _googleAdapter.GetByIdAsync(user.Id))?.Email;
			case "Apple":
				return (await _appleAdapter.GetByIdAsync(user.Id))?.Email;
			case "Microsoft":
				return (await _microsoftAdapter.GetByIdAsync(user.Id))?.Email;
			default:
				return null;
		}
	}

	#endregion
}
