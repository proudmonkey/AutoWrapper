using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AutoWrapper.Test.Helper
{
    internal static class StringExtension
    {
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
        
    }
}
