-- =============================================================================
-- Reset PayPal test data
-- =============================================================================
-- Hard-deletes all PayPal-platform Revenue / Expense / Payout rows for a
-- single user. Leaves ShopConnection + PlatformConnection rows untouched —
-- your PayPal connection stays set up, only the $ data is cleared.
--
-- PayPal does not have provider-specific Revenue/Expense/Payout subtables yet
-- (the common columns on the base tables cover everything PayPal returns).
-- If a PayPal subtable is added later, mirror the Etsy/Stripe/Gumroad pattern.
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

    -- ── Base tables (Revenue / Expense / Payout) ──

    DELETE FROM [app].[Revenue]
    WHERE [UserId] = @UserId
      AND [Platform] = N'PayPal';

    PRINT CONCAT(N'Revenue rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Expense]
    WHERE [UserId] = @UserId
      AND [Platform] = N'PayPal';

    PRINT CONCAT(N'Expense rows deleted: ', @@ROWCOUNT);

    DELETE FROM [app].[Payout]
    WHERE [UserId] = @UserId
      AND [Platform] = N'PayPal';

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
