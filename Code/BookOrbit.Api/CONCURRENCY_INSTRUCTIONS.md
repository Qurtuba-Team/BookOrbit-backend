# Optimistic Concurrency Instructions

Use this pattern when adding optimistic concurrency to any command and entity:

1. **Request contract**
   - Make the API request inherit from `BookOrbit.Api.Contracts.Requests.Common.ConcurrencyRequest`.
   - Ensure the request exposes `RowVersion` and carries the concurrency token from the client.

2. **Command contract**
   - Make the application command inherit from `BookOrbit.Application.Common.Models.ConcurrencyCommand<TResponse>`.
   - Pass the request `RowVersion` into the command.

3. **Entity contract**
   - Make the domain entity implement `BookOrbit.Domain.Common.Entities.IConcurrencyEntity`.
   - Add `RowVersion` as `byte[]` with a private setter.

4. **Entity configuration**
   - In the entity configuration class, call `BookOrbit.Infrastructure.Data.Configurations.ConcurrencyEntityConfiguration.ConfigureConcurrency()`.
   - Keep the entity configuration responsible for mapping the concurrency token.

5. **Handler flow**
   - In the command handler, load the entity.
   - Apply the entity update logic.
   - Right before `SaveChangesAsync`, call `BookOrbit.Application.Common.Interfaces.ConcurrencyServices.IConcurrencyService.SetOriginalRowVersion()`.
   - If that call fails, return its errors immediately.

6. **DTOs**
   - Add `byte[] RowVersion` to all DTOs that must send the concurrency token back to the client.
   - Ensure DTO factories/projections populate `RowVersion` from the entity.
   - even ItemslistDto should have RowVersion in their porjection 
## Reference pattern
Use these files as the model implementation:

- `Code/BookOrbit.Api/Contracts/Requests/Students/UpdateStudentRequest.cs`
- `Code/BookOrbit.Application/Features/Students/Commands/UpdateStudent/UpdateStudentCommand.cs`
- `Code/BookOrbit.Application/Features/Students/Commands/UpdateStudent/UpdateStudentCommandHandler.cs`
- `Code/BookOrbit.Domain/Students/Student.cs`
- `Code/BookOrbit.Infrastructure/Data/Configurations/StudentConfiguration.cs`

## Notes
- The concurrency token must flow from API request to command to entity.
- The entity configuration must be in place for EF Core to enforce optimistic concurrency.
- The handler must set the original row version before saving changes.
- DTOs should return the row version so clients can submit it on the next update.
