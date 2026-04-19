namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ShopConnection : DefaultEntity
{
	#region <Properties>

	public string AccessToken { get; set; } = string.Empty;

	public int ConsecutiveFailures { get; set; }

	public bool IsActive { get; set; } = true;

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public DateTime? LastSyncDateTime { get; set; }

	public DateTime NextSyncDateTime { get; set; }

	public PlatformConnection PlatformConnection { get; set; } = null!;

	public string PlatformAccountId { get; set; } = string.Empty;

	public long PlatformConnectionId { get; set; }

	public string PlatformShopId { get; set; } = string.Empty;

	public string? RefreshToken { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public DateTime? TokenExpiryDateTime { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
