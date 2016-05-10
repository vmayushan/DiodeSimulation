using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Extensions;

namespace PICSolver.Monitor
{
    // ReSharper disable once InconsistentNaming
    public class PICMonitor
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Stopwatch watchPoisson = new Stopwatch();
        private readonly Stopwatch watchInterpolation = new Stopwatch();
        private readonly Stopwatch watchIntegrator = new Stopwatch();
        private readonly Stopwatch watchInjection = new Stopwatch();
        private readonly IGrid2D grid;
        private readonly IMesh mesh;
        private readonly IPICSolver solver;

        public PICMonitor(IGrid2D grid, IMesh mesh, IParticleStorage<Particle> particles, IPICSolver solver)
        {
            this.grid = grid;
            this.particles = particles;
            this.mesh = mesh;
            this.solver = solver;
            GridX = grid.X;
            GridY = grid.Y;
            Status = PICStatus.Created;
            Rho = new double[grid.N, grid.M];
            Ex = new double[grid.N, grid.M];
            Ey = new double[grid.N, grid.M];
            Potential = new double[grid.N, grid.M];
            TestData = new double[grid.N * grid.M];
        }

        public double[,] Rho { get; set; }
        public double[,] Ex { get; set; }
        public double[,] Ey { get; set; }
        public double[] GridX { get; private set; }
        public double[] GridY { get; private set; }
        public double[,] Potential { get; set; }
        public double[] LineGraph { get; set; }
        public int Iteration { get; set; }
        public double Epsilon { get; set; }
        public int ParticlesCount => particles.Count;
        public long IterationTime { get; set; }
        public long TimePoisson { get; set; }
        public long TimeInterpolation { get; set; }
        public long TimeIntegrator { get; set; }
        public long TimeInjection { get; set; }
        public long TotalTime { get; set; }
        public PICStatus Status { get; set; }
        public IGrouping<int, Tuple<int, double, double>>[] Trajectories { get; set; }


        public double[] TestData { get; set; }



        internal void BeginIteration()
        {
            watch.Restart();
        }

        internal void EndIteration()
        {
            watch.Stop();
            Iteration++;
            IterationTime = watch.ElapsedMilliseconds;
            TotalTime += watch.ElapsedMilliseconds;
            ArrayExtension.RectangleArray(mesh.Density, grid.N, grid.M, Rho);
            ArrayExtension.RectangleArray(mesh.Ex, grid.N, grid.M, Ex);
            ArrayExtension.RectangleArray(mesh.Ey, grid.N, grid.M, Ey);
            ArrayExtension.RectangleArray(mesh.Potential, grid.N, grid.M, Potential);
            if (solver.Trajectories != null) Trajectories = solver.Trajectories.ToLookup(x => x.Item1).ToArray();
            TimePoisson = watchPoisson.ElapsedMilliseconds;
            TimeInterpolation = watchInterpolation.ElapsedMilliseconds;
            TimeIntegrator = watchIntegrator.ElapsedMilliseconds;
            TimeInjection = watchInjection.ElapsedMilliseconds;
        }

        public void Reset()
        {
            TotalTime = 0;
        }

        public double[] GetLine(PlotSource source, LinePlotAlignment alignment, int line)
        {
            switch (source)
            {
                case PlotSource.Density:
                    return (alignment == LinePlotAlignment.Horizontal) ? ArrayExtension.GetLineY(Rho, grid.N, grid.M, line) : ArrayExtension.GetLineX(Rho, grid.N, grid.M, line);
                case PlotSource.Potential:
                    return (alignment == LinePlotAlignment.Horizontal) ? ArrayExtension.GetLineY(Potential, grid.N, grid.M, line) : ArrayExtension.GetLineX(Potential, grid.N, grid.M, line);
                case PlotSource.ElectricFieldX:
                    return (alignment == LinePlotAlignment.Horizontal) ? ArrayExtension.GetLineY(Ex, grid.N, grid.M, line) : ArrayExtension.GetLineX(Ex, grid.N, grid.M, line);
                case PlotSource.ElectricFieldY:
                    return (alignment == LinePlotAlignment.Horizontal) ? ArrayExtension.GetLineY(Ey, grid.N, grid.M, line) : ArrayExtension.GetLineX(Ey, grid.N, grid.M, line);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        internal void BeginPoissonSolve()
        {
            watchPoisson.Restart();
        }

        internal void EndPoissonSolve()
        {
            watchPoisson.Stop();
        }

        internal void BeginInterpolation()
        {
            watchInterpolation.Restart();
        }

        internal void ContinueInterpolation()
        {
            watchInterpolation.Start();
        }

        internal void EndInterpolation()
        {
            watchInterpolation.Stop();
        }

        internal void BeginIntegration()
        {
            watchIntegrator.Restart();
        }

        internal void ContinueIntegration()
        {
            watchIntegrator.Start();
        }

        internal void EndIntegration()
        {
            watchIntegrator.Stop();
        }

        internal void BeginInjection()
        {
            watchInjection.Restart();
        }

        internal void EndInjection()
        {
            watchInjection.Stop();

        }
    }
}