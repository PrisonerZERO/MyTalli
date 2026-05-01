-- =============================================================================
-- Reset all platform connections + financial test data for one user
-- =============================================================================
-- Hard-deletes EVERY ShopConnection (and its provider subtable rows), every
-- PlatformConnection, and every Revenue / Expense / Payout row (plus their
-- per-platform subtables) for a single user. Intended for local testing —
-- gives you a clean slate to re-run the full first-time-connect flow.
--
-- Scope: a single user, looked up by email via [auth].[vAuthenticatedUser].
-- Set @EmailAddress below before running.
--
-- Safety: wrapped in an explicit transaction. Auto-commits on success; auto-
-- rolls back if any DELETE throws. Row counts print to the Messages tab.
-- =============================================================================

USE [MyTalli];
GO

DECLARE @EmailAddress NVARCHAR(256) = N'robertmerrilljordan@gmail.com';
DECLARE @UserId BIGINT = (SELECT [Id] FROM [auth].[vAuthenticatedUser] WHERE [EmailAddress] = @EmailAddress );

IF @UserId IS NULL
BEGIN
    PRINT CONCAT(N'No user found for email: ', @EmailAddress);
    RETURN;
END;

PRINT CONCAT(N'Resetting data for UserId = ', @UserId, N' (', @EmailAddress, N')');
PRINT N'';

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    -- ── Financial subtables first (share PK with base; FK requires base row alive) ──

    DELETE re
    FROM [app].[RevenueEtsy] re
    INNER JOIN [app].[Revenue] r ON r.[Id] = re.[RevenueId]
    WHERE r.[UserId] = @UserId;

    PRINT CONCAT(N'RevenueEtsy rows deleted:    ', @@ROWCOUNT);

    DELETE rg
    FROM [app].[RevenueGumroad] rg
    INNER JOIN [app].[Revenue] r ON r.[Id] = rg.[RevenueId]
    WHERE r.[UserId] = @UserId;

    PRINT CONCAT(N'RevenueGumroad rows deleted: ', @@ROWCOUNT);

    DELETE rm
    FROM [app].[RevenueManual] rm
    INNER JOIN [app].[Revenue] r ON r.[Id] = rm.[RevenueId]
    WHERE r.[UserId] = @UserId;

    PRINT CONCAT(N'RevenueManual rows deleted:  ', @@ROWCOUNT);

    DELETE rs
    FROM [app].[RevenueStripe] rs
    INNER JOIN [app].[Revenue] r ON r.[Id] = rs.[RevenueId]
    WHERE r.[UserId] = @UserId;

    PRINT CONCAT(N'RevenueStripe rows deleted:  ', @@ROWCOUNT);

    DELETE ee
    FROM [app].[ExpenseEtsy] ee
    INNER JOIN [app].[Expense] e ON e.[Id] = ee.[ExpenseId]
    WHERE e.[UserId] = @UserId;

    PRINT CONCAT(N'ExpenseEtsy rows deleted:    ', @@ROWCOUNT);

    DELETE eg
    FROM [app].[ExpenseGumroad] eg
    INNER JOIN [app].[Expense] e ON e.[Id] = eg.[ExpenseId]
    WHERE e.[UserId] = @UserId;

    PRINT CONCAT(N'ExpenseGumroad rows deleted: ', @@ROWCOUNT);

    DELETE em
    FROM [app].[ExpenseManual] em
    INNER JOIN [app].[Expense] e ON e.[Id] = em.[ExpenseId]
    WHERE e.[UserId] = @UserId;

    PRINT CONCAT(N'ExpenseManual rows deleted:  ', @@ROWCOUNT);

    DELETE es
    FROM [app].[ExpenseStripe] es
    INNER JOIN [app].[Expense] e ON e.[Id] = es.[ExpenseId]
    WHERE e.[UserId] = @UserId;

    PRINT CONCAT(N'ExpenseStripe rows deleted:  ', @@ROWCOUNT);

    DELETE pe
    FROM [app].[PayoutEtsy] pe
    INNER JOIN [app].[Payout] p ON p.[Id] = pe.[PayoutId]
    WHERE p.[UserId] = @UserId;

    PRINT CONCAT(N'PayoutEtsy rows deleted:     ', @@ROWCOUNT);

    DELETE pg
    FROM [app].[PayoutGumroad] pg
    INNER JOIN [app].[Payout] p ON p.[Id] = pg.[PayoutId]
    WHERE p.[UserId] = @UserId;

    PRINT CONCAT(N'PayoutGumroad rows deleted:  ', @@ROWCOUNT);

    DELETE pm
    FROM [app].[PayoutManual] pm
    INNER JOIN [app].[Payout] p ON p.[Id] = pm.[PayoutId]
    WHERE p.[UserId] = @UserId;

    PRINT CONCAT(N'PayoutManual rows deleted:   ', @@ROWCOUNT);

    DELETE ps
    FROM [app].[PayoutStripe] ps
    INNER JOIN [app].[Payout] p ON p.[Id] = ps.[PayoutId]
    WHERE p.[UserId] = @UserId;

    PRINT CONCAT(N'PayoutStripe rows deleted:   ', @@ROWCOUNT);

    -- ── Financial base tables (Revenue / Expense / Payout) ──
    -- Must be deleted before ShopConnection — FK_*_ShopConnection is Restrict.

    DELETE FROM [app].[Revenue] WHERE [UserId] = @UserId;
    PRINT CONCAT(N'Revenue rows deleted:        ', @@ROWCOUNT);

    DELETE FROM [app].[Expense] WHERE [UserId] = @UserId;
    PRINT CONCAT(N'Expense rows deleted:        ', @@ROWCOUNT);

    DELETE FROM [app].[Payout] WHERE [UserId] = @UserId;
    PRINT CONCAT(N'Payout rows deleted:         ', @@ROWCOUNT);

    -- ── ShopConnection subtables (share PK with ShopConnection) ──

    DELETE sce
    FROM [app].[ShopConnectionEtsy] sce
    INNER JOIN [app].[ShopConnection] sc ON sc.[Id] = sce.[ShopConnectionId]
    WHERE sc.[UserId] = @UserId;

    PRINT CONCAT(N'ShopConnectionEtsy rows deleted: ', @@ROWCOUNT);

    -- ── ShopConnection ──

    DELETE FROM [app].[ShopConnection] WHERE [UserId] = @UserId;
    PRINT CONCAT(N'ShopConnection rows deleted:     ', @@ROWCOUNT);

    -- ── PlatformConnection (so first-time-connect flow can be re-tested) ──

    DELETE FROM [app].[PlatformConnection] WHERE [UserId] = @UserId;
    PRINT CONCAT(N'PlatformConnection rows deleted: ', @@ROWCOUNT);

    COMMIT TRANSACTION;

    PRINT N'';
    PRINT N'>>> Committed. <<<';

END TRY
BEGIN CATCH

    PRINT N'ERROR — rolling back.';
    PRINT ERROR_MESSAGE();
    ROLLBACK TRANSACTION;

END CATCH;
GO
