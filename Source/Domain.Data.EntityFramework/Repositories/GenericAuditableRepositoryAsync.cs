namespace My.Talli.Domain.Data.EntityFramework.Repositories;

using Microsoft.EntityFrameworkCore;
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;

/// <summary>Repository</summary>
public class GenericAuditableRepositoryAsync<TEntity> : GenericRepositoryAsync<TEntity>, IAuditableRepositoryAsync<TEntity> where TEntity : class, IAuditableIdentifiable
{
    #region <Properties>

    public IAuditResolver<TEntity> AuditResolver { get; }

    #endregion

    #region <Constructors>

    public GenericAuditableRepositoryAsync(IAuditResolver<TEntity> auditResolver, TalliDbContext dbContext) : base(dbContext)
    {
        AuditResolver = auditResolver;
    }

    #endregion

    #region <Methods>

    public virtual async Task DeleteAsync(TEntity entity)
    {
        Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Remove(entity);
        }

        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        AuditResolver.Resolve(entity);
        await _dbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task InsertRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            AuditResolver.Resolve(entity);
        }

        await _dbSet.AddRangeAsync(entities);
        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        AuditResolver.Resolve(entity, updating: true);
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            AuditResolver.Resolve(entity, updating: true);
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
