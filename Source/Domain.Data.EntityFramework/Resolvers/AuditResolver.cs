namespace My.Talli.Domain.Data.EntityFramework.Resolvers;

using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;

/// <summary>Resolver</summary>
public class AuditResolver<TEntity> : IAuditResolver<TEntity> where TEntity : class, IAuditableIdentifiable
{
    #region <Properties>

    public ICurrentUserService CurrentUserService { get; }

    #endregion

    #region <Constructors>

    public AuditResolver(ICurrentUserService currentUserService)
    {
        CurrentUserService = currentUserService;
    }

    #endregion

    #region <Methods>

    public virtual void Resolve(TEntity entity, bool updating = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (updating && (!CurrentUserService.IsAuthenticated || !CurrentUserService.UserId.HasValue))
            throw new InvalidOperationException("Cannot resolve audit fields — no authenticated user.");

        var userId = CurrentUserService.UserId ?? 0;
        var timestamp = DateTime.UtcNow;

        if (!updating)
        {
            entity.CreateByUserId = userId;
            entity.CreatedOnDateTime = timestamp;
            entity.UpdatedByUserId = null;
            entity.UpdatedOnDate = null;
            return;
        }

        entity.UpdatedByUserId = userId;
        entity.UpdatedOnDate = timestamp;
    }

    #endregion
}
