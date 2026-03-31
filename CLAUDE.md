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
| `app` | Application features & revenue | Expense, Goal, GoalType, Milestone (legacy), Payout, PlatformConnection, Revenue, RevenueEtsy, RevenueGumroad, RevenueManual, RevenueStripe, Suggestion, SuggestionVote, SyncQueue |
| `components` | Third-party component tables (not EF-managed) | ELMAH_Error (auto-created by ElmahCore) |
| `dbo` | Reserved (empty) | — |

### Schema: `app`

**`app.Expense`** — platform fees not tied to a specific sale (listing fees, ad fees, subscription fees, etc.)
- `Id` (PK), `UserId` (FK → auth.User), `Amount` (decimal 18,2), `Category` (string 50 — ListingFee, AdFee, SubscriptionFee, ProcessingFee, ShippingLabel, Other), `Currency` (string 3), `Description` (string 500), `ExpenseDate` (datetime), `Platform` (string 50), `PlatformTransactionId` (nullable string 255 — dedup key)
- Composite index on `(Platform, ExpenseDate)` for dashboard queries
- Index: `IX_Expense_UserId`
- Design: Parallel to Revenue — both queried by dashboard, no FK between them. `Revenue.FeeAmount` = per-sale fees; `Expense.Amount` = standalone platform fees.

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

**`app.Payout`** — platform disbursements to user's bank account
- `Id` (PK), `UserId` (FK → auth.User), `Amount` (decimal 18,2), `Currency` (string 3), `ExpectedArrivalDate` (nullable datetime), `PayoutDate` (datetime), `Platform` (string 50), `PlatformPayoutId` (string 255 — dedup key), `Status` (string 20 — Pending, InTransit, Paid, Failed, Cancelled)
- Composite index on `(Platform, PayoutDate)` for dashboard queries
- Unique index on `PlatformPayoutId` for dedup
- Index: `IX_Payout_UserId`
- Design: No FK to Revenue — one payout covers many sales (batched). Enables cash flow view: earned vs received vs pending.

**`app.Revenue`** — normalized revenue record from all platforms (API-sourced and manual entry)
- `Id` (PK), `UserId` (FK → auth.User), `Currency` (3-char ISO), `Description`, `FeeAmount` (decimal 18,2), `GrossAmount` (decimal 18,2), `NetAmount` (decimal 18,2), `Platform` ("Manual", "Stripe", "Etsy", etc.), `PlatformTransactionId` (unique per platform), `TransactionDate`, `IsDisputed`, `IsRefunded`
- Composite index on `(Platform, TransactionDate)` for dashboard queries
- Design: Goals and dashboard analytics query **only** this normalized table. Platform-specific tables exist for drill-down detail.

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

**`app.SyncQueue`** — background sync job work list (one row per user per connected platform)
- `Id` (PK), `UserId` (FK → auth.User), `Platform` (string, max 50 — "Stripe", "Etsy", "Gumroad", "PayPal", "Shopify"), `Status` (string, max 20 — Pending, InProgress, Completed, Failed), `NextSyncDateTime` (when this row is next eligible for processing), `LastSyncDateTime` (nullable — null until first successful sync), `LastErrorMessage` (nullable, max 2000 — most recent failure reason), `ConsecutiveFailures` (int, default 0 — drives exponential backoff), `IsEnabled` (bool, default true — user can pause syncing)
- Unique constraint on `(UserId, Platform)` prevents duplicate queue entries
- Index on `(NextSyncDateTime, Status)` for sync job polling query
- Users can pause sync (`IsEnabled = false`) but cannot disconnect — connected platforms permanently occupy a plan slot

### Schema: `auth`

**`auth.User`** — core MyTalli identity (one row per person)
- `Id` (PK), `DisplayName`, `FirstName`, `LastName`, `CreatedAt`, `LastLoginAt`, `InitialProvider` (historical — which provider they first signed in with, never changes), `PreferredProvider` (which provider the user prefers, starts equal to InitialProvider), `UserPreferences` (NVARCHAR(MAX), JSON — app settings/toggles, defaults to `'{}'`)
- Email is **not** stored here — it lives on the provider auth tables. The user's email is resolved via their PreferredProvider.
- **UserPreferences** stores user-configurable app settings as JSON. This avoids contorting the User table with individual columns as settings grow over time. Serialized/deserialized by `UserPreferencesJsonSerializer` in `Domain/Components/JsonSerializers/User/`. Current structure:
  ```json
  {
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
│   ├── MyTalli_PlatformCapabilities.html # Platform API capabilities, data richness & integration roadmap
│   ├── MyTalli_ScalingPlan.html    # Scaling strategy as user base grows (tiers, triggers, capacity)
│   └── PlatformApiDataShapes.html  # Platform API data shapes, normalized schema, ERD with SyncQueue
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
    │   │   ├── JsonSerializers/
    │   │   │   └── User/
    │   │   │       └── UserPreferencesJsonSerializer.cs  # Serialize/deserialize UserPreferences JSON
    │   │   └── Tokens/
    │   │       └── UnsubscribeTokenService.cs  # HMAC-SHA256 token generate/validate for email unsubscribe links
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
    │   ├── Components/
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
    │   │   │   └── SignInHandlerBuilder.cs     # Test setup orchestrator (Lamar container, exposes handlers & adapters)
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
        │   └── RepositoryConfiguration.cs      # ICurrentUserService registration (mappers, handlers, and repositories are in Domain.DI.Lamar)
        ├── Endpoints/                 # Minimal API endpoint extension methods (one per route group)
        │   ├── AdminEndpoints.cs      # /api/admin/email/* (resend, bulk-welcome, bulk-welcome-all)
        │   ├── AuthEndpoints.cs       # /api/auth/login, /api/auth/logout
        │   ├── BillingEndpoints.cs    # /api/billing/create-checkout-session, portal, switch-plan, webhook
        │   ├── EmailEndpoints.cs      # /api/email/preferences
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
        ├── Commands/                  # Web-layer commands (execute actions, data access, notifications)
        │   ├── Notifications/         # Email and notification commands
        │   │   ├── SendSubscriptionConfirmationEmailCommand.cs # Build + send subscription confirmation email
        │   │   ├── SendWelcomeEmailCommand.cs                  # Build + send welcome email
        │   │   └── SendWeeklySummaryEmailCommand.cs            # Build + send weekly summary email (sample data)
        │   └── Endpoints/             # Commands that serve endpoint routes
        │       ├── FindActiveSubscriptionWithStripeCommand.cs  # Query active subscription + Stripe record
        │       ├── GetAdminUserListCommand.cs                  # Query users with emails from vAuthenticatedUser view
        │       └── UpdateLocalSubscriptionCommand.cs           # Sync local DB after plan switch
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
        │   │   └── mobile-menu.js  # Mobile hamburger menu toggle (CSS class-based, no Blazor interactivity)
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

> **Source of truth:** `wireframes/MyTalli_ColorPalette.html` (light) and `wireframes/MyTalli_DarkModePalette.html` (dark) — keep this section in sync with those files.

- **Color palette tool:** [Coolors](https://coolors.co) — used to create and manage the brand palette

### Page Branding — Purple Swoosh

Every page except the Landing Page uses a **purple gradient swoosh** header for consistent branding:

- **`BrandHeader` component** (`Components/Shared/BrandHeader.razor`) — reusable swoosh with logo + action slot (`ChildContent` RenderFragment). Used by Sign-In, Unsubscribe, and Error pages.
- **Dashboard** uses its own inline swoosh (no BrandHeader) because the sidebar already has the logo — the swoosh sits behind the greeting area instead.
- **Landing Page** has its own distinct hero layout and is **not** branded with the swoosh.

| Page | Swoosh | Logo | Action Slot |
|------|--------|------|-------------|
| `/signin` | `<BrandHeader>` | Yes | "Back to homepage" link |
| `/dashboard` | Inline SVG (`.dash-hero`) | No (sidebar has it) | Period pills (7D, 30D, 90D, 12M) |
| `/manual-entry` | Inline SVG (`.manual-hero`) | No (sidebar has it) | N/A |
| `/platforms` | Inline SVG (`.plat-hero`) | No (sidebar has it) | N/A |
| `/goals` | Inline SVG (`.goals-hero`) | No (sidebar has it) | "New Goal" button |
| `/suggestions` | Inline SVG (`.suggest-hero`) | No (sidebar has it) | "New Suggestion" button |
| `/settings` | Inline SVG (`.settings-hero`) | No (sidebar has it) | N/A |
| `/my-plan` | Inline SVG (`.plan-hero`) | No (sidebar has it) | N/A |
| `/subscription/cancel` | Inline SVG (`.cancel-hero`) | No (sidebar has it) | N/A |
| `/admin` | Inline SVG (`.admin-hero`) | No (sidebar has it) | N/A |
| `/unsubscribe` | `<BrandHeader>` | Yes | "Go to Homepage" link |
| `/Error` | `<BrandHeader>` | Yes | "Go Back" button |
| `/` | None | Own nav logo | N/A |

Swoosh visual: purple gradient SVG (`#6c5ce7` → `#8b5cf6` → `#6c5ce7`) with 3 decorative circles (`rgba(255,255,255,0.07)`). All `MainLayout` pages use the same SVG swoosh pattern — `viewBox="0 0 1000 600"` with path `M0,0 L1000,0 L1000,320 C850,400 650,280 450,340 C250,400 100,360 0,300 Z` filled by a per-page `linearGradient`. The hero-bg uses `height: calc(100% + 60px)` to extend the swoosh below the hero content. Pages with hero stats (ManualEntry, Platforms, Goals, Suggestions) use `margin: -32px -40px 0` and `padding: 24px 40px 40px`; pages without stats use `48px` bottom padding.
- **Font:** DM Sans (Google Fonts) — weights 400, 500, 600, 700
- **Theme approach:** Purple-tinted surfaces in both modes (no neutral grays in dark mode)

