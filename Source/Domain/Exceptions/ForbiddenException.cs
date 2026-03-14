namespace My.Talli.Domain.Exceptions;

/// <summary>Exception</summary>
public class ForbiddenException : TalliException
{
    #region <Properties>

    public override int HttpStatusCode => 403;

    #endregion

    #region <Constructors>

    public ForbiddenException() : base("Access to this resource is forbidden.") { }

    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }

    #endregion
}
