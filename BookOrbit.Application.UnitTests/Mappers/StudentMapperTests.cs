namespace BookOrbit.Application.UnitTests.Mappers;

using BookOrbit.Application.Features.Students.Dtos;
using BookOrbit.Domain.Common.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using BookOrbit.Domain.Students;
using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Domain.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

public class StudentMapperTests
{
    [Fact]
    public void FromEntity_ShouldMapPointsAndJoinDate()
    {
        // Arrange
        var studentName = StudentName.Create("Test Student").Value;
        var universityMail = UniversityMail.Create("student@std.mans.edu.eg").Value;
        var phoneNumber = PhoneNumber.Create("01012345678").Value;

        var student = Student.Create(
            Guid.NewGuid(),
            studentName,
            universityMail,
            "photo.png",
            "user-1",
            phoneNumber).Value;

        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var joinDate = createdAt.AddMinutes(5);

        student.SetCreatedAt(createdAt);
        student.MarkAsApproved(joinDate).IsSuccess.Should().BeTrue();
        student.AddPoints(Point.Create(3).Value).IsSuccess.Should().BeTrue();

        // Act
        var dto = StudentDto.FromEntity(student);

        // Assert
        dto.Id.Should().Be(student.Id);
        dto.Name.Should().Be(student.Name.Value);
        dto.Points.Should().Be(student.Points.Value);
        dto.State.Should().Be(student.State);
        dto.JoinDate.Should().Be(joinDate);
    }
}
