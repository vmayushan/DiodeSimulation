using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Extensions;
using PICSolver.Storage;

namespace PICSolver.Interpolation
{
    public class CloudInCellIterative : IInterpolationScheme
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly IGrid2D grid;
        private readonly IMesh mesh;
        private readonly bool parallel;
        private readonly int threads = 4;
        private readonly ObjectPool<double[]> densityPool;

        public CloudInCellIterative(IParticleStorage<Particle> particles, IGrid2D grid, IMesh mesh, bool parallel)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
            this.parallel = parallel;
            if (parallel)
            {
                densityPool = new ObjectPool<double[]>(() => new double[mesh.Density.Length]);
            }
        }

        public void InterpolateDensity()
        {
            if (parallel)
            {
                var rangeSize = (particles.Count / threads) + 1;
                if (rangeSize == 0) rangeSize = particles.Count;
                var rangePartitioner = Partitioner.Create(0, particles.Count, rangeSize);
                var list = particles.EnumerateIndexes().ToArray();
                Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    () => densityPool.GetObject(),
                    (range, loopstate, localStorage) =>
                     {
                         for (var i = range.Item1; i < range.Item2; i++)
                             InterpolateDensityToParticle(list[i], localStorage);
                         return localStorage;
                     },
                (finalStorage) =>
                {
                    lock (mesh.Density)
                    {
                        for (int i = 0; i < mesh.Density.Length; i++)
                        {
                            mesh.Density[i] += finalStorage[i];
                        }
                    }
                    for (int i = 0; i < finalStorage.Length; i++)
                    {
                        finalStorage[i] = 0;
                    }
                    densityPool.PutObject(finalStorage);
                });
            }
            else
            {
                foreach (var particleId in particles.EnumerateIndexes())
                    InterpolateDensityToParticle(particleId, null);
            }
        }
        public void InterpolateForces()
        {
            if (parallel)
            {
                var rangePartitioner = Partitioner.Create(0, particles.Count, (particles.Count / threads) + 1);
                var list = particles.EnumerateIndexes().ToArray();
                Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                        InterpolateForcesFromParticle(list[i]);
                });
            }
            else
            {
                foreach (var particleId in particles.EnumerateIndexes())
                    InterpolateForcesFromParticle(particleId);
            }
        }

        private void InterpolateDensityToParticle(int particleId, double[] localStorage)
        {
            var current = particles.Get(Field.Q, particleId) / grid.CellSquare;

            var x = particles.Get(Field.X, particleId);
            var y = particles.Get(Field.Y, particleId);
            var cell = grid.FindCell(x, y);
            //CurrentToCell(cell, current, x, y, x, y,localStorage); return;

            var x1 = particles.Get(Field.PrevX, particleId);
            var y1 = particles.Get(Field.PrevY, particleId);
            var cell1 = grid.FindCell(x1, y1);

            var length = LineHelpers.Length(x, y, x1, y1);

            var diff = cell - cell1;
            double x2, y2, intervalLength, currentRatio;
            switch (diff)
            {
                case 0:
                    CurrentToCell(cell, current, x, y, x1, y1, localStorage);
                    break;
                case -1: //cellId1 - right
                    x2 = grid.GetCellX(cell1);
                    y2 = LineHelpers.LineY(x1, x, y1, y, x2);
                    intervalLength = LineHelpers.Length(x, y, x2, y2);
                    currentRatio = intervalLength / length;
                    CurrentToCell(cell, currentRatio * current, x, y, x2, y2, localStorage);
                    CurrentToCell(cell1, (1 - currentRatio) * current, x2, y2, x1, y1, localStorage);
                    break;
                case 1: //cellId1 - left
                    x2 = grid.GetCellX(cell);
                    y2 = LineHelpers.LineY(x1, x, y1, y, x2);
                    intervalLength = LineHelpers.Length(x1, y1, x2, y2);
                    currentRatio = intervalLength / length;
                    CurrentToCell(cell1, currentRatio * current, x1, y1, x2, y2, localStorage);
                    CurrentToCell(cell, (1 - currentRatio) * current, x2, y2, x, y, localStorage);
                    break;
                default:
                    if (grid.UpperCell(cell) == cell1)
                    {
                        y2 = grid.GetCellY(cell1);
                        x2 = LineHelpers.LineX(x1, x, y1, y, y2);
                        intervalLength = LineHelpers.Length(x, y, x2, y2);
                        currentRatio = intervalLength / length;
                        CurrentToCell(cell, currentRatio * current, x, y, x2, y2, localStorage);
                        CurrentToCell(cell1, (1 - currentRatio) * current, x2, y2, x1, y1, localStorage);
                    }
                    else if (grid.UpperCell(cell1) == cell)
                    {
                        y2 = grid.GetCellY(cell);
                        x2 = LineHelpers.LineX(x1, x, y1, y, y2);
                        intervalLength = LineHelpers.Length(x1, y1, x2, y2);
                        currentRatio = intervalLength / length;
                        CurrentToCell(cell1, currentRatio * current, x1, y1, x2, y2, localStorage);
                        CurrentToCell(cell, (1 - currentRatio) * current, x2, y2, x, y, localStorage);
                    }
                    else
                    {
                        //CurrentToCell(cell, current, x, y, x, y, localStorage); return;
                        var intersect = new SortedList<double, double>(4) { { x, y } };
                        for (int i = 0; i < grid.N; i++)
                        {
                            if (grid.X[i] > Math.Min(x, x1) && grid.X[i] < Math.Max(x, x1))
                            {
                                intersect.Add(grid.X[i], LineHelpers.LineY(x1, x, y1, y, grid.X[i]));
                            }
                        }
                        for (int i = 0; i < grid.M; i++)
                        {
                            if (grid.Y[i] > Math.Min(y, y1) && grid.Y[i] < Math.Max(y, y1))
                            {
                                intersect.Add(LineHelpers.LineX(x1, x, y1, y, grid.Y[i]), grid.Y[i]);
                            }
                        }
                        intersect.Add(x1, y1);
                        var enumerator = intersect.GetEnumerator();
                        enumerator.MoveNext();
                        var left = enumerator.Current;
                        while (enumerator.MoveNext())
                        {
                            var right = enumerator.Current;
                            intervalLength = LineHelpers.Length(left.Key, left.Value, right.Key, right.Value);
                            currentRatio = intervalLength / length;
                            CurrentToCell(grid.FindCell(left.Key, left.Value), currentRatio * current, left.Key, left.Value, right.Key, right.Value, localStorage);
                            left = right;
                        }
                    }
                    break;

            }
        }
        private void CurrentToCell(int cellId, double current, double x1, double y1, double x2, double y2, double[] localStorage)
        {
            var topCellId = grid.UpperCell(cellId);
            var halfCurrent = 0.5 * current;
            var cellPosition = grid.GetCell(cellId);

            var p1DeltaX = (x1 - cellPosition.Key) / grid.Hx;
            var p1DeltaY = (y1 - cellPosition.Value) / grid.Hy;
            var p1DeltaXOpposite = (1 - p1DeltaX);
            var p1DeltaYOpposite = (1 - p1DeltaY);

            var p2DeltaX = (x2 - cellPosition.Key) / grid.Hx;
            var p2DeltaY = (y2 - cellPosition.Value) / grid.Hy;
            var p2DeltaXOpposite = (1 - p2DeltaX);
            var p2DeltaYOpposite = (1 - p2DeltaY);

            if (parallel)
            {
                localStorage[cellId] += halfCurrent * (p1DeltaXOpposite * p1DeltaYOpposite + p2DeltaXOpposite * p2DeltaYOpposite);
                localStorage[cellId + 1] += halfCurrent * (p1DeltaX * p1DeltaYOpposite + p2DeltaX * p2DeltaYOpposite);
                localStorage[topCellId] += halfCurrent * (p1DeltaXOpposite * p1DeltaY + p2DeltaXOpposite * p2DeltaY);
                localStorage[topCellId + 1] += halfCurrent * (p1DeltaX * p1DeltaY + p2DeltaX * p2DeltaY);
            }
            else
            {
                mesh.AddDensity(cellId, halfCurrent * (p1DeltaXOpposite * p1DeltaYOpposite + p2DeltaXOpposite * p2DeltaYOpposite));
                mesh.AddDensity(cellId + 1, halfCurrent * (p1DeltaX * p1DeltaYOpposite + p2DeltaX * p2DeltaYOpposite));
                mesh.AddDensity(topCellId, halfCurrent * (p1DeltaXOpposite * p1DeltaY + p2DeltaXOpposite * p2DeltaY));
                mesh.AddDensity(topCellId + 1, halfCurrent * (p1DeltaX * p1DeltaY + p2DeltaX * p2DeltaY));
            }
        }



        private void InterpolateForcesFromParticle(int particleId)
        {
            var cellId = particles.GetParticleCell(particleId);
            var topCellId = grid.UpperCell(cellId);

            var x = particles.Get(Field.X, particleId);
            var y = particles.Get(Field.Y, particleId);

            var cellPosition = grid.GetCell(cellId);
            var dx = (x - cellPosition.Key) / grid.Hx;
            var dy = (y - cellPosition.Value) / grid.Hy;
            var dxOpp = (1 - dx);
            var dyOpp = (1 - dy);

            particles.Increment(Field.Ex, particleId, mesh.GetEx(cellId) * (dxOpp * dyOpp) +
                                                    mesh.GetEx(cellId + 1) * (dx * dyOpp) +
                                                    mesh.GetEx(topCellId) * (dxOpp * dy) +
                                                    mesh.GetEx(topCellId + 1) * (dx * dy));
            particles.Increment(Field.Ey, particleId, mesh.GetEy(cellId) * (dxOpp * dyOpp) +
                                                    mesh.GetEy(cellId + 1) * (dx * dyOpp) +
                                                    mesh.GetEy(topCellId) * (dxOpp * dy) +
                                                    mesh.GetEy(topCellId + 1) * (dx * dy));
        }

        #region old
        //private void CurrentToCellOld(int cellId, double current, double x1, double y1, double x2, double y2, double[] localStorage)
        //{
        //    var topCellId = grid.UpperCell(cellId);
        //    var halfCurrent = 0.5 * current;

        //    if (parallel)
        //    {
        //        localStorage[cellId] += halfCurrent * (InterpolateWeight(x1, y1, topCellId + 1) + InterpolateWeight(x2, y2, topCellId + 1));
        //        localStorage[cellId + 1] += halfCurrent * (InterpolateWeight(x1, y1, topCellId) + InterpolateWeight(x2, y2, topCellId));
        //        localStorage[topCellId] += halfCurrent * (InterpolateWeight(x1, y1, cellId + 1) + InterpolateWeight(x2, y2, cellId + 1));
        //        localStorage[topCellId + 1] += halfCurrent * (InterpolateWeight(x1, y1, cellId) + InterpolateWeight(x2, y2, cellId));
        //    }
        //    else
        //    {
        //        mesh.AddDensity(cellId, halfCurrent * (InterpolateWeight(x1, y1, topCellId + 1) + InterpolateWeight(x2, y2, topCellId + 1)));
        //        mesh.AddDensity(cellId + 1, halfCurrent * (InterpolateWeight(x1, y1, topCellId) + InterpolateWeight(x2, y2, topCellId)));
        //        mesh.AddDensity(topCellId, halfCurrent * (InterpolateWeight(x1, y1, cellId + 1) + InterpolateWeight(x2, y2, cellId + 1)));
        //        mesh.AddDensity(topCellId + 1, halfCurrent * (InterpolateWeight(x1, y1, cellId) + InterpolateWeight(x2, y2, cellId)));
        //    }
        //}
        //private double InterpolateWeight(double x, double y, int cellId)
        //{
        //    var cell = grid.GetCell(cellId);
        //    return Math.Abs((cell.Key - x) * (cell.Value - y)) / grid.Hxy;
        //}
    #endregion

    }
}
