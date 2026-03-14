# My.Talli

## What Is This?

MyTalli is a side-hustle revenue aggregation dashboard. It lets creators and freelancers connect their payment platforms (Stripe, Etsy, Gumroad, PayPal, Shopify, etc.) and see all their income in one unified dashboard with real-time tracking, trends, goals, and CSV export.

**Status:** Early development — **Waitlist Mode** active (see below). Landing page, sign-in, waitlist, and dashboard pages are built. OAuth authentication is working (Google, Apple, Microsoft). Sign-in currently redirects to the waitlist; dashboard and other routes are disabled until Dashboard Mode is enabled.

## Tech Stack

- **.NET 10.0** — target framework
- **Blazor Server** (Interactive Server render mode) — `blazor.web.js`
- **Bootstrap** — bundled in `wwwroot/lib/bootstrap/`
- **C#** — backend language
- **Lamar** — IoC container (replaces default Microsoft DI)
- **MailKit** — email sending (replaces obsolete `System.Net.Mail.SmtpClient`)
- **Razor Components** — UI layer (`.razor` files)
- **SQL Server** — database (localhost, Windows Auth)

## Database

- **Engine:** SQL Server (localhost)
- **Test bed database:** `ShoppingCart` — used for schema prototyping during early development. Will be replaced with a production database later.
- **Connection:** Windows Authentication (Trusted Connection)

### Design Principles

- **No nulls** — provider-specific data lives in dedicated tables, not nullable columns on base tables
- **Provider separation** — auth providers (Google, Apple, Microsoft) and billing providers (Stripe, etc.) each get their own table with a 1-to-1 relationship to the base table. Adding a new provider = new table, no schema changes to existing tables.
- **Schema separation** — tables are organized into SQL schemas by functional domain (`auth`, `commerce`). `dbo` is reserved/empty.
- **Orders as the backbone** — subscriptions, modules, and any future products all flow through the same Order → OrderItem pipeline. A subscription is just a product.
- **No separate waitlist table** — the `auth.User` table doubles as the waitlist during Waitlist Mode. A signed-up user *is* a waitlist user until Dashboard Mode is enabled.
- **No milestones table** — milestones are hardcoded in the Waitlist page UI, not stored in the database.

### Schemas

| Schema | Purpose | Tables |
|--------|---------|--------|
| `auth` | Identity & authentication | User, UserAuthenticationGoogle, UserAuthenticationApple, UserAuthenticationMicrosoft |
| `commerce` | Products, orders, billing, subscriptions | ProductVendor, ProductType, Product, Order, OrderItem, Billing, BillingStripe, Subscription, SubscriptionStripe |
| `dbo` | Reserved (empty) | — |

### Schema: `auth`

**`auth.User`** — core MyTalli identity (one row per person)
- `Id` (PK), `DisplayName`, `FirstName`, `LastName`, `CreatedAt`, `LastLoginAt`, `InitialProvider` (historical — which provider they first signed in with, never changes), `PreferredProvider` (which provider the user prefers, starts equal to InitialProvider), `UserPreferences` (NVARCHAR(MAX), JSON — app settings/toggles, defaults to `'{}'`)
- Email is **not** stored here — it lives on the provider auth tables. The user's email is resolved via their PreferredProvider.
- **UserPreferences** stores user-configurable app settings as JSON. This avoids contorting the User table with individual columns as settings grow over time.

**`auth.UserAuthenticationGoogle`** — 1-to-1 with User
- `Id` (PK), `UserId` (FK → User, unique), `GoogleId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`, `AvatarUrl`, `EmailVerified`, `Locale`

**`auth.UserAuthenticationApple`** — 1-to-1 with User
- `Id` (PK), `UserId` (FK → User, unique), `AppleId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`, `IsPrivateRelay`

**`auth.UserAuthenticationMicrosoft`** — 1-to-1 with User
- `Id` (PK), `UserId` (FK → User, unique), `MicrosoftId` (unique), `Email`, `DisplayName`, `FirstName`, `LastName`

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

**`commerce.SubscriptionStripe`** — Stripe-specific subscription data (1-to-1 with Subscription)
- `Id` (PK), `SubscriptionId` (FK → Subscription, unique), `StripeCustomerId`, `StripeSubscriptionId`, `StripePriceId`

**`commerce.Billing`** — a payment event tied to an order
- `Id` (PK), `UserId` (FK → auth.User), `OrderId` (FK → Order), `Amount`, `Currency`, `Status`
- `OrderId` answers "which billing satisfied this order?"

**`commerce.BillingStripe`** — Stripe-specific payment data (1-to-1 with Billing)
- `Id` (PK), `BillingId` (FK → Billing, unique), `StripePaymentIntentId`, `PaymentMethod`, `CardBrand`, `CardLastFour`

### Account Linking (Consolidation)

Users may sign in with different providers over time and accidentally create multiple accounts. The auth table design supports **account consolidation**:

