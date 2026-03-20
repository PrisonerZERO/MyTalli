namespace My.Talli.Domain.Handlers.Authentication;

/// <summary>Handler Argument</summary>
public class SignInArgument
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string LastName { get; set; } = string.Empty;


	#endregion
}
