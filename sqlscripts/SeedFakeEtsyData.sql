/* ============================================================================
   SeedFakeEtsyData.sql
   ----------------------------------------------------------------------------
   Seeds synthetic Etsy-sourced Revenue, RevenueEtsy, Expense, ExpenseEtsy,
   Payout, and PayoutEtsy rows for a given user so the UI/UX can be exercised
   without a real Etsy sync.

   Re-runnable: on each run, any previously-seeded fake rows (identified by a
   'fake_etsy_' prefix on PlatformTransactionId / PlatformPayoutId) are deleted
   and fresh rows are inserted. Child subtable rows (RevenueEtsy, ExpenseEtsy,
   PayoutEtsy) cascade-delete automatically via their FK constraints.

   The fake Etsy ShopConnection gets NextSyncDateTime = 9999-12-31 so the real
   ShopSyncWorker never tries to call the Etsy API with placeholder tokens.

   USAGE: Change @Email at the top, then execute against the MyTalli database.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ============================================================================
   1. CONFIGURATION
   ============================================================================ */

DECLARE @Email                  NVARCHAR(256) = N'robertmerrilljordan@gmail.com';
DECLARE @RevenueRowCount        INT           = 80;   -- sales spread across the backfill window
DECLARE @BackfillDays           INT           = 90;   -- how far back to spread data
DECLARE @AdFeeWeekly            DECIMAL(18,2) = 7.50;
DECLARE @EtsyPlusMonthly        DECIMAL(18,2) = 10.00;
DECLARE @ListingFeePerListing   DECIMAL(18,2) = 0.20;
DECLARE @ListingsActive         INT           = 18;
DECLARE @PayoutIntervalDays     INT           = 7;

DECLARE @Now          DATETIME2 = SYSUTCDATETIME();
DECLARE @WindowStart  DATETIME2 = DATEADD(DAY, -@BackfillDays, @Now);
DECLARE @FakeSentinel NVARCHAR(20) = N'fake_etsy_';

/* ============================================================================
   2. RESOLVE USER BY EMAIL
   Check all three provider auth tables. Abort if not found.
   ============================================================================ */

DECLARE @UserId BIGINT;

SELECT TOP 1 @UserId = UserId FROM auth.UserAuthenticationGoogle     WHERE Email = @Email AND IsDeleted = 0;
IF @UserId IS NULL SELECT TOP 1 @UserId = UserId FROM auth.UserAuthenticationMicrosoft  WHERE Email = @Email AND IsDeleted = 0;
IF @UserId IS NULL SELECT TOP 1 @UserId = UserId FROM auth.UserAuthenticationApple      WHERE Email = @Email AND IsDeleted = 0;

IF @UserId IS NULL
BEGIN
    RAISERROR('No user found with email %s. Script aborted.', 16, 1, @Email);
    RETURN;
END

PRINT CONCAT('Seeding fake Etsy data for UserId ', @UserId, ' (', @Email, ')...');

