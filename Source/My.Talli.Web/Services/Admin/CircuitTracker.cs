namespace My.Talli.Web.Services.Admin;

using System.Collections.Concurrent;

/// <summary>Service</summary>
public class CircuitTracker : ICircuitTracker
{
	#region <Variables>

	private readonly ConcurrentDictionary<string, bool> _sessions = new();
	private int _cachedNonAdminCount;


	#endregion

	#region <Properties>

	public int InAppNonAdminCount => Volatile.Read(ref _cachedNonAdminCount);


	#endregion

	#region <Events>

	public event Action? CountChanged;


	#endregion

	#region <Methods>

	public void RegisterInAppSession(string sessionId, bool isAdmin)
	{
		if (string.IsNullOrEmpty(sessionId))
			return;

		var added = _sessions.TryAdd(sessionId, isAdmin);

		if (added && !isAdmin)
			RecomputeAndRaise();
	}

	public void UnregisterInAppSession(string sessionId)
	{
		if (string.IsNullOrEmpty(sessionId))
			return;

		if (_sessions.TryRemove(sessionId, out var wasAdmin) && !wasAdmin)
			RecomputeAndRaise();
	}

	private void RecomputeAndRaise()
	{
		var newCount = _sessions.Count(kv => !kv.Value);
		var oldCount = Interlocked.Exchange(ref _cachedNonAdminCount, newCount);

		if (newCount != oldCount)
			CountChanged?.Invoke();
	}


	#endregion
}
