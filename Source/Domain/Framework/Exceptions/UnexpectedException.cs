namespace My.Talli.Domain.Framework.Exceptions;

/// <summary>Exception</summary>
public class UnexpectedException : TalliException
{
    #region <Properties>

    public override int HttpStatusCode => 500;

    #endregion

    #region <Constructors>

    public UnexpectedException() : base("An unexpected error occurred.") { }

    public UnexpectedException(string message) : base(message) { }

    public UnexpectedException(string message, Exception innerException) : base(message, innerException) { }

    #endregion
}
