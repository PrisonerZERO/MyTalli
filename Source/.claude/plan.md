# Authentication Handlers — Domain Service Layer

## Current State

The three auth handlers (`GoogleAuthenticationHandler`, `AppleAuthenticationHandler`, `MicrosoftAuthenticationHandler`) live in `My.Talli.Web/Services/Authentication/`. They're stubs that:
1. Extract claims from the `OAuthCreatingTicketContext`
2. Log the sign-in
3. Have TODOs for database lookup/create/update

## Problem

The TODOs require database operations (find user, create user, update last login, add claims). This is domain logic — it shouldn't live in the Web project alongside ASP.NET middleware types.

## Plan

### 1. Domain needs a reference to Domain.Data

`Domain.csproj` currently references only `Domain.Entities`. It needs `Domain.Data` too, so it can use `IAuditableRepositoryAsync<User>` etc.

**File:** `Source/Domain/Domain.csproj`
- Add `<ProjectReference>` to `Domain.Data.csproj`

### 2. Create a Domain handler per provider

These handle the "find or create user" logic using repositories. They live in the `Handlers/` folder (already exists as an empty folder in Domain).

**Files:**
- `Source/Domain/Handlers/Authentication/GoogleAuthenticationDomainHandler.cs`
- `Source/Domain/Handlers/Authentication/AppleAuthenticationDomainHandler.cs`
- `Source/Domain/Handlers/Authentication/MicrosoftAuthenticationDomainHandler.cs`

Each handler will:
1. Accept provider-specific data (provider ID, email, name, etc.) as simple parameters — no ASP.NET types
2. Look up the auth record by provider ID (e.g., `GoogleId`)
3. If not found, look up by email across all auth tables (for account linking scenarios)
4. If still not found, create a new `User` + provider auth record
5. If found, update `LastLoginAt` (note: `User` entity doesn't have this yet — see step 5)
6. Return the `User` entity (so the Web handler can add claims)

### 3. Update the Web authentication handlers

Slim them down to:
1. Extract claims from `OAuthCreatingTicketContext` (stays here — ASP.NET dependency)
2. Call the Domain handler with the extracted data
3. Add claims to the identity from the returned User (stays here — ASP.NET dependency)

**Files (modify existing):**
- `Source/My.Talli.Web/Services/Authentication/GoogleAuthenticationHandler.cs`
- `Source/My.Talli.Web/Services/Authentication/AppleAuthenticationHandler.cs`
- `Source/My.Talli.Web/Services/Authentication/MicrosoftAuthenticationHandler.cs`

### 4. Register Domain handlers in DI

**File:** `Source/My.Talli.Web/Program.cs`
- Register the three domain handlers as scoped services

### 5. Add missing fields to User entity

The `User` entity is missing `LastLoginAt` (mentioned in CLAUDE.md schema: `LastLoginAt`). Need to add it and create a migration.

**File:** `Source/Domain.Entities/Entities/User.cs`
- Add `public DateTime LastLoginAt { get; set; }`

**File:** `Source/Domain.Data.EntityFramework/Configurations/Auth/UserConfiguration.cs`
- Add column config for `LastLoginAt`

**Migration:** `Add-Migration AddUserLastLoginAt` after entity changes

### 6. Wire up the UserPreferencesJsonSerializer

When creating a new user, set `UserPreferences` to the serialized default `UserPreferences` model (so it starts as `{"emailPreferences":{"unsubscribeAll":false,"subscriptionConfirmationEmail":true,"weeklySummaryEmail":true}}`).

## Architecture After

```
Web Handler (ASP.NET)          Domain Handler (pure C#)
──────────────────────         ────────────────────────
Extract claims from OAuth  →   Find or create User
                           →   Update last login
Add claims to identity     ←   Return User entity
```

## Open Questions

1. **Naming:** `GoogleAuthenticationDomainHandler` or something shorter? The `Domain` suffix distinguishes it from the Web handler but it's verbose.
2. **Account linking:** Should the "find by email across providers" logic be in the initial implementation, or deferred? CLAUDE.md says consolidation UX is "not yet implemented."
3. **LastLoginAt vs CreatedAt:** The `User` entity has audit fields (`CreatedOnDateTime`, `UpdatedOnDateTime`) from `AuditableIdentifiableEntity`. Should we use `UpdatedOnDateTime` as a proxy for last login, or add a dedicated `LastLoginAt`? Dedicated field is cleaner since updates could happen for reasons other than login.
