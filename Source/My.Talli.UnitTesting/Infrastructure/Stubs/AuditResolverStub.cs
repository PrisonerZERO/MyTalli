namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;

/// <summary>Stub</summary>
public class AuditResolverStub<TEntity> : IAuditResolver<TEntity> where TEntity : class, IAuditableIdentifiable
{
	#region <Properties>

	public ICurrentUserService CurrentUserService { get; }

	#endregion

	#region <Constructors>

	public AuditResolverStub(ICurrentUserService currentUserService)
	{
		CurrentUserService = currentUserService;
	}

	#endregion

	#region <Methods>

	public void Resolve(TEntity entity, bool updating = false)
	{
		if (updating)
		{
			entity.UpdatedByUserId = CurrentUserService.UserId;
			entity.UpdatedOnDate = DateTime.UtcNow;
		}
		else
		{
			entity.CreateByUserId = CurrentUserService.UserId ?? 0;
			entity.CreatedOnDateTime = DateTime.UtcNow;
			entity.UpdatedByUserId = null;
			entity.UpdatedOnDate = null;
		}
	}

	#endregion
}
