using System;
using System.Linq;
using System.Text.Json;

namespace AutoWrapper.Extensions
{
    public static class JsonExtensions
    {
        public static (bool IsEncoded, string ParsedText) VerifyBodyContent(this string text)
        {
            try
            {
                using var obj = JsonDocument.Parse(text);
                return (true, text);
            }
            catch (Exception)
            {
                return (false, text);
            }
        }

        public static (bool IsEncoded, string ParsedText, JsonDocument? JsonDoc) VerifyAndParseBodyContentToJson(this string text)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(text);
                return (true, text, jsonDocument);
            }
            catch (Exception)
            {
                return (false, text, null);
            }
        }
    }
}
