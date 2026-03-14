namespace My.Talli.Domain.Handlers.Authentication;

/// <summary>Handler Argument</summary>
public class SignInArgumentOf<T> : SignInArgument
{
	#region <Properties>

	public required T Payload { get; init; }

	#endregion
}