1. User signs in with Google → `auth.User` + `auth.UserAuthenticationGoogle` created
2. Later signs in with Apple → second `auth.User` + `auth.UserAuthenticationApple` created (empty account)
3. User realizes their data is on the Google account and triggers consolidation
4. Consolidation moves the Apple auth row to point at the original User record, deletes the orphaned User record
5. User can now sign in with either provider and land on the same account

The consolidation process itself is not yet implemented — the schema supports it, the UX flow will be designed later.

### Naming Conventions

- **Primary keys:** `PK_{TableName}` (e.g., `PK_User`, `PK_Order`)
- **Foreign keys:** `FK_{ChildTable}_{ParentTable}` (e.g., `FK_Order_User`, `FK_Subscription_Product`)
- **Unique constraints:** `UQ_{TableName}_{ColumnName}` (e.g., `UQ_UserAuthGoogle_UserId`)
- **Indexes:** `IX_{TableName}_{ColumnName}` (e.g., `IX_Order_UserId`)
- Every FK column has a non-clustered index for JOIN performance

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
    ├── Domain/                      # Domain layer (exceptions, shared types)
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
    │   ├── Extensions/
    │   │   └── AssemblyExtensions.cs          # GetManifestResourceContent() for embedded resources
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
    ├── Domain.Entities/             # Domain entity layer (database models)
    │   ├── Domain.Entities.csproj
    │   ├── AuditableIdentifiableEntity.cs  # Base class (Id + audit fields)
    │   ├── DefaultEntity.cs                # Standard entity base (adds IsActive)
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
    │   │   └── UserAuthenticationMicrosoft.cs
    │   └── Interfaces/
    │       ├── IAuditable.cs
    │       ├── IAuditableIdentifiable.cs
    │       └── IIdentifiable.cs
    └── My.Talli.Web/               # Blazor Server web project
        ├── My.Talli.Web.csproj
        ├── Program.cs              # App entry point, service config, auth, endpoints
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
        │   │   ├── Unsubscribe.razor      # Email unsubscribe confirmation (route: /unsubscribe)
        │   │   ├── Unsubscribe.razor.css
        │   │   ├── Waitlist.razor        # Waitlist progress tracker (route: /waitlist)
        │   │   ├── Waitlist.razor.css
        │   │   ├── Error.razor           # Branded error page (routes: /Error, /Error/{StatusCode})
        │   │   └── Error.razor.css
        │   └── Shared/
        │       ├── BrandHeader.razor     # Reusable purple swoosh header (logo + action slot)
        │       └── BrandHeader.razor.css
        ├── Services/
        │   ├── Authentication/
        │   │   ├── AppleAuthenticationHandler.cs
        │   │   ├── GoogleAuthenticationHandler.cs
        │   │   └── MicrosoftAuthenticationHandler.cs
        │   ├── Billing/
        │   │   ├── StripeBillingService.cs  # Stripe Checkout & Portal API wrapper
        │   │   └── StripeSettings.cs        # Stripe configuration POCO
        │   └── Email/
        │       ├── EmailSettings.cs             # SMTP config POCO (IOptions<EmailSettings>)
        │       ├── ExceptionEmailHandler.cs     # IExceptionHandler — sends email, returns false
        │       ├── IEmailService.cs             # Email sending interface
        │       └── SmtpEmailService.cs          # MailKit-based implementation
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

- `/Foundation/` — shared/core projects (contains `Domain` project)
- `/Presentation/` — contains `My.Talli.Web`

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

## Infrastructure

