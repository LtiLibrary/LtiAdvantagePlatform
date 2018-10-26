using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Validates a URL. Returns True if value is null.
    /// </summary>
    public class NullableUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return Uri.TryCreate(Convert.ToString(value), UriKind.Absolute, out _);
        }
    }
}
