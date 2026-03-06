namespace My.Talli.Domain.Exceptions;

public class DatabaseConnectionFailedException : ForbiddenException
{
    #region <Constructors>

    public DatabaseConnectionFailedException() : base("Unable to connect to the database.") { }

    public DatabaseConnectionFailedException(string message) : base(message) { }

    public DatabaseConnectionFailedException(string message, Exception innerException) : base(message, innerException) { }

    #endregion
}
