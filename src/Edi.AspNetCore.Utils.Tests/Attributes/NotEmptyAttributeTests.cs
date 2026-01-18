using Edi.AspNetCore.Utils.Attributes;
using Xunit;

namespace Edi.AspNetCore.Utils.Tests.Attributes;

public class NotEmptyAttributeTests
{
    [Fact]
    public void IsValid_NullValue_ReturnsTrue()
    {
        // Arrange
        var attribute = new NotEmptyAttribute();

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_EmptyGuid_ReturnsFalse()
    {
        // Arrange
        var attribute = new NotEmptyAttribute();

        // Act
        var result = attribute.IsValid(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NonEmptyGuid_ReturnsTrue()
    {
        // Arrange
        var attribute = new NotEmptyAttribute();
        var guid = Guid.NewGuid();

        // Act
        var result = attribute.IsValid(guid);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_SpecificNonEmptyGuid_ReturnsTrue()
    {
        // Arrange
        var attribute = new NotEmptyAttribute();
        var guid = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = attribute.IsValid(guid);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("string value")]
    [InlineData(123)]
    [InlineData(true)]
    public void IsValid_NonGuidValue_ReturnsTrue(object value)
    {
        // Arrange
        var attribute = new NotEmptyAttribute();

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DefaultErrorMessage_HasCorrectValue()
    {
        // Arrange & Act
        var errorMessage = NotEmptyAttribute.DefaultErrorMessage;

        // Assert
        Assert.Equal("The {0} field must not be empty", errorMessage);
    }

    [Fact]
    public void Attribute_HasCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(NotEmptyAttribute);

        // Act
        var attributeUsage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            attributeType, 
            typeof(AttributeUsageAttribute));

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Property));
        Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Field));
        Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Parameter));
    }
}