- **Domain registrar:** GoDaddy — `mytalli.com`
- **Hosting:** Azure Static Web Apps (Free tier) — "coming soon" landing page
- **Custom domain:** `www.mytalli.com` (validated, SSL auto-provisioned)
- **Auto-generated URL:** `delightful-grass-000c17010.6.azurestaticapps.net`
- **Analytics:** Google Analytics 4 — measurement ID `G-7X9ZL3K4GS` (gtag snippet in landing page `<head>`)
- **Google Search Console:** Property `https://www.mytalli.com/` verified via GA4 (2026-03-07). Sitemap submitted. Dashboard at [search.google.com/search-console](https://search.google.com/search-console)
- **Deployment:** SWA CLI (`swa deploy ./deploy --deployment-token TOKEN --env production`) — the `deploy/` folder contains `index.html`, `favicon.svg`, `og-image.png`, `robots.txt`, `sitemap.xml`, and `emails/` (hosted PNG assets for email templates)
- **Secrets file:** `.secrets` (git-ignored) — contains `SWA_DEPLOYMENT_TOKEN` for Azure SWA deploys
- **Note:** Azure Static Web Apps Free tier does not emit CDN metrics — GA is the only visit tracking
- **Migration note:** The `deploy/` and `favicon-concepts/` folders are for the current static HTML landing page era. When the Blazor app is deployed, static assets (`favicon.svg`, `og-image.png`, `robots.txt`, `sitemap.xml`) will move into `wwwroot/` and the `deploy/` folder will no longer be needed.

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
- **External providers only:** Google, Apple, Microsoft (via OAuth) — all working
- **Cookie auth** with 30-day sliding expiration
- **Sign-in route:** `/signin` — provider selection page
- **Login endpoint:** `/api/auth/login/{provider}` — triggers OAuth challenge, redirects to `/waitlist` on success
- **Logout endpoint:** `/api/auth/logout` — clears cookie, redirects to `/?signed-out&name={name}`
- **Sign-out toast:** Landing page detects `?signed-out` query param and shows a personalized auto-dismissing toast ("You've been signed out, {name}. See you next time!"), then strips the query param from the URL via `history.replaceState`
- **Waitlist route:** `/waitlist` — launch progress tracker with milestone timeline (not a dead-end confirmation)

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
- **Falling numbers animation:** Pure CSS `@keyframes` animation — 12 digits from the status code fall through the white space below the swoosh. Decorative only (`aria-hidden="true"`), no JS dependency so it works even when SignalR fails. Digits are generated by `ErrorViewModel.SetFallingDigits()`. Three alternating color/opacity tiers cycle via `nth-child(3n+...)`: **Bold** (`#6c5ce7`, peak 0.28 opacity), **Mid** (`#8b5cf6`, peak 0.18), **Soft** (`#a78bfa`, peak 0.10) — so some digits stand out more than others.

## Email Notifications

### Architecture

Email notifications follow a **Template + Builder** pattern modeled after the Measurement Forms Liquids project:

- **HTML templates** — stored as `EmbeddedResource` files in `Domain/.resources/emails/`, compiled into the assembly, loaded at runtime via `Assembly.GetManifestResourceContent()`
- **Notification classes** — in `Domain/Notifications/Emails/`, abstract base `EmailNotification` → generic `EmailNotificationOf<T>` → concrete implementations (e.g., `ExceptionOccurredEmailNotification`)
- **Placeholder replacement** — templates use `[[Placeholder.Name]]` tokens replaced via `string.Replace()` in the `Build()` method. All user-supplied data is HTML-encoded via `WebUtility.HtmlEncode()` before replacement.
- **SmtpNotification** — serializable POCO carrier returned by `FinalizeEmail()`, passed to `IEmailService.SendAsync()`
- **MailKit** — `SmtpEmailService` uses MailKit for SMTP delivery (Microsoft's recommended replacement for the obsolete `System.Net.Mail.SmtpClient`)

### Exception Email Pipeline

Unhandled exceptions trigger email notifications via .NET's `IExceptionHandler` interface:

1. Exception occurs → `UseExceptionHandler("/Error")` middleware runs registered `IExceptionHandler` services
2. `ExceptionEmailHandler.TryHandleAsync()` builds the notification and sends the email
3. Handler **always returns `false`** — the middleware continues re-executing to `/Error`, preserving the existing Error page behavior
4. Email failures are caught and logged — they never mask the original exception or break the error page

### Email Configuration

SMTP settings are bound from `appsettings.json` → `Email` section via `IOptions<EmailSettings>`:

- `Host`, `Port`, `Username`, `Password`, `UseSsl` — SMTP connection (sensitive values via `dotnet user-secrets`)
- `FromAddress` — default `noreply@mytalli.com`
- `FromDisplayName` — default `MyTalli`
- `ExceptionRecipients` — list of admin email addresses; if empty, no exception emails are sent

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

A dev-only endpoint at `GET /api/test/emails` sends all 3 customer emails to `hello@mytalli.com` with sample data. Only registered when `app.Environment.IsDevelopment()`. Use with a local SMTP tool like **smtp4dev** (.NET global tool — `dotnet tool install -g Rnwood.Smtp4dev`, SMTP on port 25, web UI at `http://localhost:5000`).

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

### Clean Up NUL Files

- Bash on Windows creates an actual file named `nul` when using `2>nul` redirects (instead of discarding output to the Windows NUL device). **Always delete any `nul`/`NUL` files** that get created in the repo after running shell commands.

### Namespace-First Ordering

- In C# files, the **file-scoped `namespace` declaration comes first**, followed by `using` statements below it.
- Files with no `using` statements just start with the `namespace`.

```csharp
/* Correct */
namespace My.Talli.Web.Services.Email;

using Microsoft.Extensions.Options;
using My.Talli.Domain.Notifications.Emails;

public class SmtpEmailService { ... }

/* Wrong — do not put usings above the namespace */
using Microsoft.Extensions.Options;
using My.Talli.Domain.Notifications.Emails;

namespace My.Talli.Web.Services.Email;

public class SmtpEmailService { ... }
```

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

### C# Region Convention

- Every C# class **must** use `#region` / `#endregion` to organize its members.
- Region names use angle brackets: `#region <Name>`
- Only include regions the class actually needs — omit empty ones.
- Allowed regions (in order):
  1. `<Variables>` — fields, constants, injected services
  2. `<Constructors>` — constructor overloads
  3. `<Properties>` — public/protected properties
  4. `<Events>` — lifecycle events, event handlers
  5. `<Methods>` — general methods
  6. `<Actions>` — MVC controller actions (not used yet)
- **Within each region**, order members by access modifier: `public` → `protected` → `private`
- **Within each access level**, alphabetize members by name

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
