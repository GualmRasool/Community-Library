# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project Overview

**CommunityLibrary** is a single-project ASP.NET Core Web API (`net8.0`) that models a small
community book-lending library. It exposes three capability areas:

- **Book search** — paged, filterable catalogue lookup.
- **Loans (borrowing)** — create a loan against a book, read one loan, list loans.
- **Returns** — mark a loan returned and free the book.

Persistence is **Entity Framework Core InMemory** (`LibraryDb`), seeded at startup. There is
no real database, no migrations, and no authentication. This is effectively a teaching /
prototype codebase: state resets on every restart.

Key facts:

| Aspect | Value |
| --- | --- |
| Target framework | `net8.0` |
| Installed SDK | 9.0.304 (rolls forward to build `net8.0`) |
| Nullable reference types | `enable` |
| Implicit usings | `enable` |
| XML doc generation | `true` (`NoWarn` 1591 — missing-comment warnings suppressed) |
| EF Core | 8.0.31 (`Microsoft.EntityFrameworkCore`), InMemory 8.0.30 |
| Swagger | Swashbuckle.AspNetCore 6.6.2 |
| Tests | **None — no test project exists** |
| Source control | Not a git repository |

## Architecture

A two-layer (controller → repository) design. There is **no service/application layer** —
business rules live in the repositories.

```
HTTP request
    |
    v
Controllers/            Input validation, HTTP status selection, ActionResult shaping
    |                   (BooksController, LoansController, ReturnsController)
    v
Repository/             Data access + business rules + entity-to-DTO mapping
    |                   (IBooksRepository/BooksRepository, ILoansRepository/LoansRepository)
    v
LibraryDbContext        EF Core DbContext over the InMemory provider
    |
    v
Models/Entities         Book, Loans (+ BookStatus, LoanStatus enums)
```

Supporting pieces:

- `Models/Request/*` — inbound request bodies (`BorrowRequest`, `ReturnRequest`).
- `Models/DTOs/*` — outbound shapes (`BookDTO`, `LoansDTO`, envelope + pagination types,
  `ErrorResponse`).
- `Exceptions/ApiExceptions.cs` — `ApiException` hierarchy carrying an HTTP status code plus
  an error code. **Note: nothing currently catches these** (see *Error Handling Conventions*).
- `Repository/DbSeeder.cs` — static seed data with fixed, well-known book GUIDs.

**Mapping responsibility.** Entity-to-DTO mapping is done by hand in static `MapToDto` methods
on the repositories. `BooksRepository.MapToDto` is deliberately `public static` because
`LoansRepository.MapToDto` reuses it for the nested `Book` on a loan. No AutoMapper.

**Conscious boundary.** Controllers never touch `LibraryDbContext` directly and never return
entities — only DTOs. Preserve this when adding endpoints.

## Folder Structure

```
CommunityLibrary.sln
CLAUDE.md
CommunityLibrary/
├── CommunityLibrary.csproj
├── Program.cs                     # Minimal-hosting startup: DI, Swagger, seeding, pipeline
├── CommunityLibrary.http          # Manual request scratchpad (STALE — see Test Commands)
├── appsettings.json
├── appsettings.Development.json
├── Properties/launchSettings.json # http / https / IIS Express profiles
├── Controllers/
│   ├── BooksController.cs
│   ├── LoansController.cs
│   └── ReturnsController.cs
├── Exceptions/
│   └── ApiExceptions.cs           # namespace CommunityLibrary.Api.Exceptions
├── Models/
│   ├── DTOs/
│   │   ├── BooksDTO.cs            # BookDTO, PaginationInfo, BookSearchResponse,
│   │   │                          #   ErrorDetail, ErrorResponse
│   │   └── LoansDTO.cs            # LoansDTO, LoanResponse, LoansListResponse
│   ├── Entities/
│   │   ├── Book.cs                # Book + BookStatus enum
│   │   └── Loans.cs               # Loans + LoanStatus enum + RefreshComputedStatus
│   └── Request/
│       ├── BorrowRequest.cs
│       └── ReturnRequest.cs
└── Repository/
    ├── IBooksRepository.cs / BooksRepository.cs
    ├── ILoansRepository.cs / LoansRepository.cs
    ├── LibraryDbContext.cs
    └── DbSeeder.cs                # NOTE: declares no namespace (global)
```

