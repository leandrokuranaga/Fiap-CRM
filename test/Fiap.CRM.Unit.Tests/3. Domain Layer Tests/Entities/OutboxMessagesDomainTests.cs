using Fiap.CRM.Domain.OutboxAggregate;

namespace Fiap.CRM.Unit.Tests._3._Domain_Layer_Tests.Entities;

public class OutboxMessagesDomainTests
{
    [Fact]
    public void ParameterlessConstructor_ShouldCreateOutboxMessage()
    {
        // Act
        var outboxMessage = new OutboxMessage();

        // Assert
        Assert.NotNull(outboxMessage);
        Assert.Equal(Guid.Empty, outboxMessage.Id);
        Assert.Null(outboxMessage.Type);
        Assert.Null(outboxMessage.Content);
        Assert.Equal(DateTime.MinValue, outboxMessage.OccuredOn);
        Assert.Null(outboxMessage.ProcessedOn);
    }

    [Fact]
    public void Constructor_ShouldCreateOutboxMessage_WhenValidParameters()
    {
        // Arrange
        var type = "UserCreated";
        var content = "{\"userId\": 1, \"name\": \"Test User\"}";
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, content, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
        Assert.Equal(occuredOn, outboxMessage.OccuredOn);
        Assert.Null(outboxMessage.ProcessedOn);
    }

    [Fact]
    public void Constructor_ShouldCreateOutboxMessage_WhenValidParametersWithProcessedOn()
    {
        // Arrange
        var type = "UserUpdated";
        var content = "{\"userId\": 2, \"name\": \"Updated User\"}";
        var occuredOn = DateTime.UtcNow.AddMinutes(-5);
        var processedOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, content, occuredOn, processedOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
        Assert.Equal(occuredOn, outboxMessage.OccuredOn);
        Assert.Equal(processedOn, outboxMessage.ProcessedOn);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullProcessedOn()
    {
        // Arrange
        var type = "UserDeleted";
        var content = "{\"userId\": 3}";
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, content, occuredOn, null);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
        Assert.Equal(occuredOn, outboxMessage.OccuredOn);
        Assert.Null(outboxMessage.ProcessedOn);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var outboxMessage = new OutboxMessage();
        var type = "GameAdded";
        var content = "{\"gameId\": 100}";
        var occuredOn = DateTime.UtcNow;
        var processedOn = DateTime.UtcNow.AddMinutes(1);

        // Act
        outboxMessage.Type = type;
        outboxMessage.Content = content;
        outboxMessage.OccuredOn = occuredOn;
        outboxMessage.ProcessedOn = processedOn;

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
        Assert.Equal(occuredOn, outboxMessage.OccuredOn);
        Assert.Equal(processedOn, outboxMessage.ProcessedOn);
    }

    [Theory]
    [InlineData("UserCreated", "{\"id\": 1}")]
    [InlineData("UserUpdated", "{\"id\": 2, \"name\": \"New Name\"}")]
    [InlineData("UserDeleted", "{\"id\": 3}")]
    [InlineData("GameAdded", "{\"userId\": 1, \"gameId\": 10}")]
    [InlineData("GameRemoved", "{\"userId\": 1, \"gameId\": 10}")]
    public void Constructor_ShouldAcceptDifferentTypesAndContent(string type, string content)
    {
        // Arrange
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, content, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
        Assert.Equal(occuredOn, outboxMessage.OccuredOn);
    }

    [Fact]
    public void OutboxMessage_ShouldAllowEmptyContent()
    {
        // Arrange
        var type = "EmptyEvent";
        var content = string.Empty;
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, content, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(string.Empty, outboxMessage.Content);
    }

    [Fact]
    public void OutboxMessage_ShouldAllowNullType()
    {
        // Arrange
        var content = "{\"data\": \"test\"}";
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(null, content, occuredOn);

        // Assert
        Assert.Null(outboxMessage.Type);
        Assert.Equal(content, outboxMessage.Content);
    }

    [Fact]
    public void OutboxMessage_ShouldAllowNullContent()
    {
        // Arrange
        var type = "NullContentEvent";
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, null, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Null(outboxMessage.Content);
    }

    [Fact]
    public void OutboxMessage_ShouldInheritFromEntity()
    {
        // Arrange & Act
        var outboxMessage = new OutboxMessage();

        // Assert
        Assert.IsAssignableFrom<Abp.Domain.Entities.Entity<Guid>>(outboxMessage);
        Assert.True(outboxMessage.GetType().BaseType?.Name == "Entity`1");
    }

    [Fact]
    public void OutboxMessage_ShouldHaveGuidId()
    {
        // Arrange
        var outboxMessage = new OutboxMessage("Test", "Content", DateTime.UtcNow);
        var newId = Guid.NewGuid();

        // Act
        outboxMessage.Id = newId;

        // Assert
        Assert.Equal(newId, outboxMessage.Id);
        Assert.IsType<Guid>(outboxMessage.Id);
    }

    [Fact]
    public void ProcessedOn_ShouldBeNullableDateTime()
    {
        // Arrange
        var outboxMessage = new OutboxMessage("Test", "Content", DateTime.UtcNow);

        // Act & Assert
        Assert.Null(outboxMessage.ProcessedOn);

        // Act - Set value
        var processedTime = DateTime.UtcNow;
        outboxMessage.ProcessedOn = processedTime;

        // Assert
        Assert.Equal(processedTime, outboxMessage.ProcessedOn);

        // Act - Set back to null
        outboxMessage.ProcessedOn = null;

        // Assert
        Assert.Null(outboxMessage.ProcessedOn);
    }

    [Fact]
    public void OutboxMessage_ShouldSupportLargeContent()
    {
        // Arrange
        var type = "LargeEvent";
        var largeContent = new string('x', 10000); // 10k characters
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, largeContent, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(largeContent, outboxMessage.Content);
        Assert.Equal(10000, outboxMessage.Content.Length);
    }

    [Fact]
    public void OutboxMessage_ShouldSupportComplexJsonContent()
    {
        // Arrange
        var type = "ComplexEvent";
        var complexContent = @"{
            ""userId"": 123,
            ""user"": {
                ""name"": ""John Doe"",
                ""email"": ""john@example.com"",
                ""games"": [
                    {""id"": 1, ""name"": ""Game 1"", ""price"": 59.99},
                    {""id"": 2, ""name"": ""Game 2"", ""price"": 39.99}
                ]
            },
            ""timestamp"": ""2023-01-01T00:00:00Z""
        }";
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage(type, complexContent, occuredOn);

        // Assert
        Assert.Equal(type, outboxMessage.Type);
        Assert.Equal(complexContent, outboxMessage.Content);
        Assert.Contains("John Doe", outboxMessage.Content);
        Assert.Contains("Game 1", outboxMessage.Content);
    }

    [Fact]
    public void OutboxMessage_CreationTime_ShouldBeAccurate()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var occuredOn = DateTime.UtcNow;

        // Act
        var outboxMessage = new OutboxMessage("TimeTest", "Content", occuredOn);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(outboxMessage.OccuredOn >= beforeCreation);
        Assert.True(outboxMessage.OccuredOn <= afterCreation);
    }
}