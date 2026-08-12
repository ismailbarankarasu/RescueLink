using FluentAssertions;
using RescueLink.Domain.Common;

namespace RescueLink.Domain.Tests.Common;

public class BaseEntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeEntityCorrectly()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act
        var entity = new TestEntity();
        var afterCreation = DateTimeOffset.UtcNow;

        // Assert
        entity.Id.Should().NotBeEmpty();

        entity.CreatedAt.Should()
            .BeOnOrAfter(beforeCreation)
            .And.BeOnOrBefore(afterCreation);

        entity.UpdatedAt.Should().BeNull();
    }

    private sealed class TestEntity : BaseEntity
    {
    }
}