Multiple related types share one file (e.g. all book DTOs in `BooksDTO.cs`). This is the
established convention here — follow it rather than splitting into one-type-per-file.

## Build Commands

Run from the repository root (`C:\Rasool\Automation\CommunityLibrary`).

```powershell
dotnet restore CommunityLibrary.sln
dotnet build CommunityLibrary.sln                      # Debug
dotnet build CommunityLibrary.sln -c Release
dotnet clean CommunityLibrary.sln
dotnet publish CommunityLibrary/CommunityLibrary.csproj -c Release -o ./publish
```


## Run Commands

```powershell
dotnet run --project CommunityLibrary/CommunityLibrary.csproj                      # http profile
dotnet run --project CommunityLibrary/CommunityLibrary.csproj --launch-profile https
dotnet watch --project CommunityLibrary/CommunityLibrary.csproj run                # hot reload
```

URLs (from `launchSettings.json`):

| Profile | URLs | Swagger |
| --- | --- | --- |
| `http` | `http://localhost:5046` | `http://localhost:5046/swagger` |
| `https` | `https://localhost:7143`, `http://localhost:5046` | `https://localhost:7143/swagger` |
| IIS Express | `http://localhost:20843` (ssl 44358) | `/swagger` |

All profiles set `ASPNETCORE_ENVIRONMENT=Development` and auto-launch `/swagger`.

`app.UseHttpsRedirection()` is active, so plain-HTTP calls to the `https` profile get
redirected. When testing with `curl`, prefer the HTTPS URL (add `-k` for the dev certificate)
or use the `http` profile.

## Test Commands

**There is no test project in this solution.** `dotnet test` currently finds nothing to run.
Do not claim tests pass — there are none.

Manual verification today is Swagger UI plus `CommunityLibrary/CommunityLibrary.http`. That
`.http` file is **stale**: it still calls `GET /weatherforecast/`, an endpoint that does not
exist. Updating it to hit real routes is a useful, low-risk change.

If asked to add tests, scaffold alongside the existing project and wire it into the solution:

```powershell
dotnet new xunit -o CommunityLibrary.Tests
dotnet add CommunityLibrary.Tests/CommunityLibrary.Tests.csproj reference CommunityLibrary/CommunityLibrary.csproj
dotnet sln CommunityLibrary.sln add CommunityLibrary.Tests/CommunityLibrary.Tests.csproj
dotnet test CommunityLibrary.sln
```

## Coding Standards

Match the surrounding code. Current conventions, including the rough edges:

- `using` directives at the top of the file; `namespace X { }` block style everywhere except
  `Exceptions/ApiExceptions.cs`, which uses a file-scoped namespace.
- 4-space indentation, Allman braces.
- `private readonly` dependency fields prefixed with `_`.
- Expression-bodied members for trivial mapping/factory methods (`MapToDto`,
  `ErrorResponse.Create`).
- Nullable reference types are on. Initialize reference-typed properties to `string.Empty` /
  `new()` instead of leaving them null; mark genuinely optional members `?`.
- `async`/`await` with `CancellationToken ct = default` threaded through on read paths.
- No `.Result`, no `.Wait()`, no `async void`.
- There is **no `.editorconfig`** and no analyzer package. Style is enforced only by
  convention, so read neighbouring files before writing.

### Known inconsistencies — do not "fix" as drive-by work

The codebase is not internally consistent. Leave these alone unless the user explicitly asks
for a cleanup, because renaming them is a wide, behaviour-visible change (route names and JSON
property names derive from some of these identifiers):

- Method names are `camelCase` instead of C# `PascalCase`: `getBooks`, `addLoan`, `allLoan`,
  `updateLoan`, `LoanbyID`, and the action methods `books`, `loan`, `loanbyid`, `returnLoans`,
  `BarrowBook` (the last is also a misspelling of "Borrow").
