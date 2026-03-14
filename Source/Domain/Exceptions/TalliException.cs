namespace My.Talli.Domain.Exceptions;

/// <summary>Exception</summary>
public abstract class TalliException : Exception
{
    #region <Properties>

    public abstract int HttpStatusCode { get; }

    #endregion

    #region <Constructors>

    protected TalliException() { }

    protected TalliException(string message) : base(message) { }

    protected TalliException(string message, Exception innerException) : base(message, innerException) { }

    #endregion
}
