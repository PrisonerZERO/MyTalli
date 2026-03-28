namespace My.Talli.Domain.Data.EntityFramework.Repositories;

using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            var tracked = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
            _dbSet.Remove(tracked ?? entity);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            foreach (var entity in entities)
            {
                var tracked = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
                _dbSet.Remove(tracked ?? entity);
            }

            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            AuditResolver.Resolve(entity);
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task InsertRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            foreach (var entity in entities)
            {
                AuditResolver.Resolve(entity);
            }

            await _dbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            AuditResolver.Resolve(entity, updating: true);
            DetachTrackedInstance(entity);
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            foreach (var entity in entities)
            {
                AuditResolver.Resolve(entity, updating: true);
                DetachTrackedInstance(entity);
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    private void DetachTrackedInstance(TEntity entity)
    {
        var tracked = _dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);

        if (tracked is not null && !ReferenceEquals(tracked, entity))
            _dbContext.Entry(tracked).State = EntityState.Detached;
    }


    #endregion
}
