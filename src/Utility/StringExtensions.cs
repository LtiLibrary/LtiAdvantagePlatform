using System;
using System.Collections.Generic;

namespace AdvantagePlatform.Utility
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns "[Not Set]" or replacement if string is missing.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="replacement">The replacement (defaults to "[Not Set]").</param>
        /// <returns>A string.</returns>
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
