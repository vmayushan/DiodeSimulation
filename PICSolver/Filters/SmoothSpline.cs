using AForge.Imaging.Filters;
using CenterSpace.NMath.Analysis;
using CenterSpace.NMath.Core;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra;
using PICSolver.Extensions;

namespace PICSolver.Filters
{
    internal class SmoothSpline
    {
        public DoubleVector X { get; set; }
        public DoubleVector Y { get; set; }
        public int N { get; set; }
        public int M { get; set; }

        public SmoothSpline(int n, int m)
        {
            this.N = n;
            this.M = m;
            X = new DoubleVector(M);
            Y = new DoubleVector(M);
        }

        public void Smooth2(double[] source, double lambda)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    X[j] = j;
                    Y[j] = source[N * j + i];
                }
                var spline = new SmoothCubicSpline(X, Y, lambda);
                for (int j = 0; j < M; j++)
                {
                    source[N * j + i] = spline.Func(j);
                    if (source[N * j + i] > 0) source[N * j + i] = 0;
                }
            }
        }

        public void Smooth(double[] source, double lambda)
        {
            double[,] rectangle = new double[N, M];
            ArrayExtension.RectangleArray(source, N, N, rectangle);
            
            SmoothCubicSpline[] splines = new SmoothCubicSpline[N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    X[j] = j;
                    Y[j] = rectangle[i, j];
                }
                splines[i] = new SmoothCubicSpline(X, Y, lambda);
            }

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    X[j] = j;
                    Y[j] = splines[i].Func(j);
                }
                var spline = new SmoothCubicSpline(X, Y, lambda);

                for (int j = 0; j < M; j++)
                {
                    rectangle[i, j] = spline.Func(j);
                    
                }
            }
            ArrayExtension.FlattenArray(rectangle, N, M, source);
            
        }
    }
}
