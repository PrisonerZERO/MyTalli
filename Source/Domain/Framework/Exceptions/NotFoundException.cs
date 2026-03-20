namespace My.Talli.Domain.Framework.Exceptions;

/// <summary>Exception</summary>
public class NotFoundException : TalliException
{
    #region <Properties>

    public override int HttpStatusCode => 404;


    #endregion

    #region <Constructors>

    public NotFoundException() : base("The requested resource was not found.") { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }


    #endregion
}
