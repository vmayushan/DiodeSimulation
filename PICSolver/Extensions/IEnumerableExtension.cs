using System;
using System.Collections.Generic;

namespace PICSolver.Extensions
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// List like analogue of one-line ForEach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
