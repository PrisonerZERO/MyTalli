namespace My.Talli.Web.Endpoints;

using Domain.Components.JsonSerializers;
using Domain.Components.Tokens;
using Domain.Data.Interfaces;
using Domain.Repositories;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Endpoint</summary>
public static class EmailEndpoints
{
    #region <Endpoints>

    public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/email/preferences", UpdateEmailPreferences).DisableAntiforgery();
    }

    #endregion

    #region <Methods>

    private static async Task<IResult> UpdateEmailPreferences(
        HttpContext context,
        ICurrentUserService currentUserService,
        UnsubscribeTokenService tokenService,
        RepositoryAdapterAsync<MODELS.User, ENTITIES.User> userAdapter,
        UserPreferencesJsonSerializer preferencesSerializer)
    {
        using var reader = new StreamReader(context.Request.Body);
        var json = await reader.ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<EmailPreferencesRequest>(json);

        if (request is null || string.IsNullOrWhiteSpace(request.Token))
            return Results.BadRequest("Invalid request.");

        var userId = tokenService.ValidateToken(request.Token);
        if (userId is null)
            return Results.BadRequest("Invalid or expired token.");

        currentUserService.Set(userId.Value, "unsubscribe");

        var user = await userAdapter.GetByIdAsync(userId.Value);
        if (user is null)
            return Results.BadRequest("User not found.");

        var preferences = preferencesSerializer.Deserialize(user.UserPreferences);
        preferences.EmailPreferences.SubscriptionConfirmationEmail = request.SubscriptionConfirmationEmail;
        preferences.EmailPreferences.UnsubscribeAll = request.UnsubscribeAll;
        preferences.EmailPreferences.WeeklySummaryEmail = request.WeeklySummaryEmail;
        user.UserPreferences = preferencesSerializer.Serialize(preferences);

        await userAdapter.UpdateAsync(user);

        return Results.Ok();
    }

    #endregion
}

record EmailPreferencesRequest(string Token, bool SubscriptionConfirmationEmail, bool UnsubscribeAll, bool WeeklySummaryEmail);
