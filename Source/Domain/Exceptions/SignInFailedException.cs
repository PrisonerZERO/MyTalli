namespace My.Talli.Domain.Exceptions;

public class SignInFailedException : UnauthorizedException
{
    #region <Constructors>

    public SignInFailedException() : base("Sign-in failed. Please try again.") { }

    public SignInFailedException(string message) : base(message) { }

    public SignInFailedException(string message, Exception innerException) : base(message, innerException) { }

    #endregion
}
