namespace My.Talli.Domain.Handlers.Authentication;

/// <summary>Payload</summary>
public class GoogleSignInPayload
{
	#region <Properties>

	public string AvatarUrl { get; set; } = string.Empty;

	public bool EmailVerified { get; set; }

	public string GoogleId { get; set; } = string.Empty;

	public string Locale { get; set; } = string.Empty;

	#endregion
}
