using System.ComponentModel.DataAnnotations;

namespace Edi.AspNetCore.Utils.Attributes;

/// <summary>
/// Validation attribute that ensures a property value is not the default value for its type.
/// For value types, validates that the value is not the default (e.g., 0 for int, false for bool).
/// For reference types, validates that the value is not null.
/// </summary>
/// <remarks>
/// This attribute allows null values to be considered valid. If you need to prevent null values,
/// combine this attribute with the <see cref="RequiredAttribute"/>.
/// </remarks>
public class NotDefaultAttribute : ValidationAttribute
{
    /// <summary>
    /// The default error message displayed when validation fails.
    /// </summary>
    public const string DefaultErrorMessage = "The {0} field must not have the default value";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotDefaultAttribute"/> class.
    /// </summary>
    public NotDefaultAttribute() : base(DefaultErrorMessage) { }

    /// <summary>
    /// Validates that the specified value is not the default value for its type.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value is null, or if it's a value type that is not equal to its default value,
    /// or if it's a non-null reference type; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(object value)
    {
        //NotDefault doesn't necessarily mean required
        if (value is null)
        {
            return true;
        }

        var type = value.GetType();
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return !value.Equals(defaultValue);
        }

        // non-null ref type
        return true;
    }
}