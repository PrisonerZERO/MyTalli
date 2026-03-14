namespace My.Talli.Domain.Data.Interfaces;

/// <summary>Unit of work abstraction for coordinating transactional persistence.</summary>
public interface IUnitOfWork : IDisposable
{
    #region <Methods>

    Task<int> SaveChangesAsync();

    #endregion
}