- Field typos: `_booksRepositry`, `_loansRespository`.
- `ReturnRequest.returnedAt` is camelCase, unlike every other property.
- `BorrowRequest.MemberID` vs `Loans.MemberId` — inconsistent casing of "Id".
- Entity `Loans` is plural but represents one loan; `LibraryDbContext.Loan` is the singular
  `DbSet` name for it. Both are backwards from convention.
- Unused `using`s exist in several files (e.g. `static System.Reflection.Metadata.BlobBuilder`
  in `ReturnsController.cs`, `System.Net` in several).
- `Exceptions/ApiExceptions.cs` sits in `CommunityLibrary.Api.Exceptions`, not
  `CommunityLibrary.Exceptions`; `DbSeeder` has no namespace at all.

When adding **new** code, prefer correct C# naming (PascalCase methods) rather than
propagating the existing camelCase — but never rename existing public members as a side effect
of unrelated work.

## Dependency Injection Patterns

All registration happens in `Program.cs`. There is no DI extension-method module pattern.

```csharp
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<ILoansRepository, LoansRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("LibraryDb"));
```

Rules:

- **Repositories are `Scoped`** — they depend on the scoped `LibraryDbContext`. Never register
  a repository (or anything holding a `DbContext`) as `Singleton`.
- **`TimeProvider` is `Singleton`** and is the only sanctioned source of current time. Inject
  it; do not call `DateTime.UtcNow` / `DateTime.Now` directly.
- **Constructor injection only.** No `[FromServices]` on actions, no service locator, no
  `IServiceProvider` injection into controllers or repositories.
- Every repository is registered against an interface. A new data-access type gets an
  `IFooRepository` + `FooRepository` pair in `Repository/` and one `AddScoped` line.
- Seeding runs once at startup inside an explicit `app.Services.CreateScope()` block, because
  `LibraryDbContext` is scoped and cannot be resolved from the root provider.

## Entity Framework Guidelines

The provider is **InMemory**. It is not a relational database, and that shapes several rules.

- **No migrations, no `Migrations/` folder.** The schema comes from the model. Do not run
  `dotnet ef migrations add` or `dotnet ef database update` — there is nothing to migrate, and
  the EF tools package is not referenced.
- **`LibraryDbContext` has no `OnModelCreating`.** Configuration is entirely by convention:
  `Id` properties are keys, and `Loans.BookId` + `Loans.Book` + `Book.Loans` form the
  relationship. If real constraints are needed, add `OnModelCreating` with a Fluent API block
  rather than scattering data annotations on entities.
- **Reads use `.AsNoTracking()`.** Keep this on query paths that only project to DTOs.
- **Writes must `SaveChangesAsync()`.** `addLoan` does; `updateLoan` does **not** — see *Other
  known gaps*.
- **`.Include(l => l.Book)`** is required to populate the nested `BookDTO` on a loan. Without
  it, `LoansDTO.Book` is `null` (there is no lazy loading).
- **`Contains(..., StringComparison.OrdinalIgnoreCase)` in `BooksRepository.getBooks` only
  works because of the InMemory provider**, which evaluates the predicate as LINQ-to-Objects.
  A relational provider would throw a translation error. If the project ever moves to SQL, that
  filter must be rewritten (e.g. `EF.Functions.Like`, or `.ToLower().Contains(...)`). Keep this
  in mind before suggesting a provider swap.
- Paging is `.OrderBy(...).Skip(offset).Take(limit)` — always order before skipping.
- `allLoan` materializes **all** loans with `ToListAsync` before filtering and paging, because
  `RefreshComputedStatus` is C# logic that cannot be translated to a query. That is
  intentional, not an oversight; it is acceptable at this data size.
- Data resets on every restart, and `DbSeeder.Seed` is guarded by `if (!context.Books.Any())`,
  so it is safe to re-run.

## API Conventions

### Endpoint map

| Method | Route | Action | Notes |
| --- | --- | --- | --- |
| `GET` | `/Books` | `BooksController.books` | `?query=&status=Available&limit=20&offset=0` |
| `POST` | `/books/{bookId:guid}/Loan` | `LoansController.BarrowBook` | body `BorrowRequest`, returns 201 |
| `GET` | `/Loans/{loanId:guid}` | `LoansController.loanbyid` | |
| `GET` | `/Loans` | `LoansController.loan` | `?status=All&limit=10&offset=0` |
| `POST` | `/loans/{loanId:guid}/return` | `ReturnsController.returnLoans` | body `ReturnRequest` |

