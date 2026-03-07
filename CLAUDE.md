# My.Talli

## What Is This?

MyTalli is a side-hustle revenue aggregation dashboard. It lets creators and freelancers connect their payment platforms (Stripe, Etsy, Gumroad, PayPal, Shopify, etc.) and see all their income in one unified dashboard with real-time tracking, trends, goals, and CSV export.

**Status:** Early development. Landing page, sign-in, waitlist, and dashboard pages are built. OAuth authentication is working (Google, Apple, Microsoft). Users currently land on the waitlist after sign-in (dashboard is built but waitlist is the active post-login destination).

## Tech Stack

- **.NET 9.0** — target framework
- **Blazor Server** (Interactive Server render mode) — `blazor.web.js`
- **Bootstrap** — bundled in `wwwroot/lib/bootstrap/`
- **C#** — backend language
- **Lamar** — IoC container (replaces default Microsoft DI)
- **Razor Components** — UI layer (`.razor` files)

## Solution Structure

```
My.Talli/
├── CLAUDE.md
├── MyTalli_LandingPage.html        # Static landing page mockup
├── MyTalli_Dashboard.html          # Static dashboard mockup (post-login)
├── MyTalli_WaitlistConcepts.html   # Waitlist page design concepts (A/B/C)
├── MyTalli_ColorPalette.html       # Brand color reference sheet (light mode)
├── MyTalli_DarkModePalette.html    # Brand color reference sheet (dark mode)
├── og-image.png                    # Social share image (1200×630) — source copy
├── deploy/                         # Azure SWA deploy folder (static HTML era)
│   ├── index.html                  # Copied from MyTalli_LandingPage.html
│   ├── favicon.svg                 # Copied from favicon-concepts/favicon-c-growth.svg
│   ├── og-image.png                # Social share image
│   ├── robots.txt                  # Allows all crawlers, references sitemap
│   └── sitemap.xml                 # Site map for search engines
├── favicon-concepts/               # Favicon & OG image design assets
│   ├── favicon-a-lettermark.svg    # Concept A — bold T lettermark
│   ├── favicon-b-tally.svg         # Concept B — tally marks
│   ├── favicon-c-growth.svg        # Concept C — T + growth bars (CHOSEN)
│   ├── favicon-d-tgraph.svg        # Concept D — T with graph behind
│   ├── og-image-capture.html       # Viewport-locked page for PNG capture
│   ├── og-image-mockup.html        # OG image design mockup (1200×630)
│   └── preview.html                # Side-by-side favicon comparison page
└── Source/
    ├── My.Talli.slnx               # Solution file (XML-based .slnx format)
    ├── .claude/settings.local.json
    ├── Domain/                      # Domain layer (exceptions, shared types)
    │   ├── Domain.csproj
    │   └── Exceptions/
    │       ├── TalliException.cs              # Abstract base (HttpStatusCode property)
    │       ├── ForbiddenException.cs          # 403
    │       ├── DatabaseConnectionFailedException.cs  # 403 (inherits Forbidden)
    │       ├── NotFoundException.cs           # 404
    │       ├── UnauthorizedException.cs       # 401
    │       ├── SignInFailedException.cs        # 401 (inherits Unauthorized)
    │       └── UnexpectedException.cs         # 500
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
        │   │   ├── Dashboard.razor       # Dashboard (route: /dashboard)
        │   │   ├── Dashboard.razor.css
        │   │   ├── LandingPage.razor     # Landing page (route: /)
        │   │   ├── LandingPage.razor.css
        │   │   ├── SignIn.razor          # Sign-in page (route: /signin)
        │   │   ├── SignIn.razor.css
        │   │   ├── Waitlist.razor        # Waitlist progress tracker (route: /waitlist)
        │   │   ├── Waitlist.razor.css
        │   │   ├── Error.razor           # Branded error page (routes: /Error, /Error/{StatusCode})
        │   │   └── Error.razor.css
        │   └── Shared/
        │       ├── BrandHeader.razor     # Reusable purple swoosh header (logo + action slot)
        │       └── BrandHeader.razor.css
        ├── Services/
        │   └── Authentication/
        │       ├── AppleAuthenticationHandler.cs
        │       ├── GoogleAuthenticationHandler.cs
        │       └── MicrosoftAuthenticationHandler.cs
        ├── ViewModels/
        │   ├── Pages/
        │   │   ├── DashboardViewModel.cs
        │   │   ├── LandingPageViewModel.cs
        │   │   ├── ErrorViewModel.cs
        │   │   ├── SignInViewModel.cs
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

> **Source of truth:** `MyTalli_ColorPalette.html` (light) and `MyTalli_DarkModePalette.html` (dark) — keep this section in sync with those files.

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
| `/Error` | `<BrandHeader>` | Yes | "Back to homepage" link |
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
- **Deployment:** SWA CLI (`swa deploy ./deploy --deployment-token TOKEN --env production`) — the `deploy/` folder contains `index.html`, `favicon.svg`, `og-image.png`, `robots.txt`, and `sitemap.xml`
- **Note:** Azure Static Web Apps Free tier does not emit CDN metrics — GA is the only visit tracking
- **Migration note:** The `deploy/` and `favicon-concepts/` folders are for the current static HTML landing page era. When the Blazor app is deployed, static assets (`favicon.svg`, `og-image.png`, `robots.txt`, `sitemap.xml`) will move into `wwwroot/` and the `deploy/` folder will no longer be needed.

### SEO

The landing page (`MyTalli_LandingPage.html`) includes:
- `meta description`, `robots`, `theme-color`, `canonical` URL
- Open Graph tags (`og:type`, `og:url`, `og:title`, `og:description`, `og:image`)
- Twitter Card tags (`twitter:card`, `twitter:title`, `twitter:description`, `twitter:image`)
- JSON-LD structured data (`SoftwareApplication` schema with free tier pricing)
- **Favicon:** SVG (`/favicon.svg`) — "T" with ascending growth bars on purple rounded square, using primary purple `#6c5ce7` background and lavender `#a78bfa` bars. Source: `favicon-concepts/favicon-c-growth.svg`
- **OG Share Image:** PNG (`/og-image.png`, 1200×630) — dark navy gradient with favicon icon, "MyTalli" title (lavender accent), tagline with yellow "One dashboard.", platform pills with brand colors (Stripe, Etsy, Gumroad, PayPal, Shopify), and `www.mytalli.com` footer. Source mockup: `favicon-concepts/og-image-mockup.html`

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
- **Login endpoint:** `/api/auth/login/{provider}` — triggers OAuth challenge, redirects to `/dashboard` on success
- **Logout endpoint:** `/api/auth/logout` — clears cookie, redirects to `/?signed-out&name={name}`
- **Sign-out toast:** Landing page detects `?signed-out` query param and shows a personalized auto-dismissing toast ("You've been signed out, {name}. See you next time!"), then strips the query param from the URL via `history.replaceState`
- **Waitlist route:** `/waitlist` — launch progress tracker with milestone timeline (not a dead-end confirmation)

