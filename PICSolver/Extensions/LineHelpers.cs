using System;
using System.Runtime.CompilerServices;

namespace PICSolver.Extensions
{
    public static class LineHelpers
    {
        /// <summary>
        /// Calculate Euclidian distance between 2 points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Length(double x1, double y1, double x2, double y2)
        {
            //Distance.Euclidean()
            //Control.LinearAlgebraProvider.
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        /// <summary>
        /// Interpolates line from two points and get value at X.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LineY(double x1, double x2, double y1, double y2, double x)
        {
            return ((x - x2) * y1 + (x1 - x) * y2) / (x1 - x2);
        }

        /// <summary>
        /// Interpolates line from two points and get value at Y.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LineX(double x1, double x2, double y1, double y2, double y)
        {
            return ((y1 - y) * x2 + (y - y2) * x1) / (y1 - y2);
        }
    }
}
