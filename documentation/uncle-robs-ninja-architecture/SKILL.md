---
name: uncle-robs-ninja-architecture
description: Teach and enforce universal clean-code architecture principles during any coding task. Use this skill whenever the user asks about coding best practices, architecture patterns, code organization, naming conventions, separation of concerns, dependency injection, testing strategy, error handling, or how to structure a project. Also use when the user says "clean code", "architecture review", "code review", "best practices", "ninja architecture", "how should I organize", "Uncle Rob", or asks for mentoring-style guidance on software design. This skill is language-agnostic with .NET/C# reference examples.
---

# Uncle Rob's Ninja Architecture

A ninja is disciplined, precise, and leaves no mess behind. Their work is clean — not because someone told them to clean up, but because clean *is* the work. That's how you should write code.

This isn't academic theory. These are real principles extracted from real production code. They'll make you faster, not slower — because you'll spend less time debugging, less time confused, and less time explaining your code to others (including future-you).

The reference examples use C# / .NET, but every principle here applies to PHP, Python, TypeScript, Java, Go, or any language. The *pattern* matters — the syntax is just flavor.

---

## 1. Readability is Respect

**Why:** Code is read 10x more than it's written. Readable code respects future-you and anyone who touches your code next. If your code needs a paragraph of comments to explain, the code itself isn't clear enough.

### Name Things Like You Mean It

- Use descriptive names. No abbreviations, no single-letter variables (except loop counters like `i`, `j`).
- If you can't tell what a class does from its name alone, rename it.
- Class names should be so descriptive that the doc comment is just a role label.

```csharp
// Reference (C#)

/* Good — the name says it all, summary is just a role label */
/// <summary>Repository</summary>
public class GenericAuditableRepositoryAsync<TEntity> { ... }

/* Bad — the name is vague, so the summary has to explain everything */
/// <summary>Repository implementation with automatic audit resolution on insert and update operations.</summary>
public class DataHelper<TEntity> { ... }
```

**Universal principle:** If your doc comment is longer than the class name, the class name needs work.

### Doc Comments on Every Class

- Every class and interface gets a doc comment.
- Keep it to a **short role label** — one or two words that describe what it *is* (e.g., `Repository`, `Mapper`, `Service`, `Configuration`, `Handler`).
- If you need a full sentence to explain what the class does, your class name isn't descriptive enough.

### Alphabetical Ordering — Everywhere

Alphabetical ordering removes decision fatigue. You never have to think about where to put the next import, variable, or parameter — it goes in alphabetical order. Period.

- **Imports/usings** — sorted alphabetically
- **Variables/fields** — sorted by type name (not variable name)
- **Constructor parameters** — sorted by type name
- **Constructor assignments** — sorted by variable name
- **CSS declarations** — sorted alphabetically within each rule

```csharp
// Reference (C#)

/* Good — imports alphabetical */
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

/* Bad — random order */
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
```

```csharp
// Reference (C#)

/* Good — fields sorted by type name, assignments sorted by variable name */
private readonly IAuditableRepositoryAsync<User> _userRepository;
private readonly IAuditableRepositoryAsync<UserRole> _roleRepository;
private readonly UserPreferencesJsonSerializer _preferencesSerializer;

public MyHandler(
    IAuditableRepositoryAsync<User> userRepository,
    IAuditableRepositoryAsync<UserRole> roleRepository,
    UserPreferencesJsonSerializer preferencesSerializer)
{
    _preferencesSerializer = preferencesSerializer;
    _roleRepository = roleRepository;
    _userRepository = userRepository;
}
```

### Async Naming

- Classes and interfaces whose **primary contract is async** append `Async` to the name (e.g., `IRepositoryAsync`, `GenericRepositoryAsync`).
- Async **methods** follow the standard `Async` suffix convention (e.g., `GetByIdAsync()`).
- Classes that just *happen* to have async lifecycle methods (ViewModels, handlers, services) do **not** get the suffix.

### CSS: One Line, Alphabetical

