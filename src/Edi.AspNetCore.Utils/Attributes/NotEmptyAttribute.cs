using System.ComponentModel.DataAnnotations;

namespace Edi.AspNetCore.Utils.Attributes;

/// <summary>
/// Validates that a value is not empty. Currently supports <see cref="Guid"/> validation.
/// </summary>
/// <remarks>
/// This attribute does not enforce that a value is required. Null values are considered valid.
/// For <see cref="Guid"/> types, the value must not be <see cref="Guid.Empty"/>.
/// </remarks>
// https://andrewlock.net/creating-an-empty-guid-validation-attribute/
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class NotEmptyAttribute() : ValidationAttribute(DefaultErrorMessage)
{
    /// <summary>
    /// The default error message template used when validation fails.
    /// </summary>
    public const string DefaultErrorMessage = "The {0} field must not be empty";

    /// <summary>
    /// Validates that the specified value is not empty.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value is null, or if the value is a <see cref="Guid"/> 
    /// that is not equal to <see cref="Guid.Empty"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(object value)
    {
        // NotEmpty doesn't necessarily mean required
        if (value is null) return true;

        return value switch
        {
            Guid guid => guid != Guid.Empty,
            _ => true
        };
    }
}