## Error Handling

- **Error page routes:** `/Error` (unhandled exceptions) and `/Error/{StatusCode:int}` (HTTP status codes)
- **Layout:** Uses `LandingLayout` + `BrandHeader` with "Back to homepage" link
- **Static SSR:** No `@rendermode` — intentional so error pages work even when SignalR circuit fails
- **Middleware:** `UseExceptionHandler("/Error")` + `UseStatusCodePagesWithReExecute("/Error/{0}")` — both active in all environments
- **Router 404:** `Routes.razor` has a `<NotFound>` template that renders the Error component with `StatusCode="404"`
- **Exception hierarchy:** `TalliException` (abstract base) exposes `HttpStatusCode` property. Subclasses: `ForbiddenException` (403), `NotFoundException` (404), `UnauthorizedException` (401), `UnexpectedException` (500). Specific exceptions inherit from these (e.g., `DatabaseConnectionFailedException` → `ForbiddenException`, `SignInFailedException` → `UnauthorizedException`)
- **Status code resolution:** `ErrorViewModel` checks for `TalliException` via `IExceptionHandlerFeature` to extract the status code, falls back to `Response.StatusCode`, then 500
- **Request ID:** Only shown in Development when an actual exception was caught (not on status-code-only errors)

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

### Clean Up NUL Files

- Bash on Windows creates an actual file named `nul` when using `2>nul` redirects (instead of discarding output to the Windows NUL device). **Always delete any `nul`/`NUL` files** that get created in the repo after running shell commands.

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

- [ ] **SEO** — meta description, robots, canonical URL, Open Graph tags, Twitter Card tags, JSON-LD structured data (`SoftwareApplication` schema)
- [ ] **Favicon** — link `favicon.svg` (concept C — T + growth bars) in `App.razor` `<head>`
- [ ] **Social Share Image** — add `og-image.png` (1200x630) to `wwwroot/` and reference in OG/Twitter meta tags
- [ ] **Accessibility** — skip navigation link, `<main>` landmark, ARIA labels on nav/sections, `aria-hidden` on decorative SVGs, emoji `role="img"` labels, `.sr-only` utility class, `:focus-visible` outlines, `role="contentinfo"` on footer, visually-hidden "Included:" prefixes on pricing checkmarks
