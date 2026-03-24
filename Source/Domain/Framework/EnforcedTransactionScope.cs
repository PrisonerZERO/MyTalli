namespace My.Talli.Domain.Framework;

using System.Transactions;

/// <summary>Transaction</summary>
public class EnforcedTransactionScope : IDisposable
{
    #region <Fields & Constants>
        
    private readonly TransactionScope _scope = new TransactionScope();
    private bool _completed;
    private bool _rolledBack;

    #endregion

    #region <Methods>

    public void Complete()
    {
        if (_rolledBack)
            throw new InvalidOperationException("Transaction has been rolled back & cannot complete.");

        _completed = true;
        _scope.Complete();
    }

    public void Dispose()
    {
        try
        {
            // Completed?
            if (!_completed && !_rolledBack)
                throw new InvalidOperationException("TransactionScope was disposed without calling Complete() or Rollback(). The transaction has been automatically rolled back.");
        }
        finally
        {
            _scope.Dispose(); //<-- Dispose will automatically Rollback (unless Complete() was called)
        }
    }

    /// <summary>Executes the provided action within a transaction scope, automatically handling commit or rollback.</summary>
    /// <param name="action">The action to execute within the transaction.</param>
    public static void Execute(Action action)
    {
        using (var transaction = new EnforcedTransactionScope())
        {
            try
            {
                action();
                transaction.Complete();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    /// <summary>Executes the provided function within a transaction scope, returning its result and handling commit or rollback.</summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute within the transaction.</param>
    /// <returns>The result of the function.</returns>
    public static T Execute<T>(Func<T> func)
    {
        using (var transaction = new EnforcedTransactionScope())
        {
            try
            {
                T result = func();
                transaction.Complete();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }


    /// <summary>Executes the provided async action within a transaction scope, automatically committing on success or rolling back on exception.</summary>
    /// <param name="action">The async action to execute within the transaction.</param>
    public static async Task ExecuteAsync(Func<Task> action)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await action();
        scope.Complete();
    }

    /// <summary>Executes the provided async function within a transaction scope, returning its result and automatically committing on success or rolling back on exception.</summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The async function to execute within the transaction.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var result = await func();
        scope.Complete();
        return result;
    }

    public void Rollback()
    {
        _rolledBack = true;
    }

    #endregion
}