# My.Talli

## What Is This?

MyTalli is a side-hustle revenue aggregation dashboard. It lets creators and freelancers connect their payment platforms (Stripe, Etsy, Gumroad, PayPal, Shopify, etc.) and see all their income in one unified dashboard with real-time tracking, trends, goals, and CSV export.

**Status:** Early development — **Waitlist Mode** active (see below). Landing page, sign-in, waitlist, and dashboard pages are built. OAuth authentication is working (Google, Apple, Microsoft). Sign-in currently redirects to the waitlist; dashboard and other routes are disabled until Dashboard Mode is enabled.

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
- **No separate waitlist table** — the `auth.User` table doubles as the waitlist during Waitlist Mode. A signed-up user *is* a waitlist user until Dashboard Mode is enabled.
- **No milestones table** — milestones are hardcoded in the Waitlist page UI, not stored in the database.
- **No third-party table creation** — third-party packages (e.g., ElmahCore) must never create their own tables. All tables are created by our migrations so we own the schema, naming conventions, and migration history. If a package needs a table, create it in a migration SQL script with an `IF NOT EXISTS` guard.
- **Audit field self-creation sentinel** — `CreateByUserId = 0` means "self-created" (the user created their own account). This avoids a second database round-trip to self-stamp the generated Id. Only applies to `auth.User` rows created during OAuth sign-up.
- **Audit fields on insert** — on INSERT, only `CreateByUserId` and `CreatedOnDateTime` are populated. `UpdatedByUserId` and `UpdatedOnDate` remain `null` — nothing has been updated yet. They are only set on the first actual UPDATE.

### Schemas