**Routing is deliberately mixed and route casing is inconsistent** (`/Books`, `/books/...`,
`/Loans`, `/loans/...`). ASP.NET Core route matching is case-insensitive, so this works, but:

- `BooksController` uses a class-level `[Route("/[controller]")]`.
- `LoansController` has **no** class-level route; each action carries its own absolute
  template. Its commented-out `//[Route("api/[controller]")]` is intentionally left disabled —
  enabling it would break every loans route.
- `ReturnsController` uses a class-level `[Route("/loans")]` plus a relative action route.

There is no `/api` prefix and no versioning. Do not introduce either unilaterally — it would
break existing route contracts.

### Conventions to follow

- All controllers are `[ApiController]` and derive from `ControllerBase`.
- `[Produces("application/json")]` on `BooksController`; apply it to new controllers too.
- Declare outcomes with `[ProducesResponseType(StatusCodes.StatusNNN, Type = typeof(T))]`.
  Include the `Type` for success responses — `LoansController` and `ReturnsController` omit it
  in places, which weakens the generated Swagger schema; new code should include it.
- Return `Task<ActionResult<T>>` and use the typed helpers: `Ok(...)`, `BadRequest(...)`,
  `NotFound(...)`, `CreatedAtAction(...)`.
- `POST` creates return **201** via
  `CreatedAtAction(nameof(loanbyid), new { loanId = ... }, result)`.
- Bind explicitly: `[FromQuery]` for query parameters, `[FromBody]` for request bodies.
- Route constraints on identifiers: `{bookId:guid}`, `{loanId:guid}`.
- Accept `CancellationToken ct = default` as the last parameter on read actions and pass it
  down to EF.

### Response envelopes

Collections and single resources are wrapped, never returned bare:

- Single: `{ "data": { ... } }` — `LoanResponse`.
- Collection: `{ "data": [ ... ], "pagination": { "limit", "offset", "total" } }` —
  `BookSearchResponse`, `LoansListResponse`.

Reuse `PaginationInfo` for any new paged endpoint.

### Serialization

`AddJsonOptions` registers `JsonStringEnumConverter`, so `BookStatus` and `LoanStatus`
serialize as strings (`"Active"`, `"Available"`). Property names use the default camelCase
policy. `DueDate` is a `DateOnly`; `BorrowedAt` / `ReturnedAt` are `DateTime` (UTC).

### Validation

Two styles coexist:

- **Data annotations** (`[Required]`) on request models, enforced automatically by
  `[ApiController]`, which returns an RFC 7807 `ValidationProblemDetails` — a shape that does
  **not** match `ErrorResponse`.
- **Manual guard clauses** at the top of actions for query-parameter rules (`limit` bounds,
  `offset >= 0`, enum parsing, `query` length ≤ 255).

Bounds are not uniform: `BooksController` allows `limit` 1–100, `LoansController` allows 1–10.
Preserve each endpoint's existing bounds rather than harmonizing them silently.

## Error Handling Conventions

There are **three** competing error styles in the codebase. Know which one you are in.

**1. Structured `ErrorResponse` (the intended standard).** `BooksController` only.
Machine-readable code plus human message:

```csharp
return BadRequest(ErrorResponse.Create("INVALID_LIMIT", "Limit must be between 1 and 100."));
```

Existing codes: `INVALID_QUERY`, `INVALID_STATUS`, `INVALID_LIMIT`, `INVALID_OFFSET`,
`BOOK_NOT_FOUND`, `BOOK_NOT_AVAILABLE`. Codes are `SCREAMING_SNAKE_CASE`; messages are
complete sentences. **Prefer this style for new code.**

**2. Bare strings.** `LoansController` returns `BadRequest("Invalid Status")` and
`NotFound($"Loan Not Found for {loanId}")`. Inconsistent with (1), but it is the current
contract for those endpoints.

**3. Thrown `ApiException` subclasses.** `LoansRepository` throws `NotFoundApiException`
(`BOOK_NOT_FOUND`) and `ConflictApiException` (`BOOK_NOT_AVAILABLE`).

