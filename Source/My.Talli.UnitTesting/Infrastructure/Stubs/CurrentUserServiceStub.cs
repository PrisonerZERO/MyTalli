namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Data.Interfaces;

/// <summary>Stub</summary>
public class CurrentUserServiceStub : ICurrentUserService
{
	#region <Properties>

	public string? DisplayName { get; private set; }

	public bool IsAuthenticated => UserId.HasValue;

	public long? UserId { get; private set; }

	#endregion

	#region <Methods>

	public void Clear()
	{
		UserId = null;
		DisplayName = null;
	}

	public void Set(long userId, string displayName)
	{
		UserId = userId;
		DisplayName = displayName;
	}

	#endregion
}