| Schema | Purpose | Tables |
|--------|---------|--------|
| `auth` | Identity & authentication | User, UserAuthenticationGoogle, UserAuthenticationApple, UserAuthenticationMicrosoft, UserRole |
| `commerce` | Products, orders, billing, subscriptions | ProductVendor, ProductType, Product, Order, OrderItem, Billing, BillingStripe, Subscription, SubscriptionStripe |
| `components` | Third-party component tables (not EF-managed) | ELMAH_Error (auto-created by ElmahCore) |
| `dbo` | Reserved (empty) | — |

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
    }
  }
  ```
  - Models: `UserPreferences` (root) → `EmailPreferences` (nested), both in `Domain/Models/`
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
├── wireframes/                     # Standalone HTML mockups & design concepts
│   ├── MyTalli_ColorPalette.html   # Brand color reference sheet (light mode)
│   ├── MyTalli_DarkModePalette.html # Brand color reference sheet (dark mode)
│   ├── MyTalli_Dashboard.html      # Static dashboard mockup (post-login)
│   ├── MyTalli_LandingPage.html    # Static landing page mockup
│   ├── MyTalli_Email_Welcome.html  # Welcome email wireframe (PNG-based hero)
│   ├── MyTalli_Email_SubscriptionConfirmation.html # Subscription confirmation email wireframe
│   ├── MyTalli_Email_WeeklySummary.html # Weekly summary email wireframe
│   ├── MyTalli_SuggestionBoxConcepts.html # Suggestion box design concepts (A/B/C)
│   └── MyTalli_WaitlistConcepts.html # Waitlist page design concepts (A/B/C)
└── Source/
    ├── My.Talli.slnx               # Solution file (XML-based .slnx format)
    ├── .claude/settings.local.json
    ├── Domain/                      # Domain layer (exceptions, shared types, framework)
    │   ├── Domain.csproj
    │   ├── .resources/
    │   │   └── emails/              # HTML email templates (EmbeddedResource)
    │   │       ├── ExceptionOccurredEmailNotificationTemplate.html
    │   │       ├── SubscriptionConfirmationEmailNotificationTemplate.html
    │   │       ├── WaitlistWelcomeEmailNotificationTemplate.html
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
    │   │   └── Roles.cs                       # Static role name constants (Admin, User)
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
    │   │       ├── UserAuthenticationAppleMapper.cs
    │   │       ├── UserAuthenticationGoogleMapper.cs
    │   │       ├── UserAuthenticationMicrosoftMapper.cs
    │   │       ├── UserMapper.cs
    │   │       └── UserRoleMapper.cs
    │   ├── Models/
    │   │   ├── ActionResponseOf.cs            # Generic response wrapper (ValidationResult + Payload)
    │   │   ├── EmailPreferences.cs            # Email opt-in/out preferences model
    │   │   ├── UserPreferences.cs             # Root user preferences model (wraps EmailPreferences)
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
    │   │   │   ├── User.cs
    │   │   │   ├── UserAuthenticationApple.cs
    │   │   │   ├── UserAuthenticationGoogle.cs
    │   │   │   ├── UserAuthenticationMicrosoft.cs
    │   │   │   └── UserRole.cs
    │   │   └── Presentation/                  # Aggregate/detail view models (future)
    │   ├── Handlers/
    │   │   └── Authentication/                # Sign-in handlers (one per OAuth provider)
    │   │       ├── EmailLookupService.cs       # Cross-provider email lookup for duplicate prevention
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
    │           │   ├── WaitlistWelcomeEmailNotification.cs
    │           │   ├── WaitlistWelcomeEmailNotificationPayload.cs
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
    │   ├── Repositories/
    │   │   ├── GenericRepositoryAsync.cs  # IRepositoryAsync<T> implementation
    │   │   └── GenericAuditableRepositoryAsync.cs # IAuditableRepositoryAsync<T> implementation
    │   ├── Resolvers/
    │   │   └── AuditResolver.cs           # IAuditResolver<T> implementation
    │   └── Configurations/
    │       ├── Auth/                      # Entity configs for auth schema
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
    ├── Domain.Entities/             # Domain entity layer (database models)
    │   ├── Domain.Entities.csproj
    │   ├── AuditableIdentifiableEntity.cs  # Base class (Id + audit fields)
    │   ├── DefaultEntity.cs                # Standard entity base (adds IsDeleted, IsVisible)
    │   ├── Entities/
    │   │   ├── Billing.cs
    │   │   ├── BillingStripe.cs
    │   │   ├── Order.cs
    │   │   ├── OrderItem.cs
    │   │   ├── Product.cs
    │   │   ├── ProductType.cs
    │   │   ├── ProductVendor.cs
    │   │   ├── Subscription.cs
    │   │   ├── SubscriptionStripe.cs
    │   │   ├── User.cs
    │   │   ├── UserAuthenticationApple.cs
    │   │   ├── UserAuthenticationGoogle.cs
    │   │   ├── UserAuthenticationMicrosoft.cs
    │   │   └── UserRole.cs
    │   └── Interfaces/
    │       ├── IAuditable.cs
    │       ├── IAuditableIdentifiable.cs
    │       └── IIdentifiable.cs
    └── My.Talli.Web/               # Blazor Server web project
        ├── My.Talli.Web.csproj
        ├── Program.cs              # App entry point, pipeline setup (delegates to Configuration/ and Endpoints/)
        ├── Configuration/             # Service registration extension methods (one per concern)
        │   ├── AuthenticationConfiguration.cs  # OAuth providers (Google, Microsoft, Apple) + auth handlers
        │   ├── BillingConfiguration.cs         # Stripe settings + service
        │   ├── DatabaseConfiguration.cs        # DbContext registration
        │   ├── ElmahConfiguration.cs           # Elmah error logging
        │   ├── EmailConfiguration.cs           # Email services + unsubscribe token
        │   └── RepositoryConfiguration.cs      # Repositories, mappers, sign-in handlers
        ├── Endpoints/                 # Minimal API endpoint extension methods (one per route group)
        │   ├── AuthEndpoints.cs       # /api/auth/login, /api/auth/logout
        │   ├── BillingEndpoints.cs    # /api/billing/create-checkout-session, portal, webhook
        │   ├── EmailEndpoints.cs      # /api/email/preferences
        │   └── TestEndpoints.cs       # /api/test/* (dev-only)
        ├── Middleware/                 # Custom middleware classes
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
        │   │   ├── CancelSubscription.razor  # Cancel subscription retention page (route: /subscription/cancel)
        │   │   ├── CancelSubscription.razor.css
        │   │   ├── Dashboard.razor       # Dashboard (route: /dashboard)
        │   │   ├── Dashboard.razor.css
        │   │   ├── LandingPage.razor     # Landing page (route: /)
        │   │   ├── LandingPage.razor.css
        │   │   ├── SignIn.razor          # Sign-in page (route: /signin)
        │   │   ├── SignIn.razor.css
        │   │   ├── Subscription.razor    # Subscription hub (route: /subscription)
        │   │   ├── Subscription.razor.css
        │   │   ├── SuggestionBox.razor       # Suggestion box (route: /suggestions)
        │   │   ├── SuggestionBox.razor.css
        │   │   ├── Upgrade.razor         # Upgrade pricing page (route: /upgrade)
        │   │   ├── Upgrade.razor.css
        │   │   ├── Unsubscribe.razor      # Email preference management (route: /unsubscribe?token=xxx)
        │   │   ├── Unsubscribe.razor.css
        │   │   ├── Waitlist.razor        # Waitlist progress tracker (route: /waitlist)
        │   │   ├── Waitlist.razor.css
        │   │   ├── Error.razor           # Branded error page (routes: /Error, /Error/{StatusCode})
        │   │   └── Error.razor.css
        │   └── Shared/
        │       ├── BrandHeader.razor     # Reusable purple swoosh header (logo + action slot)
        │       └── BrandHeader.razor.css
        ├── Helpers/
        │   └── LayoutHelper.cs            # Static helpers (CurrentYear, VersionNumber) for layouts
        ├── Services/
        │   ├── Authentication/
        │   │   ├── AppleAuthenticationHandler.cs
        │   │   ├── GoogleAuthenticationHandler.cs
        │   │   └── MicrosoftAuthenticationHandler.cs
        │   ├── Billing/
        │   │   ├── StripeBillingService.cs  # Stripe Checkout & Portal API wrapper
        │   │   └── StripeSettings.cs        # Stripe configuration POCO
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
        │   │   ├── CancelSubscriptionViewModel.cs
        │   │   ├── DashboardViewModel.cs
        │   │   ├── ErrorViewModel.cs
        │   │   ├── LandingPageViewModel.cs
        │   │   ├── SignInViewModel.cs
        │   │   ├── SubscriptionViewModel.cs
        │   │   ├── SuggestionBoxViewModel.cs
        │   │   ├── UnsubscribeViewModel.cs
        │   │   ├── UpgradeViewModel.cs
        │   │   └── WaitlistViewModel.cs
        │   └── Shared/
        │       └── BrandHeaderViewModel.cs
        ├── Properties/
        │   └── launchSettings.json
        ├── wwwroot/
        │   ├── app.css
        │   ├── js/
        │   │   └── landing.js    # Landing page scroll & nav interactivity
        │   └── lib/bootstrap/
        ├── appsettings.json
        └── appsettings.Development.json
```

