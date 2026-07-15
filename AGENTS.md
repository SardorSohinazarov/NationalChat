# NationalChat Development Rules

## Time Handling

- All persisted timestamps and API timestamps must use UTC.
- Never use `DateTime.Now` or `DateTime.Today` in application code.
- Prefer `TimeProvider.GetUtcNow().UtcDateTime` in services so time-dependent code remains testable.
- Use `DateTime.UtcNow` only where injecting `TimeProvider` is not practical.
- Values received from outside the application must be converted to `DateTimeKind.Utc` before persistence when they represent an instant in time.
- PostgreSQL timestamp fields that represent an instant must use `timestamp with time zone`.
- Use `DateOnly` for a calendar date that is not an instant in time.

## C# Style

- Use primary constructors for dependency-injected classes when constructor parameters only initialize dependencies.
- Use a traditional constructor only when it contains meaningful validation, transformation, or other initialization logic.

## Dependency Injection

- Keep service registrations out of `Program.cs`.
- Group service registrations in focused `IServiceCollection` extension methods under an `Extensions` folder.
- Keep `Program.cs` limited to application bootstrap and middleware pipeline configuration.
