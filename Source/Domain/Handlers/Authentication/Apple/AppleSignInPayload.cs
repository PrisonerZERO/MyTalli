namespace My.Talli.Domain.Handlers.Authentication;

/// <summary>Payload</summary>
public class AppleSignInPayload
{
	#region <Properties>

	public string AppleId { get; set; } = string.Empty;

	public bool IsPrivateRelay { get; set; }


	#endregion
}
