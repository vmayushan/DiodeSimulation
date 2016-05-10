using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PICSolver.Abstract;

namespace PICSolver.Grid
{
    public class Grid2D : IGrid2D
    {
        private double[] cell;
        private int[] up;

        public int Count { get; private set; }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Bottom { get; private set; }
        public double Top { get; private set; }

        public double CellSquare => Hx * Hy;

        public double Hx { get; private set; }
        public double Hy { get; private set; }
        public double Hxy { get; private set; }

        public int N { get; private set; }
        public int M { get; private set; }

        public double[] X { get; private set; }

        public double[] Y { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UpperCell(int cellId)
        {
            return up[cellId];
        }

        public int FindCell(double x, double y)
        {
            var findCell = (int)(y / Hy) * N + (int)(x / Hx);
            Debug.Assert(findCell <= Count || findCell >= 0);
            return findCell;
        }

        public bool IsOutOfGrid(double x, double y)
        {
            return x > Right || x < Left || y > Top || y < Bottom;
        }

        public double GetCellX(int cellId)
        {
            return cell[2 * cellId];
        }

        public double GetCellY(int cellId)
        {
            return cell[2 * cellId + 1];
        }


        public KeyValuePair<double, double> GetCell(int cellId)
        {
            var i = 2 * cellId;
            return new KeyValuePair<double, double>(cell[i], cell[i + 1]);
        }


        public void InitializeGrid(int n, int m, double left, double right, double bottom, double top)
        {
            double hx, hy;
            N = n;
            M = m;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            Count = N * M;

            X = LinearSpaced(N, left, right, out hx);
            Y = LinearSpaced(M, bottom, top, out hy);

            cell = new double[2 * Count];
            up = new int[Count];
            for (var j = 0; j < M; j++)
            {
                for (var i = 0; i < N; i++)
                {
                    var index = 2 * (j * N + i);
                    cell[index] = X[i];
                    cell[index + 1] = Y[j];
                }
            }

            for (var i = 0; i < Count; i++)
            {
                up[i] = (i + N < Count) ? i + N : -1;
            }

            Hx = hx;
            Hy = hy;
            Hxy = hx * hy;
        }

        private double[] LinearSpaced(int length, double start, double stop, out double step)
        {
            step = double.NaN;
            if (length == 0) return new double[0];
            if (length == 1) return new[] { start };

            step = (stop - start) / (length - 1);

            var data = new double[length];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = start + i * step;
            }
            data[data.Length - 1] = stop;
            return data;
        }
    }
}