### Critical gap

**`ApiException` is never caught.** `Program.cs` registers no exception-handling middleware, no
`IExceptionHandler`, no `UseExceptionHandler`, and no exception filter. The `StatusCode` and
`ErrorCode` those exceptions carry are therefore **ignored** — a thrown `NotFoundApiException`
surfaces as an unhandled exception (developer exception page in Development, bare HTTP 500
otherwise), not as a 404.

So: `POST /books/{unknownId}/Loan` returns 500, not the documented 404, and borrowing an
already-borrowed book returns 500, not 409.

The clean fix is a global exception handler in `Program.cs` mapping `ApiException.StatusCode` /
`.ErrorCode` onto an `ErrorResponse` body. **Do not implement this silently as part of another
task** — it changes status codes on live endpoints. Raise it, and implement it when the user
asks.

Meanwhile, when adding a repository method: either throw an `ApiException` (correct long-term,
currently yields 500) or return a sentinel the controller translates (matches today's
behaviour). Say which you chose.

### Other known gaps

These are real defects, not style preferences. Fix only what the task covers, and mention the
rest.

- **`LoansRepository.updateLoan` never calls `SaveChanges`** and is synchronous. Returns are
  not persisted, so `POST /loans/{id}/return` reports success while changing nothing durable.
- **`updateLoan` does not `.Include(l => l.Book)`**, so `loan.Book` is `null` and the
  `book.Status = Available` reset never runs. The book stays `Borrowed` forever.
- **`updateLoan` returns `null`** for an unknown `loanId`, and `ReturnsController` wraps it in
  `Ok(null)` — a 200 for a nonexistent loan. This is the source of the `CS8603` warning.
- **`BooksRepository.getBooks` uses the synchronous `.Count()`** on the query while the rest of
  the method is async.
- **`BooksController`'s default `status` is `"Available"`**, so `GET /Books` with no query
  string hides borrowed books. `BookStatus.All` is a valid enum value but is treated as a
  concrete filter value (`b.Status == BookStatus.All`), so `?status=All` matches **nothing**
  rather than everything.
- **`BooksController.books` declares 400 but not the error type** —
  `[ProducesResponseType(StatusCodes.Status400BadRequest)]` has no
  `Type = typeof(ErrorResponse)`.
- **`LoansController.BarrowBook` dereferences `result.Data.Id`** without a null check.
- **`BookSearchResponse.Pagination.Total`** counts filtered rows before paging — that is
  correct; keep the behaviour when editing.
- **`DbSeeder.Book4Id`** is declared but no fourth book is seeded.
- **`Loans.Notes`** is written by `updateLoan` but never surfaced in `LoansDTO`.

## Swagger Configuration

Configured in `Program.cs`:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
```

- `AddSwaggerGen` is called **twice**. This is harmless (options configuration is additive) but
  redundant — the XML-comments callback could fold into a single call. Cosmetic only.
- XML comments require `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the
  `.csproj`. **Do not remove that property** — Swagger descriptions vanish if you do.
  `<NoWarn>1591</NoWarn>` suppresses "missing XML comment" warnings so undocumented members
  stay quiet.
- Swagger UI is **Development-only**:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

- No custom `SwaggerDoc` title/version, no security definitions, no API versioning.

To document a new endpoint properly: `///` `<summary>`, a `<param>` for **every** parameter
(including `ct`, whose omission causes the `CS1573` warning), `<returns>`, plus a
`[ProducesResponseType]` per status code with `Type` set for success responses.

## Naming Conventions

Target state for new code (existing code deviates — see *Known inconsistencies*):

