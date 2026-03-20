namespace My.Talli.Domain.Framework;

/// <summary>Utility</summary>
public static class Assert
{
	#region <Methods>

	public static void AreSameType(Type expected, Type actual, string? message = null)
	{
		if (expected != actual)
			throw new InvalidOperationException(message ?? $"Expected type: {expected.FullName}, but was: {actual.FullName}.");
	}

	public static void Equals<T>(T expected, T actual, string? message = null)
	{
		if (!object.Equals(expected, actual))
			throw new InvalidOperationException(message ?? $"Expected: {expected}, but was: {actual}.");
	}

	public static void Exists(Func<bool> condition, string? message = null)
	{
		if (!condition())
			throw new InvalidOperationException(message ?? "Item does not exist.");
	}

	public static void IsInsertableIdentity(long id, string? message = null)
	{
		if (id != 0)
			throw new ArgumentException(message ?? "New identity must be 0 for insertion.");
	}

	public static void IsNotNull(object? value, string? message = null)
	{
		if (value is null)
			throw new ArgumentNullException(message ?? nameof(value));
	}

	public static void IsNullOrEmpty(string? value, string? message = null)
	{
		if (string.IsNullOrEmpty(value))
			throw new ArgumentNullException(message ?? nameof(value));
	}

	public static void IsNullOrWhitespace(string? value, string? message = null)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(message ?? nameof(value));
	}

	public static void IsTrue(Func<bool> condition, string? message = null)
	{
		if (!condition())
			throw new InvalidOperationException(message ?? "Item is not valid.");
	}

	public static void IsValidIdentity(long id, string? message = null)
	{
		if (id <= 0)
			throw new ArgumentException(message ?? "Identity must be greater than 0.");
	}

	public static void ShouldContain(string actual, string substring, string? message = null)
	{
		if (!actual.Contains(substring))
			throw new InvalidOperationException(message ?? $"The string must contain the substring '{substring}'. Actual: {actual}.");
	}


	#endregion
}
