namespace My.Talli.Web.Endpoints;

using System.Security.Claims;
using Web.Commands.Endpoints;
using Web.Commands.Notifications;
using Web.Services.Admin;

/// <summary>Endpoint</summary>
public static class AdminEndpoints
{
    #region <Endpoints>

    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        // Admin email management
        app.MapPost("/api/admin/email/resend", ResendEmail).RequireAuthorization(policy => policy.RequireRole("Admin"));
        app.MapPost("/api/admin/email/bulk-welcome", BulkSendWelcome).RequireAuthorization(policy => policy.RequireRole("Admin"));
        app.MapPost("/api/admin/email/bulk-welcome-all", BulkSendWelcomeAll).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Maintenance Mode toggle + status
        app.MapPost("/api/admin/maintenance/on", TurnMaintenanceOn).RequireAuthorization(policy => policy.RequireRole("Admin"));
        app.MapPost("/api/admin/maintenance/off", TurnMaintenanceOff).RequireAuthorization(policy => policy.RequireRole("Admin"));
        app.MapGet("/api/admin/maintenance/status", GetMaintenanceStatus).RequireAuthorization(policy => policy.RequireRole("Admin"));
    }

    #endregion

    #region <Methods>

    private static async Task<IResult> ResendEmail(
        AdminEmailResendRequest request,
        GetAdminUserListCommand userListCommand,
        SendWelcomeEmailCommand welcomeCommand,
        SendSubscriptionConfirmationEmailCommand subscriptionCommand,
        SendWeeklySummaryEmailCommand summaryCommand)
    {
        var users = await userListCommand.ExecuteAsync();
        var user = users.FirstOrDefault(u => u.UserId == request.UserId);

        if (user is null)
            return Results.NotFound(new { message = "User not found." });

        switch (request.EmailType)
        {
            case "welcome":
                await welcomeCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
                return Results.Ok(new { message = $"Welcome email sent to {user.Email}." });

            case "subscription-confirmation":
                if (!user.HasActiveSubscription)
                    return Results.BadRequest(new { message = "User has no active subscription." });

                var sent = await subscriptionCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);

                return sent
                    ? Results.Ok(new { message = $"Subscription confirmation sent to {user.Email}." })
                    : Results.BadRequest(new { message = "No subscription data found for this user." });

            case "weekly-summary":
                await summaryCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
                return Results.Ok(new { message = $"Weekly summary sent to {user.Email}." });

            default:
                return Results.BadRequest(new { message = $"Unknown email type: {request.EmailType}" });
        }
    }

    private static async Task<IResult> BulkSendWelcome(
        AdminBulkWelcomeRequest request,
        GetAdminUserListCommand userListCommand,
        SendWelcomeEmailCommand welcomeCommand)
    {
        var users = await userListCommand.ExecuteAsync();
        var targetUsers = users.Where(u => request.UserIds.Contains(u.UserId)).ToList();

        var sent = 0;

        foreach (var user in targetUsers)
        {
            await welcomeCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
            sent++;
        }

        return Results.Ok(new { message = $"Welcome email sent to {sent} user(s)." });
    }

    private static async Task<IResult> BulkSendWelcomeAll(
        GetAdminUserListCommand userListCommand,
        SendWelcomeEmailCommand welcomeCommand)
    {
        var users = await userListCommand.ExecuteAsync();
        var sent = 0;

        foreach (var user in users)
        {
            await welcomeCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
            sent++;
        }

        return Results.Ok(new { message = $"Welcome email sent to {sent} user(s)." });
    }

    private static async Task<IResult> TurnMaintenanceOn(HttpContext context, IMaintenanceModeService maintenanceModeService)
    {
        var actingUserId = ResolveAdminUserId(context);
        await maintenanceModeService.SetEnabledAsync(true, actingUserId);

        return Results.Ok(new { isEnabled = true });
    }

    private static async Task<IResult> TurnMaintenanceOff(HttpContext context, IMaintenanceModeService maintenanceModeService)
    {
        var actingUserId = ResolveAdminUserId(context);
        await maintenanceModeService.SetEnabledAsync(false, actingUserId);

        return Results.Ok(new { isEnabled = false });
    }

    private static IResult GetMaintenanceStatus(IMaintenanceModeService maintenanceModeService)
    {
        return Results.Ok(new { isEnabled = maintenanceModeService.IsEnabled });
    }

    private static long ResolveAdminUserId(HttpContext context)
    {
        var raw = context.User.FindFirst("UserId")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(raw, out var userId) ? userId : 0L;
    }

    #endregion
}

/// <summary>Request body for single email resend</summary>
public class AdminEmailResendRequest
{
    public string EmailType { get; set; } = string.Empty;

    public long UserId { get; set; }
}

/// <summary>Request body for bulk welcome email</summary>
public class AdminBulkWelcomeRequest
{
    public long[] UserIds { get; set; } = [];
}
