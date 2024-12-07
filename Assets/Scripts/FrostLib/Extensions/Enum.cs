using System;
using System.Linq;

namespace FrostLib.Extensions
{
    public class Enum<T> where T : Enum
    {
        public static T[] GetEnumValues()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return typeof(T).GetEnumValues().Cast<T>().ToArray();
        }

        public static int Count
        {
            get
            {
                if (!typeof(T).IsEnum)
                    throw new ArgumentException("T must be an enumerated type");

                return typeof(T).GetEnumNames().Length;
            }
        }
    }
}