### Solution Folders (in .slnx)

- `/Foundation/` — shared/core projects (`Domain`, `Domain.Data`, `Domain.Data.EntityFramework`, `Domain.Entities`)
- `/Presentation/` — contains `My.Talli.Web`

### Project Reference Chain

```
Domain.Entities          ← entity classes (no dependencies)
Domain.Data              ← abstractions (IRepository, IUnitOfWork) → Domain.Entities
Domain.Data.EntityFramework ← EF Core implementation (DbContext, configs) → Domain.Data, Domain.Entities
Domain                   ← exceptions, notifications → Domain.Entities
My.Talli.Web             ← Blazor Server app → Domain
```

## Brand & Design

> **Source of truth:** `wireframes/MyTalli_ColorPalette.html` (light) and `wireframes/MyTalli_DarkModePalette.html` (dark) — keep this section in sync with those files.

- **Color palette tool:** [Coolors](https://coolors.co) — used to create and manage the brand palette

### Page Branding — Purple Swoosh

Every page except the Landing Page uses a **purple gradient swoosh** header for consistent branding:

- **`BrandHeader` component** (`Components/Shared/BrandHeader.razor`) — reusable swoosh with logo + action slot (`ChildContent` RenderFragment). Used by Sign-In and Waitlist pages.
- **Dashboard** uses its own inline swoosh (no BrandHeader) because the sidebar already has the logo — the swoosh sits behind the greeting area instead.
- **Landing Page** has its own distinct hero layout and is **not** branded with the swoosh.

| Page | Swoosh | Logo | Action Slot |
|------|--------|------|-------------|
| `/signin` | `<BrandHeader>` | Yes | "Back to homepage" link |
| `/waitlist` | `<BrandHeader>` | Yes | "Sign Out" link |
| `/dashboard` | Inline SVG (`.dash-hero`) | No (sidebar has it) | "Sign Out" link |
| `/suggestions` | Inline SVG (`.suggest-hero`) | No (sidebar has it) | "New Suggestion" button |
| `/subscription` | Inline SVG (`.sub-hero`) | No (sidebar has it) | N/A |
| `/subscription/cancel` | Inline SVG (`.cancel-hero`) | No (sidebar has it) | N/A |
| `/upgrade` | Inline SVG (`.upgrade-hero`) | No (sidebar has it) | N/A |
| `/unsubscribe` | `<BrandHeader>` | Yes | "Go to Homepage" link |
| `/Error` | `<BrandHeader>` | Yes | "Go Back" button |
| `/` | None | Own nav logo | N/A |

Swoosh visual: purple gradient SVG (`#6c5ce7` → `#8b5cf6` → `#6c5ce7`) with 3 decorative circles (`rgba(255,255,255,0.07)`).
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
- **Highlight Yellow:** `#fff176` — attention flash (waitlist input highlight)

## Development

### Build & Run

```bash
dotnet build Source/My.Talli.slnx
dotnet run --project Source/My.Talli.Web
```

### Dev URLs

- HTTPS: `https://localhost:7012`
- HTTP: `http://localhost:5034`

### Version Number

- **`<Version>0.1.0.0</Version>`** in `My.Talli.Web.csproj` — single source of truth for the app version. Format: `Major.Minor.Patch.Revision`.
- **Revision number** — incremented with each fix deployment. Only the revision (4th segment) changes per fix. The version (`Major.Minor.Patch`) only changes for feature releases or breaking changes. The full 4-segment version is always displayed in the UI so deployment slots (staging vs production) can be visually distinguished.
- **`LayoutHelper.VersionNumber`** reads `AssemblyInformationalVersionAttribute` (set by `<Version>`) at runtime
- **`LayoutHelper.CurrentYear`** provides the current year for copyright footers
- **Landing Page** — version shown inline in footer: `© 2026 MyTalli v0.1.0.0 — All rights reserved.`
- **MainLayout pages** — version shown in a subtle `div.app-version` at the bottom of the content area
- **LandingLayout pages** (Sign-In, Waitlist, Error) — no version displayed

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
- **Login endpoint:** `/api/auth/login/{provider}` — triggers OAuth challenge, redirects to `/waitlist` on success
- **Logout endpoint:** `/api/auth/logout` — clears cookie, redirects to `/?signed-out&name={name}`
- **Sign-out toast:** Landing page detects `?signed-out` query param and shows a personalized auto-dismissing toast ("You've been signed out, {name}. See you next time!"), then strips the query param from the URL via `history.replaceState`
- **Waitlist route:** `/waitlist` — launch progress tracker with milestone timeline (not a dead-end confirmation)

## Authorization

- **Role-based** — roles are stored in `auth.UserRole` (junction table, 1-to-many with User) and added as `ClaimTypes.Role` claims during OAuth sign-in
- **Role constants** — defined in `Domain/Framework/Roles.cs` (no database lookup table). Current roles: `Admin`, `User`
- **Default role** — every new user gets the `User` role on sign-up. Existing users with no roles are self-healed on next sign-in.
- **Admin assignment** — no UI yet. Assign via direct database insert into `auth.UserRole`.
- **Claims flow** — domain sign-in handlers query `UserRole`, populate `User.Roles` on the model → web auth handlers map each role to a `ClaimTypes.Role` claim on the identity

## App Modes

The app operates in one of two modes. The **current mode is Waitlist Mode**.

### Waitlist Mode ← CURRENT

Only the landing page, sign-in, waitlist, and error pages are active. All other routes redirect to `/waitlist`. Use this mode while building out platform connectors and dashboard features before public launch.

- **Middleware:** `Program.cs` — inline `app.Use(...)` block after `UseAntiforgery()` redirects disabled routes
- **Disabled routes:** `/dashboard`, `/suggestions`, `/subscription`, `/subscription/cancel`, `/upgrade` — all redirect to `/waitlist`
- **Active routes:** `/` (landing), `/signin`, `/waitlist`, `/unsubscribe`, `/Error`, `/Error/{StatusCode}`
- **OAuth redirect:** Set to `/waitlist` in the login endpoint (`Program.cs`)

### Dashboard Mode

Full app experience — sign-in takes users to the dashboard, all routes are active, sidebar navigation is functional. Enable this mode when platform connectors and the dashboard are ready.

- **Active routes:** All routes (`/dashboard`, `/suggestions`, `/subscription`, `/upgrade`, etc.)
- **OAuth redirect:** Set to `/dashboard` in the login endpoint (`Program.cs`)

### Switching Modes

| From → To | Steps |
|-----------|-------|
| Waitlist → Dashboard | 1. Remove the waitlist-mode middleware block in `Program.cs` 2. Change `RedirectUri` from `/waitlist` to `/dashboard` |
| Dashboard → Waitlist | 1. Add the waitlist-mode middleware block back in `Program.cs` 2. Change `RedirectUri` from `/dashboard` to `/waitlist` |

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
- **Customer-facing emails** use the **Landing Hero** design — an organic purple blob (`#6c5ce7` → `#8b5cf6` → `#6c5ce7` gradient) on the right with dark text on white left, matching the brand swoosh style. Hero uses the **bulletproof background image pattern** (`<td background>` + CSS `background-image` + VML conditional comments for Outlook) with hosted PNGs at `https://www.mytalli.com/emails/`. Body icons use HTML entity emojis (render natively, not blocked). Four customer emails are built: Waitlist Welcome, Welcome, Subscription Confirmation, Weekly Summary.

### Adding a New Email Notification

1. Create a payload class in `Domain/Notifications/Emails/` with the data properties
2. Create an HTML template in `Domain/.resources/emails/` with `[[Placeholder]]` tokens — use table-based layout with inline styles for email client compatibility
3. Create a concrete notification class extending `EmailNotificationOf<TPayload>` — implement `Build()` to load the template, replace tokens, and set Subject
4. The `EmbeddedResource` glob in `Domain.csproj` (`**/*.html`) picks up new templates automatically
5. Create a handler/trigger in the Web project that builds and sends the notification via `IEmailService`

### Test Emails (Development Only)

A dev-only endpoint at `GET /api/test/emails` sends all 4 customer emails to `hello@mytalli.com` with sample data via ACS. Only registered when `app.Environment.IsDevelopment()`.

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

Integration with each revenue platform uses OAuth so users grant MyTalli read-only access to their sales/payment data.

### Stripe

- **API:** REST API (extensive) — [docs.stripe.com/api](https://docs.stripe.com/api)
- **Auth:** OAuth via Stripe Connect (Standard or Express) — user authorizes MyTalli to read their account
- **Key endpoints:** Balance Transactions (charges, refunds, fees, payouts), Charges, PaymentIntents, Reports API (scheduled CSV reports), Revenue Recognition API
- **Data richness:** Excellent — granular transaction-level data, fees, net amounts, metadata
- **Caveats:** None significant. Best-documented API of the three.

### Etsy

- **API:** Etsy Open API v3 (REST) — [developers.etsy.com](https://developers.etsy.com/)
- **Auth:** OAuth 2.0 (PKCE flow)
- **Key endpoints:** Shop Receipts (orders/sales per shop), Transactions (line-item detail), Payments (payment & transaction lookups by shop/listing/receipt)
- **Data richness:** Good — order-level sales, item details, shop stats
- **Caveats:** Multi-seller apps (like MyTalli) require **commercial access approval** from Etsy. Must apply and be approved before production use.

### Gumroad

- **API:** REST API — [gumroad.com/api](https://gumroad.com/api)
- **Auth:** OAuth 2.0
- **Key endpoints:** Sales (list sales with filtering), Products (product info & pricing), Subscribers (subscription data)
- **Data richness:** Basic — covers sales and products but less granular than Stripe (no fee breakdowns, limited filtering)
- **Caveats:** Simpler API overall. Sufficient for revenue aggregation but won't support deep financial reporting.

### PayPal (not yet researched in detail)

- Known to have extensive REST APIs and OAuth for third-party access. Needs detailed review before integration.

### Shopify (not yet researched in detail)

- Known to have Admin API (REST + GraphQL) with OAuth. Needs detailed review before integration.

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
- Pages using `LandingLayout` (Sign-In, Waitlist, Error) use the **`BrandHeader`** component.
- See the "Page Branding — Purple Swoosh" table in the Brand & Design section for the full mapping.

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
- **Middleware** goes in `Middleware/` — proper middleware classes with `InvokeAsync` and a companion `Use{Name}` extension method on `IApplicationBuilder`. Lightweight inline middleware (e.g., waitlist mode redirect) may stay in Program.cs when it's only a few lines and tightly coupled to pipeline ordering.
- When adding a new service concern, create a new `Configuration/{Name}Configuration.cs` file. When adding new API routes, create a new `Endpoints/{Name}Endpoints.cs` file. When adding new middleware, create a new `Middleware/{Name}Middleware.cs` file. Do not add inline registrations, endpoint lambdas, or substantial middleware to Program.cs.
- Namespace: `My.Talli.Web.Configuration` for configuration classes, `My.Talli.Web.Endpoints` for endpoint classes, `My.Talli.Web.Middleware` for middleware classes.

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

## Blazor TODO

Features already shipped in the static HTML landing page (`deploy/index.html`) that still need to be ported to the Blazor app:

- [x] **SEO** — meta description, robots, canonical URL, Open Graph tags, Twitter Card tags, JSON-LD structured data (`SoftwareApplication` schema)
- [x] **Favicon** — link `favicon.svg` (concept C — T + growth bars) in `App.razor` `<head>`
- [x] **Social Share Image** — add `og-image.png` (1200x630) to `wwwroot/` and reference in OG/Twitter meta tags
- [x] **Accessibility** — skip navigation link, `<main>` landmark, ARIA labels on nav/sections, `aria-hidden` on decorative SVGs, emoji `role="img"` labels, `.sr-only` utility class, `:focus-visible` outlines, `role="contentinfo"` on footer, visually-hidden "Included:" prefixes on pricing checkmarks

Upcoming features:

- [ ] **Admin Page** — role-based admin section (`/admin`) for managing waitlist signups, viewing all suggestion box submissions, user management, platform connection health, and feature flag/tier management. Accessible only to accounts with an `Admin` role.
- [ ] **Email Asset Hosting** — email image assets (`email-hero-bg.png`, `email-icon-graph.png`) are currently served from `wwwroot/emails/` on the App Service (deployed with the app). Phase 2: migrate to Azure Blob Storage with a public container (e.g., `https://mytallistorage.blob.core.windows.net/emails/`) and update all 4 customer email template URLs. This decouples email assets from app deployments so images are always available regardless of deploy state.
