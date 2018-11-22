using System;
using System.Collections.Generic;

namespace AdvantagePlatform.Utility
{
    public static class StringExtensions
    {
        public static bool TryConvertToDictionary(this string properties, out Dictionary<string, string> dictionary)
        {
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
