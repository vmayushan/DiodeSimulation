using System.Runtime.InteropServices;
using Emgu.CV;

namespace PICSolver.Extensions
{
    public static class EmguCVExtension
    {
        /// <summary>
        /// Get double value from OpenCV Matrix
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static double GetDoubleValue(this Mat mat, int row, int col)
        {
            var value = new double[1];
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        /// <summary>
        /// Set double value to OpenCV Matrix
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public static void SetDoubleValue(this Mat mat, int row, int col, double value)
        {
            var target = new[] { value };
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
    }
}
