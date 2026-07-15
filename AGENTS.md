# NationalChat Development Rules

## Clean Architecture

- Follow Clean Architecture for every new feature and refactor.
- `Domain` contains business entities, value types, enums, and domain rules only. It must not reference other projects or framework/persistence concerns.
- `Application` contains use cases, DTOs, and interfaces (ports). It may reference only `Domain`.
- `Infrastructure` implements Application interfaces for databases, external services, email, cryptography, and other technical concerns. It may reference `Application` and `Domain`.
- `API` is the composition root and HTTP transport layer. It may reference `Application` and `Infrastructure`, but business logic must not be placed in controllers or `Program.cs`.
- Dependencies must always point inward: `API -> Infrastructure -> Application -> Domain`.
- Do not let `Domain` or `Application` reference `Infrastructure` or `API`.

## Time Handling

- All persisted timestamps and API timestamps must use UTC.
- Never use `DateTime.Now` or `DateTime.Today` in application code.
- Prefer `TimeProvider.GetUtcNow().UtcDateTime` in services so time-dependent code remains testable.
- Use `DateTime.UtcNow` only where injecting `TimeProvider` is not practical.
- Values received from outside the application must be converted to `DateTimeKind.Utc` before persistence when they represent an instant in time.
- PostgreSQL timestamp fields that represent an instant must use `timestamp with time zone`.
- Use `DateOnly` for a calendar date that is not an instant in time.

## Pagination

- Use cursor (keyset) pagination for mutable, ordered data such as chats and messages; do not use page-number pagination there.
- Use `beforeId` and a bounded `limit` (1-100) for loading older records, ordered by descending ID.
- Return `items`, `nextCursor`, and `hasMore`; avoid `totalCount` unless a concrete UI requirement justifies its query cost.

## C# Style

- Use primary constructors for dependency-injected classes when constructor parameters only initialize dependencies.
- Use a traditional constructor only when it contains meaningful validation, transformation, or other initialization logic.

## Dependency Injection

- Keep service registrations out of `Program.cs`.
- Group service registrations in focused `IServiceCollection` extension methods under an `Extensions` folder.
- Keep `Program.cs` limited to application bootstrap and middleware pipeline configuration.

## Layer Boundaries

- Keep HTTP controllers, middleware configuration, and transport concerns in the API layer.
- Keep external technical adapters, including JWT token issuers, SMTP senders, persistence stores, and cryptographic implementations, in Infrastructure.
- Keep DTOs in Application under `Features/<FeatureName>/DataTransferObjects`, grouped by purpose such as `Requests`, `Commands`, `Responses`, or `Session`.
- Name persistence abstractions in Application as `I<Feature>Repository` and EF Core implementations in Infrastructure as `Ef<Feature>Repository`.
- Place EF Core repositories under `Infrastructure/Persistence/Repositories` and inherit from `BaseRepository<TEntity>` when applicable.