| Element | Convention | Example |
| --- | --- | --- |
| Namespace | `CommunityLibrary.<Folder>` | `CommunityLibrary.Models.DTOs` |
| Class / interface | PascalCase, `I`-prefixed interfaces | `BooksRepository`, `IBooksRepository` |
| Method | **PascalCase** | `GetBooks` (existing: `getBooks`) |
| Public property | PascalCase | `PublishedYear` |
| Private field | `_camelCase` | `_db`, `_timeProvider` |
| Local / parameter | camelCase | `bookStatus`, `loanId` |
| Enum type / member | PascalCase, singular type | `BookStatus.Available` |
| Entity | PascalCase singular | `Book` (existing `Loans` is wrongly plural) |
| DTO | `<Name>DTO` | `BookDTO`, `LoansDTO` |
| Response envelope | `<Name>Response` | `LoanResponse`, `BookSearchResponse` |
| List envelope | `<Name>ListResponse` | `LoansListResponse` |
| Request model | `<Verb>Request` | `BorrowRequest`, `ReturnRequest` |
| Exception | `<Reason>ApiException` | `NotFoundApiException` |
| Error code | `SCREAMING_SNAKE_CASE` | `BOOK_NOT_AVAILABLE` |
| Route segment | Plural resource | `/Books`, `/Loans` |
| JSON property | camelCase (default policy) | `publishedYear` |

`DbSet` properties: `Books` (plural, correct) and `Loan` (singular, inconsistent). Use
`_db.Loan` — that is the actual member name.

## Things Claude Should Avoid Modifying

**Never touch:**

- `.vs/` — Visual Studio's local cache (`.suo`, Copilot indices, `FileContentIndex`,
  `DesignTimeBuild`, `ProjectEvaluation`). Machine-specific; never edit or delete.
- `bin/`, `obj/` — build output, regenerated by `dotnet build`.
- `CommunityLibrary.csproj.user` — per-developer local settings.
- `CommunityLibrary.sln` GUIDs — `{E28FFB31-...}` and `SolutionGuid` must stay stable.

**Do not change without the user explicitly asking:**

- **Route templates or action-method names.** Renaming `BarrowBook`, `loanbyid`, `books`,
  `loan`, or `returnLoans` — or normalizing `/Books` vs `/books` casing — changes the public
  API surface. `nameof(loanbyid)` in `CreatedAtAction` also depends on that name.
- **`LoansController`'s commented-out `//[Route("api/[controller]")]`.** Uncommenting it breaks
  every loans route.
- **The camelCase method names and typo'd fields** (`_booksRepositry`, `_loansRespository`,
  `MemberID`, `returnedAt`). Cosmetic renames here churn a lot of files, and `returnedAt` is
  part of the JSON request contract.
- **`Loans` (plural entity) / `DbSet<Loans> Loan` (singular set).** Renaming either touches
  every repository and would alter the model.
- **`builder.Services.AddSingleton(TimeProvider.System)` and every `_timeProvider` call site.**
  This indirection exists for testability. Do not "simplify" it to `DateTime.UtcNow`.
- **`.AsNoTracking()` on read paths** and `.Include(l => l.Book)` on loan queries — removing
  either silently changes behaviour (nested `Book` becomes `null`).
- **`DbSeeder`'s hardcoded GUIDs** (`550e8400-e29b-41d4-a716-44665544000{0..3}`). They are
  stable fixtures used for manual testing; changing them invalidates saved requests.
- **`<GenerateDocumentationFile>` and `<NoWarn>1591</NoWarn>`** in the `.csproj` — both are
  required by the Swagger XML-comments setup.
- **The `net8.0` target framework.** The installed SDK is 9.0.304 and *could* target `net9.0`,
  but retargeting is a deliberate upgrade decision, not a cleanup.
- **The InMemory provider / `UseInMemoryDatabase("LibraryDb")`.** Swapping to a real database
  requires new packages, migrations, a connection string, and a rewrite of the
  `StringComparison`-based search filter.
- **`UseHttpsRedirection()` / `UseAuthorization()`** in the pipeline. `UseAuthorization` has no
  authentication behind it and is currently a no-op; removing it is still out of scope.
- **The global exception handler gap.** Adding one is the right fix, but it changes live status
  codes (500 → 404/409). Propose it; implement only on request.

**Flag to the user rather than deciding alone:** adding an `/api` prefix, API versioning,
authentication, AutoMapper, a service layer between controllers and repositories, or an
`.editorconfig` / analyzer package. Each is a cross-cutting architectural change to a codebase
that currently has none of them.

**Not a git repository.** There is no branch or commit history to fall back on, so there is no
safety net for destructive edits. Make changes incrementally; do not mass-delete or mass-rewrite
files.
