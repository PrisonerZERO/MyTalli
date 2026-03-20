namespace My.Talli.Domain.Data.Interfaces;

using Domain.Entities.Interfaces;

/// <summary>Resolver</summary>
public interface IAuditResolver<TEntity> where TEntity : class, IAuditableIdentifiable
{
    #region <Properties>

    ICurrentUserService CurrentUserService { get; }


    #endregion

    #region <Methods>

    void Resolve(TEntity entity, bool updating = false);


    #endregion
}
