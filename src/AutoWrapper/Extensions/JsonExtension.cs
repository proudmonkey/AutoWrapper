using System;
using System.Text.Json;

namespace AutoWrapper.Extensions
{
    public static class JsonExtensions
    {
        public static (bool IsEncoded, string ParsedText) VerifyBodyContent(this string text)
        {
            try
            {
                var obj = JsonDocument.Parse(text);
                return (true, text);
            }
            catch (Exception)
            {
                return (false, text);
            }
        }
    }
}
