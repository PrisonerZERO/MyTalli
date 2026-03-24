namespace My.Talli.UnitTesting.Framework;

using TRANSACTION = Domain.Framework.EnforcedTransactionScope;

/// <summary>Tests</summary>
public class EnforcedTransactionScopeTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_Generic_ReturnsValueFromDelegate()
	{
		var result = await TRANSACTION.ExecuteAsync(async () =>
		{
			await Task.CompletedTask;
			return 42;
		});

		Assert.Equal(42, result);
	}

	[Fact]
	public async Task ExecuteAsync_Generic_ThrowsException_PropagatesException()
	{
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await TRANSACTION.ExecuteAsync<int>(async () =>
			{
				await Task.CompletedTask;
				throw new InvalidOperationException("Test failure");
			});
		});
	}

	[Fact]
	public async Task ExecuteAsync_Void_CompletesSuccessfully()
	{
		var executed = false;

		await TRANSACTION.ExecuteAsync(async () =>
		{
			await Task.CompletedTask;
			executed = true;
		});

		Assert.True(executed);
	}

	[Fact]
	public async Task ExecuteAsync_Void_ThrowsException_PropagatesException()
	{
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await TRANSACTION.ExecuteAsync(async () =>
			{
				await Task.CompletedTask;
				throw new InvalidOperationException("Test failure");
			});
		});
	}

	#endregion
}
