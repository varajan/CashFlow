using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CashFlowBot.Extensions
{
    public static class StringExtensions
    {
        public static string SubStringTo(this string line, string to) => line.Contains(to) ? line.Split(to).First() : line;

        public static string SubString(this string line, string from, string to)
        {
            var start = line.IndexOf(from, StringComparison.OrdinalIgnoreCase) + from.Length;
            var count = line.IndexOf(to, Math.Min(start, line.Length), StringComparison.OrdinalIgnoreCase) - start;

            return line.IndexOf(from, StringComparison.OrdinalIgnoreCase) >= 0 ? line.Substring(start, count) : string.Empty;
        }

        public static string SubString(this string line, string from)
        {
            var start = line.ContainsIgnoreCase(from)
                ? line.IndexOf(from, StringComparison.OrdinalIgnoreCase) + from.Length
                : line.Length;

            return line.Substring(start);
        }

        public static bool ContainsIgnoreCase(this string line, string word) => line.IndexOf(word ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

        public static bool IsValidEmail(this string email) => new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").IsMatch(email);

        public static T ParseEnum<T>(this string value) => (T) Enum.Parse(typeof(T), value, true);

        public static long ToLong(this string number)
        {
            try
            {
                return long.Parse(number);
            }
            catch
            {
                return 0;
            }
        }

        public static int ToInt(this string number)
        {
            try
            {
                return int.Parse(number);
            }
            catch
            {
                return 0;
            }
        }

        public static decimal ToDecimal(this string number)
        {
            try
            {
                return decimal.Parse(number, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }
    }
}
