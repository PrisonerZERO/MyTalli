namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserPreferences
{
	#region <Properties>

	public EmailPreferences EmailPreferences { get; set; } = new();

	public bool FunGreetings { get; set; } = true;


	#endregion
}
