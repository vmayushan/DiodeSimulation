using System;
using System.Linq;

namespace PICSolver.Filters
{
    public static class Median
    {
        public static T[,] MedianFilter<T, TKey>(this T[,] source, int filterHeight, int filterWidth, Func<T, TKey> selectorFunc)
        {
            int i;
            int tempLength = filterHeight * filterWidth;
            T[] temp = new T[tempLength];

            T[,] result = source.Clone() as T[,];

            int filterOffsetWidth = (filterWidth - 1) / 2;
            int filterOffsetHeight = (filterHeight - 1) / 2;

            for (int offsetY = filterOffsetHeight; offsetY < source.GetLength(0) - filterOffsetHeight; offsetY++)
            {
                for (int offsetX = filterOffsetWidth; offsetX < source.GetLength(1) - filterOffsetWidth; offsetX++)
                {
                    i = 0;
                    for (int filterY = -filterOffsetHeight; filterY <= filterOffsetHeight; filterY++)
                    {
                        for (int filterX = -filterOffsetWidth; filterX <= filterOffsetWidth; filterX++)
                        {
                            temp[i] = source[filterY + offsetY, filterX + offsetX];
                            i++;
                        }
                    }
                    var temp2 = temp.OrderBy(selectorFunc).ToArray();
                    result[offsetY, offsetX] = temp2[tempLength / 2];
                }
            }
            return result;
        }
    }
}
