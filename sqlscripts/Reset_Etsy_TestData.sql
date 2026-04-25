-- =============================================================================
-- Reset Etsy test data
-- =============================================================================
-- Hard-deletes all Etsy-platform Revenue / Expense / Payout rows (and their
-- subtables) for a single user. Leaves ShopConnection + PlatformConnection
-- rows untouched — your Etsy shop connection stays set up, only the $ data
-- is cleared.
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

    DELETE re
    FROM [app].[RevenueEtsy] re
    INNER JOIN [app].[Revenue] r ON r.[Id] = re.[RevenueId]
    WHERE r.[UserId] = @UserId
      AND r.[Platform] = N'Etsy';

    PRINT CONCAT(N'RevenueEtsy rows deleted: ', @@ROWCOUNT);

    DELETE ee
    FROM [app].[ExpenseEtsy] ee
    INNER JOIN [app].[Expense] e ON e.[Id] = ee.[ExpenseId]
    WHERE e.[UserId] = @UserId
      AND e.[Platform] = N'Etsy';

    PRINT CONCAT(N'ExpenseEtsy rows deleted: ', @@ROWCOUNT);

    DELETE pe
    FROM [app].[PayoutEtsy] pe
    INNER JOIN [app].[Payout] p ON p.[Id] = pe.[PayoutId]
    WHERE p.[UserId] = @UserId
      AND p.[Platform] = N'Etsy';

    PRINT CONCAT(N'PayoutEtsy rows deleted: ', @@ROWCOUNT);

    -- ── Base tables (Revenue / Expense / Payout) ──

    DELETE FROM [app].[Revenue]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Etsy';

    PRINT CONCAT(N'Revenue rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Expense]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Etsy';

    PRINT CONCAT(N'Expense rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Payout]
    WHERE [UserId] = @UserId
      AND [Platform] = N'Etsy';

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