Every CSS rule goes on a single line. Declarations within the rule are alphabetical. This keeps stylesheets scannable and diffs clean.

```css
/* Good */
.card { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); padding: 24px; }

/* Bad — multi-line sprawl */
.card {
    padding: 24px;
    background: #fff;
    box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    border-radius: 12px;
}
```

### Import Aliases Are LOUD

When you create import aliases, make them ALL CAPS. This makes aliases visually distinct from type names — you can spot them instantly.

```csharp
// Reference (C#)

/* Good — aliases are ALL CAPS, separated by blank line */
using Domain.Framework;
using System.Text.Json;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/* Bad — aliases blend in with normal imports */
using Domain.Framework;
using Models = Domain.Models;
using System.Text.Json;
```

---

## 2. Every Layer Has One Job

**Why:** When everything is tangled together, changing one thing breaks five others. Layers let you swap, test, and reason about pieces independently. A change to how you store data should never require a change to your UI — and vice versa.

### The Layer Stack

Every well-structured app has these layers, whether you name them explicitly or not:

```
Entities          — data shapes (what does the data look like?)
Data Access       — persistence (how do we read/write it?)
Domain Logic      — business rules (what are the rules?)
Presentation      — UI + API (what does the user see?)
```

**Each layer only knows about the layer directly below it.** Never skip layers. The UI never touches the database directly. Business logic never knows about HTML.

### Entities Stay Home

Entities (your database models) never leak to the presentation layer. Instead, create **models** — simplified versions stripped of internal concerns.

**What models exclude:**
- Audit fields (who created it, when it was updated) — internal bookkeeping
- Navigation properties (related entity references) — use FK IDs instead
- Database concerns (computed columns, concurrency tokens)

```csharp
// Reference (C#)

/* Entity — lives in the data layer, has everything */
public class User
{
    public long Id { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public bool IsDeleted { get; set; }
    public long CreateByUserId { get; set; }        // audit field
    public DateTime CreatedOnDateTime { get; set; }  // audit field
    public List<UserRole> Roles { get; set; }        // navigation property
}

/* Model — lives in the domain layer, clean for the UI */
public class User
{
    public long Id { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public bool IsDeleted { get; set; }
    // No audit fields. No navigation properties. Clean.
}
```

**Universal principle:** The UI should never see implementation details of the data layer. Use mappers to translate between entities and models — explicit, readable, property-by-property. No magic auto-mapping.

### No "Model" Suffix

Model classes use the **same name** as their entity. The namespace already disambiguates them. Don't add a `Model` suffix — it's noise.

```csharp
// Reference (C#)

/* Good — same name, namespace does the work */
namespace MyApp.Entities;
public class User { ... }

namespace MyApp.Models;
public class User { ... }

// Use aliases when both are in scope:
using ENTITIES = MyApp.Entities;
using MODELS = MyApp.Models;

/* Bad */
public class UserModel { ... }
public class UserDto { ... }
```

### Thin Entry Point

Your app's entry point (e.g., `Program.cs`, `index.php`, `app.py`) should be a **thin orchestrator** — it calls functions, it doesn't contain logic.

- **Service registration** goes in dedicated configuration files (one per concern)
- **Route/endpoint mapping** goes in dedicated route files (one per route group)
- **Middleware** goes in dedicated middleware classes

```csharp
// Reference (C#)

/* Good — Program.cs is a thin orchestrator */
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();

app.MapAuthEndpoints();
app.MapBillingEndpoints();

/* Bad — hundreds of lines of inline registration and route lambdas */
builder.Services.AddDbContext<MyDbContext>(options => ...);
builder.Services.AddScoped<IUserRepository, UserRepository>();
// ... 200 more lines ...
app.MapGet("/api/auth/login", async (HttpContext ctx) => { /* 50 lines of logic */ });
```

### Markup and Logic: Separate Lives

Markup files (HTML, Razor, templates) contain **markup only** — no business logic, no data access, no inline code blocks.

All logic lives in a **code-behind file** (ViewModel, controller, presenter — whatever your framework calls it). The markup file references the code-behind, never the other way around.

