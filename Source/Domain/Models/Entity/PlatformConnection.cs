namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class PlatformConnection : DefaultModel
{
	#region <Properties>

	public string ConnectionStatus { get; set; } = string.Empty;

	public DateTime CreatedOn { get; set; }

	public string Platform { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
