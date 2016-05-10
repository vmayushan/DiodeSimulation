using System.Collections.Generic;
namespace PICSolver.Extensions
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Adds new value with key or increase if exist
        /// </summary>
        public static void AddOrIncrease(this Dictionary<int, double> dictionary, int key, double value)
        {
            double old;
            if (dictionary.TryGetValue(key, out old))
            {
                dictionary[key] = old + value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
