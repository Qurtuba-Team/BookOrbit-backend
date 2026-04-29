---
description: "Use when: adding or updating subcutaneous tests in BookOrbit.Application.SubcutaneousTests. Covers structure, setup, and patterns for handlers."
applyTo: "BookOrbit.Application.SubcutaneousTests/**/*.cs"
---

# Subcutaneous tests (BookOrbit)

## Scope
- These instructions apply to tests in BookOrbit.Application.SubcutaneousTests.
- Tests exercise handlers (commands, queries, event handlers) with real EF Core in-memory context and minimal fakes.

## Style and tooling
- Use xUnit `[Fact]` and FluentAssertions.
- Follow Arrange / Act / Assert structure with clear variable names.
- Prefer `var` for locals unless explicit types improve readability.
- Keep tests focused on handler behavior and data changes.

## Project setup
- Use `StudentTestFactory.CreateDbContext()` for an EF Core in-memory `AppDbContext`.
- Use `StudentTestFactory.CreateHybridCache()` for `HybridCache`.
- Use minimal fakes in `Students/TestDoubles` for required services (e.g., `IIdentityService`, `IMaskingService`, `ISender`).
- When needed, configure fake responses explicitly (e.g., `FakeIdentityService.SetEmailConfirmed`).

## Test structure by handler type
- **Commands**: Verify state changes on domain entities and persistence in `AppDbContext`.
- **Queries**: Verify returned DTOs or values and that filtering/paging works as expected.
- **Event handlers**: Verify downstream commands/requests sent via `RecordingSender`.

## Data setup guidelines
- Build minimal valid domain entities using factory helpers.
- Set required navigation properties with `StudentTestFactory.SetNavigation` when projections or query joins depend on them.
- When date-sensitive behavior is involved, set timestamps using `StudentTestFactory.SetCreatedAt` and `TimeProvider` where applicable.

## Assertions checklist
- Verify `Result.IsSuccess` for successful paths.
- Assert key mapped fields on returned DTOs.
- Assert entity state changes (e.g., `Student.State`, `JoinDateUtc`, `PersonalPhotoFileName`).
- For list queries, assert count, pagination fields, and representative items.

## File organization
- Commands: `Students/Commands/*SubcutaneousTests.cs`
- Queries: `Students/Queries/*SubcutaneousTests.cs`
- Event handlers: `Students/EventHandlers/*SubcutaneousTests.cs`
- Shared helpers/fakes: `Students/StudentTestFactory.cs` and `Students/TestDoubles/*`
