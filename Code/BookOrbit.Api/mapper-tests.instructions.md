---
description: "Use when: writing mapper unit tests for DTO mapping in BookOrbit. Covers mapping of nested objects, collections, totals, and IDs."
applyTo: "**/*MapperTests.cs"
---

# Mapper unit tests (BookOrbit)

## Scope
- These instructions apply to mapper unit tests that validate mapping from domain entities to DTOs or view models.
- Keep tests focused on mapping correctness, not domain behavior.

## Style and tooling
- Use xUnit `[Fact]` and `[Theory]` for tests.
- Use FluentAssertions for assertions (e.g., `result.Should().Be(...)`).
- Follow Arrange / Act / Assert structure with clear variable names.
- Prefer `var` for locals unless explicit types improve readability.

## Test structure
- Name tests as `MethodName_ShouldExpectedBehavior`.
- Create minimal valid domain objects needed for mapping; set navigation properties when required.
- Validate the full mapping surface:
  - IDs and scalar fields.
  - Nested objects (e.g., `Labor`, `Vehicle`) and their key fields.
  - Collections (e.g., `RepairTasks`) including expected count and key values.
  - Derived totals and durations (sum of parts, labor, total cost, duration minutes).
  - Optional references (null or missing data) where applicable.

## Assertions checklist
- Verify all mapped fields, especially ones computed from child collections.
- Assert that optional nested objects are not null when provided.
- If the mapper depends on assigned navigation properties, set them explicitly in Arrange.

## Example patterns (high level)
- Build a parent entity with child entities and computed totals.
- Map to DTO and assert every corresponding field.
- Add a list mapping test for `IEnumerable<T>` to DTO list conversions.
