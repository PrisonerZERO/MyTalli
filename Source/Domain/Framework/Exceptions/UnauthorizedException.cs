namespace My.Talli.Domain.Framework.Exceptions;

/// <summary>Exception</summary>
public class UnauthorizedException : TalliException
{
    #region <Properties>

    public override int HttpStatusCode => 401;


    #endregion

    #region <Constructors>

    public UnauthorizedException() : base("Authentication is required to access this resource.") { }

    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }


    #endregion
}