BEGIN TRY
    BEGIN TRAN;

    /* ========================================================================
       3. UPSERT PlatformConnection (auth = 'Etsy')
       ======================================================================== */

    DECLARE @PlatformConnectionId BIGINT;

    SELECT @PlatformConnectionId = Id
    FROM app.PlatformConnection
    WHERE UserId = @UserId AND Platform = N'Etsy' AND IsDeleted = 0;

    IF @PlatformConnectionId IS NULL
    BEGIN
        INSERT INTO app.PlatformConnection
            (UserId, ConnectionStatus, Platform, IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
        VALUES
            (@UserId, N'Active', N'Etsy', 0, 1, @UserId, @Now);

        SET @PlatformConnectionId = SCOPE_IDENTITY();
        PRINT CONCAT('  Created PlatformConnection Id=', @PlatformConnectionId);
    END
    ELSE
    BEGIN
        PRINT CONCAT('  Reusing PlatformConnection Id=', @PlatformConnectionId);
    END

    /* ========================================================================
       4. UPSERT ShopConnection + ShopConnectionEtsy
       Fake shop uses PlatformShopId = 'fake_etsy_shop'. NextSyncDateTime is
       far in the future so the real worker never tries to sync it.
       ======================================================================== */

    DECLARE @FakeShopId         NVARCHAR(255) = N'fake_etsy_shop';
    DECLARE @FakeAccountId      NVARCHAR(255) = N'fake_etsy_account';
    DECLARE @ShopConnectionId   BIGINT;

    SELECT @ShopConnectionId = Id
    FROM app.ShopConnection
    WHERE PlatformConnectionId = @PlatformConnectionId
      AND PlatformShopId = @FakeShopId
      AND IsDeleted = 0;

    IF @ShopConnectionId IS NULL
    BEGIN
        INSERT INTO app.ShopConnection
            (PlatformConnectionId, UserId, AccessToken, ConsecutiveFailures, IsActive, IsEnabled,
             LastErrorMessage, LastSyncDateTime, NextSyncDateTime, PlatformAccountId, PlatformShopId,
             RefreshToken, RefreshTokenExpiryDateTime, ShopName, Status, TokenExpiryDateTime,
             IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
        VALUES
            (@PlatformConnectionId, @UserId, N'fake_access_token', 0, 1, 1,
             NULL, @Now, '9999-12-31', @FakeAccountId, @FakeShopId,
             N'fake_refresh_token', '9999-12-31', N'My Cozy Test Shop', N'Completed', '9999-12-31',
             0, 1, @UserId, @Now);

        SET @ShopConnectionId = SCOPE_IDENTITY();
        PRINT CONCAT('  Created ShopConnection Id=', @ShopConnectionId);
    END
    ELSE
    BEGIN
        UPDATE app.ShopConnection
        SET LastSyncDateTime = @Now,
            NextSyncDateTime = '9999-12-31',
            Status = N'Completed',
            ConsecutiveFailures = 0,
            LastErrorMessage = NULL,
            UpdatedByUserId = @UserId,
            UpdatedOnDate = @Now
        WHERE Id = @ShopConnectionId;

        PRINT CONCAT('  Reusing ShopConnection Id=', @ShopConnectionId);
    END

    IF NOT EXISTS (SELECT 1 FROM app.ShopConnectionEtsy WHERE ShopConnectionId = @ShopConnectionId)
    BEGIN
        INSERT INTO app.ShopConnectionEtsy
            (ShopConnectionId, CountryCode, IsVacationMode, ShopCurrency, ShopUrl,
             IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
        VALUES
            (@ShopConnectionId, N'US', 0, N'USD', N'https://www.etsy.com/shop/MyCozyTestShop',
             0, 1, @UserId, @Now);

        PRINT '  Created ShopConnectionEtsy detail';
    END

    /* ========================================================================
       5. CLEAN UP PREVIOUS FAKE ROWS
       Identified by the sentinel prefix on PlatformTransactionId/PlatformPayoutId.
       Hard delete (not soft delete) so the script is idempotent.
       ======================================================================== */

    DELETE re
    FROM app.RevenueEtsy re
    INNER JOIN app.Revenue r ON r.Id = re.RevenueId
    WHERE r.UserId = @UserId
      AND r.PlatformTransactionId LIKE @FakeSentinel + '%';

    DELETE FROM app.Revenue WHERE UserId = @UserId AND PlatformTransactionId LIKE @FakeSentinel + '%';
    DELETE FROM app.Expense WHERE UserId = @UserId AND PlatformTransactionId LIKE @FakeSentinel + '%';
    DELETE FROM app.Payout  WHERE UserId = @UserId AND PlatformPayoutId      LIKE @FakeSentinel + '%';

    PRINT '  Cleaned previous fake rows.';

    /* ========================================================================
       6. INSERT REVENUE + RevenueEtsy
       Generates @RevenueRowCount sales distributed across the backfill window.
       Amounts roughly match a small Etsy shop: $8-$95 gross, ~6.5% + $0.20 fees.
       ~5% disputed, ~5% refunded, rest normal paid sales.
       ======================================================================== */

    ;WITH Tally AS (
        SELECT TOP (@RevenueRowCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ),
    Generated AS (
        SELECT
            N,
            /* Spread dates across the window, slightly clustered in recent weeks */
            DATEADD(MINUTE,
                -CAST(ABS(CHECKSUM(NEWID())) % (@BackfillDays * 24 * 60) AS INT),
                @Now) AS TxnDate,
            /* Gross: $8 - $95 */
            CAST(8 + (ABS(CHECKSUM(NEWID())) % 8700) / 100.0 AS DECIMAL(18,2)) AS Gross,
            /* Outcome bucket: 0-89 paid, 90-94 refunded, 95-99 disputed */
            ABS(CHECKSUM(NEWID())) % 100 AS Bucket,
            /* Listing/Receipt ids */
            1000000000 + (ABS(CHECKSUM(NEWID())) % 99999999) AS ListingId,
            2000000000 + (ABS(CHECKSUM(NEWID())) % 99999999) AS ReceiptId
        FROM Tally
    ),
    Priced AS (
        SELECT
            N, TxnDate, Gross, Bucket, ListingId, ReceiptId,
            /* Etsy transaction fee (6.5%) + payment processing (~3% + $0.25) — approximate */
            CAST(ROUND(Gross * 0.065 + (Gross * 0.03) + 0.25, 2) AS DECIMAL(18,2)) AS Fee
        FROM Generated
    )
    INSERT INTO app.Revenue
        (ShopConnectionId, UserId, Currency, Description, FeeAmount, GrossAmount, IsDisputed, IsRefunded,
         NetAmount, Platform, PlatformTransactionId, TransactionDate,
         IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        @ShopConnectionId,
        @UserId,
        N'USD',
        CONCAT(N'Handmade item #', N),
        Fee,
        Gross,
        CASE WHEN Bucket >= 95 THEN 1 ELSE 0 END,
        CASE WHEN Bucket BETWEEN 90 AND 94 THEN 1 ELSE 0 END,
        CAST(Gross - Fee AS DECIMAL(18,2)),
        N'Etsy',
        CONCAT(@FakeSentinel, N'txn_', ReceiptId, N'_', N),
        TxnDate,
        0,
        1,
        @UserId,
        TxnDate
    FROM Priced;

    DECLARE @RevenueInserted INT = @@ROWCOUNT;
    PRINT CONCAT('  Inserted ', @RevenueInserted, ' Revenue rows.');

    /* RevenueEtsy — 1:1 with Revenue rows we just inserted (PK column: RevenueId). */
    INSERT INTO app.RevenueEtsy
        (RevenueId, AdjustedFees, AdjustedGross, AdjustedNet,
         ListingId, ReceiptId, ShopCurrency,
         IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        r.Id,
        NULL,
        NULL,
        NULL,
        1000000000 + (ABS(CHECKSUM(NEWID())) % 99999999),
        2000000000 + (ABS(CHECKSUM(NEWID())) % 99999999),
        N'USD',
        0,
        1,
        @UserId,
        r.CreatedOnDateTime
    FROM app.Revenue r
    WHERE r.UserId = @UserId
      AND r.PlatformTransactionId LIKE @FakeSentinel + '%';

    PRINT CONCAT('  Inserted ', @RevenueInserted, ' RevenueEtsy detail rows.');

    /* ========================================================================
       7. INSERT EXPENSE ROWS
       - Monthly listing fees (~18 active listings × $0.20)
       - Weekly ad fees
       - Monthly Etsy Plus subscription
       ======================================================================== */

    DECLARE @MonthsInWindow INT = CEILING(@BackfillDays / 30.0);
    DECLARE @WeeksInWindow  INT = CEILING(@BackfillDays / 7.0);
    DECLARE @ExpensesInserted INT = 0;

    /* Listing fees — one row per month */
    ;WITH MonthTally AS (
        SELECT TOP (@MonthsInWindow) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS M
        FROM sys.all_objects
    )
    INSERT INTO app.Expense
        (ShopConnectionId, UserId, Amount, Category, Currency, Description, ExpenseDate,
         Platform, PlatformTransactionId, IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        @ShopConnectionId, @UserId,
        @ListingFeePerListing * @ListingsActive,
        N'Listing Fee', N'USD',
        CONCAT(N'Listing fees for ', @ListingsActive, N' active items'),
        DATEADD(DAY, -M * 30, @Now),
        N'Etsy',
        CONCAT(@FakeSentinel, N'listing_', M),
        0, 1, @UserId, DATEADD(DAY, -M * 30, @Now)
    FROM MonthTally;

    SET @ExpensesInserted = @ExpensesInserted + @@ROWCOUNT;

    /* Ad fees — one row per week */
    ;WITH WeekTally AS (
        SELECT TOP (@WeeksInWindow) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS W
        FROM sys.all_objects
    )
    INSERT INTO app.Expense
        (ShopConnectionId, UserId, Amount, Category, Currency, Description, ExpenseDate,
         Platform, PlatformTransactionId, IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        @ShopConnectionId, @UserId,
        @AdFeeWeekly,
        N'Ad Fee', N'USD',
        N'Etsy Ads — weekly spend',
        DATEADD(DAY, -W * 7, @Now),
        N'Etsy',
        CONCAT(@FakeSentinel, N'ad_', W),
        0, 1, @UserId, DATEADD(DAY, -W * 7, @Now)
    FROM WeekTally;

    SET @ExpensesInserted = @ExpensesInserted + @@ROWCOUNT;

    /* Subscription fees — monthly Etsy Plus */
    ;WITH SubTally AS (
        SELECT TOP (@MonthsInWindow) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS M
        FROM sys.all_objects
    )
    INSERT INTO app.Expense
        (ShopConnectionId, UserId, Amount, Category, Currency, Description, ExpenseDate,
         Platform, PlatformTransactionId, IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        @ShopConnectionId, @UserId,
        @EtsyPlusMonthly,
        N'Subscription Fee', N'USD',
        N'Etsy Plus monthly subscription',
        DATEADD(DAY, -M * 30, @Now),
        N'Etsy',
        CONCAT(@FakeSentinel, N'sub_', M),
        0, 1, @UserId, DATEADD(DAY, -M * 30, @Now)
    FROM SubTally;

    SET @ExpensesInserted = @ExpensesInserted + @@ROWCOUNT;

    PRINT CONCAT('  Inserted ', @ExpensesInserted, ' Expense rows.');

    /* ExpenseEtsy — 1:1 with every fake Etsy expense (PK column: ExpenseId).
       - AdCampaignId + ListingId are populated only for Ad Fee rows
         (listing fees are batch-level, subscription fees aren't listing-scoped).
       - LedgerEntryId is populated for every row. */
    INSERT INTO app.ExpenseEtsy
        (ExpenseId, AdCampaignId, LedgerEntryId, ListingId,
         IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        e.Id,
        CASE WHEN e.Category = N'Ad Fee' THEN 3000000000 + (ABS(CHECKSUM(NEWID())) % 99999999) ELSE NULL END,
        4000000000 + (ABS(CHECKSUM(NEWID())) % 99999999),
        CASE WHEN e.Category = N'Ad Fee' THEN 1000000000 + (ABS(CHECKSUM(NEWID())) % 99999999) ELSE NULL END,
        0, 1, @UserId, e.CreatedOnDateTime
    FROM app.Expense e
    WHERE e.UserId = @UserId
      AND e.PlatformTransactionId LIKE @FakeSentinel + '%';

    PRINT CONCAT('  Inserted ', @ExpensesInserted, ' ExpenseEtsy detail rows.');

    /* ========================================================================
       8. INSERT PAYOUT ROWS
       Weekly disbursements. Most recent is 'In Transit', the rest are 'Paid'.
       Amount = sum of net revenue for that week (approximated via a per-week sum).
       ======================================================================== */

    DECLARE @PayoutCount INT = @BackfillDays / @PayoutIntervalDays;
    DECLARE @PayoutsInserted INT = 0;

    ;WITH PayoutTally AS (
        SELECT TOP (@PayoutCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS P
        FROM sys.all_objects
    ),
    PayoutWindows AS (
        SELECT
            P,
            DATEADD(DAY, -P * @PayoutIntervalDays, @Now) AS WindowEnd,
            DATEADD(DAY, -(P + 1) * @PayoutIntervalDays, @Now) AS WindowStart
        FROM PayoutTally
    )
    INSERT INTO app.Payout
        (ShopConnectionId, UserId, Amount, Currency, ExpectedArrivalDate, PayoutDate,
         Platform, PlatformPayoutId, Status, IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        @ShopConnectionId, @UserId,
        ISNULL((
            SELECT SUM(r.NetAmount)
            FROM app.Revenue r
            WHERE r.UserId = @UserId
              AND r.PlatformTransactionId LIKE @FakeSentinel + '%'
              AND r.TransactionDate >= pw.WindowStart
              AND r.TransactionDate <  pw.WindowEnd
              AND r.IsDisputed = 0
              AND r.IsRefunded = 0
        ), 0),
        N'USD',
        CASE WHEN P = 0 THEN DATEADD(DAY, 2, @Now) ELSE NULL END,
        pw.WindowEnd,
        N'Etsy',
        CONCAT(@FakeSentinel, N'payout_', P),
        CASE WHEN P = 0 THEN N'In Transit' ELSE N'Paid' END,
        0, 1, @UserId, pw.WindowEnd
    FROM PayoutWindows pw
    /* Skip the very first window if it happens to have zero revenue — keeps
       the payouts list looking realistic. */
    WHERE EXISTS (
        SELECT 1 FROM app.Revenue r
        WHERE r.UserId = @UserId
          AND r.PlatformTransactionId LIKE @FakeSentinel + '%'
          AND r.TransactionDate >= pw.WindowStart
          AND r.TransactionDate <  pw.WindowEnd
    );

    SET @PayoutsInserted = @@ROWCOUNT;
    PRINT CONCAT('  Inserted ', @PayoutsInserted, ' Payout rows.');

    /* PayoutEtsy — 1:1 with every fake Etsy payout (PK column: PayoutId). */
    INSERT INTO app.PayoutEtsy
        (PayoutId, LedgerEntryId, ShopCurrency,
         IsDeleted, IsVisible, CreateByUserId, CreatedOnDateTime)
    SELECT
        p.Id,
        5000000000 + (ABS(CHECKSUM(NEWID())) % 99999999),
        N'USD',
        0, 1, @UserId, p.CreatedOnDateTime
    FROM app.Payout p
    WHERE p.UserId = @UserId
      AND p.PlatformPayoutId LIKE @FakeSentinel + '%';

    PRINT CONCAT('  Inserted ', @PayoutsInserted, ' PayoutEtsy detail rows.');

    COMMIT TRAN;

    /* ========================================================================
       9. SUMMARY
       ======================================================================== */

    PRINT '';
    PRINT '=== Fake Etsy data seeded successfully ===';
    PRINT CONCAT('  UserId:            ', @UserId);
    PRINT CONCAT('  ShopConnectionId:  ', @ShopConnectionId);
    PRINT CONCAT('  Revenue + RevenueEtsy: ', @RevenueInserted);
    PRINT CONCAT('  Expense + ExpenseEtsy: ', @ExpensesInserted);
    PRINT CONCAT('  Payout  + PayoutEtsy:  ', @PayoutsInserted);

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();
    RAISERROR('Fake Etsy seed failed at line %d: %s', 16, 1, @ErrLine, @ErrMsg);
END CATCH