```
/* Good — clean separation */
Dashboard.razor          → markup only, uses @inherits DashboardViewModel
DashboardViewModel.cs    → all C# logic, data loading, state management

/* Bad — everything jammed together */
Dashboard.razor          → markup + @code { 200 lines of C# }
```

**Universal principle:** If you're writing business logic inside your template/view file, stop and move it out.

---

## 3. Never `new` Up Your Friends

**Why:** When you `new` up a dependency inside a class, you've welded it in place. You can't swap it for testing, you can't change it without editing every file that uses it, and you can't see what a class needs without reading its entire implementation. Dependency injection fixes all of this.

### Constructor Injection

All dependencies come through the constructor and are stored in private readonly fields. This makes dependencies **visible** — you can see exactly what a class needs by looking at its constructor.

```csharp
// Reference (C#)

/* Good — dependencies are visible, injectable, testable */
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;

    public OrderService(
        IEmailService emailService,
        IOrderRepository orderRepository)
    {
        _emailService = emailService;
        _orderRepository = orderRepository;
    }
}

/* Bad — hidden dependency, impossible to test without a real database */
public class OrderService
{
    public void CreateOrder(Order order)
    {
        var repo = new SqlOrderRepository("connection-string");  // welded in place
        repo.Save(order);
    }
}
```

### Depend on Interfaces, Not Implementations

Your code should depend on the **contract** (interface), not the **implementation** (concrete class). This is the key that makes testing possible — you can swap a real database for an in-memory stub.

```csharp
// Reference (C#)

/* Good — depends on interface */
public class OrderService
{
    private readonly IOrderRepository _repository;
    ...
}

/* Bad — depends on concrete class */
public class OrderService
{
    private readonly SqlOrderRepository _repository;
    ...
}
```

### Lifetime Concepts

When registering services with your DI container, understand the three lifetimes:

| Lifetime | Created | Use When |
|----------|---------|----------|
| **Transient** | Every time it's requested | Lightweight, stateless services |
| **Scoped** | Once per request/circuit | Database contexts, user-specific state |
| **Singleton** | Once for the entire app | Configuration, caches, thread-safe utilities |

**Rule of thumb:** When in doubt, use scoped. It's the safest default.

### The Adapter/Gateway Pattern

Have a **single gateway** to your data layer. All data access flows through one class (an adapter or repository facade). This gives you:

- One place to enforce rules (all queries exclude soft-deleted records)
- One place to add logging, caching, or metrics
- One seam to mock in tests

```
/* Good — all data access through the adapter */
Presentation → RepositoryAdapter → Repository → Database

/* Bad — presentation code reaches directly into the database */
Presentation → DbContext → Database
Presentation → Repository → Database  (sometimes)
Presentation → Raw SQL → Database     (other times)
```

### Isolate DI Registration

Keep your DI container registration in its own file or project — not scattered across the entry point. One file per concern (authentication services, database services, billing services, etc.).

---

## 4. A Place for Everything

**Why:** When a new developer opens your project, they should find anything in under 10 seconds. Consistent organization makes that possible. It also eliminates the "where should I put this?" question — you already know.

### Class Member Ordering

Every class organizes its members into sections, in this order:

1. **Fields/Variables** — private fields, constants, injected services
2. **Constructors**
3. **Properties** — public and protected properties
4. **Events** — lifecycle events, event handlers
5. **Methods** — general methods

**Within each section**, order by access modifier: `public` -> `protected` -> `private`.
**Within each access level**, alphabetize by type/class name.

```csharp
// Reference (C#) — using regions to mark sections

#region <Variables>

private readonly IEmailService _emailService;
private readonly IOrderRepository _orderRepository;

#endregion

#region <Constructors>

public OrderService(
    IEmailService emailService,
    IOrderRepository orderRepository)
{
    _emailService = emailService;
    _orderRepository = orderRepository;
}

#endregion

#region <Properties>

public bool IsInitialized { get; private set; }

#endregion

#region <Methods>

public async Task CreateOrderAsync(Order order) { ... }

private void ValidateOrder(Order order) { ... }

#endregion
```

