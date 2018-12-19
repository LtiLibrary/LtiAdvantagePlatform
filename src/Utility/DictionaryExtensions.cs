using System.Collections.Generic;

namespace AdvantagePlatform.Utility
{
    public static class DictionaryExtensions
    {
        public static string ToDatabaseString(this Dictionary<string, string> dictionary)
        {
            var keyValues = new List<string>();
            foreach (var keyValue in dictionary)
            {
                keyValues.Add($"{keyValue.Key}={keyValue.Value}");
            }

            return string.Join("\r\n", keyValues);
        }
    }
}
