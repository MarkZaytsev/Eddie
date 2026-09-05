using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Eddie.EventDispatching
{
    internal static class Extensions
    {
        public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source,
            string delimiter)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (delimiter == null)
                throw new ArgumentNullException(nameof(delimiter));

            return ToDelimitedStringImpl(source, delimiter, (sb, e) => sb.Append(e));
        }

        private static string ToDelimitedStringImpl<T>(IEnumerable<T> source, string delimiter,
            Func<StringBuilder, T, StringBuilder> append)
        {
            Debug.Assert(source != null);
            Debug.Assert(delimiter != null);
            Debug.Assert(append != null);

            var sb = new StringBuilder();
            var i = 0;

            foreach (var value in source)
            {
                if (i++ > 0)
                    sb.Append(delimiter);
                append(sb, value);
            }

            return sb.ToString();
        }
    }
}