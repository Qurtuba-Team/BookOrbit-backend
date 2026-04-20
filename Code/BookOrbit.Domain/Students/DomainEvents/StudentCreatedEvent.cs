namespace BookOrbit.Domain.Students.DomainEvents;
public class StudentCreatedEvent(Guid studentId, UniversityMail universityMail) : DomainEvent
{
    public Guid StudentId { get; } = studentId;
    public UniversityMail UniversityMail { get; } = universityMail;
}
