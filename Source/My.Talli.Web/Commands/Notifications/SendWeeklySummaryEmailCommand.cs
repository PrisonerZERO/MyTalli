namespace My.Talli.Web.Commands.Notifications;

using Domain.Components.Tokens;
using Domain.Notifications.Emails;
using Web.Services.Email;

/// <summary>Command</summary>
public class SendWeeklySummaryEmailCommand
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly ILogger<SendWeeklySummaryEmailCommand> _logger;
    private readonly UnsubscribeTokenService _tokenService;

    #endregion

    #region <Constructors>

    public SendWeeklySummaryEmailCommand(IEmailService emailService, ILogger<SendWeeklySummaryEmailCommand> logger, UnsubscribeTokenService tokenService)
    {
        _emailService = emailService;
        _logger = logger;
        _tokenService = tokenService;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(string email, string firstName, long userId)
    {
        try
        {
            var notification = new WeeklySummaryEmailNotification();
            var unsubscribeToken = _tokenService.GenerateToken(userId);
            var payload = new WeeklySummaryEmailNotificationPayload
            {
                FirstName = firstName,
                GoalCurrent = "$2,847.00",
                GoalPercent = "57%",
                GoalRemaining = "$2,153.00",
                GoalTarget = "$5,000.00",
                PlatformRows = BuildSamplePlatformRows(),
                TotalRevenue = "$1,247.50",
                TrendDirection = "\u25b2",
                TrendPercent = "12.4%",
                UnsubscribeToken = unsubscribeToken,
                WeekRange = "Mar 7 \u2013 Mar 13, 2026"
            };
            var argument = new EmailNotificationArgumentOf<WeeklySummaryEmailNotificationPayload> { Payload = payload };
            var smtp = notification.Build(argument);

            smtp.To = [email];
            await _emailService.SendAsync(smtp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send weekly summary email for user {UserId}", userId);
        }
    }

    private static string BuildSamplePlatformRows()
    {
        return """
            <tr>
                <td style="padding: 12px 0; border-bottom: 1px solid #f0edff;">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                        <tr>
                            <td width="4" style="background: #635bff; border-radius: 2px;"></td>
                            <td style="padding-left: 12px;">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">Stripe</p>
                                <p style="color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">18 transactions</p>
                            </td>
                            <td align="right" valign="top">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">$842.00</p>
                                <p style="color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">&#9650; 8.2%</p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="padding: 12px 0; border-bottom: 1px solid #f0edff;">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                        <tr>
                            <td width="4" style="background: #f56400; border-radius: 2px;"></td>
                            <td style="padding-left: 12px;">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">Etsy</p>
                                <p style="color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">7 transactions</p>
                            </td>
                            <td align="right" valign="top">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">$283.50</p>
                                <p style="color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">&#9650; 22.1%</p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="padding: 12px 0;">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                        <tr>
                            <td width="4" style="background: #ff90e8; border-radius: 2px;"></td>
                            <td style="padding-left: 12px;">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">Gumroad</p>
                                <p style="color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">3 transactions</p>
                            </td>
                            <td align="right" valign="top">
                                <p style="color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;">$122.00</p>
                                <p style="color: #e74c3c; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;">&#9660; 5.3%</p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            """;
    }

    #endregion
}
