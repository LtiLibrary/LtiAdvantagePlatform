using System;
using System.Collections.Generic;

namespace AdvantagePlatform.Utility
{
    public static class StringExtensions
    {
        /// <summary>
        /// True if the value is null, empty, or whitespace.
        /// </summary>
        public static bool IsMissing(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// True if the value is not null, empty, or whitespace.
        /// </summary>
        public static bool IsPresent(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Ensures the URL ends with a single trailing slash.
        /// </summary>
        public static string EnsureTrailingSlash(this string url)
        {
            if (url == null) return null;
            return url.EndsWith("/") ? url : url + "/";
        }

        /// <summary>
        /// Returns "[Not Set]" or replacement if string is missing.
        /// </summary>
        public static string IfMissingThen(this string value, string replacement = "[Not Set]")
        {
            return string.IsNullOrWhiteSpace(value) ? replacement : value;
        }

        public static bool TryConvertToDictionary(this string properties, out Dictionary<string, string> dictionary)
        {
            dictionary = default(Dictionary<string, string>);
            if (string.IsNullOrWhiteSpace(properties))
            {
                return false;
            }

            dictionary = new Dictionary<string, string>();

            var lines = properties.Split(new []{'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var pair = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                {
                    dictionary = null;
                    return false;
                }

                var key = pair[0];
                if (string.IsNullOrWhiteSpace(key))
                {
                    dictionary = null;
                    return false;
                }

                var value = pair[1];
                if (string.IsNullOrWhiteSpace(value))
                {
                    dictionary = null;
                    return false;
                }

                if (!dictionary.TryAdd(key, value))
                {
                    dictionary = null;
                    return false;
                }
            }

            return true;
        }
    }
}
