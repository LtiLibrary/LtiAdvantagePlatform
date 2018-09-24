using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class UrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return Uri.TryCreate(Convert.ToString(value), UriKind.Absolute, out _);
        }
    }
}
