using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Application.Features.Students.Commands.CreateStudent;

public class CreateStudentCommandHandler(
    ILogger<CreateStudentCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IMaskingService maskingService,
    IIdentityService identityService)
    : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(CreateStudentCommand command, CancellationToken ct)
    {
        var emailResult = await EnsureEmailIsValidAndUnique(command.UniversityMailAddress, ct);
        if (emailResult.IsFailure)
            return emailResult.Errors;


        TelegramUserId? telegramUserId = null;
        if(!string.IsNullOrWhiteSpace(command.TelegramUserId))
        {
            var telegramUserIdResult =
            await EnsureTelegramIdIsValidAndUnique(command.TelegramUserId, ct);

            if(telegramUserIdResult.IsFailure)
                return telegramUserIdResult.Errors;

            telegramUserId = telegramUserIdResult.Value;
        }


        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneNumberResult =
                await EnsurePhoneNumberIsValidAndUnique(command.PhoneNumber, ct);

            if (phoneNumberResult.IsFailure)
                return phoneNumberResult.Errors;

            phoneNumber = phoneNumberResult.Value;
        }


        var nameCreationResult = StudentName.Create(command.Name);

        if(nameCreationResult.IsFailure)
            return nameCreationResult.Errors;

        //Create User

        var userCreationResult = await identityService.CreateStudent(emailResult.Value.Value,command.Password,ct);

        if(userCreationResult.IsFailure)
            return userCreationResult.Errors;

        string personalPhotoFileName = Path.GetFileName(command.PersonalPhotoFileName);// ignore the path


        var createdStudentResult = Student.Create(
            id: Guid.NewGuid(),
            name: nameCreationResult.Value,
            universityMail: emailResult.Value,
            personalPhotoFileName: personalPhotoFileName,
            userId:userCreationResult.Value,
            phoneNumber: phoneNumber,
            telegramUserId: telegramUserId);

        if (createdStudentResult.IsFailure)
        {
            logger.LogWarning("Student creation failed. Errors : {Errors}", string.Join(',', createdStudentResult.Errors));
            await RollbackCreatedUserAsync(userCreationResult.Value, ct);
            return createdStudentResult.Errors;
        }

        try
        {
            context.Students.Add(createdStudentResult.Value);
            await context.SaveChangesAsync(ct);
        }
        catch
        {
            await RollbackCreatedUserAsync(userCreationResult.Value, ct);
            throw; //To Pipeline to be handled
        }

        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student created successfully with ID: {StudentId}", createdStudentResult.Value.Id);

        return StudentDto.FromEntity(createdStudentResult.Value);
    }

    private async Task RollbackCreatedUserAsync(string UserId,CancellationToken ct)
    {
        var deleteResult = await identityService.DeleteUserByIdAsync(UserId, ct);

        if (deleteResult.IsFailure)
        {
            logger.LogCritical(
                "CRITICAL: Failed to rollback user creation for user {UserId}",
                UserId);
        }
    }
    private async Task<Result<UniversityMail>> EnsureEmailIsValidAndUnique(string email, CancellationToken ct)
    {
        var emailResult = UniversityMail.Create(email);

        if (emailResult.IsFailure)
            return emailResult.Errors;


        if (
            await identityService.UserEmailExists(emailResult.Value.Value, ct)
        || await context.Students.AnyAsync(s => s.UniversityMail.Value == emailResult.Value.Value, ct)
        )
        {
            logger.LogWarning(
                "Student creation failed. Reason: {Reason}, Value: {Value}",
                "Email Exists",
                maskingService.MaskEmail(email));
            return StudentApplicationErrors.EmailAlreadyExists;
        }

        return emailResult;
    }
    private async Task<Result<TelegramUserId>> EnsureTelegramIdIsValidAndUnique(string telegrameUserId, CancellationToken ct)
    {
        var telegramUserIdResult = TelegramUserId.Create(telegrameUserId);

        if (telegramUserIdResult.IsFailure)
            return telegramUserIdResult.Errors;

        var telegramUserIdExists = await context.Students.AnyAsync(
            s => s.TelegramUserId != null
            && s.TelegramUserId.Value == telegramUserIdResult.Value.Value, ct);

        if (telegramUserIdExists)
        {
            logger.LogWarning(
               "Student creation failed. Reason: {Reason}, Value: {Value}",
               "Telegramuser ID Exists",
              maskingService.MaskTelegramUserId(telegramUserIdResult.Value.Value));

            return StudentApplicationErrors.TelegramUserIdAlreadyExists;
        }

        return telegramUserIdResult;
    }
    private async Task<Result<PhoneNumber>> EnsurePhoneNumberIsValidAndUnique(string phoneNumber, CancellationToken ct)
    {
        var phoneNumberResult = PhoneNumber.Create(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Errors;

        var phoneNumberExists = await context.Students.AnyAsync(
            s => s.PhoneNumber != null
            && s.PhoneNumber.Value == phoneNumberResult.Value.Value, ct);

        if (phoneNumberExists)
        {
            logger.LogWarning(
               "Student creation failed. Reason: {Reason}, Value: {Value}",
               "Phone Number Exists",
              maskingService.MaskPhoneNumber(phoneNumberResult.Value.Value));

            return StudentApplicationErrors.PhoneNumberAlreadyExists;
        }
        return phoneNumberResult;
    }
}