namespace My.Talli.Domain.Data.EntityFramework.Resolvers;

using My.Talli.Domain.Data.Interfaces;
using My.Talli.Domain.Entities.Interfaces;

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

        if (!CurrentUserService.IsAuthenticated || !CurrentUserService.UserId.HasValue)
            throw new InvalidOperationException("Cannot resolve audit fields — no authenticated user.");

        var userId = CurrentUserService.UserId.Value;
        var timestamp = DateTime.UtcNow;

        if (!updating)
        {
            entity.CreateByUserId = userId;
            entity.CreatedOnDateTime = timestamp;
            entity.UpdatedByUserId = userId;
            entity.UpdatedOnDate = timestamp;
            return;
        }

        entity.UpdatedByUserId = userId;
        entity.UpdatedOnDate = timestamp;
    }

    #endregion
}
