using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoWrapper.Extensions
{
    public static class TypeConverterExtension
    {
        public static DateTime ToDateTime(this string value)
            => DateTime.TryParse(value, out var result)? result : default;

        public static short ToInt16(this string value)
            => short.TryParse(value, out var result) ? result : default;

        public static int ToInt32(this string value)
            => int.TryParse(value, out var result) ? result : default;

        public static long ToInt64(this string value)
            => long.TryParse(value, out var result) ? result : default;

        public static bool ToBoolean(this string value)
            => bool.TryParse(value, out var result) ? result : default;

        public static float ToFloat(this string value)
            => float.TryParse(value, out var result) ? result : default;

        public static decimal ToDecimal(this string value)
            => decimal.TryParse(value, out var result) ? result : default;

        public static double ToDouble(this string value)
           => double.TryParse(value, out var result) ? result : default;

        public static bool IsNumber(this string value)
            => Regex.IsMatch(value, @"^\d+$");

        public static bool IsWholeNumber(this string value)
            => long.TryParse(value, out _);

        public static bool IsDecimalNumber(this string value)
         => decimal.TryParse(value, out _);

        public static bool IsBoolean(this string value)
           => bool.TryParse(value, out var _);

    }
}
