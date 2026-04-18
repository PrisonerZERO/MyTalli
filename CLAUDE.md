# My.Talli

## What Is This?

MyTalli is a side-hustle revenue aggregation dashboard. It lets creators and freelancers connect their payment platforms (Stripe, Etsy, Gumroad, PayPal, Shopify, etc.) and see all their income in one unified dashboard with real-time tracking, trends, goals, and CSV export.

**Status:** Early development — landing page, sign-in, dashboard, and other pages are built. OAuth authentication is working (Google, Apple, Microsoft). Sign-in redirects to the dashboard. All routes are active. Stripe billing is integrated (checkout, plan switching, cancellation, reactivation).

## Tech Stack

- **.NET 10.0** — target framework
- **Blazor Server** (Interactive Server render mode) — `blazor.web.js`
- **Bootstrap** — bundled in `wwwroot/lib/bootstrap/`
- **C#** — backend language
- **ElmahCore** — error logging (SQL Server provider, dashboard at `/elmah`)
- **Entity Framework Core** — ORM (SQL Server provider)
- **Lamar** — IoC container (replaces default Microsoft DI)
- **Azure Communication Services (ACS) Email** — transactional email sending (NuGet: `Azure.Communication.Email`)
- **Razor Components** — UI layer (`.razor` files)
- **SQL Server** — database (localhost, Windows Auth)
- **Stripe** — payment processing (NuGet: `Stripe.net` v50, Stripe Checkout + Customer Portal + Webhooks)

## Database