### Brand Colors (Light Mode)

- **Primary Purple:** `#6c5ce7` — CTAs, logo accent, links, active states
- **Primary Hover:** `#5a4bd1` — hover & pressed states
- **Light Purple:** `#8b5cf6` — gradient mid-point, secondary accent
- **Lavender:** `#a78bfa` — accents on dark backgrounds
- **Soft Purple:** `#f0edff` — tags, badges, light backgrounds
- **Muted Purple:** `#e0dce8` — input borders, subtle dividers
- **Page Background:** `#f8f7fc` — alternating section backgrounds
- **Dark Navy:** `#1a1a2e` — primary text, dark sections

### Brand Colors (Dark Mode)

#### Surfaces
- **Page Background:** `#0f0f1a` — deepest layer, main page bg
- **Card Surface:** `#1a1a2e` — cards, sidebar, inputs (Dark Navy repurposed)
- **Elevated Surface:** `#242440` — hover states, dropdowns, tooltips
- **Border:** `#2a2745` — card borders, dividers, table lines
- **Subtle Divider:** `#1e1c30` — table row borders, faint separators

#### Accents
- **Primary Purple:** `#7c6cf7` — CTAs, active states (slightly lifted for dark bg contrast)
- **Primary Hover:** `#6c5ce7` — hover & pressed (original primary becomes hover)
- **Lavender:** `#a78bfa` — logo accent, section tags (promoted role in dark mode)
- **Active Tint:** `#2a2154` — active nav bg, selected states, tags (replaces `#f0edff`)
- **Active Tint Hover:** `#362d6b` — hover on active tint areas, progress bar tracks

#### Text
- **Primary Text:** `#e8e6f0` — headings, card values (warm purple-white, not pure `#fff`)
- **Secondary Text:** `#a09cae` — body paragraphs, descriptions
- **Muted Text:** `#7a7790` — labels, timestamps, helper text
- **Disabled / Faintest:** `#5c5977` — disabled states, chart grid lines

#### UI Colors (Dark Mode Adjusted)
- **Success / Growth:** `#2ecc71` — slightly brighter for pop on dark
- **Success Tint:** `#1a3a2a` — growth badge background
- **Danger / Decline:** `#e74c3c` — negative revenue, errors
- **Danger Tint:** `#3a1a1e` — danger badge background
- **Warning / Highlight:** `#f5c842` — attention states (warmer than light mode yellow)

### Platform Connector Colors

| Platform | Light Mode | Dark Mode  | Notes                              |
|----------|------------|------------|------------------------------------|
| Stripe   | `#635bff`  | `#635bff`  | No change needed                   |
| Etsy     | `#f56400`  | `#f56400`  | No change needed                   |
| Gumroad  | `#ff90e8`  | `#ff90e8`  | No change needed                   |
| PayPal   | `#003087`  | `#2a7fff`  | Lightened — `#003087` invisible on dark |
| Shopify  | `#96bf48`  | `#96bf48`  | No change needed                   |

### UI Colors (Light Mode)

- **Success / Growth:** `#27ae60` — positive revenue changes, growth indicators
- **Body Text:** `#555` — secondary paragraph text
- **Muted Text:** `#999` — footnotes, helper text, timestamps
- **White:** `#ffffff` — cards, inputs, clean backgrounds
- **Highlight Yellow:** `#fff176` — attention flash, input highlights

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