**Universal principle:** Pick an ordering convention and follow it in every file. The specific order matters less than consistency.

### One Class Per File, One Job Per Class

- Each file contains exactly one class.
- Each class does exactly one thing.
- File name matches class name.

### Folder Structure Mirrors Functionality

Organize folders by **what things do**, not what they are technically.

```
/* Good — organized by what they handle */
Handlers/
  Authentication/
    GoogleSignInHandler.cs
    AppleSignInHandler.cs
  Billing/
    CheckoutCompletedHandler.cs
    SubscriptionUpdatedHandler.cs

/* Bad — organized by technical role */
Handlers/
    GoogleSignInHandler.cs
    AppleSignInHandler.cs
    CheckoutCompletedHandler.cs
    SubscriptionUpdatedHandler.cs
```

### Subfolder Namespace Convention

Subfolders used purely for **file organization** do not add to the namespace. The namespace stops at the **functional grouping level** — the last meaningful segment.

```csharp
// Reference (C#)

/* File: Handlers/Authentication/Google/GoogleSignInHandler.cs */

/* Good — namespace stops at the functional group */
namespace MyApp.Handlers.Authentication;

/* Bad — the "Google" subfolder is organizational, not functional */
namespace MyApp.Handlers.Authentication.Google;
```

### Handlers vs Commands

When endpoints need to do work, delegate to dedicated classes:

- **Handlers** react to events. They orchestrate the pipeline: map incoming data to domain objects, call domain logic inside transactions, trigger side effects (emails, logging). Each handler owns everything it does.
- **Commands** execute discrete actions. Data access queries, notification sends, or any reusable operation. Each command exposes a single `Execute()` method.

**Both are:**
- Non-static classes with constructor-injected dependencies
- One class per operation (not one class per domain area)

```
/* Good — one handler per event, one command per action */
CheckoutCompletedHandler.cs      → handles checkout completed
SubscriptionUpdatedHandler.cs    → handles subscription updated
SendWelcomeEmailCommand.cs       → sends a welcome email
GetUserListCommand.cs            → queries the user list

/* Bad — one god class per domain area */
BillingHandler.cs                → handles ALL billing events (500 lines)
EmailCommand.cs                  → sends ALL emails (400 lines)
```

### Endpoint Files: Declarations and Implementations

Endpoint/route files use two sections:

1. **Declarations** — one-liner route-to-method mappings (no inline lambdas)
2. **Implementations** — thin private methods that validate, delegate to handlers/commands, and return results

**No data access, business logic, or side effects in endpoint methods.** They are traffic directors, not workers.

---

## 5. Guard the Database Like a Vault

**Why:** The database is the most important part of any app. It's also the hardest thing to fix when it goes wrong. A messy data layer causes bugs that are expensive, painful, and sometimes impossible to undo.

### Soft Delete

Never physically delete records. Add an `IsDeleted` flag (default `false`) and a global query filter that automatically excludes deleted records from all queries.

```csharp
// Reference (C#)

/* Entity has the flag */
public bool IsDeleted { get; set; } = false;

/* DbContext applies a global filter */
modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);

/* "Delete" just flips the flag */
user.IsDeleted = true;
await repository.UpdateAsync(user);
```

**Why not just delete?** Because:
- Accidental deletions are recoverable
- Audit trails are preserved
- Cascading deletes don't nuke related data
- Customer support can investigate issues on "deleted" accounts

### Audit Fields

Every record tracks who created it, when, who last updated it, and when. These four fields are on every entity:

- `CreateByUserId` + `CreatedOnDateTime` — set on INSERT, never changed
- `UpdatedByUserId` + `UpdatedOnDate` — set on UPDATE only (`null` until first update)

**Critical rule:** Audit fields are populated **automatically by a resolver** — never by application code. No handler, service, or controller should ever set these fields. The resolver reads the current user from the DI-injected user service and stamps the fields during save.

### Transaction Scope: DB Inside, Side Effects Outside

