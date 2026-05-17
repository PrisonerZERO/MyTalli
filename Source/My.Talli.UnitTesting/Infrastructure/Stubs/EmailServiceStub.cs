namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Notifications.Emails;
using My.Talli.Web.Services.Email;

/// <summary>Stub</summary>
public class EmailServiceStub : IEmailService
{
	#region <Properties>

	public List<SmtpNotification> Sent { get; } = new();

	#endregion

	#region <Methods>

	public Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default)
	{
		Sent.Add(notification);
		return Task.CompletedTask;
	}

	#endregion
}
