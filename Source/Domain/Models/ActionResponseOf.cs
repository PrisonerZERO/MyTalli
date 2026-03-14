namespace My.Talli.Domain.Models;

/// <summary>Response</summary>
public class ActionResponseOf<T> : ValidationResult
{
	#region <Properties>

	public T Payload { get; set; } = default!;

	#endregion
}
