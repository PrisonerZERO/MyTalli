namespace My.Talli.Web.Helpers;

using System.Security.Claims;

/// <summary>Helper</summary>
public static class UserClaimsHelper
{
    #region <Variables>

    private static readonly Random Random = new();

    private static readonly string[] FunGreetings =
    [
        "cash flow captain",
        "chart topper",
        "dream chaser",
        "empire builder",
        "future millionaire",
        "goal crusher",
        "hustler extraordinaire",
        "legend",
        "money maker",
        "mystery hustler",
        "profit whisperer",
        "revenue rockstar",
        "side-hustle hero",
        "side-hustle wizard",
        "stack builder",
        "tally master"
    ];

    #endregion

    #region <Methods>

    public static UserDisplayInfo Resolve(ClaimsPrincipal user)
    {
        var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
        var displayName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        var hasRealName = !string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(displayName);

        var fullName = !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : email.Split('@')[0];

        var greeting = !string.IsNullOrWhiteSpace(firstName)
            ? firstName
            : !string.IsNullOrWhiteSpace(displayName)
                ? displayName.Split(' ')[0]
                : FunGreetings[Random.Next(FunGreetings.Length)];

        var initials = ResolveInitials(firstName, lastName, displayName, email);

        return new UserDisplayInfo
        {
            Email = email,
            FirstName = greeting,
            FullName = fullName,
            HasRealName = hasRealName,
            Initials = initials
        };
    }

    private static string ResolveInitials(string firstName, string lastName, string displayName, string email)
    {
        if (firstName.Length > 0 && lastName.Length > 0)
            return $"{char.ToUpper(firstName[0])}{char.ToUpper(lastName[0])}";

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length >= 2
                ? $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}"
                : $"{char.ToUpper(parts[0][0])}";
        }

        if (!string.IsNullOrWhiteSpace(email))
            return $"{char.ToUpper(email[0])}";

        return "?";
    }


    #endregion
}

/// <summary>Resolved user display information from claims</summary>
public class UserDisplayInfo
{
    #region <Properties>

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public bool HasRealName { get; init; }

    public string Initials { get; init; } = string.Empty;

    #endregion
}
