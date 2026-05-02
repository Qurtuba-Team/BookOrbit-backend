using BookOrbit.Domain.Students;

namespace BookOrbit.Domain.ChatGroups;

public class ChatGroup : AuditableEntity
{
    public Guid Student1Id { get; }
    public Guid Student2Id { get; }

    public Student? Student1 { get; private set; }
    public Student? Student2 { get; private set; }

    private ChatGroup() { }

    private ChatGroup(Guid id, Guid student1Id, Guid student2Id) : base(id)
    {
        Student1Id = student1Id;
        Student2Id = student2Id;
    }

    public static Result<ChatGroup> Create(Guid id, Guid student1Id, Guid student2Id)
    {
        if (id == Guid.Empty)
            return ChatGroupErrors.IdRequired;

        if (student1Id == Guid.Empty)
            return ChatGroupErrors.Student1IdRequired;

        if (student2Id == Guid.Empty)
            return ChatGroupErrors.Student2IdRequired;

        if (student1Id == student2Id)
            return ChatGroupErrors.SameStudent;

        // Ensure a consistent ordering to prevent duplicate groups like (A,B) and (B,A)
        var firstStudentId = student1Id.CompareTo(student2Id) < 0 ? student1Id : student2Id;
        var secondStudentId = student1Id.CompareTo(student2Id) < 0 ? student2Id : student1Id;

        return new ChatGroup(id, firstStudentId, secondStudentId);
    }
}
