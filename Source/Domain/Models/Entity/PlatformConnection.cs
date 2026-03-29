namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class PlatformConnection : DefaultModel
{
	#region <Properties>

	public string AccessToken { get; set; } = string.Empty;

	public string ConnectionStatus { get; set; } = string.Empty;

	public DateTime CreatedOn { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string PlatformAccountId { get; set; } = string.Empty;

	public string? RefreshToken { get; set; }

	public DateTime? TokenExpiryDateTime { get; set; }

	public long UserId { get; set; }

	#endregion
}
