using System;
using System.Globalization;

namespace Game.Utils
{
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            string.Empty,
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc"
        };

        public static string Format(float value, int decimals = 2)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "0";

            decimals = Math.Max(0, decimals);

            double absValue = Math.Abs(value);

            if (absValue < 1000d)
                return Math.Truncate(value).ToString(CultureInfo.InvariantCulture);

            int suffixIndex = 0;
            double shortenedValue = absValue;

            while (shortenedValue >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                shortenedValue /= 1000d;
                suffixIndex++;
            }

            shortenedValue = Math.Round(shortenedValue, decimals, MidpointRounding.AwayFromZero);

            if (shortenedValue >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                shortenedValue /= 1000d;
                suffixIndex++;
            }

            string sign = value < 0f ? "-" : string.Empty;
            return $"{sign}{shortenedValue.ToString($"F{decimals}", CultureInfo.InvariantCulture)}{Suffixes[suffixIndex]}";
        }
    }
}