- **Domain registrar:** GoDaddy — `mytalli.com`
- **Custom domain:** `www.mytalli.com` — CNAME pointing to `mytalli-web-f5b9f2a0h4cwdwa6.centralus-01.azurewebsites.net`, SSL via App Service Managed Certificate (SNI SSL, auto-renewing)
- **DNS verification:** TXT record `asuid.www` with Custom Domain Verification ID for Azure domain ownership proof
- **Previous hosting:** Azure Static Web Apps (Free tier) — `delightful-grass-000c17010.6.azurestaticapps.net` (static "coming soon" landing page, now superseded by the Blazor app on App Service)
- **Analytics:** Google Analytics 4 — measurement ID `G-7X9ZL3K4GS` (gtag snippet in landing page `<head>`)
- **Google Search Console:** Property `https://www.mytalli.com/` verified via GA4 (2026-03-07). Sitemap submitted. Dashboard at [search.google.com/search-console](https://search.google.com/search-console)
- **Secrets file:** `.secrets` (git-ignored) — contains `SWA_DEPLOYMENT_TOKEN` for Azure SWA deploys (legacy)
- **Static assets note:** The `deploy/` and `favicon-concepts/` folders are from the static HTML era. Static assets (`favicon.svg`, `og-image.png`, `robots.txt`, `sitemap.xml`) now live in `wwwroot/`. The `deploy/emails/` folder is still needed — it hosts PNG images referenced by customer-facing email templates.

### Business Entity

- **Entity:** MyTalli LLC — single-member LLC, Texas
- **Formation:** Filed 2026-03-27 via LegalZoom (Basic plan, $301 state filing fee only)
- **Owner/Organizer/Registered Agent:** Robert Merrill Jordan
- **Management:** Member-managed
- **Business address:** 5423 Oakhaven Ln, Houston, TX 77091 (home address, on public record)
- **Industry:** Software
- **Fiscal year end:** December 31
- **Status:** Pending Texas Secretary of State approval (5-14 business days from filing)
- **EIN:** Not yet obtained — apply at [irs.gov/ein](https://www.irs.gov/businesses/small-businesses-self-employed/apply-for-an-employer-identification-number-ein-online) after Texas approves (free, instant)
- **Operating agreement:** Not yet created — use a free single-member template after approval
- **Business bank account:** Not yet opened — requires EIN letter + Articles of Organization
- **Texas franchise tax report:** Due annually by May 15 (first due May 15, 2027)
- **Documentation:** `documentation/MyTalli_PlatformApprovals.html` — LLC formation details, Etsy/PayPal approval strategy

### Scaling & Cost Planning

- **Documentation:** `documentation/MyTalli_ScalingPlan.html` (scaling strategy) and `documentation/MyTalli_CostingPlan.html` (cost projections & optimization)
- **Blazor Server memory per circuit:** ~400 KB for MyTalli (dashboard with KPI cards, charts, scoped services)
- **Current capacity (S1):** ~500 concurrent users (1.75 GB RAM, 1 core)
- **Recommended upgrade (P0v3):** ~1,200 concurrent users (4 GB RAM, 1 core) for only ~$4/mo more than S1
- **Concurrent vs registered:** A dashboard app typically sees 5-15% of registered users online at any given time
- **Circuit defaults:** `DisconnectedCircuitRetentionPeriod` = 3 minutes, `DisconnectedCircuitMaxRetained` = 100
- **Azure SignalR Service:** Not needed until scaling out to multiple App Service instances (~2,000+ concurrent users)
- **Scale-up triggers:** Memory consistently above 70% → scale up App Service tier. DTU consistently above 80% → scale up SQL tier.
- **Break-even:** At 5% Pro conversion ($12/mo), infrastructure costs are covered at ~8 paying users

### Social Media

- **X (Twitter):** [@MyTalliApp](https://x.com/MyTalliApp) — verified (blue check, yearly subscription). Profile icon: favicon PNG. Banner: Coming Soon image. Pinned post: launch teaser with branded image.
- **LinkedIn:** [MyTalli company page](https://www.linkedin.com/company/mytalli) — company page under Robert Jordan's personal account. Profile icon: favicon PNG. Description and tagline set.
- **Social assets folder:** `social-assets/` — contains `linkedin-cover.html` (source for LinkedIn cover banner). X Coming Soon image generated from `wireframes/` or `social-assets/`.

### Azure App Service (Blazor Server)

- **App Service Plan:** `mytalli-centralus-asp` (Linux, Standard S1, Central US) — ~$69/mo
- **App Service:** `mytalli-web` (Linux, .NET 10.0)
- **Default domain:** `mytalli-web-f5b9f2a0h4cwdwa6.centralus-01.azurewebsites.net`
- **Resource Group:** `MyTalli-CentralUS-ResourceGroup`
- **Deployment:** Visual Studio Publish to the **staging** slot → verify → **Swap** to production (zero downtime). Sign in as `hello@mytalli.com` (MyTalli tenant). The publish profile (`mytalli-web-staging - Zip Deploy.pubxml`) targets the staging slot directly. Do not use Kudu ZIP deploy — it was unreliable.
- **Deployment slots:** Standard S1 tier — `mytalli-web` (production, 100% traffic) and `mytalli-web-staging` (staging, 0% traffic). Deploy to staging first, warm up, then swap to production for zero-downtime releases.
- **Connection string:** `DefaultConnection` configured as SQLAzure type in App Service Configuration
- **App settings:** OAuth credentials (`Authentication__Google__*`, `Authentication__Microsoft__*`, `Authentication__Apple__*`), ACS connection string, email settings, Stripe keys, and unsubscribe token secret are configured in App Service Configuration (use `__` for nested keys)
- **ElmahCore dependency:** `System.Data.SqlClient` NuGet package explicitly added to `My.Talli.Web.csproj` — required on Linux where ElmahCore.Sql cannot resolve it automatically

### SEO

The landing page (`wireframes/MyTalli_LandingPage.html`) includes:
- `meta description`, `robots`, `theme-color`, `canonical` URL
- Open Graph tags (`og:type`, `og:url`, `og:title`, `og:description`, `og:image`)
- Twitter Card tags (`twitter:card`, `twitter:title`, `twitter:description`, `twitter:image`)
- JSON-LD structured data (`SoftwareApplication` schema with free tier pricing)
- **Favicon:** SVG (`/favicon.svg`) — "T" with ascending growth bars on purple rounded square, using primary purple `#6c5ce7` background and lavender `#a78bfa` bars. Source: `favicon-concepts/favicon-c-growth.svg`
- **OG Share Image:** PNG (`/og-image.png`, 1200×630) — dark navy gradient with favicon icon, "MyTalli" title (lavender accent), tagline with yellow "One dashboard.", platform pills with brand colors (Stripe, Etsy, Gumroad, PayPal, Shopify), and `www.mytalli.com` footer. Source mockup: `favicon-concepts/og-image-mockup.html`

### Accessibility

The landing page (`deploy/index.html` and `wireframes/MyTalli_LandingPage.html`) includes:
- **Skip navigation** — hidden "Skip to main content" link, visible on keyboard focus (`.skip-link`)
- **Landmarks** — `<main id="main">`, `<nav aria-label="Main navigation">`, `<footer role="contentinfo">`
- **Section labeling** — `aria-labelledby` on each content section pointing to its `<h2>` id; `aria-label="Hero"` on hero section
- **Decorative hiding** — `aria-hidden="true"` on hero background shapes, wave divider SVG, section tags, and step numbers
- **Dashboard mockup** — `role="img"` with descriptive `aria-label` (announced as a single image, inner elements hidden)
- **Emoji icons** — wrapped in `<span role="img" aria-label="...">` with descriptive labels
- **Pricing checkmarks** — visually-hidden `<span class="sr-only">Included: </span>` prefix on each list item
- **Step context** — `aria-label="Step 1: Connect your platforms"` etc. on each `.step` div
- **Logo** — `aria-label="MyTalli, go to top of page"` on nav logo link
- **Focus indicators** — `:focus-visible { outline: 3px solid #6c5ce7; outline-offset: 2px; }`
- **Utility class** — `.sr-only` for visually-hidden screen-reader-only text

Deploy folder also contains:
- `favicon.svg` — chosen favicon (concept C)
- `og-image.png` — social share image (1200×630 PNG)
- `robots.txt` — allows all crawlers, references sitemap
- `sitemap.xml` — single entry for `https://www.mytalli.com/` (update as pages are added)

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

### Architecture

- **Stripe Checkout** — hosted payment page for new subscriptions. Created via `StripeBillingService.CreateCheckoutSessionAsync()`, triggered from the Upgrade page.
- **Stripe Customer Portal** — hosted billing management (update payment, view invoices, cancel). Created via `StripeBillingService.CreatePortalSessionAsync()`, triggered from the Subscription page's "Manage Billing" button.
- **Webhooks** — Stripe sends events to `/api/billing/webhook`. The endpoint verifies the signature, then delegates to `StripeWebhookHandler` in the Domain layer. Handled events: `checkout.session.completed`, `customer.subscription.updated`, `customer.subscription.deleted`.
- **Plan switching** — `/api/billing/switch-plan?plan=monthly|yearly` calls `StripeBillingService.SwitchPlanAsync()` and updates the local DB directly (doesn't wait for the webhook). Stripe prorates automatically.
- **`StripeConfiguration.ApiKey`** — set globally at startup in `BillingConfiguration.AddBilling()`.

### Subscription Statuses

| Status | Meaning | Trigger |
|--------|---------|---------|
| `Active` | Subscription is current and billing normally | Checkout completed, reactivation |
| `Cancelling` | User cancelled; active until end of billing period | `cancel_at_period_end = true` on webhook |
| `Cancelled` | Subscription has ended | `customer.subscription.deleted` webhook |
| `PastDue` | Payment failed, grace period | Stripe status `past_due` |
| `Unpaid` | Payment failed, no grace | Stripe status `unpaid` |

- **Cancelling vs Cancelled:** "Cancelling" means the user requested cancellation but still has access until the billing period ends. "Cancelled" means the subscription is fully terminated. The Subscription page shows a warning banner and "Reactivate" button during "Cancelling" state.
- **Queries:** Any query for "active" subscriptions must include both `Active` and `Cancelling` statuses (the user still has Pro access in both states). This applies to: `MyPlanViewModel`, `NavMenuViewModel`, `ManualEntryViewModel`, portal endpoint, switch-plan endpoint.

### Webhook Handler

`StripeWebhookHandler` (`Domain/Handlers/Billing/`) creates all commerce records on checkout:
1. `Order` + `OrderItem` — purchase event
2. `Subscription` + `SubscriptionStripe` — ongoing subscription state
3. `Billing` + `BillingStripe` — payment record

Product resolution uses `ProductId` (not product name). The web-layer `CheckoutCompletedHandler` resolves the product ID from the Stripe price ID via `ResolveProductId()` — mapping `MonthlyPriceId` → 1, `YearlyPriceId` → 2, and module price IDs from the `Stripe:Modules` config. The same pattern exists in `SubscriptionUpdatedHandler`. This allows the webhook to handle Pro plans and module subscriptions identically.

On subscription updates, it syncs status, dates, and product changes. On deletion, it sets status to `Cancelled`.

### CurrentUserMiddleware

`CurrentUserMiddleware` (`Middleware/CurrentUserMiddleware.cs`) runs after `UseAuthorization()` on every request. It reads the `"UserId"` claim from `HttpContext.User` and calls `ICurrentUserService.Set()`. This ensures the `AuditResolver` can stamp audit fields on DB operations in API endpoints. Webhook requests from Stripe have no auth cookie — the `StripeWebhookHandler` sets `ICurrentUserService` manually from the subscription's `UserId`.

**Blazor Server scoping caveat:** `CurrentUserMiddleware` sets `ICurrentUserService` on the HTTP request's DI scope, but the Blazor SignalR circuit creates its **own** DI scope with a fresh `ICurrentUserService` instance. This means the middleware-set user is not available in Blazor components. **Any ViewModel that performs updates via `RepositoryAdapterAsync` must call `CurrentUserService.Set(userId, ...)` in `OnInitializedAsync`** to ensure the `AuditResolver` has the user for audit field stamping. Inserts work without this (they use `userId ?? 0`), but updates require an authenticated user and will throw `InvalidOperationException` if the service is empty. See `ManualEntryViewModel` and `SuggestionBoxViewModel` for the pattern.

### Local Development

- **Stripe CLI listener:** `stripe listen --forward-to https://localhost:7012/api/billing/webhook` — must be running to receive webhooks during local dev.
- **Stripe CLI path:** `C:\Users\Robert\AppData\Local\Microsoft\WinGet\Packages\Stripe.StripeCli_Microsoft.Winget.Source_8wekyb3d8bbwe\stripe.exe`
- **Test card:** `4242 4242 4242 4242`, any future expiry, any CVC.
- **Resend events:** `stripe events resend <event_id>` — useful when the app wasn't running when a webhook fired.

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

### Architecture

Email notifications follow a **Template + Builder** pattern modeled after the Measurement Forms Liquids project:

- **HTML templates** — stored as `EmbeddedResource` files in `Domain/.resources/emails/`, compiled into the assembly, loaded at runtime via `Assembly.GetManifestResourceContent()`
- **Notification classes** — in `Domain/Notifications/Emails/`, abstract base `EmailNotification` → generic `EmailNotificationOf<T>` → concrete implementations (e.g., `ExceptionOccurredEmailNotification`)
- **Placeholder replacement** — templates use `[[Placeholder.Name]]` tokens replaced via `string.Replace()` in the `Build()` method. All user-supplied data is HTML-encoded via `WebUtility.HtmlEncode()` before replacement.
- **SmtpNotification** — serializable POCO carrier returned by `FinalizeEmail()`, passed to `IEmailService.SendAsync()`
- **Azure Communication Services** — `AcsEmailService` (active) sends via ACS Email SDK. `SmtpEmailService` (MailKit) retained as fallback for local dev with smtp4dev.

### Exception Email Pipeline

Unhandled exceptions trigger email notifications via .NET's `IExceptionHandler` interface:

1. Exception occurs → `UseExceptionHandler("/Error")` middleware runs registered `IExceptionHandler` services
2. `ExceptionEmailHandler.TryHandleAsync()` builds the notification and sends the email
3. Handler **always returns `false`** — the middleware continues re-executing to `/Error`, preserving the existing Error page behavior
4. Email failures are caught and logged — they never mask the original exception or break the error page

### Email Configuration

**ACS settings** are bound from `appsettings.json` → `AzureCommunicationServices` section:

- `ConnectionString` — ACS connection string (in `appsettings.Development.json` for dev, App Service Configuration for prod)

**Email settings** are bound from `appsettings.json` → `Email` section via `IOptions<EmailSettings>`:

- `FromAddress` — default `DoNotReply@mytalli.com` (must match an ACS verified sender)
- `FromDisplayName` — default `MyTalli`
- `ExceptionRecipients` — list of admin email addresses; if empty, no exception emails are sent
- `Host`, `Port`, `Username`, `Password`, `UseSsl` — SMTP settings (only used by `SmtpEmailService` fallback)

### Email Branding

There are two tiers of email branding:

| Tier | Audience | Branding Level | Example |
|------|----------|----------------|---------|
| **Internal** | Developers, admins | Simple — MyTalli text logo, brand colors, clean layout | Exception notifications |
| **Customer** | End users | Full — polished design, logo image, professional copywriting, mobile-responsive, tested across email clients | Welcome emails, subscription confirmations, weekly summaries |

- **Internal emails** use the current template style: purple header (`#6c5ce7`) with "MyTalli" text (no image dependency), functional layout, monospace stack traces. Acceptable as-is.
- **Customer-facing emails** use the **Landing Hero** design — an organic purple blob (`#6c5ce7` → `#8b5cf6` → `#6c5ce7` gradient) on the right with dark text on white left, matching the brand swoosh style. Hero uses the **bulletproof background image pattern** (`<td background>` + CSS `background-image` + VML conditional comments for Outlook) with hosted PNGs at `https://www.mytalli.com/emails/`. Body icons use HTML entity emojis (render natively, not blocked). Three customer emails are built: Welcome, Subscription Confirmation, Weekly Summary.

### Adding a New Email Notification

1. Create a payload class in `Domain/Notifications/Emails/` with the data properties
2. Create an HTML template in `Domain/.resources/emails/` with `[[Placeholder]]` tokens — use table-based layout with inline styles for email client compatibility
3. Create a concrete notification class extending `EmailNotificationOf<TPayload>` — implement `Build()` to load the template, replace tokens, and set Subject
4. The `EmbeddedResource` glob in `Domain.csproj` (`**/*.html`) picks up new templates automatically
5. Create a handler/trigger in the Web project that builds and sends the notification via `IEmailService`

### Test Emails (Development Only)

A dev-only endpoint at `GET /api/test/emails` sends all 3 customer emails to `hello@mytalli.com` with sample data via ACS. Only registered when `app.Environment.IsDevelopment()`.

A dev-only endpoint at `GET /api/test/unsubscribe-token/{userId:long}` generates an unsubscribe token for testing the `/unsubscribe` page.

### Unsubscribe Token

All customer emails include a tokenized unsubscribe link (`/unsubscribe?token=xxx`) so users can manage email preferences without signing in (CAN-SPAM compliance).

- **Token format:** `Base64Url(userId + "." + HMAC-SHA256-signature)` — no expiration (unsubscribe links must work indefinitely)
- **Service:** `UnsubscribeTokenService` (`Domain/Components/Tokens/`) — `GenerateToken(long userId)` / `ValidateToken(string? token) → long?`
- **Config:** `UnsubscribeToken:SecretKey` in `appsettings.json` (bound via `UnsubscribeTokenSettings`)
- **Generation:** Auth handlers generate the token during sign-up and pass it to the email payload's `UnsubscribeToken` property
- **Template placeholder:** `[[UnsubscribeUrl]]` — replaced in each notification's `Build()` method with the full tokenized URL
- **Unsubscribe page:** `/unsubscribe?token=xxx` — validates token, loads user preferences, renders toggle UI for email opt-in/out. Invalid/missing token shows a fallback with "Sign In" CTA.

### Embedded Resource Naming

Templates embedded from `Domain/.resources/emails/` get resource names like:
`My.Talli.Domain..resources.emails.{FileName}.html` (dots replace path separators, the leading dot in `.resources` creates a double dot). Use `assembly.GetManifestResourceNames()` to debug if a template fails to load.

## Platform API Notes

Integration with each revenue platform uses OAuth so users grant MyTalli read-only access to their sales/payment data. Full comparison document: `documentation/MyTalli_PlatformCapabilities.html`. Data shapes, normalized schema, and ERD: `documentation/PlatformApiDataShapes.html`.

**Integration priority:** Stripe → Gumroad → Etsy → Shopify → PayPal (based on data richness, complexity, and approval timelines).

### Revenue Sync Architecture

Platform API rate limits are **application-level** — they apply to MyTalli's API keys, not per-user. If hundreds of users connect the same platform, every API call counts against one shared limit. Hitting platform APIs on every page load would exhaust rate limits almost immediately at scale.

**Solution: Local Cache + Periodic Sync**

1. **Dashboard reads are local only.** When a user visits the dashboard (or any revenue-displaying page), all data comes from `app.Revenue` in our database. No external API calls are made during page loads.
2. **Background sync job runs once per hour.** A scheduled process iterates through all users with connected platforms, pulls their latest transactions from each platform's API, and upserts into `app.Revenue`. The job controls its own pace — adding delays between users and between platforms to stay within each platform's rate limits.

This means:
- Users always see data instantly (local DB read)
- Rate limits are manageable (one controlled background process, not N concurrent users)
- The sync job can spread work across the full hour, throttle per-platform, and retry failures without affecting the user experience

**`app.Revenue` is the single source of truth for the dashboard.** Both API-sourced data (from the sync job) and manual entries flow into this same normalized table. The `Platform` column distinguishes the source ("Stripe", "Etsy", "Manual", etc.) and `PlatformTransactionId` prevents duplicate inserts during re-syncs.

**`app.SyncQueue`** — the sync job's work list. One row per user per connected platform.

- `Id` (PK), `UserId` (FK → auth.User), `Platform` (string — "Stripe", "Etsy", "Gumroad", "PayPal", "Shopify"), `Status` (Pending, InProgress, Completed, Failed), `LastSyncDateTime` (nullable — null until first successful sync), `NextSyncDateTime` (when this row is next eligible for processing), `LastErrorMessage` (nullable — most recent failure reason), `ConsecutiveFailures` (int, default 0 — for exponential backoff), `IsEnabled` (bool, default true — user can pause syncing)
- Unique constraint on `(UserId, Platform)` — one queue entry per user per platform
- Index on `(NextSyncDateTime, Status)` — the sync job queries "give me rows where `NextSyncDateTime <= now AND Status = Pending AND IsEnabled = true`", ordered by `NextSyncDateTime` ASC (oldest first)
- **Row lifecycle:** Created when a user connects a platform. `NextSyncDateTime` set to now (immediate first sync). After each sync: `LastSyncDateTime` = now, `NextSyncDateTime` = now + 1 hour, `Status` = Pending, `ConsecutiveFailures` = 0. On failure: `ConsecutiveFailures` incremented, `NextSyncDateTime` pushed out with exponential backoff, `LastErrorMessage` updated.
- **Backoff strategy:** On failure, `NextSyncDateTime` = now + (base interval × 2^ConsecutiveFailures), capped at a max delay (e.g., 24 hours). This prevents hammering a platform that's down or rate-limiting us.
- **Sync pause:** Users can toggle `IsEnabled` off to pause syncing for a connected platform. The platform remains connected and still counts against the plan limit.
- **No disconnect.** Once a platform is connected, it cannot be disconnected. This prevents Free tier users from gaming the platform limit by rotating connections — connect one platform, sync it, disconnect, connect another. A connected platform permanently occupies a plan slot.
- **Pre-connection warning.** Before completing a platform connection, the user must be shown a confirmation dialog explaining that this connection is permanent and will occupy a plan slot. The user must explicitly confirm before the OAuth flow begins.

**Sync completion notification:** When a sync completes for a user, the app notifies them in real time via the Blazor SignalR circuit. If the user is currently on the dashboard (or any revenue-displaying page), they see a non-intrusive toast or banner (e.g., "Stripe data updated just now") so they know fresh data is available. The page can then refresh its data from `app.Revenue` without a full page reload. If the user is not online, no notification is needed — they'll see the latest data on their next visit. Failed syncs do not notify the user (the backoff mechanism handles retries silently); persistent failures surface on the Platforms page as a connection status indicator.

### Stripe

- **API:** REST API — [docs.stripe.com/api](https://docs.stripe.com/api)
- **Auth:** OAuth 2.0 via Stripe Connect (Standard accounts), scope: `read_only`. Access token: 1hr, refresh token: 1yr rolling.
- **Key endpoints:** Balance Transactions (`/v1/balance_transactions`), Charges, PaymentIntents, Payouts, Refunds
- **Data richness:** Excellent — gross, net, fee (per-component breakdown), currency, payout schedule, exchange rates
- **Webhooks:** Full catalog (`charge.succeeded`, `charge.refunded`, `payout.paid`, etc.). Connect webhook endpoint with `account` property per connected account.
- **Rate limits:** 25 read req/s per endpoint, 100 req/s global (live)
- **Approval:** None for Connect OAuth. Stripe App Marketplace listing requires ~4 day review.
- **Caveat:** Stripe is steering new platforms toward Stripe Apps (Marketplace) rather than traditional Connect OAuth. Confirm recommended path with Stripe support before building.

### Etsy

- **API:** Etsy Open API v3 (REST) — [developers.etsy.com](https://developers.etsy.com/)
- **Auth:** OAuth 2.0 + PKCE (S256), scopes: `transactions_r shops_r`. Access token: 1hr, refresh token: 90 days.
- **Key endpoints:** Shop Receipts, Transactions, Payments, Ledger Entries (running account balance)
- **Data richness:** Good — order totals, item prices, shipping, taxes, Etsy fees, refunds, multi-currency
- **Webhooks:** 4 order events (`order.paid`, `order.canceled`, `order.shipped`, `order.delivered`). Payloads are lightweight (URL only) — require follow-up API call for data.
- **Rate limits:** ~10 QPS, ~10,000 QPD (sliding window). Receipts endpoint may enforce 1 req/s/shop.
- **Approval:** **Commercial access required** for 4+ shops (~20+ day review). Apply early — approved at Etsy's sole discretion.
- **Caveat:** Refresh token expires in 90 days — if a user doesn't visit MyTalli for 3 months, their connection breaks and they must re-authorize.
- **Developer account:** Registered under `hello@mytalli.com` at [developers.etsy.com](https://developers.etsy.com/). App name: `mytalli`. Keystring (client ID): `nqbjy0nj18t8o0d1yudbzr5t`.
- **Test shop:** `MyTalliTestShop` — shop creation paused at the payment setup step. Waiting for LLC approval → EIN → business bank account before completing setup.

### Gumroad

- **API:** REST API v2 — [gumroad.com/api](https://gumroad.com/api)
- **Auth:** OAuth 2.0, scope: `view_sales`. Access tokens **never expire** — simplest auth of all platforms.
- **Key endpoints:** Sales (`/v2/sales` with date filtering), Products, Subscribers
- **Data richness:** Basic — sale amount, flat Gumroad fee (10%, no breakdown), product details, refunds, subscriptions
- **Webhooks:** Ping feature — `sale`, `refund`, `subscription_updated`, `subscription_ended`. HMAC-SHA256 verification.
- **Rate limits:** Undocumented — implement adaptive backoff
- **Approval:** None — immediate access
- **Caveats:** No payout/disbursement API. No net amount (calculate manually). API docs are sparse. Platform stability uncertain (open-sourced, company changes).

### PayPal

- **API:** REST API v1 — [developer.paypal.com/docs/api/transaction-search/v1/](https://developer.paypal.com/docs/api/transaction-search/v1/)
- **Auth:** OAuth 2.0 Authorization Code via "Log In with PayPal", scopes: `openid` + `reporting/search/read` + `reporting/balances/read`. Access token: ~8hr, refresh token: 180 days.
- **Key endpoints:** Transaction Search (`/v1/reporting/transactions`, 31-day max range, 500/page, 10K max records), Balance (`/v1/reporting/balances`)
- **Data richness:** Good — transaction amount, fees, status, timestamp, payer info (not anonymized), multi-currency balances. No net amount field (calculate gross - fees). Payouts via T-code filtering.
- **Webhooks:** Full catalog (`PAYMENT.CAPTURE.COMPLETED`, `PAYMENT.CAPTURE.REFUNDED`, etc.). Up to 10 webhook URLs per app. Retries up to 25 times over 3 days.
- **Rate limits:** ~50 req/min per IP (dynamic, not formally published)
- **Approval:** Reporting scopes require PayPal approval (24-72hr). **Third-party access may require Partner program enrollment** — path is unclear. Contact PayPal partner team early.
- **Caveats:** Transaction data delayed 3-72 hours in Search API — must use webhooks for real-time. 31-day max date range per query (12+ calls for a year). Refresh token expires at 180 days.

### Shopify

- **API:** GraphQL Admin API (required for new apps since April 2025) — [shopify.dev/docs/api/admin-graphql](https://shopify.dev/docs/api/admin-graphql/latest)
- **Auth:** OAuth 2.0 Authorization Code, scopes: `read_orders` (60 days) + `read_all_orders` (full history, requires approval) + `read_shopify_payments_payouts` + `read_shopify_payments_accounts`. Offline access token: 60min, refresh token: 90 days rolling. **Expiring offline tokens mandatory for new apps April 1, 2026.**
- **Key endpoints:** Orders (with nested transactions, refunds in single GraphQL query), Shopify Payments Balance/Payouts/Balance Transactions
- **Data richness:** Good — order totals, subtotals, taxes, shipping, discounts, multi-currency (shop + presentment). **Fee/net data only available for Shopify Payments merchants** — third-party gateway merchants only have gross amounts.
- **Webhooks:** Full catalog (`orders/paid`, `refunds/create`, `order_transactions/create`, `disputes/create`). HMAC-SHA256 verification. Delivery not guaranteed — retries for 48hr.
- **Rate limits:** 1,000pt bucket, 50pt/s restore (Standard plans). GraphQL calculated query cost.
- **Approval:** `read_all_orders` is a protected scope — request in Partner Dashboard. Unlisted public app distribution (no App Store listing required).
- **Caveats:** Fee data is Shopify Payments only. 60-day order limit without `read_all_orders` approval. GraphQL only (no REST for new apps). Mandatory compliance webhooks (`customers/data_request`, `customers/redact`, `shop/redact`).

## Planned Features

- Real-time revenue tracking across connected platforms
- Trends & month-over-month comparisons
- Revenue goals with visual progress tracking
- CSV export for tax prep / bookkeeping
- Weekly email summaries (Pro tier)

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
- **Admin page is the reference implementation** for new sidebar pages. Match its SVG (`viewBox="0 0 1000 600"`, swoosh path, gradient fill), hero-bg (`height: calc(100% + 60px)`), and SVG CSS (`min-height: 280px`) exactly. Pages with hero stats use `margin: -32px -40px 0` and `padding: 24px 40px 40px`; pages without stats use `margin: -32px -40px 60px` and `padding: 24px 40px 48px`.
- **Hero stat numbers** use colorized `nth-child` styling: 1st stat → lavender `#a78bfa`, 2nd stat → contextual color (green `#2ecc71` for money/success, gold `#f5c842` for counts), 3rd stat → white `#fff`. Font size is `22px` on all pages — keep this consistent. Labels are `rgba(255, 255, 255, 0.6)` at `12px`.
- **Hero stat labels display inline to the right of the number**, never below it. Use `margin-left: 6px` on the label (or flex with `gap`) — never `flex-direction: column` on the stat container.
- **Never use CSS `background: linear-gradient(...)` on the hero section.** The SVG gradient provides the purple — this is what creates the curved swoosh edge instead of a flat block.

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
- **Dataset classes** — all faked/sample data lives in static classes in `Models/SampleData/` named `{Feature}Dataset` (e.g., `DashboardDataset`, `GoalsDataset`, `ManualEntryDataset`). Each class returns typed collections or values via static methods.
- **`IsSampleData` flag** — on the ViewModel, controls the banner visibility. When `true`: CTA banner shown, "New Entry" / action buttons hidden, edit/delete hidden, grid fully interactive (sort, paginate, density).
- **The page doesn't know or care** whether data is real or sample — it renders the same grid either way.
- **Grid preferences** still save for sample data viewers — their density/sort/page size choices persist.

### Mobile-First Responsive Strategy

- **Principle: "Keyhole Data"** — phones are for glancing at numbers, not configuring things. Desktop gets the full experience; mobile gets a focused, read-only snapshot.
- **Don't block routes** — never return a 404 or redirect based on viewport. If a user deep-links to a desktop-oriented page on mobile, show a friendly "better on desktop" message with a link back to the dashboard.
- **Hide non-mobile nav items** — on small screens, hide sidebar links for pages that don't render well on mobile (e.g., Platforms, Export, Settings). Keep Dashboard, Goals, Suggestions visible.
- **Simplify, don't remove** — pages that are visible on mobile should render a simplified "keyhole" view, not the full desktop layout. Example: Goals on mobile shows progress bars and numbers, not the full goal editor.
- **Decide per page** — each page's mobile treatment is determined when building that page, not planned upfront. The content will make the right answer obvious.

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
| **Settings** | `/settings` | Account preferences, email settings, linked providers | Hidden |
| **Admin** | `/admin` | Email resend, bulk welcome send, user list (Admin role only) | Hidden |

### Sample Data for New Users

- **New users with no connected platforms** see sample/mock data on the dashboard so they can immediately understand the product's value. An empty dashboard would be a dead end.
- **Sample data banner** — when sample data is active, a branded banner is shown: "You're viewing **sample data**. Connect a platform to see your real revenue." with a CTA to `/platforms`.
- **`IsSampleData` flag** — `DashboardViewModel.IsSampleData` controls whether the banner is visible. Set to `true` by default; set to `false` once the user has at least one connected platform.
- **Once a platform is connected**, sample data disappears entirely and real data takes over. No mixing of sample and real data.

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

Endpoint-supporting logic lives in dedicated classes under `Handlers/` and `Commands/` in the web project, organized by subfolder.

- **Handlers** (`Handlers/Endpoints/`) — react to events. They orchestrate the pipeline: map external objects (e.g., Stripe SDK types) to Domain payloads, call Domain handlers inside transactions, handle side effects (logging, emails). Each handler owns everything it does — mapping methods, email building, etc. live inside the handler, not back in the endpoint.
- **Commands** (`Commands/`) — execute actions. Data access operations (queries, updates), notification sending, or any reusable operation that a handler or endpoint shouldn't inline. Each command exposes a single `ExecuteAsync()` method. Organized by subfolder based on **what the command does**, not who calls it: `Commands/Endpoints/` for data access commands, `Commands/Notifications/` for email/notification commands, etc.
- Both are **non-static classes** with constructor-injected dependencies — no `HttpContext.RequestServices.GetRequiredService` calls.
- Both are registered as **scoped** in `BillingConfiguration.cs` (or the relevant `Configuration/{Name}Configuration.cs`).
- **One class per operation** — not one class per domain area. `CheckoutCompletedHandler` handles checkout completed events, not "all billing webhook events."
- **Namespace:** `My.Talli.Web.Handlers.Endpoints` for handlers, `My.Talli.Web.Commands.Endpoints` for commands. The `Endpoints` subfolder is organizational only (following the Subfolder Namespace Convention).

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

- **WAVE** (wave.webaim.org) — web accessibility evaluation tool. Paste a URL to get a visual overlay of ARIA landmarks, contrast errors, heading structure, and missing labels. Note: WAVE cannot evaluate contrast for text over positioned/overlapping backgrounds (e.g., nav links over the hero gradient) — expect false positives there.
- **Lighthouse** — built into Chrome DevTools (F12 > Lighthouse tab). Scores accessibility, performance, SEO, and best practices out of 100.
- **axe DevTools** — Chrome extension by Deque. Runs in the Elements panel and catches WCAG violations with fix suggestions.
- **NVDA** (nvaccess.org) — free Windows screen reader for manual testing of the full blind-user experience.

### Accessibility Notes

- **WAVE contrast errors (28):** Mostly false positives from nav links (`rgba(255,255,255,0.85)`) over the purple hero gradient — WAVE sees them against the white `<body>` background. A few real failures exist on platform brand colors (Shopify `#96bf48`, Gumroad `#ff90e8`, Etsy `#f56400` on `#f8f7fc`), but these are intentional brand colors kept as-is.
- **WAVE alert (1):** Skipped heading level — the `<h3>` inside the dashboard mockup jumps from `<h1>`. Harmless because the mockup is marked `role="img"` with a descriptive `aria-label`.

## Etsy Setup TODO

- [x] **Developer Account** — registered under `hello@mytalli.com` at [developers.etsy.com](https://developers.etsy.com/)
- [x] **App Registration** — app name `mytalli`, Seller Tools, commercial, read sales data. Keystring: `nqbjy0nj18t8o0d1yudbzr5t`. Status: Pending Personal Approval.
- [ ] **API Key Approval** — waiting for Etsy to approve the personal access key (check back on developer dashboard periodically — no notification on denial)
- [x] **Test Shop (started)** — `MyTalliTestShop` created through shop preferences and naming steps. Paused at payment setup — requires business bank account.
- [ ] **Test Shop (complete)** — finish shop setup after LLC approval → EIN → business bank account. Remaining steps: payment info, billing info, shop security.
- [ ] **API Keys to Config** — add Keystring and Shared Secret to `appsettings.Development.json` (`Etsy:ClientId`, `Etsy:ClientSecret`)
- [ ] **Commercial Access** — apply for commercial access (4+ shops) before public launch (~20-day review)

## Stripe Setup TODO

- [x] **Stripe Account** — created sandbox under `robertmerrilljordan@gmail.com`
- [x] **Branding** — brand color `#6c5ce7`, accent `#8b5cf6`, icon uploaded (favicon PNG)
- [x] **Business Model** — Platform (not Marketplace)
- [x] **Payment Integration** — Prebuilt checkout form (Stripe Checkout Sessions)
- [x] **Products & Prices** — Pro product with two prices: monthly ($12/mo, default) and yearly ($99/yr, description "Annual"). Product ID: `prod_UBpqjWROUeH1OY`. Monthly Price ID: `price_1TDSAwRC4AM5SkTgiNbOw53a`. Yearly Price ID: `price_1TDSHVRC4AM5SkTgToKjXCny`. Free tier has no Stripe product (it's just the absence of a subscription). Manual Entry Module: Product ID `prod_UEPfDUVNr9l4kJ`, Price ID `price_1TFwpvRC4AM5SkTgEZMliKrz` ($3/mo). Module price IDs are configured in `Stripe:Modules` (key = DB product ID, value = Stripe price ID).
- [x] **Webhook Endpoint** — using Stripe CLI local listener (`stripe listen --forward-to https://localhost:7012/api/billing/webhook`). Stripe CLI installed via winget at `C:\Users\Robert\AppData\Local\Microsoft\WinGet\Packages\Stripe.StripeCli_Microsoft.Winget.Source_8wekyb3d8bbwe\stripe.exe`.
- [x] **API Keys** — test keys added to `appsettings.Development.json` (`Stripe:SecretKey`, `Stripe:PublishableKey`)
- [x] **Webhook Secret** — webhook signing secret added to `appsettings.Development.json` (from Stripe CLI listener)
- [x] **Customer Portal** — configured: customer info (name, email, billing address, phone), payment methods, cancellations (end of billing period, collect reason). Portal Configuration ID: `bpc_1TDSZQRC4AM5SkTggFFtu6cQ`.
- [x] **Test Checkout Flow** — end-to-end verified: Upgrade page → Stripe Checkout → webhook → DB records → Subscription page shows Pro. Also tested: plan switching (monthly ↔ yearly), cancel (end-of-period with "Cancelling" state), reactivate via Customer Portal.
- [ ] **Production Keys** — add live keys to Azure App Service Configuration (when ready to go live)
- [ ] **Custom Domains** — `pay.mytalli.com` (Checkout), `billing.mytalli.com` (Customer Portal) — production only, CNAME records in GoDaddy

## Blazor TODO

Features already shipped in the static HTML landing page (`deploy/index.html`) that still need to be ported to the Blazor app:

- [x] **SEO** — meta description, robots, canonical URL, Open Graph tags, Twitter Card tags, JSON-LD structured data (`SoftwareApplication` schema)
- [x] **Favicon** — link `favicon.svg` (concept C — T + growth bars) in `App.razor` `<head>`
- [x] **Social Share Image** — add `og-image.png` (1200x630) to `wwwroot/` and reference in OG/Twitter meta tags
- [x] **Accessibility** — skip navigation link, `<main>` landmark, ARIA labels on nav/sections, `aria-hidden` on decorative SVGs, emoji `role="img"` labels, `.sr-only` utility class, `:focus-visible` outlines, `role="contentinfo"` on footer, visually-hidden "Included:" prefixes on pricing checkmarks

Upcoming features:

- [x] **Admin Page** — role-based admin section (`/admin`) with email management: resend any customer email (Welcome, Subscription Confirmation, Weekly Summary) to a specific user, bulk-send Welcome emails to selected or all users. Visible only to `Admin` role via conditional NavMenu link. Uses `vAuthenticatedUser` view (keyless entity) for user list with emails. ViewModel redirects non-admins to `/dashboard`; API endpoints enforce Admin role via `.RequireAuthorization()`.
- [x] **Admin Email Resend** — admin ability to resend any customer email (Welcome, Subscription Confirmation, Weekly Summary) to a specific user, plus bulk-send Welcome emails to selected or all users. Implemented as part of the Admin page (`/admin`). API endpoints: `POST /api/admin/email/resend`, `POST /api/admin/email/bulk-welcome`, `POST /api/admin/email/bulk-welcome-all`. Commands: `SendSubscriptionConfirmationEmailCommand` (validates active subscription exists), `SendWeeklySummaryEmailCommand` (uses sample data). Fail-silent on individual errors during bulk sends.
- [x] **Manual Entry Module** — `app.Revenue` (base normalized revenue table) and `app.RevenueManual` (1-to-1 manual entry detail, includes `Quantity` column). Sold as a monthly module subscription ($3/mo). Product seeded as `commerce.Product` Id 3, `commerce.ProductType` "Software Module" Id 2. Page at `/manual-entry` with data grid (sortable columns, user-selectable pagination 10/25/50, row density toggle compact/comfortable/spacious). Grid columns: Date, Description, Category, Qty, Price (unit price), Fees, Net, Actions. **Quick-entry row** pinned at top of `<tbody>` for fast new entries — type description + price, hit Enter, row resets and refocuses. **Inline editing** for existing rows — click Edit, row cells become inputs, Enter to save, Escape to cancel. Notes toggle via icon button expands a sub-row below the editing row. No modal. `New*` fields serve quick-entry, `Edit*` fields serve inline edit (separate state). Grid preferences (density, page size, sort) persist in `UserPreferences` JSON under `gridPreferences["manualEntry.entryGrid"]`. Non-subscribers see sample data (`ManualEntryDataset`) with CTA banner instead of a lock gate. Delete uses `ConfirmDialog` component. Empty state renders inside grid tbody. Categories: Sale, Service, Freelance, Consulting, Digital Product, Physical Product, Other.
- [x] **My Plan Page** — consolidated plan and module management at `/my-plan`. Replaces the old `/subscription` and `/upgrade` pages (both deleted). Free users see inline pricing cards (Free vs Pro with monthly/yearly toggle). Pro users see their plan card with billing actions (Manage Billing, Change Plan, Cancel). Module owners see per-module cards with billing/cancel. Available modules listed at the bottom. Sidebar upgrade card shows "Pro Plan" for subscribers, "Upgrade to Pro" for free users, with a single "My Plan" button.
- [ ] **Navigation & Data Architecture** — organizing grids, graphs, and reports across platforms (Manual Entry, Stripe, Etsy, Gumroad, PayPal, Shopify) plus an aggregate view. Each platform needs Revenue, Expenses, Payouts, and Cashflow sections. Four navigation patterns wireframed in `wireframes/MyTalli_NavigationPatterns.html`: (A) Hub & Spoke, (B) Data-Type First, (C) Hybrid, (D) Dashboard + Tabs. Three mobile treatments wireframed per pattern in `wireframes/MyTalli_MobilePatterns_Hub_and_Spoke.html` and `wireframes/MyTalli_MobilePatterns_Dashboard_plus_Tabs.html`. **Decision: Hub & Spoke** — aggregate dashboard is the hub, each connected platform is a spoke with its own detail page containing tabs for Overview/Revenue/Expenses/Payouts. Sidebar shows Dashboard, then a Platforms group listing connected platforms. **Mobile Decision: Keyhole Hybrid** (wireframe: `wireframes/MyTalli_MobilePatterns_Keyhole_Hybrid.html`) — combines Desktop Message + Keyhole View. Formula: summary cards with mini charts at top, platform-specific stats on spokes (avg. sale, fee rate, payout status), 5 most recent records as phone-native transaction cards (not grid rows), and a "details on desktop" CTA at the bottom. Hub shows cross-platform activity (with platform color dots); spokes show platform-filtered activity (no dots needed). The 5-card cap sets a clear boundary — this is a preview, not a portal. **Constraint:** regardless of pattern chosen, all page heroes must remain branded with the purple gradient swoosh (see "Page Hero Branding" rule). Platform-specific pages may tint the gradient with platform brand colors (e.g., Etsy orange) but must keep the swoosh shape and decorative circles.
- [ ] **Module Checkout Flow** — extend `/api/billing/create-checkout-session` to handle module product IDs (currently only handles `plan=monthly|yearly` for Pro). Needed for "Add Module" button on My Plan page.
- [ ] **Email Asset Hosting** — email image assets (`email-hero-bg.png`, `email-icon-graph.png`) are currently served from `wwwroot/emails/` on the App Service (deployed with the app). Phase 2: migrate to Azure Blob Storage with a public container (e.g., `https://mytallistorage.blob.core.windows.net/emails/`) and update all 3 customer email template URLs. This decouples email assets from app deployments so images are always available regardless of deploy state.
