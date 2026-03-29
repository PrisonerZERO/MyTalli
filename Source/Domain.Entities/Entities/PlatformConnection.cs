namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class PlatformConnection : DefaultEntity
{
	#region <Properties>

	public string AccessToken { get; set; } = string.Empty;

	public string ConnectionStatus { get; set; } = string.Empty;

	public string Platform { get; set; } = string.Empty;

	public string PlatformAccountId { get; set; } = string.Empty;

	public string? RefreshToken { get; set; }

	public DateTime? TokenExpiryDateTime { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
