namespace My.Talli.Domain.Data.Interfaces;

/// <summary>Unit of Work</summary>
public interface IUnitOfWorkAsync : IDisposable
{
    #region <Methods>

    Task<int> SaveChangesAsync();

    #endregion
}
