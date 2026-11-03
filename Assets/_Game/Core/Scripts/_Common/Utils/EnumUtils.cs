using System;

namespace Game.Utils
{
    public static class EnumUtils
    {
        public static bool IsDefined<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static bool Exists<T>(string value, bool ignoreCase = true) where T : struct, Enum
        {
            return Enum.TryParse(value, ignoreCase, out T result) && Enum.IsDefined(typeof(T), result);
        }

        public static string ToString<T>(T value) where T : struct, Enum
        {
            return value.ToString();
        }

        public static T FromString<T>(string value, bool ignoreCase = true) where T : struct, Enum
        {
            if (!TryParse(value, out T result, ignoreCase))
            {
                throw new ArgumentException($"Value '{value}' is not valid for enum {typeof(T).Name}");
            }

            return result;
        }

        public static bool TryParse<T>(string value, out T result, bool ignoreCase = true) where T : struct, Enum
        {
            if (Enum.TryParse(value, ignoreCase, out result))
            {
                return Enum.IsDefined(typeof(T), result);
            }

            return false;
        }
    }
}