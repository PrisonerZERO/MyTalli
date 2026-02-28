# My.Talli

## What Is This?

MyTalli is a side-hustle revenue aggregation dashboard. It lets creators and freelancers connect their payment platforms (Stripe, Etsy, Gumroad, PayPal, Shopify, etc.) and see all their income in one unified dashboard with real-time tracking, trends, goals, and CSV export.

**Status:** Early development. Landing page and sign-in page are built; authentication is UI-only (not yet wired to providers).

## Tech Stack

- **.NET 9.0** — target framework
- **Blazor Server** (Interactive Server render mode) — `blazor.web.js`
- **Bootstrap** — bundled in `wwwroot/lib/bootstrap/`
- **C#** — backend language
- **Razor Components** — UI layer (`.razor` files)

## Solution Structure

```
My.Talli/
├── CLAUDE.md
├── MyTalli_LandingPage.html      # Static landing page mockup
├── MyTalli_ColorPalette.html     # Brand color reference sheet
└── Source/
    ├── My.Talli.slnx             # Solution file (XML-based .slnx format)
    ├── .claude/settings.local.json
    └── My.Talli.Web/             # Blazor Server web project
        ├── My.Talli.Web.csproj
        ├── Program.cs            # App entry point, service config
        ├── Components/
        │   ├── App.razor         # Root HTML document
        │   ├── Routes.razor      # Routing setup
        │   ├── _Imports.razor    # Global usings
        │   ├── Layout/
        │   │   ├── LandingLayout.razor   # Minimal layout (no sidebar)
        │   │   ├── MainLayout.razor      # Page layout shell
        │   │   ├── MainLayout.razor.css
        │   │   ├── NavMenu.razor         # Sidebar navigation
        │   │   └── NavMenu.razor.css
        │   └── Pages/
        │       ├── LandingPage.razor     # Landing page (route: /)
        │       ├── SignIn.razor          # Sign-in page (route: /signin)
        │       ├── Waitlist.razor        # Waitlist confirmation (route: /waitlist)
        │       └── Error.razor
        ├── ViewModels/
        │   └── Pages/
        │       ├── LandingPageViewModel.cs
        │       ├── SignInViewModel.cs
        │       └── WaitlistViewModel.cs
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

- `/Foundation/` — shared/core projects (empty, reserved for future domain/infrastructure layers)
- `/Presentation/` — contains `My.Talli.Web`

## Brand & Design

> **Source of truth:** `MyTalli_ColorPalette.html` — keep this section in sync with that file.

- **Font:** DM Sans (Google Fonts) — weights 400, 500, 600, 700

### Brand Colors

- **Primary Purple:** `#6c5ce7` — CTAs, logo accent, links, active states
- **Primary Hover:** `#5a4bd1` — hover & pressed states
- **Light Purple:** `#8b5cf6` — gradient mid-point, secondary accent
- **Lavender:** `#a78bfa` — accents on dark backgrounds
- **Soft Purple:** `#f0edff` — tags, badges, light backgrounds
- **Muted Purple:** `#e0dce8` — input borders, subtle dividers
- **Page Background:** `#f8f7fc` — alternating section backgrounds
- **Dark Navy:** `#1a1a2e` — primary text, dark sections

### Platform Connector Colors

| Platform | Color     |
|----------|-----------|
| Stripe   | `#635bff` |
| Etsy     | `#f56400` |
| Gumroad  | `#ff90e8` |
| PayPal   | `#003087` |
| Shopify  | `#96bf48` |

### UI Colors

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

## Authentication

- **No local passwords** — MyTalli does not store or manage usernames/passwords.
- **External providers only:** Google, Apple, Microsoft (via OAuth)
- **Current status:** UI-only (sign-in page with provider buttons; not yet wired to backend auth)
- **Sign-in route:** `/signin`
- **Waitlist route:** `/waitlist` — shown after sign-in; confirms user is on the waitlist

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
- Namespace follows the folder: `My.Talli.Web.ViewModels.Pages`, `My.Talli.Web.ViewModels.Layout`, etc.

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

### CSS Class Ordering

- Where possible, all CSS class attributes should be in alphabetical order.

## Conventions

- Use the `.slnx` solution format (not `.sln`)
- Organize projects into solution folders: Foundation (domain/infra), Presentation (web/UI)
- Follow standard .NET/Blazor project conventions
- Namespace root: `My.Talli`
