namespace My.Talli.UnitTesting.Framework;

using DOMAINASSERT = Domain.Framework.Assert;

/// <summary>Tests</summary>
public class AssertTests
{
	#region <Methods>

	[Fact]
	public void AreSameType_DifferentTypes_ThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => DOMAINASSERT.AreSameType(typeof(string), typeof(int)));
	}

	[Fact]
	public void AreSameType_MatchingTypes_DoesNotThrow()
	{
		DOMAINASSERT.AreSameType(typeof(string), typeof(string));
	}

	[Fact]
	public void Equals_MatchingValues_DoesNotThrow()
	{
		DOMAINASSERT.Equals(5, 5);
	}

	[Fact]
	public void Equals_MismatchedValues_ThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => DOMAINASSERT.Equals(5, 10));
	}

	[Fact]
	public void Exists_FalseCondition_ThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => DOMAINASSERT.Exists(() => false));
	}

	[Fact]
	public void Exists_TrueCondition_DoesNotThrow()
	{
		DOMAINASSERT.Exists(() => true);
	}

	[Fact]
	public void IsInsertableIdentity_NonZeroId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => DOMAINASSERT.IsInsertableIdentity(1));
	}

	[Fact]
	public void IsInsertableIdentity_ZeroId_DoesNotThrow()
	{
		DOMAINASSERT.IsInsertableIdentity(0);
	}

	[Fact]
	public void IsNotNull_NullObject_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DOMAINASSERT.IsNotNull(null));
	}

	[Fact]
	public void IsNotNull_ValidObject_DoesNotThrow()
	{
		DOMAINASSERT.IsNotNull(new object());
	}

	[Fact]
	public void IsNullOrEmpty_EmptyString_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DOMAINASSERT.IsNullOrEmpty(""));
	}

	[Fact]
	public void IsNullOrEmpty_NullString_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DOMAINASSERT.IsNullOrEmpty(null));
	}

	[Fact]
	public void IsNullOrEmpty_ValidString_DoesNotThrow()
	{
		DOMAINASSERT.IsNullOrEmpty("hello");
	}

	[Fact]
	public void IsNullOrWhitespace_NullString_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DOMAINASSERT.IsNullOrWhitespace(null));
	}

	[Fact]
	public void IsNullOrWhitespace_ValidString_DoesNotThrow()
	{
		DOMAINASSERT.IsNullOrWhitespace("hello");
	}

	[Fact]
	public void IsNullOrWhitespace_WhitespaceString_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => DOMAINASSERT.IsNullOrWhitespace("   "));
	}

	[Fact]
	public void IsTrue_FalseCondition_ThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => DOMAINASSERT.IsTrue(() => false));
	}

	[Fact]
	public void IsTrue_TrueCondition_DoesNotThrow()
	{
		DOMAINASSERT.IsTrue(() => true);
	}

	[Fact]
	public void IsValidIdentity_NegativeId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => DOMAINASSERT.IsValidIdentity(-5));
	}

	[Fact]
	public void IsValidIdentity_PositiveId_DoesNotThrow()
	{
		DOMAINASSERT.IsValidIdentity(1);
	}

	[Fact]
	public void IsValidIdentity_ZeroId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => DOMAINASSERT.IsValidIdentity(0));
	}

	[Fact]
	public void ShouldContain_SubstringMissing_ThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => DOMAINASSERT.ShouldContain("hello", "xyz"));
	}

	[Fact]
	public void ShouldContain_SubstringPresent_DoesNotThrow()
	{
		DOMAINASSERT.ShouldContain("hello world", "world");
	}

	#endregion
}
