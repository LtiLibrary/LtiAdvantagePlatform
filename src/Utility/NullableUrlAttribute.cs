using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Utility
{
    /// <inheritdoc />
    /// <summary>
    /// Validates a URL. Returns True if value is null.
    /// </summary>
    public class NullableUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value == null || Uri.TryCreate(Convert.ToString(value), UriKind.Absolute, out _);
        }
    }
}
