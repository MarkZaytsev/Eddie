using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostLib.Extensions
{
    public static class EnumerableExtensions
    {
        public static void RemoveAny<T>(this List<T> source, Func<T, bool> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var element in source.Where(action))
            {
                source.Remove(element);
                return;
            }
        }
    }
}