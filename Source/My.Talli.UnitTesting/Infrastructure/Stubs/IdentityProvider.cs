namespace My.Talli.UnitTesting.Infrastructure.Stubs;

/// <summary>Stub</summary>
public class IdentityProvider
{
	#region <Variables>

	private readonly Dictionary<Type, long> _counters = [];

	#endregion

	#region <Methods>

	public long Next<TEntity>()
	{
		var type = typeof(TEntity);

		if (!_counters.ContainsKey(type))
			_counters[type] = 0;

		return ++_counters[type];
	}

	#endregion
}
