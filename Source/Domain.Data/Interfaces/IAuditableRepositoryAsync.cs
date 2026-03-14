namespace My.Talli.Domain.Data.Interfaces;

using Domain.Entities.Interfaces;

/// <summary>Repository</summary>
public interface IAuditableRepositoryAsync<TEntity> : IRepositoryAsync<TEntity> where TEntity : class, IAuditableIdentifiable
{
    #region <Properties>

    IAuditResolver<TEntity> AuditResolver { get; }

    #endregion

    #region <Methods>

    Task InsertAsync(TEntity entity);

    Task InsertRangeAsync(IEnumerable<TEntity> entities);

    Task UpdateAsync(TEntity entity);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities);

    Task DeleteAsync(TEntity entity);

    Task DeleteRangeAsync(IEnumerable<TEntity> entities);

    Task<int> SaveChangesAsync();

    #endregion
}
