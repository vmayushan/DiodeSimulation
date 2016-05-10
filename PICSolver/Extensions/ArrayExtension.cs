using System;
using System.Numerics;

namespace PICSolver.Extensions
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Rectangle flatten array A[i, j]=B[n * j + i]
        /// </summary>
        /// <param name="source">source flatten array</param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="result"></param>
        public static void RectangleArray(double[] source, int n, int m, double[,] result)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = source[n * j + i];
                }
            }
        }
        /// <summary>
        /// Rectangle flatten array A[i, j]=B[n * j + i]
        /// </summary>
        /// <param name="source">source flatten array</param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        public static double[,] RectangleArray(double[] source, int n, int m)
        {
            double[,] result = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = source[n * j + i];
                }
            }
            return result;
        }

        /// <summary>
        /// Flatten rectangle array A[n * j + i]=B[i, j]
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] FlattenArray(double[,] source, int n, int m)
        {
            var result = new double[n * m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[n * j + i] = source[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// Flatten rectangle array A[n * j + i]=B[i, j]
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static void FlattenArray(double[,] source, int n, int m, double[] result)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[n * j + i] = source[i, j];
                }
            }
        }

        /// <summary>
        /// Complex rectangle array from flatten double
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="result"></param>
        public static void RectangleComplexArray(double[] source, int n, int m, Complex[,] result)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = new Complex(source[n * j + i], 0);
                }
            }
        }

        /// <summary>
        /// Double flattem array from complex (Re or Im)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="result"></param>
        /// <param name="selectorFunc"></param>
        public static void FlattenComplexArray(Complex[,] source, int n, int m, double[] result, Func<Complex, double> selectorFunc)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[n * j + i] = selectorFunc(source[i, j]);
                }
            }
        }

        /// <summary>
        /// Gets vertical line from flatten array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="line">line number from 1 to n</param>
        /// <returns></returns>
        public static double[] GetLineArrayX(double[] source, int n, int m, int line)
        {
            var result = new double[m];

            for (int j = 0; j < m; j++)
            {
                result[j] = source[n * j + line];
            }
            return result;
        }

        /// <summary>
        /// Gets horizontal line from flatten array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="line">line number from 1 to m</param>
        /// <returns></returns>
        public static double[] GetLineArrayY(double[] source, int n, int m, int line)
        {
            var result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = source[n * line + i];
            }
            return result;
        }

        /// <summary>
        /// Gets vertical line from rectagle array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="line">line number from 1 to n</param>
        /// <returns></returns>
        public static double[] GetLineX(double[,] source, int n, int m, int line)
        {
            var result = new double[m];

            for (int j = 0; j < m; j++)
            {
                result[j] = source[line, j];
            }
            return result;
        }

        /// <summary>
        /// Gets horizontal line from rectangle array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="line">line number from 1 to m</param>
        /// <returns></returns>
        public static double[] GetLineY(double[,] source, int n, int m, int line)
        {
            var result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = source[i, line];
            }
            return result;
        }

        /// <summary>
        /// Multiplies all array values on scalar
        /// </summary>
        /// <param name="source">input array</param>
        /// <param name="scalar"> </param>
        /// <returns></returns>
        public static double[] MultiplyVectorOnScalar(double[] source, double scalar)
        {
            for (var i = 0; i < source.Length; i++) source[i] *= scalar;
            return source;
        }

        public static double[] Sum(double[] lhs, double[] rhs)
        {
            for (var i = 0; i < lhs.Length; i++) lhs[i] = lhs[i] + rhs[i];
            return lhs;
        }

        /// <summary>
        /// Calculates ratio error difference between two arrays
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static double Epsilon(double[] lhs, double[] rhs)
        {
            var max = 0.0;
            for (var i = 0; i < lhs.Length; i++)
            {
                if (Math.Abs(lhs[i]) < double.Epsilon) continue;
                max = Math.Max(max, Math.Abs((lhs[i] - rhs[i]) / lhs[i]));
            }
            return max;
        }


    }
}
