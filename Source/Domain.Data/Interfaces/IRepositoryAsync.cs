namespace My.Talli.Domain.Data.Interfaces;

using Domain.Entities.Interfaces;
using System.Linq.Expressions;

/// <summary>Repository</summary>
public interface IRepositoryAsync<T> where T : class, IIdentifiable
{
    #region <Methods>

    Task<T?> GetByIdAsync(long id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Server-side COUNT(*) for the predicate. Use instead of FindAsync(predicate).Count() when you only need the count — avoids materializing every matching row into memory.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);


    #endregion
}
