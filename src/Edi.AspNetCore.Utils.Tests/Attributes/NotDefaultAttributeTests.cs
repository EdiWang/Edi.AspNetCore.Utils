using Edi.AspNetCore.Utils.Attributes;

namespace Edi.AspNetCore.Utils.Tests.Attributes;

public class NotDefaultAttributeTests
{
    private readonly NotDefaultAttribute _attribute = new();

    #region Null Values

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsTrue()
    {
        // Act
        var result = _attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Integer Value Types

    [Fact]
    public void IsValid_WhenIntIsDefault_ReturnsFalse()
    {
        // Arrange
        int value = 0;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenIntIsNotDefault_ReturnsTrue()
    {
        // Arrange
        int value = 42;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WhenIntIsNegative_ReturnsTrue()
    {
        // Arrange
        int value = -1;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Guid Value Types

    [Fact]
    public void IsValid_WhenGuidIsEmpty_ReturnsFalse()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenGuidIsNotEmpty_ReturnsTrue()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region DateTime Value Types

    [Fact]
    public void IsValid_WhenDateTimeIsDefault_ReturnsFalse()
    {
        // Arrange
        var value = default(DateTime);

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenDateTimeIsNotDefault_ReturnsTrue()
    {
        // Arrange
        var value = DateTime.Now;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Boolean Value Types

    [Fact]
    public void IsValid_WhenBoolIsDefault_ReturnsFalse()
    {
        // Arrange
        bool value = false;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenBoolIsTrue_ReturnsTrue()
    {
        // Arrange
        bool value = true;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Decimal Value Types

    [Fact]
    public void IsValid_WhenDecimalIsDefault_ReturnsFalse()
    {
        // Arrange
        decimal value = 0m;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenDecimalIsNotDefault_ReturnsTrue()
    {
        // Arrange
        decimal value = 123.45m;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Double and Float Value Types

    [Fact]
    public void IsValid_WhenDoubleIsDefault_ReturnsFalse()
    {
        // Arrange
        double value = 0.0;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenDoubleIsNotDefault_ReturnsTrue()
    {
        // Arrange
        double value = 3.14;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Reference Types

    [Fact]
    public void IsValid_WhenStringIsNotNull_ReturnsTrue()
    {
        // Arrange
        string value = "test";

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WhenStringIsEmpty_ReturnsTrue()
    {
        // Arrange
        string value = string.Empty;

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WhenObjectIsNotNull_ReturnsTrue()
    {
        // Arrange
        var value = new object();

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Custom Struct

    [Fact]
    public void IsValid_WhenCustomStructIsDefault_ReturnsFalse()
    {
        // Arrange
        var value = default(CustomStruct);

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenCustomStructIsNotDefault_ReturnsTrue()
    {
        // Arrange
        var value = new CustomStruct { Value = 42 };

        // Act
        var result = _attribute.IsValid(value);

        // Assert
        Assert.True(result);
    }

    private struct CustomStruct
    {
        public int Value { get; set; }
    }

    #endregion

    #region Error Message

    [Fact]
    public void DefaultErrorMessage_HasCorrectFormat()
    {
        // Assert
        Assert.Equal("The {0} field must not have the default value", NotDefaultAttribute.DefaultErrorMessage);
    }

    #endregion
}