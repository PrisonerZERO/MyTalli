namespace My.Talli.Web.Endpoints;

using Domain.Components.Tokens;
using Domain.Notifications.Emails;
using Web.Services.Email;

/// <summary>Endpoint</summary>
public static class TestEndpoints
{
    #region <Methods>

    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test/unsubscribe-token/{userId:long}", (long userId, UnsubscribeTokenService tokenService) =>
        {
            var token = tokenService.GenerateToken(userId);
            return Results.Text(token);
        });

        app.MapGet("/api/test/emails", async (IEmailService emailService, UnsubscribeTokenService tokenService) =>
        {
            var testRecipient = "hello@mytalli.com";
            var testToken = tokenService.GenerateToken(1);

            // 1. Welcome Email
            var welcomeNotification = new WelcomeEmailNotification();
            var welcomeEmail = welcomeNotification.Build(new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
            {
                Payload = new WelcomeEmailNotificationPayload { FirstName = "Robert", UnsubscribeToken = testToken }
            });
            welcomeEmail.To = [testRecipient];
            await emailService.SendAsync(welcomeEmail);

            // 2. Subscription Confirmation Email
            var subNotification = new SubscriptionConfirmationEmailNotification();
            var subEmail = subNotification.Build(new EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload>
            {
                Payload = new SubscriptionConfirmationEmailNotificationPayload
                {
                    Amount = "$12.00/mo",
                    CardLastFour = "4242",
                    FirstName = "Robert",
                    Plan = "Pro",
                    RenewalDate = "April 14, 2026",
                    UnsubscribeToken = testToken
                }
            });
            subEmail.To = [testRecipient];
            await emailService.SendAsync(subEmail);

            // 3. Weekly Summary Email
            var summaryNotification = new WeeklySummaryEmailNotification();
            var summaryEmail = summaryNotification.Build(new EmailNotificationArgumentOf<WeeklySummaryEmailNotificationPayload>
            {
                Payload = new WeeklySummaryEmailNotificationPayload
                {
                    FirstName = "Robert",
                    UnsubscribeToken = testToken,
                    GoalCurrent = "$2,847.00",
                    GoalPercent = "57%",
                    GoalRemaining = "$2,153.00",
                    GoalTarget = "$5,000.00",
                    TotalRevenue = "$1,247.50",
                    TrendDirection = "\u25b2",
                    TrendPercent = "12.4%",
                    WeekRange = "Mar 7 \u2013 Mar 13, 2026",
                    PlatformRows = @"
                    <tr>
                        <td style=""padding: 12px 0; border-bottom: 1px solid #f0edff;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #635bff; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Stripe</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">18 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$842.00</p>
                                        <p style=""color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9650; 8.2%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px 0; border-bottom: 1px solid #f0edff;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #f56400; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Etsy</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">7 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$283.50</p>
                                        <p style=""color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9650; 22.1%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px 0;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #ff90e8; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Gumroad</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">3 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$122.00</p>
                                        <p style=""color: #e74c3c; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9660; 5.3%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>"
                }
            });
            summaryEmail.To = [testRecipient];
            await emailService.SendAsync(summaryEmail);

            return Results.Text("3 test emails sent to hello@mytalli.com");
        });

        app.MapGet("/api/test/error", () =>
        {
            throw new InvalidOperationException("Test exception — verifying error email pipeline is working.");
        });
    }

    #endregion
}