When you have a multi-step operation (save to DB + send email + write log), wrap **only the DB writes** in a transaction. Keep side effects (emails, logging, analytics) **outside** the transaction.

**Why?** If the email fails, the DB commit should still hold. If the DB fails, no email should be sent. Each concern fails independently.

```csharp
// Reference (C#)

/* Good — DB writes inside transaction, email outside */
// TRANSACTION
var result = await TransactionScope.ExecuteAsync(async () =>
{
    var order = await orderHandler.CreateOrderAsync(payload);
    var subscription = await subscriptionHandler.ActivateAsync(order);
    return new { order, subscription };
});

// Side effects — outside the transaction
await emailService.SendConfirmationAsync(result.order);
logger.LogInformation("Order created: {OrderId}", result.order.Id);

/* Bad — everything in one transaction (email failure rolls back the order!) */
using var transaction = await db.BeginTransactionAsync();
var order = await orderHandler.CreateOrderAsync(payload);
await emailService.SendConfirmationAsync(order);  // if this fails, order rolls back!
await transaction.CommitAsync();
```

**Mark transaction boundaries** with a `// TRANSACTION` comment immediately above the scope. This makes them scannable.

### No Nulls Philosophy

When different variants of a record have different data, use **dedicated tables** instead of nullable columns on the base table.

```
/* Good — each provider gets its own table */
User (Id, DisplayName, FirstName)
UserAuthGoogle (UserId → User.Id, GoogleId, Email, AvatarUrl)
UserAuthApple  (UserId → User.Id, AppleId, Email, IsPrivateRelay)

/* Bad — one table with nullable columns that grow forever */
User (Id, DisplayName, FirstName,
      GoogleId?, GoogleEmail?, GoogleAvatarUrl?,
      AppleId?, AppleEmail?, AppleIsPrivateRelay?,
      MicrosoftId?, MicrosoftEmail?, ...)
```

**Why?** Adding a new provider means adding a new table — no schema changes to existing tables. Each table is clean and fully populated (no nulls). The base table stays focused on what all variants share.

### Column Ordering Convention

Be consistent about column order in every table:

1. **Primary key** first
2. **Foreign keys** (alphabetical)
3. **Domain columns** (alphabetical)
4. **Flags** (`IsDeleted`, `IsVisible`)
5. **Audit columns** (`CreatedBy`, `CreatedOn`, `UpdatedBy`, `UpdatedOn`)

### Schema Separation

Group tables into schemas by functional domain (e.g., `auth`, `commerce`, `app`). Keep the default schema (`dbo`, `public`) empty.

### One Migration Per Release

Don't create multiple migration files for the same release. Consolidate schema changes into a single migration before finalizing. This keeps the migration history clean and deployments simple.

---

## 6. Fail Like a Ninja

**Why:** Every app encounters errors. The question is whether it handles them gracefully (shows a branded error page, notifies the team, recovers) or crashes and burns (white screen of death, lost data, silent failure).

### Exception Hierarchy

Build a structured exception hierarchy — not a flat list of random exception types:

```
AppException (abstract base — has HttpStatusCode property)
├── UnauthorizedException (401)
│   └── SignInFailedException
├── ForbiddenException (403)
│   └── DatabaseConnectionFailedException
├── NotFoundException (404)
└── UnexpectedException (500)
```

**Why a hierarchy?** Because your error handler can catch the base type and read the status code, instead of having a giant `switch` statement for every exception type.

### Rollback Before Rethrow

If something fails inside a transaction, roll back the transaction **first**, then let the exception propagate. Never leave a half-committed transaction in the database.

```csharp
// Reference (C#)

/* The transaction scope handles this automatically: */
// - Success → commit
// - Exception → rollback, then rethrow
var result = await TransactionScope.ExecuteAsync(async () =>
{
    // If anything here throws, the whole block rolls back
    await repository.InsertAsync(order);
    await repository.InsertAsync(billing);
    return order;
});
// Exception rethrows AFTER rollback — downstream handlers see the original error
```

### Side Effects Outside Transactions

