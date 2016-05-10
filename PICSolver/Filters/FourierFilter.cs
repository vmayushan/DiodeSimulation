using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AForge.Math;
using FFTWSharp;
using PICSolver.Extensions;

namespace PICSolver.Filters
{
    public static class FourierFilter
    {
        /// <summary>
        /// Shift The FFT of the Image
        /// </summary>
        /// <param name="input">input complex array</param>
        /// <param name="output">output complex array</param>
        public static void FFTShift(Complex[,] input, Complex[,] output)
        {
            int i, j;
            var nx = input.GetLength(0);
            var ny = input.GetLength(1);

            for (i = 0; i <= (nx / 2) - 1; i++)
            {
                for (j = 0; j <= (ny / 2) - 1; j++)
                {
                    output[i + (nx / 2), j + (ny / 2)] = input[i, j];
                    output[i, j] = input[i + (nx / 2), j + (ny / 2)];
                    output[i + (nx / 2), j] = input[i, j + (ny / 2)];
                    output[i, j + (ny / 2)] = input[i + (nx / 2), j];
                }
            }
        }

        /// <summary>
        /// Removes FFT Shift for FFTshifted array
        /// </summary>
        /// <param name="input">shifted array</param>
        /// <param name="output">output array</param>
        public static void RemoveFFTShift(Complex[,] input, Complex[,] output)
        {
            int i, j;
            var nx = input.GetLength(0);
            var ny = input.GetLength(1);

            for (i = 0; i <= (nx / 2) - 1; i++)
            {
                for (j = 0; j <= (ny / 2) - 1; j++)
                {
                    output[i + (nx / 2), j + (ny / 2)] = input[i, j];
                    output[i, j] = input[i + (nx / 2), j + (ny / 2)];
                    output[i + (nx / 2), j] = input[i, j + (ny / 2)];
                    output[i, j + (ny / 2)] = input[i + (nx / 2), j];
                }
            }
        }

        /// <summary>
        /// Apply rectangle window on fft image
        /// </summary>
        /// <param name="A">image array</param>
        /// <param name="B">rectange window</param>
        public static void ApplyMatrixFilter(Complex[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            int m = A.GetLength(1);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    A[i, j] = A[i, j] * B[i, j];
                }
            }
        }

        /// <summary>
        /// Shift The FFT of the Image
        /// </summary>
        /// <param name="input">input complex array</param>
        /// <param name="output">output complex array</param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        public static void FFTShift(double[] input, double[] output, int n, int m)
        {
            int i, j;

            for (i = 0; i <= (n / 2) - 1; i++)
            {
                for (j = 0; j <= (m / 2) - 1; j++)
                {
                    output[Index.Re(i + (n / 2), j + (m / 2), n, m)] = input[Index.Re(i, j, n, m)];
                    output[Index.Im(i + (n / 2), j + (m / 2), n, m)] = input[Index.Im(i, j, n, m)];

                    output[Index.Re(i, j, n, m)] = input[Index.Re(i + (n / 2), j + (m / 2), n, m)];
                    output[Index.Im(i, j, n, m)] = input[Index.Im(i + (n / 2), j + (m / 2), n, m)];

                    output[Index.Re(i + (n / 2), j, n, m)] = input[Index.Re(i, j + (m / 2), n, m)];
                    output[Index.Im(i + (n / 2), j, n, m)] = input[Index.Im(i, j + (m / 2), n, m)];

                    output[Index.Re(i, j + (m / 2), n, m)] = input[Index.Re(i + (n / 2), j, n, m)];
                    output[Index.Im(i, j + (m / 2), n, m)] = input[Index.Im(i + (n / 2), j, n, m)];
                }
            }
        }

        /// <summary>
        /// Removes FFT Shift for FFTshifted array
        /// </summary>
        /// <param name="input">shifted array</param>
        /// <param name="output">output array</param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        public static void RemoveFFTShift(double[] input, double[] output, int n, int m)
        {
            int i, j;

            for (i = 0; i <= (n / 2) - 1; i++)
            {
                for (j = 0; j <= (m / 2) - 1; j++)
                {
                    output[Index.Re(i + (n / 2), j + (m / 2), n, m)] = input[Index.Re(i, j, n, m)];
                    output[Index.Im(i + (n / 2), j + (m / 2), n, m)] = input[Index.Im(i, j, n, m)];

                    output[Index.Re(i, j, n, m)] = input[Index.Re(i + (n / 2), j + (m / 2), n, m)];
                    output[Index.Im(i, j, n, m)] = input[Index.Im(i + (n / 2), j + (m / 2), n, m)];

                    output[Index.Re(i + (n / 2), j, n, m)] = input[Index.Re(i, j + (m / 2), n, m)];
                    output[Index.Im(i + (n / 2), j, n, m)] = input[Index.Im(i, j + (m / 2), n, m)];

                    output[Index.Re(i, j + (m / 2), n, m)] = input[Index.Re(i + (n / 2), j, n, m)];
                    output[Index.Im(i, j + (m / 2), n, m)] = input[Index.Im(i + (n / 2), j, n, m)];
                }
            }
        }

        public static void ApplyMatrixFilter(double[] source, double[] filter)
        {

            for (int i = 0; i < filter.Length; i++)
            {
                source[2 * i] *= filter[i];
                source[2 * i + 1] *= filter[i];
            }
        }

        public static void RectangleFFTDoubleArray(double[] source, int n, int m, double[,] result)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = source[2 * (n * j + i)];
                }
            }
        }
    }

    public class FFTWFilter
    {
        private GCHandle hdin, hdout;
        private readonly double[] din;
        private readonly double[] dout;
        private IntPtr fplan;
        public double[] FFTFilter { get; set; }

        public FFTWFilter(int length)
        {
            din = new double[length * 2];
            dout = new double[length * 2];
        }

        public void Execute(double[] source, int n, int m, double[,] debug)
        {
            for (int i = 0; i < source.Length; i++)
            {
                din[2 * i] = source[i];
            }
            hdin = GCHandle.Alloc(din, GCHandleType.Pinned);
            hdout = GCHandle.Alloc(dout, GCHandleType.Pinned);
            fplan = FFTWNativeMethods.dft_2d(m, n, hdin.AddrOfPinnedObject(), hdout.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Estimate);
            FFTWNativeMethods.execute(fplan);
            FFTWNativeMethods.destroy_plan(fplan);
            for (int i = 0; i < 2 * source.Length; i++)
            {
                dout[i] = dout[i] / n / m;
            }
            FourierFilter.FFTShift(dout, din, n, m);
            //FourierFilter.RectangleFFTDoubleArray(din, n, m, debug);
            FourierFilter.ApplyMatrixFilter(din, FFTFilter);
            FourierFilter.RemoveFFTShift(din, dout, n, m);
            fplan = FFTWNativeMethods.dft_2d(m, n, hdout.AddrOfPinnedObject(), hdin.AddrOfPinnedObject(), fftw_direction.Backward, fftw_flags.Estimate);
            FFTWNativeMethods.execute(fplan);
            FFTWNativeMethods.destroy_plan(fplan);
            hdin.Free();
            hdout.Free();
            FFTWNativeMethods.cleanup();
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = din[2 * i];
            }
        }
    }
}
