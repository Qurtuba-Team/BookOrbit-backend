namespace BookOrbit.Application.Features.Chat.Dtos;
public record ChatGroupListItemDto
{
    public Guid ChatGroupId { get; set; } = Guid.Empty;
    public Guid OtherStudentId { get; set; } = Guid.Empty;
    public string OtherStudentName { get; set; } = string.Empty;
    public string OtherStudentPersonalPhotoFileName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private ChatGroupListItemDto() { }

    public ChatGroupListItemDto(
        Guid chatGroupId,
        Guid otherStudentId,
        string otherStudentName,
        string otherStudentPersonalPhotoFileName,
        DateTimeOffset createdAtUtc)
    {
        ChatGroupId = chatGroupId;
        OtherStudentId = otherStudentId;
        OtherStudentName = otherStudentName;
        OtherStudentPersonalPhotoFileName = otherStudentPersonalPhotoFileName;
        CreatedAtUtc = createdAtUtc;
    }
}

