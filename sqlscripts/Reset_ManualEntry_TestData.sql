-- =============================================================================
-- Reset Manual Entry test data
-- =============================================================================
-- Hard-deletes all Manual-platform Revenue / Expense / Payout rows (and their
-- subtables) for a single user. Leaves ShopConnection + PlatformConnection
-- rows untouched — your manual shops stay set up, only the $ data is cleared.
--
-- Scope: a single user. Set @UserId below before running.
-- Safety: wrapped in an explicit transaction so you can ROLLBACK if the row
-- counts look wrong.
-- =============================================================================

USE [MyTalli];
GO

DECLARE @UserId BIGINT = 1; -- ← change this to the user you want to reset

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    -- ── Subtables first (share PK with base table; FK requires base row alive) ──

    DELETE rm
    FROM [app].[RevenueManual] rm
    INNER JOIN [app].[Revenue] r ON r.[Id] = rm.[RevenueId]
    WHERE r.[UserId] = @UserId
      AND r.[Platform] = N'Manual';

    PRINT CONCAT(N'RevenueManual rows deleted: ', @@ROWCOUNT);

    DELETE em
    FROM [app].[ExpenseManual] em
    INNER JOIN [app].[Expense] e ON e.[Id] = em.[ExpenseId]
    WHERE e.[UserId] = @UserId
      AND e.[Platform] = N'Manual';

    PRINT CONCAT(N'ExpenseManual rows deleted: ', @@ROWCOUNT);

    DELETE pm
    FROM [app].[PayoutManual] pm
    INNER JOIN [app].[Payout] p ON p.[Id] = pm.[PayoutId]
    WHERE p.[UserId] = @UserId
      AND p.[Platform] = N'Manual';

    PRINT CONCAT(N'PayoutManual rows deleted: ', @@ROWCOUNT);

    -- ── Base tables (Revenue / Expense / Payout) ──

    DELETE FROM [app].[Revenue]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Manual';

    PRINT CONCAT(N'Revenue rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Expense]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Manual';

    PRINT CONCAT(N'Expense rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Payout]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Manual';

    PRINT CONCAT(N'Payout rows deleted: ', @@ROWCOUNT);

    -- Review the row counts above. If correct, run:
    --     COMMIT TRANSACTION;
    -- If wrong, run:
    --     ROLLBACK TRANSACTION;
    PRINT N'';
    PRINT N'>>> Review the deletion counts above. <<<';
    PRINT N'>>> Run COMMIT TRANSACTION to keep, or ROLLBACK TRANSACTION to undo. <<<';

END TRY
BEGIN CATCH

    PRINT N'ERROR — rolling back.';
    PRINT ERROR_MESSAGE();
    ROLLBACK TRANSACTION;

END CATCH;
GO
