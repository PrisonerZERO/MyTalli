namespace My.Talli.Web.Models;

/// <summary>Cache</summary>
internal sealed class DailyCache<T> where T : class
{
	#region <Variables>

	private readonly Func<DateTime, T> _generate;
	private readonly object _gate = new();
	private DateTime _cachedDate;
	private T? _cache;

	#endregion

	#region <Constructors>

	public DailyCache(Func<DateTime, T> generate)
	{
		_generate = generate;
	}

	#endregion

	#region <Methods>

	public T Get()
	{
		var today = DateTime.Today;
		var local = _cache;

		if (local is not null && _cachedDate == today)
			return local;

		lock (_gate)
		{
			if (_cache is null || _cachedDate != today)
			{
				_cachedDate = today;
				_cache = _generate(today);
			}

			return _cache;
		}
	}

	#endregion
}
