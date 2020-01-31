using System;
using System.Text;

namespace Bijou.Projected
{
    /// <summary>
    /// Provides Base64 encoding/decoding functionality.
    /// Functions are lowerCamelCase as projected to JS functions are lowerCamelCase.
    /// </summary>
    public static class JSBase64Encoding
    {
        public static string atob(string encodedData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
        }

        public static string btoa(string stringToEncode)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringToEncode));
        }
    }
}