This bears repeating because it's the most common mistake:

- **Inside the transaction:** DB writes, claims updates, cache invalidation — anything that must be atomic
- **Outside the transaction:** Email sends, logging, analytics, webhook calls — anything that shouldn't prevent a DB commit

A failed email notification should **never** roll back a successful payment record.

### Middleware Pipeline

Structure your error handling as a pipeline:

1. **Probe filter** — short-circuit known bot/scanner requests before they hit the app
2. **Exception handler** — catch unhandled exceptions, log them, redirect to error page
3. **Status code pages** — handle HTTP errors (404, 403, etc.) with branded pages
4. **Error page** — extract exception details, render appropriate response

The pipeline means each concern is handled by a single, testable piece — not one massive try-catch block.

---

## 7. Test Like You Mean It

**Why:** Tests are your safety net. But testing the wrong things wastes time and creates a false sense of security. Test the things that can break, not the things that can't.

### Test Naming

Use the pattern: `MethodName_Scenario_ExpectedBehavior`

```csharp
// Reference (C#)

/* Good — tells you exactly what's being tested */
HandleAsync_NewUserWithGoogleProvider_CreatesUserAndReturnsNewUserFlag()
ValidateToken_ExpiredToken_ReturnsNull()
Serialize_NullPreferences_ReturnsEmptyJsonObject()

/* Bad — vague, doesn't tell you the scenario */
TestHandleAsync()
TokenTest()
SerializerWorks()
```

### What to Test

Test logic that **computes, transforms, validates, or can fail**:
- Cryptographic operations (token generation, hashing)
- Serialization/deserialization
- Precondition checks and validation
- Business rules and decision logic
- Sign-in flows and authentication handlers
- Edge cases (null input, empty strings, boundary values)

### What NOT to Test

Do **not** write tests for:
- Public property getters and setters (they can't break)
- Simple property-to-property mapping (mappers that just copy fields)
- POCO defaults (a class that just has properties with default values)
- Framework behavior (don't test that Entity Framework can save to a database)

**Rule of thumb:** If the code has no `if`, no loop, no calculation, and no external call — it doesn't need a test.

### In-Memory Stubs

For fast, database-free testing, create **stub implementations** of your repository interfaces backed by an in-memory list:

```csharp
// Reference (C#)

/* Stub repository — backed by List<T> instead of a database */
public class RepositoryStub<T> : IRepository<T>
{
    private readonly List<T> _store = new();

    public Task<T> GetByIdAsync(long id) => ...;
    public Task InsertAsync(T entity) { _store.Add(entity); ... }
}
```

This lets your tests run in milliseconds instead of seconds, with no database setup required.

### Test Infrastructure Pattern

Build reusable test infrastructure:

- **Builders** — orchestrate test setup (create DI container, expose handlers and services as properties)
- **Stubs** — in-memory implementations of interfaces (repositories, user service, audit resolver)
- **Identity providers** — generate sequential IDs for test entities (mimics database auto-increment)

The test DI container should **extend** the production container and **override** only the things that need stubs (database, external services). Everything else stays real.

---

## 8. The Ninja Code (Quick Reference)

Before declaring any task complete, scan this checklist:

- [ ] Every class has a doc comment (role label, not a sentence)
- [ ] Imports/usings are alphabetical
- [ ] Fields sorted by type name, assignments by variable name
- [ ] No entity types in presentation code — models only
- [ ] No audit field manipulation outside the resolver
- [ ] All DB writes in a transaction scope, side effects outside
- [ ] No inline code in markup files — logic lives in code-behind
- [ ] Tests follow `Method_Scenario_Expected` naming
- [ ] No tests for simple getters/setters or mappers
- [ ] CSS rules on single lines, declarations alphabetical
- [ ] One class per operation (handlers, commands)
- [ ] Entry point is a thin orchestrator calling extension methods
- [ ] Dependencies injected via constructor, never `new`ed up
- [ ] Interfaces over implementations in constructor parameters
- [ ] Folder structure mirrors functional domains, not technical roles
