namespace My.Talli.Web.Services.Email;

using Domain.Notifications.Emails;
using ElmahCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

/// <summary>Handler</summary>
public class ExceptionEmailHandler : IExceptionHandler
{
    #region <Variables>

    private readonly EmailSettings _settings;
    private readonly IEmailService _emailService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExceptionEmailHandler> _logger;


    #endregion

    #region <Constructors>

    public ExceptionEmailHandler(
        IEmailService emailService,
        IHostEnvironment environment,
        ILogger<ExceptionEmailHandler> logger,
        IOptions<EmailSettings> settings)
    {
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
        _settings = settings.Value;
    }


    #endregion

    #region <Methods>

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ElmahExtensions.RaiseError(httpContext, exception);

        if (_settings.ExceptionRecipients.Count == 0)
            return false;

        try
        {
            var payload = new ExceptionOccurredEmailNotificationPayload
            {
                EnvironmentName = _environment.EnvironmentName,
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace ?? string.Empty,
                ExceptionTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
                RequestMethod = httpContext.Request.Method,
                RequestPath = httpContext.Request.Path.Value ?? string.Empty
            };

            var arguments = new EmailNotificationArgumentOf<ExceptionOccurredEmailNotificationPayload>
            {
                Payload = payload
            };

            var notification = new ExceptionOccurredEmailNotification();
            var email = notification.Build(arguments);

            email.To = _settings.ExceptionRecipients;

            await _emailService.SendAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send exception notification email");
        }

        return false;
    }


    #endregion
}