- **Engine:** SQL Server
- **Database:** `MyTalli`
- **Local (dev):** `localhost`, Windows Authentication (Trusted Connection) — `ConnectionStrings:DefaultConnection`
- **Azure (prod):** `mytalli-centralus-sql.database.windows.net,1433`, SQL Authentication — `ConnectionStrings:AzureConnection`
- **App user:** `MyTalli-User` (SQL login) — `db_datareader`, `db_datawriter`, `EXECUTE`. Created by Pre-Deployment Script (uses `TRY/CATCH` for Azure SQL compatibility since `sys.server_principals` isn't accessible from user databases). The server login must be created manually on `master` before running migrations. Admin user (`MyTalli-Administrator`) is for schema changes only.
- **Rule:** All development and migrations run against localhost. Never run dev operations against the Azure database.
- **Migrations:** EF Core code-first, stored in `Domain.Data.EntityFramework/Migrations/`. All migrations inherit from `DbMigrationBase` (not `Migration` directly) — see "Migration SQL Scripts" below.
- **Migration commands (Package Manager Console):**
  - Add: `Add-Migration <Name> -Project Domain.Data.EntityFramework -StartupProject My.Talli.Web`
  - Apply: `Update-Database -Project Domain.Data.EntityFramework -StartupProject My.Talli.Web`
  - Remove last: `Remove-Migration -Project Domain.Data.EntityFramework -StartupProject My.Talli.Web`
- **Migration commands (CLI):**
  - Add: `dotnet ef migrations add <Name> --project Domain.Data.EntityFramework --startup-project My.Talli.Web --output-dir Migrations`
  - Apply: `dotnet ef database update --project Domain.Data.EntityFramework --startup-project My.Talli.Web`
  - Remove last: `dotnet ef migrations remove --project Domain.Data.EntityFramework --startup-project My.Talli.Web`
  - Generate script: `dotnet ef migrations script --project Domain.Data.EntityFramework --startup-project My.Talli.Web --output ../migrations/<MigrationName>.sql`
- **Production deployment:** Never run `dotnet ef database update` against production. Instead, generate a SQL script, review it, and run it manually in SSMS against the Azure database.
- **Migration script folder:** `migrations/` (git-ignored) — stores generated `.sql` deployment scripts
- **Migration script guard:** Every generated migration script must have a guard block prepended at the top that checks `__EFMigrationsHistory` for the migration ID and aborts with `RAISERROR` + `RETURN` if already applied. This prevents accidental re-runs against production.
  ```sql
  IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
      AND EXISTS (
          SELECT 1
          FROM [__EFMigrationsHistory]
          WHERE [MigrationId] = N'<MigrationId>'
      )
  BEGIN
      RAISERROR('Migration <MigrationId> has already been applied. Script aborted.', 16, 1);
      RETURN;
  END
  GO
  ```
- **Cascade delete restrictions:** `FK_Billing_User`, `FK_Subscription_User`, and `FK_Subscription_Product` use `DeleteBehavior.Restrict` to avoid SQL Server multiple cascade path errors. These entities are still reachable via indirect cascade paths (e.g., User → Order → Billing).

### Design Principles

- **No nulls** — provider-specific data lives in dedicated tables, not nullable columns on base tables
- **Provider separation** — auth providers (Google, Apple, Microsoft) and billing providers (Stripe, etc.) each get their own table with a 1-to-1 relationship to the base table. Adding a new provider = new table, no schema changes to existing tables.
- **Shared primary key for 1-to-1 tables** — 1-to-1 tables (e.g., `UserAuthenticationGoogle`, `BillingStripe`) use the parent's PK as their own PK. No separate identity column or FK column — `Id` serves as both PK and FK. Configured with `ValueGeneratedNever()` and `HasForeignKey<T>(e => e.Id)`. The C# property stays `Id` (so `IIdentifiable` and the repository chain work unchanged), but the **database column is renamed** via `HasColumnName()` to show data lineage: `UserId` for auth provider tables, `BillingId` for `BillingStripe`, `SubscriptionId` for `SubscriptionStripe`.
- **Column ordering convention** — EF configurations use `HasColumnOrder(N)` on every property. Order: PK (0) → FK columns (alphabetical, starting at 1) → domain columns (alphabetical) → `IsDeleted` → `IsVisible` → audit columns (`CreateByUserId`, `CreatedOnDateTime`, `UpdatedByUserId`, `UpdatedOnDate`).
- **Soft delete** — every entity has `IsDeleted` (default `false`) for logical deletion and `IsVisible` (default `true`) for hiding active records from views. All entities have a global query filter `HasQueryFilter(e => !e.IsDeleted)` so soft-deleted records are automatically excluded from queries. To include soft-deleted records, use `IgnoreQueryFilters()`.
- **Schema separation** — tables are organized into SQL schemas by functional domain (`auth`, `commerce`). `dbo` is reserved/empty.
- **Orders as the backbone** — subscriptions, modules, and any future products all flow through the same Order → OrderItem pipeline. A subscription is just a product.
- **No separate waitlist table** — (historical) during the earlier Waitlist Mode, the `auth.User` table doubled as the waitlist. Waitlist mode has since been removed.
- **Milestones in database** — the `app.Milestone` table still exists in the database but is no longer used by the app (code references were removed when waitlist functionality was removed).
- **No third-party table creation** — third-party packages (e.g., ElmahCore) must never create their own tables. All tables are created by our migrations so we own the schema, naming conventions, and migration history. If a package needs a table, create it in a migration SQL script with an `IF NOT EXISTS` guard.
- **Audit field self-creation sentinel** — `CreateByUserId = 0` means "self-created" (the user created their own account). This avoids a second database round-trip to self-stamp the generated Id. Only applies to `auth.User` rows created during OAuth sign-up.
- **Audit fields on insert** — on INSERT, only `CreateByUserId` and `CreatedOnDateTime` are populated. `UpdatedByUserId` and `UpdatedOnDate` remain `null` — nothing has been updated yet. They are only set on the first actual UPDATE.

### DbContext Thread Safety

Blazor Server renders layout components (NavMenu) and page components in parallel. All scoped services — including `TalliDbContext` and every repository — share the same instance per circuit. Without protection, concurrent async DB calls from different components hit the same non-thread-safe DbContext and throw `InvalidOperationException`.

- **`TalliDbContext.ConcurrencyLock`** — a `SemaphoreSlim(1, 1)` property on the DbContext itself. Since the DbContext is scoped (one per circuit), all repositories sharing it automatically share the same lock.
- **`GenericRepositoryAsync<T>`** — every method (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Remove`, `Update`) acquires `_dbContext.ConcurrencyLock` before touching the DbContext and releases it in a `finally` block.
- **`GenericAuditableRepositoryAsync<T>`** — every method (`InsertAsync`, `UpdateAsync`, `DeleteAsync`, and their Range/SaveChanges variants) acquires the lock once and does all work inside — including `_dbSet.Remove()` calls (inlined, not delegated to the base class, to avoid deadlocking the non-re-entrant semaphore). Both `UpdateAsync` and `DeleteAsync` check `_dbSet.Local` for already-tracked entities to avoid `InvalidOperationException` when another query has already loaded the same entity into the change tracker.
- **Automatic protection** — any code using `RepositoryAdapterAsync` (the only gateway to the data layer) is automatically serialized. New entities, pages, and adapters get protection without any per-page wiring.
- **Direct DbContext access** — code that queries `TalliDbContext` directly (e.g., `GetAdminUserListCommand` for the `AuthenticatedUsers` view) must manually acquire `_dbContext.ConcurrencyLock`. This is rare — per conventions, `RepositoryAdapterAsync` is the standard gateway.
- **`UserDisplayCache`** — retains its own `SemaphoreSlim` for caching purposes (avoiding redundant DB calls). The DbContext-level lock makes the serialization aspect redundant but harmless.

### Schemas

| Schema | Purpose | Tables |
|--------|---------|--------|
| `auth` | Identity & authentication | User, UserAuthenticationGoogle, UserAuthenticationApple, UserAuthenticationMicrosoft, UserRole |
| `commerce` | Products, orders, billing, subscriptions | ProductVendor, ProductType, Product, Order, OrderItem, Billing, BillingStripe, Subscription, SubscriptionStripe |
| `app` | Application features & revenue | Expense, Goal, GoalType, Milestone (legacy), Payout, PlatformConnection, Revenue, RevenueEtsy, RevenueGumroad, RevenueManual, RevenueStripe, ShopConnection, ShopConnectionEtsy, Suggestion, SuggestionVote |
| `components` | Third-party component tables (not EF-managed) | ELMAH_Error (auto-created by ElmahCore) |
| `dbo` | Reserved (empty) | — |

### Schema: `app`

**`app.Expense`** — platform fees not tied to a specific sale (listing fees, ad fees, subscription fees, etc.), and user-created manual expenses (entered via Manual Entry module)
- `Id` (PK), `ShopConnectionId` (nullable FK → ShopConnection — identifies which specific shop the expense came from for per-shop breakdowns; null for manual entries), `UserId` (FK → auth.User), `Amount` (decimal 18,2), `Category` (string 50 — Listing Fee, Ad Fee, Subscription Fee, Processing Fee, Shipping Label, Other), `Currency` (string 3), `Description` (string 500), `ExpenseDate` (datetime), `Platform` (string 50), `PlatformTransactionId` (nullable string 255 — dedup key, `manual_{guid}` for manual entries)
- Composite index on `(Platform, ExpenseDate)` for dashboard queries
- Indexes: `IX_Expense_UserId`, `IX_Expense_ShopConnectionId`
- FK behavior: `FK_Expense_ShopConnection` Restrict (preserves historical data if a shop is ever removed)
- Design: Parallel to Revenue — both queried by dashboard, no FK between them. `Revenue.FeeAmount` = per-sale fees; `Expense.Amount` = standalone platform fees or manual expenses. Actively used by Manual Entry module for full CRUD. Per-shop breakdowns group by `ShopConnectionId`.

**`app.Goal`** — user revenue goals (1:N from User, 1:N from GoalType)
- `Id` (PK), `UserId` (FK → auth.User), `GoalTypeId` (FK → GoalType), `EndDate` (nullable datetime), `Platform` (nullable string 50 — optional filter for platform-specific goals), `StartDate` (datetime), `Status` (string 20), `TargetAmount` (decimal 18,2)
- Indexes: `IX_Goal_UserId`, `IX_Goal_GoalTypeId`
- Goals query `app.Revenue` via `SUM(NetAmount) WHERE date range + optional platform` — no direct FK to Revenue

**`app.GoalType`** — lookup table for goal categories (seed data)
- `Id` (PK), `Name` (string 100)
- Seeded values: Monthly Revenue Target, Yearly Revenue Target, Platform Monthly Target, Growth Rate Target

**`app.Milestone`** — (legacy) waitlist progress tracker milestones. The table still exists in the database but all app code references (entity, model, mapper, configuration, framework constants) have been removed. The data remains for historical reference.
- `Id` (PK), `Description`, `MilestoneGroup` (Beta, FullLaunch), `SortOrder` (display order within group), `Status` (Complete, InProgress, Upcoming), `Title`
- `MilestoneStatuses.cs` and `MilestoneGroups.cs` (formerly in `Domain/Framework/`) have been removed.

**`app.PlatformConnection`** — OAuth tokens and platform account linking (one row per user per connected platform)
- `Id` (PK), `UserId` (FK → auth.User), `AccessToken` (nvarchar max), `ConnectionStatus` (string 50 — active, expired, revoked), `Platform` (string 50 — "Stripe", "Etsy", "Gumroad", "PayPal", "Shopify"), `PlatformAccountId` (string 255), `RefreshToken` (nullable, nvarchar max), `TokenExpiryDateTime` (nullable datetime)
- Unique constraint on `(UserId, Platform)` — one connection per user per platform
- Index: `IX_PlatformConnection_UserId`

**`app.Payout`** — platform disbursements to user's bank account, and user-created manual payouts (entered via Manual Entry module)
- `Id` (PK), `ShopConnectionId` (nullable FK → ShopConnection — identifies which specific shop the payout came from for per-shop breakdowns; null for manual entries), `UserId` (FK → auth.User), `Amount` (decimal 18,2), `Currency` (string 3), `ExpectedArrivalDate` (nullable datetime), `PayoutDate` (datetime), `Platform` (string 50), `PlatformPayoutId` (string 255 — dedup key, `manual_{guid}` for manual entries), `Status` (string 20 — Pending, In Transit, Paid, Failed, Cancelled)
- Composite index on `(Platform, PayoutDate)` for dashboard queries
- Unique index on `PlatformPayoutId` for dedup
- Indexes: `IX_Payout_UserId`, `IX_Payout_ShopConnectionId`
- FK behavior: `FK_Payout_ShopConnection` Restrict (preserves historical data if a shop is ever removed)
- Design: No FK to Revenue — one payout covers many sales (batched). Enables cash flow view: earned vs received vs pending. Actively used by Manual Entry module for full CRUD. Per-shop breakdowns group by `ShopConnectionId`.

**`app.Revenue`** — normalized revenue record from all platforms (API-sourced and manual entry)
- `Id` (PK), `ShopConnectionId` (nullable FK → ShopConnection — identifies which specific shop the sale came from for per-shop breakdowns; null for manual entries), `UserId` (FK → auth.User), `Currency` (3-char ISO), `Description`, `FeeAmount` (decimal 18,2), `GrossAmount` (decimal 18,2), `NetAmount` (decimal 18,2), `Platform` ("Manual", "Stripe", "Etsy", etc.), `PlatformTransactionId` (nullable, unique per platform), `TransactionDate`, `IsDisputed`, `IsRefunded`
- Composite index on `(Platform, TransactionDate)` for dashboard queries
- Indexes: `IX_Revenue_UserId`, `IX_Revenue_ShopConnectionId`
- FK behavior: `FK_Revenue_ShopConnection` Restrict (preserves historical data if a shop is ever removed)
- Design: Goals and dashboard analytics query **only** this normalized table. Platform-specific tables exist for drill-down detail. Per-shop breakdowns (e.g., "Etsy Shop A vs Etsy Shop B") group by `ShopConnectionId`.

**`app.RevenueEtsy`** — Etsy-specific revenue detail (1-to-1 with Revenue, shared PK)
- `RevenueId` (PK/FK → Revenue, C# property: `Id`), `AdjustedFees` (nullable decimal 18,2), `AdjustedGross` (nullable decimal 18,2), `AdjustedNet` (nullable decimal 18,2), `ListingId` (long), `ReceiptId` (long), `ShopCurrency` (string 3)

**`app.RevenueGumroad`** — Gumroad-specific revenue detail (1-to-1 with Revenue, shared PK)
- `RevenueId` (PK/FK → Revenue, C# property: `Id`), `DiscoverFee` (nullable decimal 18,2), `LicenseKey` (nullable string 500), `SaleId` (string 255)

**`app.RevenueManual`** — Manual Entry detail (1-to-1 with Revenue, shared PK)
- `RevenueId` (PK/FK → Revenue, C# property: `Id`), `Category` (Sale, Service, Freelance, Consulting, Digital Product, Physical Product, Other), `Notes` (nullable), `Quantity` (int, default 1)

**`app.RevenueStripe`** — Stripe-specific revenue detail (1-to-1 with Revenue, shared PK)
- `RevenueId` (PK/FK → Revenue, C# property: `Id`), `BalanceTransactionId` (string 255), `ExchangeRate` (nullable decimal 18,6), `PaymentMethod` (string 50), `RiskScore` (nullable int)

**`app.Suggestion`** — user-submitted feature requests and feedback
- `Id` (PK), `UserId` (FK → auth.User), `AdminNote` (nullable, max 500 — admin-visible note on the card), `Category` (max 50 — Feature, Integration, Export, UI / UX), `Description` (max 2000), `Status` (max 20 — New, UnderReview, InProgress, Planned, Completed, Declined), `Title` (max 200)
- Index on `UserId` (`IX_Suggestion_UserId`)

**`app.SuggestionVote`** — user votes on suggestions (junction: User ↔ Suggestion)
- `Id` (PK), `UserId` (FK → auth.User), `SuggestionId` (FK → Suggestion)
- Unique constraint on `(UserId, SuggestionId)` prevents duplicate votes

**`app.ShopConnection`** — sync target (the thing we sync from). One row per shop under a platform connection. Most platforms are 1:1 with `PlatformConnection` (Stripe, Gumroad, PayPal, Shopify); Etsy is 1:N because a seller can own multiple shops under one OAuth grant.
- `Id` (PK), `PlatformConnectionId` (FK → PlatformConnection), `UserId` (FK → auth.User, denormalized for per-user queries), `PlatformShopId` (string, max 255 — platform's native shop identifier), `ShopName` (string, max 255), `IsActive` (bool, default true — on free tier only one shop per user is active; Pro may have many), `Status` (string, max 20 — Pending, InProgress, Completed, Failed), `NextSyncDateTime` (when this shop is next eligible for processing; stepped to now + 24h after successful sync), `LastSyncDateTime` (nullable — null until first successful sync), `ConsecutiveFailures` (int, default 0 — drives exponential backoff), `LastErrorMessage` (nullable, max 2000 — most recent failure reason), `IsEnabled` (bool, default true — user can pause syncing)
- Unique constraint on `(PlatformConnectionId, PlatformShopId)` — one row per shop per connection
- Index on `(NextSyncDateTime, Status)` for sync worker polling
- Indexes: `IX_ShopConnection_UserId`, `IX_ShopConnection_PlatformConnectionId`
- FK behavior: `FK_ShopConnection_PlatformConnection` Cascade; `FK_ShopConnection_User` Restrict (avoids multiple cascade path collision with `FK_PlatformConnection_User`)
- Replaces the former `app.SyncQueue` table. The sync-queue fields (`Status`, `NextSyncDateTime`, `LastSyncDateTime`, `ConsecutiveFailures`, `LastErrorMessage`, `IsEnabled`) now live here, so there's one row per shop instead of one row per (user, platform).
- Users can pause sync (`IsEnabled = false`) but cannot disconnect — connected shops permanently occupy a plan slot.

**`app.ShopConnectionEtsy`** — Etsy-specific 1-to-1 extension of ShopConnection (shared PK)
- `ShopConnectionId` (PK/FK → ShopConnection, C# property: `Id`), `CountryCode` (char 2, ISO alpha-2), `IsVacationMode` (bool, default false — suppress "stale data" warnings when seller is on break), `ShopCurrency` (char 3, ISO 4217), `ShopUrl` (string, max 500 — deep-link target for the Platforms page)
- Other platforms (Stripe, Gumroad, PayPal, Shopify) don't have a provider-specific subtable yet — the common fields on `ShopConnection` cover them. Subtables get added (following the same shared-PK convention) when a provider-unique shop-level field appears.

### Schema: `auth`

**`auth.User`** — core MyTalli identity (one row per person)
- `Id` (PK), `DisplayName`, `FirstName`, `LastName`, `CreatedAt`, `LastLoginAt`, `InitialProvider` (historical — which provider they first signed in with, never changes), `PreferredProvider` (which provider the user prefers, starts equal to InitialProvider), `UserPreferences` (NVARCHAR(MAX), JSON — app settings/toggles, defaults to `'{}'`)
- Email is **not** stored here — it lives on the provider auth tables. The user's email is resolved via their PreferredProvider.
- **UserPreferences** stores user-configurable app settings as JSON. This avoids contorting the User table with individual columns as settings grow over time. Serialized/deserialized by `UserPreferencesJsonSerializer` in `Domain/Components/JsonSerializers/User/`. Current structure:
  ```json
  {
    "darkMode": "system",
    "emailPreferences": {
      "unsubscribeAll": false,
      "subscriptionConfirmationEmail": true,
      "weeklySummaryEmail": true
    },
    "funGreetings": true,
    "gridPreferences": {
      "manualEntry.entryGrid": {
        "density": "compact",
        "pageSize": 25,
        "sortColumn": "TransactionDate",
        "sortDescending": true
      }
    }
  }
  ```
  - Models: `UserPreferences` (root) → `EmailPreferences` (nested) + `GridPreference` (dictionary), all in `Domain/Models/`
  - **DarkMode** — `string` with values `"system"` (default), `"light"`, `"dark"`. Controls the app's color theme for authenticated pages only. `"system"` follows the OS `prefers-color-scheme` setting. Stored in `UserPreferences`, applied via `theme.js` on page load.
  - **GridPreferences** — `Dictionary<string, GridPreference>` keyed by `page.control` name. Each grid/widget saves its own density, page size, sort column, and sort direction. Keys use dot notation: `"manualEntry.entryGrid"`, `"dashboard.revenueGrid"`, etc. Future widget types (charts, filters) will get their own typed dictionaries.
  - `unsubscribeAll` is a master kill switch — if `true`, no emails are sent regardless of individual settings
  - Individual toggles default to `true` (opt-out model). Adding a new email type = new `bool` property with `true` default.
  - Welcome email is excluded — it's a one-time transactional email, not a recurring subscription.

**`auth.UserAuthenticationGoogle`** — 1-to-1 with User (shared PK)
- `UserId` (PK/FK → User, C# property: `Id`), `GoogleId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`, `AvatarUrl`, `EmailVerified`, `Locale`

**`auth.UserAuthenticationApple`** — 1-to-1 with User (shared PK)
- `UserId` (PK/FK → User, C# property: `Id`), `AppleId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`, `IsPrivateRelay`

**`auth.UserAuthenticationMicrosoft`** — 1-to-1 with User (shared PK)
- `UserId` (PK/FK → User, C# property: `Id`), `MicrosoftId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`

**`auth.UserRole`** — role assignments (1-to-many with User)
- `Id` (PK), `UserId` (FK → User), `Role` (string, max 50)
- Unique constraint on `(UserId, Role)` prevents duplicate assignments
- Role values are code constants defined in `Domain/Framework/Roles.cs` (no lookup table)
- Current roles: `Admin`, `User`
- Self-healing: if a user signs in with no roles, the `User` role is automatically assigned

### Schema: `commerce`

**`commerce.ProductVendor`** — who sells the product (e.g., "MyTalli", "Some Other Online Site")
- `Id` (PK), `VendorName`

**`commerce.ProductType`** — category of product (e.g., "Software Subscription", "Software Module")
- `Id` (PK), `ProductTypeName`

**`commerce.Product`** — a specific thing for sale (e.g., "12-Month Pro Subscription" at $12)
- `Id` (PK), `VendorId` (FK → ProductVendor), `ProductTypeId` (FK → ProductType), `ProductName`, `VendorPrice`

**`commerce.Order`** — a user's specific purchase event
- `Id` (PK), `UserId` (FK → auth.User), `OrderDateTime`, `TaxCharged`

**`commerce.OrderItem`** — line items within an order (junction table: Order ↔ Product)
- `Id` (PK), `OrderId` (FK → Order), `ProductId` (FK → Product), `ProductPriceCharged`, `ProductQuantity`

**`commerce.Subscription`** — ongoing state of a user's subscription (instanceOf — "what we currently have")
- `Id` (PK), `UserId` (FK → auth.User), `ProductId` (FK → Product), `OrderItemId` (FK → OrderItem), `Status`, `StartDate`, `EndDate`, `RenewalDate`, `CancelledDate`
- `ProductId` answers "which product does this subscription track?"
- `OrderItemId` answers "which order supports this subscription?"

**`commerce.SubscriptionStripe`** — Stripe-specific subscription data (1-to-1 with Subscription, shared PK)
- `SubscriptionId` (PK/FK → Subscription, C# property: `Id`), `StripeCustomerId`, `StripeSubscriptionId`, `StripePriceId`

**`commerce.Billing`** — a payment event tied to an order
- `Id` (PK), `UserId` (FK → auth.User), `OrderId` (FK → Order), `Amount`, `Currency`, `Status`
- `OrderId` answers "which billing satisfied this order?"

**`commerce.BillingStripe`** — Stripe-specific payment data (1-to-1 with Billing, shared PK)
- `BillingId` (PK/FK → Billing, C# property: `Id`), `StripePaymentIntentId`, `PaymentMethod`, `CardBrand`, `CardLastFour`

### Duplicate Prevention

When a user signs in with a new provider but uses an **email that already exists** on another provider's auth table, the app must detect this and link the new provider to the **existing** User record instead of creating a duplicate. This is automatic — the user is the same person, same email, just a different sign-in method.

- **Detection:** During sign-in, query all provider auth tables for the incoming email address
- **Match found:** Create the new provider auth row pointing at the existing User (no new User record). Update `LastLoginAt`.
- **No match:** Create a new User + provider auth row as normal (new account)

This prevents the "same person, same email, two accounts" problem.

### Account Consolidation

A user may have **different emails** on different providers (e.g., `robertmerrilljordan@gmail.com` on Google, `hello@mytalli.com` on Microsoft). These correctly create separate User records — the app has no way to know they're the same person.

Account consolidation is a **user-initiated** action where someone chooses to merge two accounts they own:

1. User signs in with Google (`gmail`) → `auth.User` #1 + `auth.UserAuthenticationGoogle` created
2. Later signs in with Microsoft (`mytalli.com`) → `auth.User` #2 + `auth.UserAuthenticationMicrosoft` created
3. User realizes they want one account and triggers consolidation
4. Consolidation moves the Microsoft auth row to point at User #1, migrates any data, deletes the orphaned User #2
5. User can now sign in with either provider and land on the same account

The consolidation process is not yet implemented — the schema supports it, the UX flow will be designed later.

### Naming Conventions

- **Primary keys:** `PK_{TableName}` (e.g., `PK_User`, `PK_Order`)
- **Foreign keys:** `FK_{ChildTable}_{ParentTable}` (e.g., `FK_Order_User`, `FK_Subscription_Product`)
- **Unique constraints:** `UQ_{TableName}_{ColumnName}` (e.g., `UQ_UserAuthGoogle_UserId`)
- **Indexes:** `IX_{TableName}_{ColumnName}` (e.g., `IX_Order_UserId`)
- Every FK column has a non-clustered index for JOIN performance
- **Views:** `v{AdjectiveNoun}` (e.g., `vAuthenticatedUser`, not `vUserAuthenticated`) — adjective before noun, matching class naming style

### Migration SQL Scripts

All migrations inherit from **`DbMigrationBase`** (`Migrations/DbMigrationBase.cs`) instead of `Migration` directly. The base class automatically discovers and executes embedded `.sql` files organized in versioned subfolders.

**How it works:**
1. Each migration declares a `MigrationFolder` (e.g., `"01_0"`)
2. The base class `Up()` runs: Pre-Deployment Scripts → `UpTables()` → Post-Deployment Scripts → Functions → Views → Stored Procedures → Triggers → Assemblies
3. Each subfolder is scanned for embedded `.sql` resources; if none exist, it's silently skipped
4. Scripts within each subfolder execute in alphabetical order (use numeric prefixes to control order)

**Concrete migrations override `UpTables()`/`DownTables()`** (not `Up()`/`Down()`) — the EF-generated table/index code goes there.

**Folder convention:**
```
Migrations/
├── DbMigrationBase.cs
├── {version}/                      # e.g., 01_0, 02_0
│   ├── Pre-Deployment Scripts/     # Run before table changes
│   ├── Post-Deployment Scripts/    # Run after table changes (seed data, etc.)
│   ├── Functions/                  # Scalar & table-valued functions
│   ├── Views/                      # SQL views
│   ├── Stored Procedures/          # Stored procedures
│   ├── Triggers/                   # Triggers
│   └── Assemblies/                 # CLR assemblies
```

**SQL file naming:** `{##}.{schema}.{objectName}.sql` — e.g., `00.auth.vAuthenticatedUser.sql`. The numeric prefix controls execution order within the subfolder.

**`.csproj` setup:** A `Migrations\**\*.sql` glob automatically embeds all SQL files as resources — no per-file entries needed.

**`GO` batch splitting:** SQL scripts may contain `GO` batch separators (required for DDL like `CREATE VIEW`, `CREATE PROCEDURE`). `DbMigrationBase` splits on `GO` lines and executes each batch as a separate `migrationBuilder.Sql()` call, since EF Core does not natively support `GO`.

**Note:** .NET prepends `_` to resource names for folders starting with a digit (`01_0` → `_01_0`) and replaces hyphens with underscores (`Post-Deployment Scripts` → `Post_Deployment_Scripts`). `DbMigrationBase` handles both transformations automatically.

## Solution Structure

```
My.Talli/
├── .secrets                        # Local secrets file (git-ignored) — SWA deploy token
├── CLAUDE.md
├── mytalli-logo.png                # Brand logo (transparent bg)
├── mytalli-logo-white-bg.png       # Brand logo (white bg)
├── og-image.png                    # Social share image (1200×630) — source copy
├── setup-iis.ps1                   # IIS setup script for local dev
├── documentation/                  # Internal planning & reference documents
│   ├── cost-report/                # Skill — branded financial/costing HTML document builder
│   │   └── SKILL.md
│   ├── scaling-plan/               # Skill — branded scaling/capacity planning HTML document builder
│   │   └── SKILL.md
│   ├── MyTalli_CostingPlan.html    # Infrastructure cost projections & optimization strategies
│   ├── MyTalli_Kanban.html         # Active work kanban — backlog, next up, in progress, done
│   ├── MyTalli_PlatformCapabilities.html # Platform API capabilities, data richness & integration roadmap
│   ├── MyTalli_ScalingPlan.html    # Scaling strategy as user base grows (tiers, triggers, capacity)
│   ├── MyTalli_ShopConnectionERD.html # ERD for ShopConnection + ShopConnectionEtsy sync-target layer
│   ├── MyTalli_SyncScalingPlan.html # Platform API rate-limit strategy, daily-baseline sync, 3-phase plan
│   └── PlatformApiDataShapes.html  # Platform API data shapes, normalized schema, ERD (historical — pre-ShopConnection)
├── deploy/                         # Azure SWA deploy folder (static HTML era)
│   ├── index.html                  # Copied from wireframes/MyTalli_LandingPage.html
│   ├── favicon.svg                 # Copied from favicon-concepts/favicon-c-growth.svg
│   ├── og-image.png                # Social share image
│   ├── robots.txt                  # Allows all crawlers, references sitemap
│   ├── sitemap.xml                 # Site map for search engines
│   └── emails/                     # Hosted email assets (PNG images for email templates)
│       ├── email-hero-bg.svg       # Landing Hero background source SVG (600×320)
│       ├── email-hero-bg.png       # Landing Hero background PNG (rendered from SVG)
│       ├── email-icon-graph.svg    # Bar graph icon source SVG (40×40)
│       └── email-icon-graph.png    # Bar graph icon PNG (rendered at 80×80 for retina)
├── favicon-concepts/               # Favicon & OG image design assets
│   ├── favicon-a-lettermark.svg    # Concept A — bold T lettermark
│   ├── favicon-b-tally.svg         # Concept B — tally marks
│   ├── favicon-c-growth.svg        # Concept C — T + growth bars (CHOSEN)
│   ├── favicon-d-tgraph.svg        # Concept D — T with graph behind
│   ├── og-image-capture.html       # Viewport-locked page for PNG capture
│   ├── og-image-mockup.html        # OG image design mockup (1200×630)
│   └── preview.html                # Side-by-side favicon comparison page
├── social-assets/                  # Social media images & source HTML
│   └── linkedin-cover.html         # LinkedIn cover banner source (1584×792)
├── wireframes/                     # Standalone HTML mockups & design concepts
│   ├── MyTalli_ColorPalette.html   # Brand color reference sheet (light mode)
│   ├── MyTalli_DarkModePalette.html # Brand color reference sheet (dark mode)
│   ├── MyTalli_Dashboard.html      # Static dashboard mockup (post-login)
│   ├── MyTalli_LandingPage.html    # Static landing page mockup
│   ├── MyTalli_Email_Welcome.html  # Welcome email wireframe (PNG-based hero)
│   ├── MyTalli_Email_SubscriptionConfirmation.html # Subscription confirmation email wireframe
│   ├── MyTalli_Email_WeeklySummary.html # Weekly summary email wireframe
│   ├── MyTalli_SuggestionBoxConcepts.html # Suggestion box design concepts (A/B/C)
│   ├── MyTalli_SuggestionCardConcepts.html # Suggestion card layout concepts (admin notes, status tags)
│   ├── MyTalli_WaitlistConcepts.html # Waitlist page design concepts (A/B/C)
│   ├── MyTalli_MobilePatterns_Dashboard_plus_Tabs.html # Mobile wireframes for Dashboard+Tabs nav pattern (3 treatments)
│   ├── MyTalli_MobilePatterns_Hub_and_Spoke.html # Mobile wireframes for Hub & Spoke nav pattern (3 treatments)
│   ├── MyTalli_MobilePatterns_Keyhole_Hybrid.html # Mobile wireframe — chosen pattern (Hub & Spoke + Keyhole Hybrid)
│   └── MyTalli_NavigationPatterns.html # Navigation IA wireframes — 4 patterns for grid/data organization
└── Source/
    ├── My.Talli.slnx               # Solution file (XML-based .slnx format)
    ├── .claude/settings.local.json
    ├── Domain/                      # Domain layer (exceptions, shared types, framework)
    │   ├── Domain.csproj
    │   ├── .resources/
    │   │   └── emails/              # HTML email templates (EmbeddedResource)
    │   │       ├── ExceptionOccurredEmailNotificationTemplate.html
    │   │       ├── SubscriptionConfirmationEmailNotificationTemplate.html
    │   │       ├── WelcomeEmailNotificationTemplate.html
    │   │       └── WeeklySummaryEmailNotificationTemplate.html
    │   ├── Exceptions/
    │   │   ├── TalliException.cs              # Abstract base (HttpStatusCode property)
    │   │   ├── ForbiddenException.cs          # 403
    │   │   ├── DatabaseConnectionFailedException.cs  # 403 (inherits Forbidden)
    │   │   ├── NotFoundException.cs           # 404
    │   │   ├── UnauthorizedException.cs       # 401
    │   │   ├── SignInFailedException.cs        # 401 (inherits Unauthorized)
    │   │   └── UnexpectedException.cs         # 500
    │   ├── .extensions/
    │   │   └── AssemblyExtensions.cs          # GetManifestResourceContent() for embedded resources
    │   ├── Framework/
    │   │   ├── Assert.cs                      # Static validation utility (precondition checks)
    │   │   ├── EnforcedTransactionScope.cs    # Atomic transaction wrapper (sync + async, rethrows after rollback)
    │   │   ├── Roles.cs                       # Static role name constants (Admin, User)
    │   │   └── SubscriptionStatuses.cs        # Static subscription status constants (Active, Cancelling, Cancelled, PastDue, Unpaid)
    │   ├── Components/
    │   │   ├── Etsy/                          # Etsy OAuth + API POCOs (shared with Web's EtsyService)
    │   │   │   ├── AuthorizeChallenge.cs       # PKCE challenge + state + authorize URL
    │   │   │   ├── EtsyPkceGenerator.cs        # Static helpers: BuildAuthorizeChallenge, ExtractEtsyUserId
    │   │   │   ├── EtsyShop.cs                 # Etsy shop payload (shop_id, shop_name, currency, etc.)
    │   │   │   └── EtsyTokenResponse.cs        # OAuth token exchange response (access_token, refresh_token, expires_in)
    │   │   ├── JsonSerializers/
    │   │   │   └── User/
    │   │   │       └── UserPreferencesJsonSerializer.cs  # Serialize/deserialize UserPreferences JSON
    │   │   └── Tokens/
    │   │       └── UnsubscribeTokenService.cs  # HMAC-SHA256 token generate/validate for email unsubscribe links
    │   ├── CommandsAndQueries/                # CQRS umbrella (Commands now; Queries in the future). Organizational — does NOT affect namespace.
    │   │   └── Commands/
    │   │       ├── Billing/                    # namespace: My.Talli.Domain.Commands.Billing
    │   │       │   ├── FindActiveSubscriptionWithStripeCommand.cs  # Query active subscription + Stripe record
    │   │       │   └── UpdateLocalSubscriptionCommand.cs           # Sync local DB after plan switch
    │   │       └── Platforms/                  # namespace: My.Talli.Domain.Commands.Platforms
    │   │           └── ConnectEtsyCommand.cs   # Upsert PlatformConnection + ShopConnection + ShopConnectionEtsy after OAuth
    │   ├── Mappers/
    │   │   ├── EntityMapper.cs                 # Abstract mapper (collection methods via LINQ)
    │   │   ├── IEntityMapper.cs               # Generic entity↔model mapper interface
    │   │   └── Entity/                        # Concrete mappers (one per entity/model pair)
    │   │       ├── BillingMapper.cs
    │   │       ├── BillingStripeMapper.cs
    │   │       ├── OrderItemMapper.cs
    │   │       ├── OrderMapper.cs
    │   │       ├── ProductMapper.cs
    │   │       ├── ProductTypeMapper.cs
    │   │       ├── ProductVendorMapper.cs
    │   │       ├── SubscriptionMapper.cs
    │   │       ├── SubscriptionStripeMapper.cs
    │   │       ├── SuggestionMapper.cs
    │   │       ├── SuggestionVoteMapper.cs
    │   │       ├── UserAuthenticationAppleMapper.cs
    │   │       ├── UserAuthenticationGoogleMapper.cs
    │   │       ├── UserAuthenticationMicrosoftMapper.cs
    │   │       ├── UserMapper.cs
    │   │       └── UserRoleMapper.cs
    │   ├── Models/
    │   │   ├── ActionResponseOf.cs            # Generic response wrapper (ValidationResult + Payload)
    │   │   ├── EmailPreferences.cs            # Email opt-in/out preferences model
    │   │   ├── GridPreference.cs              # Per-widget grid preferences (density, pageSize, sort)
    │   │   ├── UserPreferences.cs             # Root user preferences model (wraps EmailPreferences, GridPreferences)
    │   │   ├── DefaultModel.cs                # Standard model base (Id + IsDeleted + IsVisible)
    │   │   ├── ValidationResult.cs            # Abstract base (IsValid, ValidationSummary, WarningSummary)
    │   │   ├── Entity/                        # 1-to-1 entity representations (no audit fields, no nav properties)
    │   │   │   ├── Billing.cs
    │   │   │   ├── BillingStripe.cs
    │   │   │   ├── Order.cs
    │   │   │   ├── OrderItem.cs
    │   │   │   ├── Product.cs
    │   │   │   ├── ProductType.cs
    │   │   │   ├── ProductVendor.cs
    │   │   │   ├── Subscription.cs
    │   │   │   ├── SubscriptionStripe.cs
    │   │   │   ├── Suggestion.cs
    │   │   │   ├── SuggestionVote.cs
    │   │   │   ├── User.cs
    │   │   │   ├── UserAuthenticationApple.cs
    │   │   │   ├── UserAuthenticationGoogle.cs
    │   │   │   ├── UserAuthenticationMicrosoft.cs
    │   │   │   └── UserRole.cs
    │   │   └── Presentation/                  # Aggregate/detail view models
    │   │       └── AdminUserListItem.cs       # Admin user list with email, provider, subscription status
    │   ├── Handlers/
    │   │   ├── Authentication/                # Sign-in handlers (one per OAuth provider)
    │   │   │   ├── EmailLookupService.cs       # Cross-provider email lookup for duplicate prevention
    │   │       ├── SignInArgument.cs           # Base sign-in argument
    │   │       ├── SignInArgumentOf.cs         # Generic sign-in argument with provider payload
    │   │       ├── Apple/
    │   │       │   ├── AppleSignInHandler.cs
    │   │       │   └── AppleSignInPayload.cs
    │   │       ├── Google/
    │   │       │   ├── GoogleSignInHandler.cs
    │   │       │   └── GoogleSignInPayload.cs
    │   │       └── Microsoft/
    │   │           ├── MicrosoftSignInHandler.cs
    │   │           └── MicrosoftSignInPayload.cs
    │   │   └── Billing/                       # Stripe webhook handlers
    │   │       ├── CheckoutCompletedPayload.cs
    │   │       ├── CheckoutCompletedResult.cs
    │   │       ├── StripeWebhookHandler.cs     # Handles checkout.session.completed, subscription.updated/deleted
    │   │       ├── SubscriptionDeletedPayload.cs
    │   │       └── SubscriptionUpdatedPayload.cs
    │   ├── Repositories/
    │   │   └── RepositoryAdapterAsync.cs      # Model↔Entity adapter (only gateway to data layer)
    │   └── Notifications/
    │       └── Emails/
    │           ├── EmailNotification.cs               # Abstract base (FinalizeEmail → SmtpNotification)
    │           ├── EmailNotificationOf.cs             # Generic abstract with Build() method
    │           ├── EmailNotificationArgument.cs        # Base argument class
    │           ├── EmailNotificationArgumentOf.cs      # Generic argument with Payload
    │           ├── SmtpNotification.cs                # Serializable POCO carrier
    │           ├── Customer/
    │           │   ├── SubscriptionConfirmationEmailNotification.cs
    │           │   ├── SubscriptionConfirmationEmailNotificationPayload.cs
    │           │   ├── WelcomeEmailNotification.cs
    │           │   ├── WelcomeEmailNotificationPayload.cs
    │           │   ├── WeeklySummaryEmailNotification.cs
    │           │   └── WeeklySummaryEmailNotificationPayload.cs
    │           └── Exceptions/
    │               ├── ExceptionOccurredEmailNotification.cs
    │               └── ExceptionOccurredEmailNotificationPayload.cs
    ├── Domain.Data/                 # Data access abstractions (ORM-agnostic)
    │   ├── Domain.Data.csproj
    │   └── Interfaces/
    │       ├── IAuditableRepositoryAsync.cs # Repository + audit resolution interface (async)
    │       ├── IAuditResolver.cs          # Audit field stamping interface
    │       ├── ICurrentUserService.cs     # Current user identity interface
    │       └── IRepositoryAsync.cs        # Generic repository interface (async)
    ├── Domain.Data.EntityFramework/  # EF Core implementation of data access
    │   ├── Domain.Data.EntityFramework.csproj
    │   ├── TalliDbContext.cs              # DbContext with all DbSets
    │   ├── Migrations/                    # EF Core code-first migrations
    │   │   ├── DbMigrationBase.cs           # Abstract migration base (embedded SQL script execution)
    │   │   ├── 01_0/                        # SQL scripts for InitialCreate migration
    │   │   │   ├── Pre-Deployment Scripts/
    │   │   │   │   └── 00.dbo.MyTalli-User.sql  # App database user creation (least privilege)
    │   │   │   ├── Post-Deployment Scripts/
    │   │   │   │   └── 00.components.ELMAH_Error.sql
    │   │   │   └── Views/
    │   │   │       └── 00.auth.vAuthenticatedUser.sql
    │   │   └── 02_0/                        # SQL scripts for AddMilestone migration
    │   │       └── Post-Deployment Scripts/
    │   │           └── 00.app.Milestone.sql  # Seed milestone data (17 rows)
    │   ├── Repositories/
    │   │   ├── GenericRepositoryAsync.cs  # IRepositoryAsync<T> implementation
    │   │   └── GenericAuditableRepositoryAsync.cs # IAuditableRepositoryAsync<T> implementation
    │   ├── Resolvers/
    │   │   └── AuditResolver.cs           # IAuditResolver<T> implementation
    │   └── Configurations/
    │       ├── App/                       # Entity configs for app schema
    │       │   ├── RevenueConfiguration.cs
    │       │   ├── RevenueManualConfiguration.cs
    │       │   ├── SuggestionConfiguration.cs
    │       │   └── SuggestionVoteConfiguration.cs
    │       ├── Auth/                      # Entity configs for auth schema
    │       │   ├── AuthenticatedUserConfiguration.cs  # Keyless entity config for vAuthenticatedUser view
    │       │   ├── UserConfiguration.cs
    │       │   ├── UserAuthenticationAppleConfiguration.cs
    │       │   ├── UserAuthenticationGoogleConfiguration.cs
    │       │   ├── UserAuthenticationMicrosoftConfiguration.cs
    │       │   └── UserRoleConfiguration.cs
    │       └── Commerce/                  # Entity configs for commerce schema
    │           ├── BillingConfiguration.cs
    │           ├── BillingStripeConfiguration.cs
    │           ├── OrderConfiguration.cs
    │           ├── OrderItemConfiguration.cs
    │           ├── ProductConfiguration.cs
    │           ├── ProductTypeConfiguration.cs
    │           ├── ProductVendorConfiguration.cs
    │           ├── SubscriptionConfiguration.cs
    │           └── SubscriptionStripeConfiguration.cs
    ├── Domain.DI.Lamar/              # Lamar IoC container registration (isolated from web layer)
    │   ├── Domain.DI.Lamar.csproj
    │   └── IoC/
    │       └── ContainerRegistry.cs       # Lamar ServiceRegistry — registers all mappers, repositories, handlers
    ├── Domain.Entities/             # Domain entity layer (database models)
    │   ├── Domain.Entities.csproj
    │   ├── AuditableIdentifiableEntity.cs  # Base class (Id + audit fields)
    │   ├── DefaultEntity.cs                # Standard entity base (adds IsDeleted, IsVisible)
    │   ├── Entities/
    │   │   ├── AuthenticatedUser.cs         # Keyless entity mapped to auth.vAuthenticatedUser view
    │   │   ├── Billing.cs
    │   │   ├── BillingStripe.cs
    │   │   ├── Order.cs
    │   │   ├── OrderItem.cs
    │   │   ├── Product.cs
    │   │   ├── ProductType.cs
    │   │   ├── ProductVendor.cs
    │   │   ├── Subscription.cs
    │   │   ├── SubscriptionStripe.cs
    │   │   ├── Suggestion.cs
    │   │   ├── SuggestionVote.cs
    │   │   ├── User.cs
    │   │   ├── UserAuthenticationApple.cs
    │   │   ├── UserAuthenticationGoogle.cs
    │   │   ├── UserAuthenticationMicrosoft.cs
    │   │   └── UserRole.cs
    │   └── Interfaces/
    │       ├── IAuditable.cs
    │       ├── IAuditableIdentifiable.cs
    │       └── IIdentifiable.cs
    ├── My.Talli.UnitTesting/        # xUnit unit test project
    │   ├── My.Talli.UnitTesting.csproj
    │   ├── Commands/
    │   │   └── Platforms/
    │   │       └── ConnectEtsyCommandTests.cs      # Upsert behavior, multi-shop, null-field handling
    │   ├── Components/
    │   │   ├── Etsy/
    │   │   │   └── EtsyPkceGeneratorTests.cs       # PKCE challenge format, SHA256 invariant, ExtractEtsyUserId edge cases
    │   │   ├── JsonSerializers/
    │   │   │   └── UserPreferencesJsonSerializerTests.cs
    │   │   └── Tokens/
    │   │       └── UnsubscribeTokenServiceTests.cs
    │   ├── Framework/
    │   │   └── AssertTests.cs
    │   ├── Handlers/
    │   │   └── Authentication/
    │   │       ├── AppleSignInHandlerTests.cs
    │   │       ├── EmailLookupServiceTests.cs
    │   │       ├── GoogleSignInHandlerTests.cs
    │   │       ├── MicrosoftSignInHandlerTests.cs
    │   │       └── SignInScenarioTests.cs
    │   ├── Infrastructure/
    │   │   ├── Builders/
    │   │   │   ├── BillingHandlerBuilder.cs        # Test setup for Stripe webhook handler + related adapters
    │   │   │   ├── PlatformHandlerBuilder.cs       # Test setup for ConnectEtsyCommand + PlatformConnection/ShopConnection adapters
    │   │   │   └── SignInHandlerBuilder.cs         # Test setup orchestrator (Lamar container, exposes handlers & adapters)
    │   │   ├── IoC/
    │   │   │   └── ContainerRegistry.cs        # Test IoC registry (extends Domain.DI.Lamar, swaps in stubs)
    │   │   └── Stubs/
    │   │       ├── AuditableRepositoryStub.cs  # In-memory IAuditableRepositoryAsync<T> for tests
    │   │       ├── AuditResolverStub.cs
    │   │       ├── CurrentUserServiceStub.cs
    │   │       └── IdentityProvider.cs         # Auto-incrementing ID generator for test entities
    │   └── Notifications/
    │       └── Emails/
    │           ├── SubscriptionConfirmationEmailNotificationTests.cs
    │           ├── WeeklySummaryEmailNotificationTests.cs
    │           └── WelcomeEmailNotificationTests.cs
    └── My.Talli.Web/               # Blazor Server web project
        ├── My.Talli.Web.csproj
        ├── Program.cs              # App entry point, pipeline setup (delegates to Configuration/ and Endpoints/)
        ├── Configuration/             # Service registration extension methods (one per concern)
        │   ├── AdminConfiguration.cs          # Admin commands registration
        │   ├── AuthenticationConfiguration.cs  # OAuth providers (Google, Microsoft, Apple) + auth handlers
        │   ├── BillingConfiguration.cs         # Stripe settings + service
        │   ├── DatabaseConfiguration.cs        # DbContext registration
        │   ├── ElmahConfiguration.cs           # Elmah error logging
        │   ├── EmailConfiguration.cs           # Email services + unsubscribe token
        │   ├── PlatformsConfiguration.cs       # Etsy (and future platform) settings + HttpClient wiring
        │   └── RepositoryConfiguration.cs      # ICurrentUserService registration (mappers, handlers, and repositories are in Domain.DI.Lamar)
        ├── Endpoints/                 # Minimal API endpoint extension methods (one per route group)
        │   ├── AdminEndpoints.cs      # /api/admin/email/* (resend, bulk-welcome, bulk-welcome-all)
        │   ├── AuthEndpoints.cs       # /api/auth/login, /api/auth/logout
        │   ├── BillingEndpoints.cs    # /api/billing/create-checkout-session, portal, switch-plan, webhook
        │   ├── EmailEndpoints.cs      # /api/email/preferences
        │   ├── PlatformEndpoints.cs   # /api/platforms/etsy/connect, /api/platforms/etsy/callback (PKCE, data-protected cookie)
        │   └── TestEndpoints.cs       # /api/test/* (dev-only)
        ├── Handlers/                  # Web-layer handlers (react to events, orchestrate domain calls)
        │   ├── Authentication/        # OAuth ticket handlers (map claims → domain sign-in → add claims → welcome email)
        │   │   ├── AppleAuthenticationHandler.cs
        │   │   ├── GoogleAuthenticationHandler.cs
        │   │   └── MicrosoftAuthenticationHandler.cs
        │   └── Endpoints/             # Handlers that serve endpoint routes
        │       ├── CheckoutCompletedHandler.cs    # Stripe checkout.session.completed → domain handler + email
        │       ├── SubscriptionDeletedHandler.cs  # Stripe customer.subscription.deleted → domain handler
        │       └── SubscriptionUpdatedHandler.cs  # Stripe customer.subscription.updated → domain handler
        ├── Commands/                  # Web-layer commands (execute actions that require Web-layer deps)
        │   ├── Notifications/         # Email and notification commands (depend on IEmailService)
        │   │   ├── SendSubscriptionConfirmationEmailCommand.cs # Build + send subscription confirmation email
        │   │   ├── SendWelcomeEmailCommand.cs                  # Build + send welcome email
        │   │   └── SendWeeklySummaryEmailCommand.cs            # Build + send weekly summary email (sample data)
        │   └── Endpoints/             # Commands that need Web-layer primitives (DbContext direct, HttpContext, etc.)
        │       └── GetAdminUserListCommand.cs                  # Direct TalliDbContext access for the vAuthenticatedUser view
        ├── Middleware/                 # Custom middleware classes
        │   ├── CurrentUserMiddleware.cs   # Populates ICurrentUserService from HttpContext.User claims on every request
        │   └── ProbeFilterMiddleware.cs  # Bot/scanner probe filter (short-circuits .env, .php, wp-admin, etc.)
        ├── Components/
        │   ├── App.razor           # Root HTML document
        │   ├── Routes.razor        # Routing setup
        │   ├── _Imports.razor      # Global usings
        │   ├── Layout/
        │   │   ├── LandingLayout.razor   # Minimal layout (no sidebar)
        │   │   ├── MainLayout.razor      # Sidebar + content layout shell
        │   │   ├── MainLayout.razor.css
        │   │   ├── NavMenu.razor         # Sidebar navigation (brand styled)
        │   │   └── NavMenu.razor.css
        │   ├── Pages/
        │   │   ├── Admin.razor           # Admin page (route: /admin, Admin role only)
        │   │   ├── Admin.razor.css
        │   │   ├── CancelSubscription.razor  # Cancel subscription retention page (route: /subscription/cancel)
        │   │   ├── CancelSubscription.razor.css
        │   │   ├── Dashboard.razor       # Dashboard (route: /dashboard)
        │   │   ├── Dashboard.razor.css
        │   │   ├── Goals.razor           # Revenue goals (route: /goals)
        │   │   ├── Goals.razor.css
        │   │   ├── LandingPage.razor     # Landing page (route: /)
        │   │   ├── LandingPage.razor.css
        │   │   ├── ManualEntry.razor       # Manual entry module (route: /manual-entry)
        │   │   ├── ManualEntry.razor.css
        │   │   ├── MyPlan.razor          # Consolidated plan & module management (route: /my-plan)
        │   │   ├── MyPlan.razor.css
        │   │   ├── Platforms.razor       # Platform connections (route: /platforms)
        │   │   ├── Platforms.razor.css
        │   │   ├── Settings.razor        # Account settings (route: /settings)
        │   │   ├── Settings.razor.css
        │   │   ├── SignIn.razor          # Sign-in page (route: /signin)
        │   │   ├── SignIn.razor.css
        │   │   ├── SuggestionBox.razor       # Suggestion box (route: /suggestions)
        │   │   ├── SuggestionBox.razor.css
        │   │   ├── Unsubscribe.razor      # Email preference management (route: /unsubscribe?token=xxx)
        │   │   ├── Unsubscribe.razor.css
        │   │   ├── Error.razor           # Branded error page (routes: /Error, /Error/{StatusCode})
        │   │   └── Error.razor.css
        │   └── Shared/
        │       ├── BrandHeader.razor     # Reusable purple swoosh header (logo + action slot)
        │       ├── BrandHeader.razor.css
        │       ├── ConfirmDialog.razor       # Reusable Yes/No confirmation dialog (danger/primary variants)
        │       └── ConfirmDialog.razor.css
        ├── Helpers/
        │   └── LayoutHelper.cs            # Static helpers (CurrentYear, VersionNumber) for layouts
        ├── Services/
        │   ├── Billing/
        │   │   ├── StripeBillingService.cs  # Stripe Checkout, Portal, & plan switch API wrapper
        │   │   └── StripeSettings.cs        # Stripe configuration POCO
        │   ├── Identity/
        │   │   ├── CurrentUserService.cs    # ICurrentUserService implementation (scoped, set by CurrentUserMiddleware)
        │   │   └── UserDisplayCache.cs      # Scoped cache — serializes DB access for user display info across concurrent Blazor components
        │   ├── Email/
        │   │   ├── EmailSettings.cs             # SMTP config POCO (IOptions<EmailSettings>)
        │   │   ├── ExceptionEmailHandler.cs     # IExceptionHandler — sends email, returns false
        │   │   ├── IEmailService.cs             # Email sending interface
        │   │   ├── AcsEmailService.cs           # Azure Communication Services implementation (active)
        │   │   └── SmtpEmailService.cs          # MailKit-based implementation (local dev fallback)
        │   ├── Platforms/
        │   │   ├── EtsySettings.cs              # Etsy OAuth config POCO (ClientId, ClientSecret, RedirectUri, Scope)
        │   │   └── EtsyService.cs               # Thin HTTP wrapper — token exchange + shop fetch. Uses Domain.Components.Etsy helpers.
        │   └── Tokens/
        │       └── UnsubscribeTokenSettings.cs  # Config POCO for unsubscribe token secret key
        ├── ViewModels/
        │   ├── Pages/
        │   │   ├── AdminViewModel.cs
        │   │   ├── CancelSubscriptionViewModel.cs
        │   │   ├── DashboardViewModel.cs
        │   │   ├── ErrorViewModel.cs
        │   │   ├── GoalsViewModel.cs
        │   │   ├── LandingPageViewModel.cs
        │   │   ├── ManualEntryViewModel.cs
        │   │   ├── MyPlanViewModel.cs
        │   │   ├── PlatformsViewModel.cs
        │   │   ├── SettingsViewModel.cs
        │   │   ├── SignInViewModel.cs
        │   │   ├── SuggestionBoxViewModel.cs
        │   │   └── UnsubscribeViewModel.cs
        │   └── Shared/
        │       ├── BrandHeaderViewModel.cs
        │       └── ConfirmDialogViewModel.cs
        ├── Properties/
        │   └── launchSettings.json
        ├── wwwroot/
        │   ├── app.css
        │   ├── js/
        │   │   ├── landing.js      # Landing page scroll & nav interactivity
        │   │   ├── mobile-menu.js  # Mobile hamburger menu toggle (CSS class-based, no Blazor interactivity)
        │   │   └── theme.js        # Dark mode — applies data-theme attribute, listens for OS preference changes
        │   └── lib/bootstrap/
        ├── appsettings.json
        └── appsettings.Development.json
```

### Solution Folders (in .slnx)

- `/Foundation/` — shared/core projects (`Domain`, `Domain.Data`, `Domain.Data.EntityFramework`, `Domain.DI.Lamar`, `Domain.Entities`)
- `/Presentation/` — contains `My.Talli.Web`
- `/Testing/` — contains `My.Talli.UnitTesting`

### Project Reference Chain

```
Domain.Entities          ← entity classes (no dependencies)
Domain.Data              ← abstractions (IRepository, IUnitOfWork) → Domain.Entities
Domain.Data.EntityFramework ← EF Core implementation (DbContext, configs) → Domain.Data, Domain.Entities
Domain                   ← exceptions, notifications → Domain.Data, Domain.Entities
Domain.DI.Lamar          ← IoC container registration → Domain, Domain.Data, Domain.Data.EntityFramework, Domain.Entities
My.Talli.Web             ← Blazor Server app → Domain, Domain.Data.EntityFramework, Domain.DI.Lamar
My.Talli.UnitTesting     ← xUnit tests → Domain, Domain.Data, Domain.DI.Lamar, Domain.Entities
```

## Brand & Design

> **Moved to memory:** `reference_brand_design.md` — color palettes (light/dark), dark mode architecture, platform connector colors, swoosh hero branding, font, theme approach. Source of truth files: `wireframes/MyTalli_ColorPalette.html` (light) and `wireframes/MyTalli_DarkModePalette.html` (dark).

## Development

### Build & Run

```bash
dotnet build Source/My.Talli.slnx
dotnet run --project Source/My.Talli.Web
```

### Dev URLs

- HTTPS: `https://localhost:7012`
- HTTP: `http://localhost:5034`

### Unit Testing

- **Framework:** xUnit (with coverlet for coverage)
- **Project:** `My.Talli.UnitTesting` (solution folder: `/Testing/`)
- **Run tests:**
  ```bash
  dotnet test Source/My.Talli.UnitTesting/My.Talli.UnitTesting.csproj
  ```
- **Test file location:** Mirror the source project folder structure (e.g., `Components/Tokens/UnsubscribeTokenServiceTests.cs` tests `Domain/Components/Tokens/UnsubscribeTokenService.cs`)
- **Test class naming:** `{ClassUnderTest}Tests.cs`
- **Test method naming:** `MethodName_Scenario_ExpectedBehavior`
- **What to test:** Logic that computes, transforms, validates, or can fail — cryptographic operations, serialization, precondition checks, business rules, sign-in handlers
- **What NOT to test:** Do not write tests for public property getters/setters or simple property-to-property mapping (e.g., mappers, POCO defaults). Only test properties that are set privately, through constructors, or via computed logic.
- **Domain Assert collision:** The Domain layer has its own `Assert` class (`Domain.Framework.Assert`). In test files that reference it, use a `DOMAINASSERT` alias to avoid collision with xUnit's `Assert`.
- **Test infrastructure** (`Infrastructure/`):
  - **`SignInHandlerBuilder`** (`Infrastructure/Builders/`) — orchestrates test setup with a Lamar container. Exposes sign-in handlers, repository adapters, and stub services as properties. All handler tests use this builder.
  - **`ContainerRegistry`** (`Infrastructure/IoC/`) — extends `Domain.DI.Lamar.IoC.ContainerRegistry` and overrides repository/audit registrations with in-memory stubs.
  - **`AuditableRepositoryStub<T>`** (`Infrastructure/Stubs/`) — in-memory `List<T>`-backed `IAuditableRepositoryAsync<T>` for fast, database-free testing. Supports Insert/Update/Delete with automatic ID generation and audit resolution.
  - **`IdentityProvider`** (`Infrastructure/Stubs/`) — maintains type-based counters for generating sequential IDs during tests.
  - **`CurrentUserServiceStub`** (`Infrastructure/Stubs/`) — mock `ICurrentUserService` with `Set()`/`Clear()` methods for test scenarios.
  - **`AuditResolverStub`** (`Infrastructure/Stubs/`) — no-op `IAuditResolver<T>` for tests.

### Version Number

- **`<Version>0.1.0.0</Version>`** in `My.Talli.Web.csproj` — single source of truth for the app version. Format: `Major.Minor.Patch.Revision`.
- **Revision number** — incremented with each fix deployment. Only the revision (4th segment) changes per fix. The version (`Major.Minor.Patch`) only changes for feature releases or breaking changes. The full 4-segment version is always displayed in the UI so deployment slots (staging vs production) can be visually distinguished.
- **`LayoutHelper.VersionNumber`** reads `AssemblyInformationalVersionAttribute` (set by `<Version>`) at runtime
- **`LayoutHelper.CurrentYear`** provides the current year for copyright footers
- **Landing Page** — version shown inline in footer: `© 2026 MyTalli v0.1.0.0 — All rights reserved.`
- **MainLayout pages** — version shown in a subtle `div.app-version` at the bottom of the content area
- **LandingLayout pages** (Sign-In, Error) — no version displayed

### Local Secrets

- **All local dev secrets live in `appsettings.Development.json`** — OAuth credentials, ACS connection strings, email settings, unsubscribe token keys, etc.
- **Do not use `dotnet user-secrets`** — keep one source of truth for local config.
- **Azure (production)** uses App Service Configuration (environment variables) for the same values.
- `appsettings.Development.json` is **not git-ignored** — this is acceptable for a side project with a single developer. If collaborators are added, secrets should move to `dotnet user-secrets` or a `.env` file.

## Infrastructure

> **Moved to memory:** `reference_infrastructure.md` — Azure hosting, domain/DNS, business entity (LLC), scaling, social media, analytics, SEO, accessibility notes.

## Authentication

- **No local passwords** — MyTalli does not store or manage usernames/passwords.
- **External providers only:** Google, Apple, Microsoft (via OAuth). Google and Microsoft are active. Apple is optional — the app starts without Apple credentials configured.
- **OAuth redirect URIs:** Each provider requires redirect URIs registered for every environment. Callback paths: `/signin-google`, `/signin-microsoft`, `/signin-apple`. Registered origins: `https://localhost:7012` (dev), `https://mytalli-web-f5b9f2a0h4cwdwa6.centralus-01.azurewebsites.net` (Azure), `https://www.mytalli.com` (production).
- **Google OAuth:** Managed in [Google Cloud Console](https://console.cloud.google.com) → APIs & Services → Credentials → OAuth 2.0 Client ID "MyTalli Web" (project: `mytalli`)
- **Apple OAuth:** Managed in [Apple Developer Portal](https://developer.apple.com/account) → Certificates, Identifiers & Profiles. Account: Robert Jordan. Team ID: `9T4K978XVF`.
  - **App ID:** `MyTalli` / `com.mytalli.web` — "Sign in with Apple" capability enabled
  - **Services ID:** `MyTalli Web` / `com.mytalli.web.auth` — this is the `ClientId` for web OAuth
  - **Registered domains:** `mytalli.com`, `mytalli-web-f5b9f2a0h4cwdwa6.centralus-01.azurewebsites.net`, `www.mytalli.com`
  - **Return URLs:** `https://mytalli.com/signin-apple`, `https://mytalli-web-f5b9f2a0h4cwdwa6.centralus-01.azurewebsites.net/signin-apple`, `https://www.mytalli.com/signin-apple`
  - **No localhost:** Apple requires TLS-verified domains — `localhost` cannot be registered. Apple Sign-In cannot be tested locally. The app handles this gracefully (conditional registration in `Program.cs`).
  - **Key:** `MyTalli Sign In` / Key ID `Z8J35PS4U6` — `.p8` file (`Apple.AuthKey_Z8J35PS4U6.p8`, git-ignored). Local dev uses `PrivateKeyPath` (file path); Azure uses `PrivateKeyContent` (key text as env var).
- **Microsoft OAuth:** Managed in Azure Portal → Microsoft Entra ID → App registrations → "My.Talli" (tenant: `MyTalli` / `mytalli.com`, account: `hello@mytalli.com`). Client ID: `bf93e9cf-78b4-4827-9ef5-71877e392f63`. Client secret description: `MyTalli-Microsoft-OAuth` (expires 2028-03-15, 24 months).
- **Cookie auth** with 30-day sliding expiration
- **Sign-in route:** `/signin` — provider selection page
- **Login endpoint:** `/api/auth/login/{provider}` — triggers OAuth challenge, redirects to `/dashboard` on success
- **Logout endpoint:** `/api/auth/logout` — clears cookie, redirects to `/?signed-out&name={name}`
- **Sign-out toast:** Landing page detects `?signed-out` query param and shows a personalized auto-dismissing toast ("You've been signed out, {name}. See you next time!"), then strips the query param from the URL via `history.replaceState`

## Authorization

- **Role-based** — roles are stored in `auth.UserRole` (junction table, 1-to-many with User) and added as `ClaimTypes.Role` claims during OAuth sign-in
- **Role constants** — defined in `Domain/Framework/Roles.cs` (no database lookup table). Current roles: `Admin`, `User`
- **Default role** — every new user gets the `User` role on sign-up. Existing users with no roles are self-healed on next sign-in.
- **Admin assignment** — no UI yet. Assign via direct database insert into `auth.UserRole`.
- **Claims flow** — domain sign-in handlers query `UserRole`, populate `User.Roles` on the model → web auth handlers map each role to a `ClaimTypes.Role` claim on the identity

## Billing

> **Moved to memory:** `reference_billing.md` — Stripe billing architecture, checkout, portal, webhooks, subscription statuses, CurrentUserMiddleware, local dev setup.

## App Mode

The app runs in **Dashboard Mode** — full app experience with all routes active. Sign-in takes users to the dashboard, sidebar navigation is functional.

- **Active routes:** All routes (`/dashboard`, `/suggestions`, `/my-plan`, `/manual-entry`, etc.)
- **OAuth redirect:** Set to `/dashboard` in the login endpoint (`Program.cs`)
- **Historical note:** The app previously operated in Waitlist Mode (landing page, sign-in, and waitlist only, all other routes redirected to `/waitlist`). Waitlist Mode and its associated code (page, view model, milestone display) have been removed. The branch `main_WAITLIST` is a frozen snapshot of `main` at the end of Waitlist Mode, preserved for historical reference.

## Error Handling

- **Error page routes:** `/Error` (unhandled exceptions) and `/Error/{StatusCode:int}` (HTTP status codes)
- **Layout:** Uses `LandingLayout` + `BrandHeader` with "Go Back" button
- **Static SSR:** No `@rendermode` — intentional so error pages work even when SignalR circuit fails
- **Middleware:** `UseExceptionHandler("/Error")` + `UseStatusCodePagesWithReExecute("/Error/{0}")` — both active in all environments
- **Router 404:** `Routes.razor` has a `<NotFound>` template that renders the Error component with `StatusCode="404"`
- **Exception hierarchy:** `TalliException` (abstract base) exposes `HttpStatusCode` property. Subclasses: `ForbiddenException` (403), `NotFoundException` (404), `UnauthorizedException` (401), `UnexpectedException` (500). Specific exceptions inherit from these (e.g., `DatabaseConnectionFailedException` → `ForbiddenException`, `SignInFailedException` → `UnauthorizedException`)
- **Status code resolution:** `ErrorViewModel` checks for `TalliException` via `IExceptionHandlerFeature` to extract the status code, falls back to `Response.StatusCode`, then 500
- **Request ID:** Only shown in Development when an actual exception was caught (not on status-code-only errors)
- **Probe filter middleware:** `ProbeFilterMiddleware` (`Middleware/ProbeFilterMiddleware.cs`), registered via `app.UseProbeFilter()` in `Program.cs`, positioned before `UseElmah()`. Short-circuits known bot/scanner paths (`.env`, `.php`, `wp-admin`, etc.) with a bare 404, OPTIONS requests with 204, and `/_blazor/disconnect` POST requests with 200 (expired circuits return 400, polluting Elmah). None of these reach Elmah, error pages, or Blazor routing.
- **Falling numbers animation:** Pure CSS `@keyframes` animation — 12 digits from the status code fall through the white space below the swoosh. Decorative only (`aria-hidden="true"`), no JS dependency so it works even when SignalR fails. Digits are generated by `ErrorViewModel.SetFallingDigits()`. Three alternating color/opacity tiers cycle via `nth-child(3n+...)`: **Bold** (`#6c5ce7`, peak 0.28 opacity), **Mid** (`#8b5cf6`, peak 0.18), **Soft** (`#a78bfa`, peak 0.10) — so some digits stand out more than others.

## Email Notifications

> **Moved to memory:** `reference_email_notifications.md` — email template architecture, ACS config, exception pipeline, unsubscribe tokens, branding tiers, how to add new emails.

## Platform API Notes

> **Moved to memory:** Platform API details (auth, endpoints, rate limits, webhooks, sync architecture) for all 5 platforms are in the `reference_platform_api_notes.md` memory file. Loaded on demand when working on platform integrations.

## Planned Features

- Real-time revenue tracking across connected platforms
- Trends & month-over-month comparisons
- CSV export for tax prep / bookkeeping
- Weekly email summaries (Pro tier)
- **Product Development Module** — future module for managing product campaigns/efforts. May eventually support collaboration (inviting people into a campaign to handle specific tasks). **Teams are explicitly deferred** — build the module single-user first, then let real usage patterns define what collaboration looks like. The current schema (everything scoped to `UserId`, provider-separation pattern) is friendly to adding `TeamId` later without reworking existing tables.

## Pricing Model

- **Free:** 1 connected platform, basic dashboard, 30-day history
- **Pro ($12/mo or $99/year):** Unlimited platforms, full history, goals, weekly emails, CSV export

## Rules

### Task Completion

- Before declaring a task complete, verify all Rules in this section have been followed.
- When you finish a task, **always explicitly say "Done."** or equivalent so it's clear the work is complete.
- Do not wait for the user to ask "Are you done?" — proactively declare completion.

### One Migration Per Version

- Each version/release must produce **exactly one migration file**. Do not create multiple migrations for the same version.
- If schema changes accumulate during development, consolidate them into a single migration before finalizing.
- To consolidate: revert the database to before the migrations (`Update-Database 0`), remove them (`Remove-Migration`), then regenerate a single migration (`Add-Migration`).

### Accessibility & SEO Checklist

- **Every new or modified page** must be checked for accessibility and SEO before the task is considered complete.
- **Accessibility requirements:**
  - `aria-label` on interactive elements without visible text (dismiss buttons, icon-only buttons)
  - `aria-hidden="true"` on decorative SVGs and visual-only elements
  - `role="status"` or `role="alert"` on toast/notification elements
  - `aria-pressed` on toggle buttons (see SuggestionBox filter pills for pattern)
  - Semantic `<section>` wrappers with `aria-label` or `aria-labelledby` on content regions
  - `sr-only` spans on list items where visual context (like checkmarks) conveys meaning
  - `role="group" aria-label="..."` on related button groups
- **SEO:** Only applies to public (unauthenticated) pages. Authenticated pages behind `MainLayout` do not need SEO meta tags — `<PageTitle>` is sufficient.

### Page Hero Branding

- **Every page** in the app (except the Landing Page) must include a purple gradient swoosh hero section for consistent branding.
- Pages using `MainLayout` (sidebar pages like Dashboard, Suggestions) use an **inline swoosh** hero within the page markup.
- Pages using `LandingLayout` (Sign-In, Error) use the **`BrandHeader`** component.
- See the "Page Branding — Purple Swoosh" table in the Brand & Design section for the full mapping.
- **Admin page is the reference implementation** for new sidebar pages. Match its SVG (`viewBox="0 0 1000 600"`, swoosh path, gradient fill), hero-bg (`height: calc(100% + 60px)`), and SVG CSS (`min-height: 280px`) exactly. Pages with hero stats use `margin: -32px -40px 0` and `padding: 24px 40px 40px`; pages without stats use `margin: -32px -40px 60px` and `padding: 24px 40px 48px`. **Exception:** Pages with spoke tabs (Dashboard, Manual Entry) use `margin: -32px -40px 0` and `padding: 24px 40px 120px` — the extra bottom padding gives the swoosh curve room to display before the tab bar.
- **Hero stat numbers** use colorized `nth-child` styling: 1st stat → lavender `#a78bfa`, 2nd stat → contextual color (green `#2ecc71` for money/success, gold `#f5c842` for counts), 3rd stat → white `#fff`. Font size is `22px` on all pages — keep this consistent. Labels are `rgba(255, 255, 255, 0.6)` at `12px`.
- **Hero stat labels display inline to the right of the number**, never below it. Use `margin-left: 6px` on the label (or flex with `gap`) — never `flex-direction: column` on the stat container.
- **Never use CSS `background: linear-gradient(...)` on the hero section.** The SVG gradient provides the purple — this is what creates the curved swoosh edge instead of a flat block.

### Spoke Tabs

- **Every hub and spoke page** (Dashboard, Manual Entry, and future platform pages) must have a 4-tab bar: **Overview, Revenue, Expenses, Payouts**. This creates a consistent mental model — same structure everywhere, different data scope.
- **Dashboard** (hub) defaults to the **Overview** tab. **Manual Entry** and platform spokes default to the **Revenue** tab.
- **Tab bar placement:** Below the hero swoosh, above page content. Uses the shared `.spoke-tabs` class from `app.css`.
- **Tab bar styling:** Muted purple-charcoal background (`#5c5777`, `#0a0a14` in dark mode), white text, purple `#8b5cf6` underline on active tab. Colors are hardcoded (not CSS variables) to avoid specificity issues with scoped CSS.
- **Hero padding for tab pages:** Pages with spoke tabs use `120px` bottom hero padding to give the swoosh curve room before the tab bar starts. The hero `margin-bottom` is `0` so the tab bar sits flush below.
- **ViewModel pattern:** `ActiveTab` (string property, default varies by page), `SelectTab(string tab)` method. Page content wrapped in `@if (ActiveTab == "xxx")` blocks with `role="tabpanel"` and `aria-label`.
- **`PageTitle`** updates based on active tab (e.g., "Dashboard — Revenue — MyTalli").

### Modal Behavior

- **Modals do not close on backdrop click.** Only the Cancel button (or equivalent) closes the modal. This prevents accidental data loss when users click outside a form modal.
- **Exception:** The `UserProfileButton` dropdown closes on backdrop click — this is intentional since it's a menu, not a form.

### ConfirmDialog Component

- **`ConfirmDialog`** (`Components/Shared/ConfirmDialog.razor`) — reusable Yes/No confirmation dialog. The component provides the modal shell + buttons; the caller passes in content via `ChildContent` (RenderFragment).
- **Parameters:** `Visible` (bool), `ConfirmStyle` (`"primary"` or `"danger"`), `OnConfirm` (EventCallback), `OnCancel` (EventCallback).
- **Button text:** Always "Yes" / "No" — not customizable.
- **Usage:** `<ConfirmDialog Visible="..." ConfirmStyle="danger" OnConfirm="..." OnCancel="...">` with icon, heading, and message as child content.
- **CSS isolation:** Uses `::deep` for `.confirm-body` styles to reach projected `ChildContent`. SVGs passed as child content should have explicit `height`/`width` attributes.

### Sample Data for Gated Features

- **Never show a lock gate for paid features.** Always show the page with sample data + a CTA banner at the top (same pattern as the Dashboard).
- **Dataset classes** — all faked/sample data lives in static classes in `Models/SampleData/` named `{Feature}Dataset` (e.g., `DashboardDataset`, `GoalsDataset`, `ManualEntryDataset`, `ExpenseDataset`, `PayoutDataset`). Each class returns typed collections or values via static methods. Expense and Payout datasets provide both dashboard-scoped (cross-platform) and manual-scoped (Manual Entry only) sample data via separate methods (e.g., `GetDashboardExpenses()`, `GetManualExpenses()`).
- **`IsSampleData` flag** — on the ViewModel, controls the banner visibility. When `true`: CTA banner shown, "New Entry" / action buttons hidden, edit/delete hidden, grid fully interactive (sort, paginate, density).
- **The page doesn't know or care** whether data is real or sample — it renders the same grid either way.
- **Grid preferences** still save for sample data viewers — their density/sort/page size choices persist.

### Mobile-First Responsive Strategy

- **Principle: "Keyhole Data"** — phones are for glancing at numbers, not configuring things. Desktop gets the full experience; mobile gets a focused, read-only snapshot.
- **Don't block routes** — never return a 404 or redirect based on viewport. If a user deep-links to a desktop-oriented page on mobile, show a friendly "better on desktop" message with a link back to the dashboard.
- **Hide non-mobile nav items** — on small screens, hide sidebar links for pages that don't render well on mobile (e.g., Platforms, Export, Settings). Keep Dashboard, Goals, Suggestions visible.
- **Simplify, don't remove** — pages that are visible on mobile should render a simplified "keyhole" view, not the full desktop layout. Example: Goals on mobile shows progress bars and numbers, not the full goal editor.
- **Decide per page** — each page's mobile treatment is determined when building that page, not planned upfront. The content will make the right answer obvious.

### Sidebar Layout

- **Two-layer architecture:** `.sidebar` (outer) is a plain flex child of `.page` — no explicit height, stretches naturally to match the full page height via flex `align-items: stretch`. `.sidebar-inner` (inner) is `position: sticky; top: 0; height: 100vh` — locks nav content to the viewport while scrolling.
- **Why two layers:** The outer div provides the full-height dark background (no gap at the bottom). The inner div provides the viewport-locked sticky behavior. Combining both on one element (the old approach) caused a gap below the sidebar content when the page was taller than the viewport.
- **Dark mode body background:** `[data-theme="dark"]` in `app.css` includes `background: #1a1a2e` directly on the selector. Since `data-theme` is set on `<html>`, this makes the HTML element's background dark navy in dark mode — eliminating any white gaps below `.page`. The landing page never gets `data-theme="dark"`, so it's completely unaffected. Do **not** set `background` on `html, body` globally — it would affect the landing page.
- **No `.nav-spacer`:** The nav links stay top-aligned within `.sidebar-inner` because `.sidebar-nav` has `flex: 1`, absorbing leftover space and pushing the upgrade card + user section to the bottom.

### Mobile Navigation

- **Breakpoint:** `max-width: 640.98px` — all mobile-specific styles live behind this media query in `MainLayout.razor.css`
- **Hamburger button** — `.mobile-hamburger` in `MainLayout.razor`, fixed position top-left (`left: 16px; top: 16px`), hidden on desktop (`display: none`). Toggles the sidebar open/closed.
- **Sidebar slide-in** — on mobile, `.sidebar` is `position: fixed; transform: translateX(-100%)`. Adding `.mobile-open` class slides it in (`translateX(0)`) with a `box-shadow` and `0.25s ease` transition.
- **Backdrop** — `.mobile-backdrop` is always in the DOM, hidden by default. Adding `.active` class shows a semi-transparent overlay (`rgba(0, 0, 0, 0.4)`, `z-index: 999`).
- **JavaScript toggle** — `wwwroot/js/mobile-menu.js` handles all toggle logic via event delegation on `document`. Uses CSS class manipulation (`.mobile-open` on sidebar, `.active` on backdrop), not Blazor `@onclick`, because `MainLayout` renders statically (layout components don't inherit page render modes). Clicking the backdrop or any `.nav-link` inside the sidebar closes the menu.
- **Hero padding** — `.hero-top` gets `padding-left: 48px` on mobile (in `app.css`) to clear the fixed hamburger button so hero titles don't overlap.

### Sidebar Navigation Pages

| Page | Route | Purpose | Mobile |
|------|-------|---------|--------|
| **Dashboard** | `/dashboard` | Revenue overview — KPI cards, charts, trends, recent transactions | Yes (keyhole) |
| **Manual Entry** | `/manual-entry` | Record revenue from non-integrated sources (module, $3/mo) | Yes |
| **Platforms** | `/platforms` | Connect/manage platform integrations (Stripe, Etsy, etc.) | Hidden |
| **Goals** | `/goals` | Set and track monthly/yearly revenue targets | Yes (simplified) |
| **Export** | `/export` | CSV export for tax prep / bookkeeping | Hidden |
| **Suggestions** | `/suggestions` | User feedback and feature requests (vote, edit own) | Yes |
| **Settings** | `/settings` | Account preferences, email settings, theme (dark mode), linked providers | Hidden |
| **Admin** | `/admin` | Email resend, bulk welcome send, user list (Admin role only) | Hidden |

### Sample Data for New Users

- **New users with no connected platforms or modules** see sample/mock data on the dashboard so they can immediately understand the product's value. An empty dashboard would be a dead end.
- **Sample data banner** — when sample data is active, a branded banner is shown: "You're viewing **sample data**. Connect a platform or activate Manual Entry to see your real revenue." with a CTA to `/my-plan`.
- **`IsSampleData` flag** — `DashboardViewModel.IsSampleData` controls whether the banner is visible. Set to `true` by default; set to `false` once the user has at least one connected platform or an active module subscription (ProductId >= 3, status Active or Cancelling).
- **Once a platform is connected or a module is activated**, sample data disappears entirely and real data takes over. No mixing of sample and real data. The Dashboard queries `app.Revenue`, `app.Expense`, `app.Payout`, and `app.Goal` for all user data across all platforms. Summary cards, chart SVG paths, platform breakdown, and recent transactions are all computed from real data. Period pills (7D/30D/90D/12M) filter revenue data by date range and reload asynchronously.
- **Goal card** — queries `app.Goal` for any active goal covering the current month (not limited to a single GoalType). Computes earned revenue from `app.Revenue` using the goal's date range + optional platform filter — same algorithm as the Goals page. Shows circle progress, projected pace (on track / behind), and days remaining. If a goal exists, shows "View goals →" linking to `/goals`. If no goal exists, shows "Set a goal →" linking to `/goals`.

### Missing Name Fallback

- **Names can be missing for multiple reasons:** OAuth providers (especially Apple) may not provide a name, or users may clear their name in Settings. The UI must never show blank names, empty initials, or broken layouts when name data is missing.
- **`UserClaimsHelper.Resolve()`** (`Helpers/UserClaimsHelper.cs`) is the single source of truth for resolving user display info. Has two overloads: one from `ClaimsPrincipal` (used by claims-only contexts), one from raw strings (used by DB-backed contexts). Any new ViewModel that needs user display info should use it.
- **`UserDisplayCache`** (`Services/Identity/UserDisplayCache.cs`) — scoped service that loads user display info from the database once per Blazor circuit, caches it, and serializes access with a `SemaphoreSlim`. Both `DashboardViewModel` and `NavMenuViewModel` use it to avoid concurrent `DbContext` access (Blazor Server renders layout and page components in parallel). `SettingsViewModel` calls `Invalidate()` after saving so the next navigation picks up updated names.
- **Display info comes from the database, not claims.** Auth cookie claims contain name data frozen at sign-in time. The `UserDisplayCache` reads from `auth.User` so name changes in Settings take effect immediately without requiring sign-out/sign-in.
- **Fallback chain for display name:** DisplayName → email prefix (before `@`)
- **Fallback chain for greeting (first name):** FirstName → first word of DisplayName → random Fun Greeting (title case, e.g., "Good morning, Stack Builder")
- **Fallback chain for initials:** First+Last initials → first+last word of DisplayName → first letter of email → `"?"`
- **Fun Greetings** — when no name is available, the greeting falls back to a random title-cased fun greeting (e.g., "Revenue Rockstar", "Side-Hustle Hero"). This is the last-resort fallback in `Resolve()` and always activates when names are empty, regardless of the Fun Greetings user preference. The Fun Greetings preference adds randomness on top (a different greeting each visit) when the user *does* have a name.
- **Email notifications** — all customer emails (`WelcomeEmailNotification`, `SubscriptionConfirmationEmailNotification`, `WeeklySummaryEmailNotification`) fall back to `"there"` when FirstName is empty (e.g., "Welcome to MyTalli, there!").

### Summary Tag Convention

- Every C# class and interface **must** have a `/// <summary>` tag.
- Keep it to a **short role label** (e.g., `Repository`, `Resolver`, `Entity`, `Configuration`, `Service`).
- If the summary needs a full sentence to explain what the class does, the class name needs to be more descriptive instead.

```csharp
/* Correct */
/// <summary>Repository</summary>
public class GenericAuditableRepositoryAsync<TEntity> { ... }

/* Wrong — the class name already says this */
/// <summary>Repository implementation with automatic audit resolution on insert and update operations.</summary>
public class GenericAuditableRepositoryAsync<TEntity> { ... }
```

### Async Naming Convention

- Synchronous classes and interfaces are named plainly (e.g., `ICurrentUserService`, `AuditResolver`).
- Asynchronous classes and interfaces append **`Async`** to the name (e.g., `IRepositoryAsync`, `GenericRepositoryAsync`).
- This applies to the **class/interface name** — async **methods** already follow the standard .NET `Async` suffix convention.
- Only apply to classes whose primary contract is async. ViewModels, handlers, and services with async lifecycle or framework methods do **not** get the suffix.

### Subfolder Namespace Convention

- Subfolders used purely for **file organization** do not add to the C# namespace.
- The namespace stops at the **functional grouping level** — the last meaningful segment.
- Examples:
  - `Domain.Entities/Entities/User.cs` → `namespace My.Talli.Domain.Entities;` (not `...Entities.Entities`)
  - `Domain/Components/JsonSerializers/User/UserPreferencesJsonSerializer.cs` → `namespace My.Talli.Domain.Components.JsonSerializers;` (not `...JsonSerializers.User`)
  - `Domain/Handlers/Authentication/Google/GoogleSignInHandler.cs` → `namespace My.Talli.Domain.Handlers.Authentication;` (not `...Authentication.Google`)
  - `Domain/CommandsAndQueries/Commands/Platforms/ConnectEtsyCommand.cs` → `namespace My.Talli.Domain.Commands.Platforms;` (the `CommandsAndQueries/` umbrella is organizational only — a reserved slot for future `Queries/` siblings — and does NOT appear in the namespace)

### Clean Up NUL Files

- Bash on Windows creates an actual file named `nul` when using `2>nul` redirects (instead of discarding output to the Windows NUL device). **Always delete any `nul`/`NUL` files** that get created in the repo after running shell commands.

### Namespace-First Ordering

- In C# files, the **file-scoped `namespace` declaration comes first**, followed by `using` statements below it.
- Files with no `using` statements just start with the `namespace`.

```csharp
/* Correct */
namespace My.Talli.Web.Services.Email;

using Microsoft.Extensions.Options;
using Domain.Notifications.Emails;

public class SmtpEmailService { ... }

/* Wrong — do not put usings above the namespace */
using Microsoft.Extensions.Options;
using My.Talli.Domain.Notifications.Emails;

namespace My.Talli.Web.Services.Email;

public class SmtpEmailService { ... }
```

### Relative Using Statements

- Because `using` statements appear **below** the file-scoped `namespace`, C# resolves them relative to that namespace's root.
- Use **shortened relative paths** for internal project references instead of the fully qualified namespace.

```csharp
/* Correct — under namespace My.Talli.Domain.Components.JsonSerializers */
using Domain.Framework;

/* Wrong — unnecessarily verbose */
using My.Talli.Domain.Framework;
```

### Alphabetical Using Order

- All `using` statements must be listed in **alphabetical order**.
- Regular (non-alias) usings are sorted alphabetically among themselves.
- Alias usings are sorted alphabetically among themselves (in their own group, separated by a blank line).

```csharp
/* Correct */
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

/* Wrong — not alphabetical */
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
```

### Uppercase Using Aliases

- When creating `using` aliases in C#, the alias name must be **ALL CAPS**.
- This makes aliases visually distinct from type names and easier to spot.
- Alias `using` statements are separated from normal `using` statements by a **blank line**, and grouped together.

```csharp
/* Correct */
using Domain.Framework;
using System.Text.Json;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/* Wrong — alias mixed in with normal usings, not capitalized, not alphabetical */
using System.Text.Json;
using Models = My.Talli.Domain.Models;
using My.Talli.Domain.Framework;
```

### Program.cs Organization

- **Program.cs** is a thin orchestrator — it calls extension methods, not inline logic.
- **Service registration** goes in `Configuration/` — one static class per concern, each exposing an `IServiceCollection` extension method (e.g., `AddAuthenticationProviders`, `AddDatabase`, `AddRepositories`). Methods that need config values accept `IConfiguration` as a parameter.
- **Endpoint mapping** goes in `Endpoints/` — one static class per route group, each exposing an `IEndpointRouteBuilder` extension method (e.g., `MapAuthEndpoints`, `MapBillingEndpoints`).
- **Middleware** goes in `Middleware/` — proper middleware classes with `InvokeAsync` and a companion `Use{Name}` extension method on `IApplicationBuilder`. Lightweight inline middleware may stay in Program.cs when it's only a few lines and tightly coupled to pipeline ordering.
- When adding a new service concern, create a new `Configuration/{Name}Configuration.cs` file. When adding new API routes, create a new `Endpoints/{Name}Endpoints.cs` file. When adding new middleware, create a new `Middleware/{Name}Middleware.cs` file. Do not add inline registrations, endpoint lambdas, or substantial middleware to Program.cs.
- Namespace: `My.Talli.Web.Configuration` for configuration classes, `My.Talli.Web.Endpoints` for endpoint classes, `My.Talli.Web.Middleware` for middleware classes.

### Endpoint File Structure

- Each endpoint class uses two regions: **`<Endpoints>`** for route declarations and **`<Methods>`** for endpoint implementations.
- The `<Endpoints>` region contains only the `Map{Name}Endpoints` extension method with one-liner route-to-method mappings — no inline lambdas.
- The `<Methods>` region contains `private static` endpoint methods that the routes point to. Endpoint methods should be thin — validate the request, delegate to handlers/commands, return a result.
- **No data access, business logic, or side effects in endpoint methods.** Delegate to handlers and commands instead.

```csharp
public static class AuthEndpoints
{
    #region <Endpoints>

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/login/{provider}", Login);
        app.MapGet("/api/auth/logout", Logout);
    }

    #endregion

    #region <Methods>

    private static async Task Login(string provider, HttpContext context) { ... }
    private static async Task Logout(HttpContext context) { ... }

    #endregion
}
```

### Handlers and Commands

Endpoint-supporting logic lives in dedicated classes under `Handlers/` and `Commands/`. **Commands are split between Domain and Web based on what they depend on** — handlers are always Web.

- **Handlers** (Web only — `My.Talli.Web/Handlers/Endpoints/`) — react to events. They orchestrate the pipeline: map external objects (e.g., Stripe SDK types, Etsy API responses) to Domain payloads, call Domain commands/handlers inside transactions, handle side effects (logging, emails). Each handler owns everything it does — mapping methods, email building, etc. live inside the handler, not back in the endpoint.
- **Commands** — execute actions. Each command exposes a single `ExecuteAsync()` method. Organized by subfolder based on **what the command does**, not who calls it.
  - **Domain commands** (`Domain/CommandsAndQueries/Commands/{Area}/`) — the default home for commands. Use only Domain-layer deps: `RepositoryAdapterAsync`, Domain POCOs, `EnforcedTransactionScope`. Registered in `Domain.DI.Lamar.IoC.ContainerRegistry`. Example: `ConnectEtsyCommand`, `FindActiveSubscriptionWithStripeCommand`, `UpdateLocalSubscriptionCommand`. These are testable from `My.Talli.UnitTesting` via the in-memory repository stubs.
  - **Web commands** (`My.Talli.Web/Commands/{Area}/`) — only when the command genuinely needs Web-layer primitives: `IEmailService`, direct `TalliDbContext` access (for keyless view queries like `vAuthenticatedUser`), or other infrastructure interfaces the Domain shouldn't see. Registered in the relevant `Configuration/{Name}Configuration.cs`. Example: `GetAdminUserListCommand` (direct DbContext), `SendWelcomeEmailCommand` (IEmailService).
  - **Default to Domain.** A command belongs in Web only if moving it breaks the "Domain stays HTTP- and SDK-free" rule. When in doubt, try Domain first and let the compiler push back.
- Both handlers and commands are **non-static classes** with constructor-injected dependencies — no `HttpContext.RequestServices.GetRequiredService` calls.
- All are registered as **scoped** (Web commands in `Configuration/{Name}Configuration.cs`; Domain commands in `Domain.DI.Lamar.IoC.ContainerRegistry`).
- **One class per operation** — not one class per domain area. `CheckoutCompletedHandler` handles checkout completed events, not "all billing webhook events."
- **Namespaces:**
  - Web: `My.Talli.Web.Handlers.Endpoints` / `My.Talli.Web.Commands.{Area}`.
  - Domain: `My.Talli.Domain.Commands.{Area}` (the `CommandsAndQueries/` umbrella folder is organizational and does NOT appear in the namespace — see Subfolder Namespace Convention).

### No Inline Code Blocks

- **NEVER** use `@code {}` blocks in `.razor` files (pages, components, or layouts).
- All `.razor` files contain **markup only** — no C# logic.

### Code-Behind Pattern

- Any page or component that requires C# logic **must** use a code-behind file.
- Code-behind files inherit from `ComponentBase` (or `LayoutComponentBase` for layouts) and the `.razor` file uses `@inherits` to reference it.
- Example: `LandingPage.razor` → `@inherits LandingPageViewModel`

### ViewModels Folder

- All code-behind files live in the `ViewModels/` folder within the web project.
- Code-behind classes are named `{ComponentName}ViewModel.cs`.
- Mirror the component folder structure inside `ViewModels/`:
  - `Components/Pages/LandingPage.razor` → `ViewModels/Pages/LandingPageViewModel.cs`
  - `Components/Layout/MainLayout.razor` → `ViewModels/Layout/MainLayoutViewModel.cs`
  - `Components/Shared/BrandHeader.razor` → `ViewModels/Shared/BrandHeaderViewModel.cs`
- Namespace follows the folder: `My.Talli.Web.ViewModels.Pages`, `My.Talli.Web.ViewModels.Layout`, `My.Talli.Web.ViewModels.Shared`, etc.

### Entity Models

- **Never expose entities directly** to the presentation layer. Always map to a model class via `IEntityMapper`.
- **Never expose audit fields** (`CreateByUserId`, `CreatedOnDateTime`, `UpdatedByUserId`, `UpdatedOnDate`) in models.
- **Never expose navigation properties** in models — use FK IDs instead.
- **`DefaultModel`** (`Domain/Models/DefaultModel.cs`) — base class for all entity models. Provides `Id`, `IsDeleted`, and `IsVisible`. Mirrors `DefaultEntity` on the entity side. All entity models inherit from `DefaultModel`.
- **`Models/Entity/`** — 1-to-1 representations of an entity (same class name, no suffix). Disambiguate from entities via using aliases (`ENTITIES`, `MODELS`).
- **`Models/Presentation/`** — aggregate or detail representations (custom shapes for specific UI needs).
- **No "Model" suffix** — model classes use the same name as their entity. The `Models` namespace already disambiguates.
- **Namespace:** All models use `My.Talli.Domain.Models` regardless of subfolder (`Entity/` and `Presentation/` are organizational only).
- **IEntityMapper** (`Domain/Mappers/IEntityMapper.cs`) — generic interface for entity↔model mapping. Concrete mappers live in `Domain/Mappers/Entity/` (one per pair). When adding a new entity/model pair, create a mapper and register it in `Program.cs`.
- **RepositoryAdapterAsync** (`Domain/Repositories/RepositoryAdapterAsync.cs`) — the only gateway to the data layer. Never use `IAuditableRepositoryAsync<TEntity>` or `GenericAuditableRepositoryAsync<TEntity>` directly in presentation-layer code.
- **Handlers must not touch audit fields** — no handler, service, or any code in or above the Domain layer should set `CreateByUserId`, `CreatedOnDateTime`, `UpdatedByUserId`, or `UpdatedOnDate`. Audit field stamping is solely the job of `AuditResolver`. Handlers work with models (which don't have audit fields) via `RepositoryAdapterAsync`.

### EnforcedTransactionScope

- **`EnforcedTransactionScope`** (`Domain/Framework/EnforcedTransactionScope.cs`) — static utility that wraps a block of code in a `TransactionScope`. If the block succeeds, the transaction commits. If it throws, the transaction rolls back and the exception rethrows after rollback.
- **Lives in Domain/Framework** — general-purpose utility like `Assert`, not tied to repositories.
- **Used in the presentation/service layer, not in handlers.** Handlers are pure business logic with no transaction awareness. The **caller** (endpoint, auth handler) decides the transaction boundary because it knows the full scope of what needs to be atomic.
- **Wrap all DB writes + critical follow-up operations** inside the scope. Keep side effects (email sends, logging) **outside** — a failed email should not roll back a successful DB commit.
- **Elmah safety:** Elmah writes to SQL Server on its own connection. Because the exception rethrows *after* the scope disposes (rollback complete), Elmah's error insert is not affected by the rolled-back transaction.
- **Mark with `// TRANSACTION` comment** — place the comment immediately above the `EnforcedTransactionScope.ExecuteAsync` call for scannability.

**Auth handler pattern** — DB writes + claims inside, email outside:
```csharp
// TRANSACTION
var user = await EnforcedTransactionScope.ExecuteAsync(async () =>
{
    var u = await _signInHandler.HandleAsync(argument);

    var identity = (ClaimsIdentity)principal.Identity!;
    identity.AddClaim(new Claim("UserId", u.Id.ToString()));

    foreach (var role in u.Roles)
        identity.AddClaim(new Claim(ClaimTypes.Role, role));

    return u;
});

if (user.IsNewUser)
    await SendWelcomeEmailAsync(argument.Email, user.FirstName, user.Id);
```

**Endpoint pattern** — handler call inside, logging + email outside:
```csharp
// TRANSACTION
var handler = context.RequestServices.GetRequiredService<StripeWebhookHandler>();
var result = await EnforcedTransactionScope.ExecuteAsync(async () => await handler.HandleCheckoutCompletedAsync(payload));

logger.LogInformation("Checkout completed for user {UserId}", result.UserId);
await SendSubscriptionConfirmationEmailAsync(context, result);
```

### C# Region Convention

- Every C# class **must** use `#region` / `#endregion` to organize its members.
- Region names use angle brackets: `#region <Name>`
- **Blank line after `#region`** and **blank line before `#endregion`** — content is always separated from the region boundaries by one empty line.
- Only include regions the class actually needs — omit empty ones.
- Allowed regions (in order):
  1. `<Variables>` — fields, constants, injected services
  2. `<Constructors>` — constructor overloads
  3. `<Properties>` — public/protected properties
  4. `<Events>` — lifecycle events, event handlers
  5. `<Methods>` — general methods
  6. `<Actions>` — MVC controller actions (not used yet)
- **Within each region**, order members by access modifier: `public` → `protected` → `private`
- **Within each access level**, alphabetize members by **type/class name** (not by variable name)

```csharp
/* Correct — sorted by class name, blank lines around content */
#region <Variables>

private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;
private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> _appleAuthRepository;
private readonly UserPreferencesJsonSerializer _preferencesSerializer;

#endregion

/* Wrong — no blank lines, sorted by variable name */
#region <Variables>
private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> _appleAuthRepository;
private readonly UserPreferencesJsonSerializer _preferencesSerializer;
private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;
#endregion
```

- **Constructor parameters** follow the same type/class name ordering as `<Variables>`
- **Constructor assignments** are alphabetized by **variable name**

```csharp
/* Correct — parameters sorted by type, assignments sorted by variable name */
#region <Constructors>

public AppleSignInHandler(
    IAuditableRepositoryAsync<ENTITIES.User> userRepository,
    IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> appleAuthRepository,
    UserPreferencesJsonSerializer preferencesSerializer)
{
    _appleAuthRepository = appleAuthRepository;
    _preferencesSerializer = preferencesSerializer;
    _userRepository = userRepository;
}

#endregion
```

### CSS Formatting

- Each CSS rule set must be written on a **single line** — selector, opening brace, all declarations, and closing brace.
- Where possible, all CSS declarations within a rule should be in **alphabetical order**.

```css
/* Correct */
.signin-page { background: #f8f7fc; min-height: 100vh; overflow: hidden; padding: 0 0 80px; position: relative; }
.signin-shell { margin: 0 auto; max-width: 420px; position: relative; text-align: center; z-index: 2; }

/* Wrong — do not use multi-line format */
.signin-page {
    background: #f8f7fc;
    min-height: 100vh;
}
```

## Conventions

- Use the `.slnx` solution format (not `.sln`)
- Organize projects into solution folders: Foundation (domain/infra), Presentation (web/UI)
- Follow standard .NET/Blazor project conventions
- Namespace root: `My.Talli`

## Testing Tools

> **Moved to memory:** `reference_testing_tools.md` — WAVE, Lighthouse, axe DevTools, NVDA; known WAVE contrast false positives.

## Etsy Setup TODO

> **Moved to memory:** `project_etsy_setup.md` — API key approved, test shop & commercial access pending.

## Stripe Setup TODO

> **Moved to memory:** `project_stripe_setup.md` — dev environment working, production keys & custom domains pending.

## Blazor TODO

> **Moved to memory:** `project_blazor_todo.md` — completed features (Admin, Manual Entry, Goals, My Plan) and remaining backlog (Nav architecture, Module checkout, Email hosting).
