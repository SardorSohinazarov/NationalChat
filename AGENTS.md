# NationalChat Development Rules

## Time Handling

- All persisted timestamps and API timestamps must use UTC.
- Never use `DateTime.Now` or `DateTime.Today` in application code.
- Prefer `TimeProvider.GetUtcNow().UtcDateTime` in services so time-dependent code remains testable.
- Use `DateTime.UtcNow` only where injecting `TimeProvider` is not practical.
- Values received from outside the application must be converted to `DateTimeKind.Utc` before persistence when they represent an instant in time.
- PostgreSQL timestamp fields that represent an instant must use `timestamp with time zone`.
- Use `DateOnly` for a calendar date that is not an instant in time.
