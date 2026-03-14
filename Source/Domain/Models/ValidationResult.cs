namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public abstract class ValidationResult
{
	#region <Properties>

	public bool IsValid => ValidationSummary.Count == 0;

	public List<string> ValidationSummary { get; set; } = [];

	public List<string> WarningSummary { get; set; } = [];

	#endregion
}
