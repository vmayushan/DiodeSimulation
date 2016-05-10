using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.ElectricField;
using PICSolver.Emitter;
using PICSolver.Extensions;
using PICSolver.Filters;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.Mesh;
using PICSolver.Monitor;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Project;
using PICSolver.Storage;
using Constants = PICSolver.Domain.Constants;

namespace PICSolver
{
    public class IterativeSolver2D : IPICSolver
    {
        private IEmitter emitter;
        private IGrid2D grid;
        private IMesh mesh;
        private IInterpolationScheme interpolator;
        private IMover mover;
        private IParticleStorage<Particle> particles;
        private IFieldSolver poissonSolver;
        private double alfa;
        private bool backscattering;
        private double beta;
        private double[] density;
        private double h;
        private int[] injectedParticlesId;
        private double step;
        private int trajectoryInterval;
        private BoundaryConditions boundaryConditions;
        private HashSet<int> tracksId;
        private Normal normalAlfa;
        private Normal normalBeta;
        private PICProject project;
        private DensitySmoothing filtration;
        public List<Tuple<int, double, double>> Trajectories { get; set; }
        public PICMonitor Monitor { get; set; }

        public IMesh GetMesh()
        {
            return mesh;
        }

        public void Prepare(PICProject proj)
        {
            project = proj;
            backscattering = project.Backscattering.IsEnabled;
            alfa = project.Backscattering.Alfa;
            beta = project.Backscattering.Beta;
            step = project.Properties.Step;

            particles = new ParticleArrayStorageModified<Particle>(project.Emitter.ParticlesCount);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => project.Diode.Voltage, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(project.Emitter.Left, project.Emitter.Bottom, project.Emitter.Right,
                project.Emitter.Top, project.Emitter.ParticlesCount, 0, 0, project.Emitter.CurrentDensity, step,
                project.Emitter.EmissionType);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(project.Properties.GridN, project.Properties.GridM, 0, project.Diode.Length, 0,
                project.Diode.Height);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCellIterative(particles, grid, mesh, project.Properties.Parallel);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.FdmMatrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles, this);
            density = new double[grid.N * grid.M];
            injectedParticlesId = new int[emitter.ParticlesCount];
            tracksId = new HashSet<int>();
            var trackInterval = project.Emitter.ParticlesCount / project.Properties.TracksCount;
            for (int i = 0; i < project.Properties.TracksCount; i++)
            {
                tracksId.Add(i * trackInterval);
            }

            Trajectories = new List<Tuple<int, double, double>>(project.Properties.TracksCount * 1000);
           
            if (project.Backscattering.Random)
            {
                normalAlfa = new Normal(project.Backscattering.Alfa, (project.Backscattering.Alfa / 3));
                normalBeta = new Normal(project.Backscattering.Beta, (project.Backscattering.Beta / 3));
            }
            filtration = new DensitySmoothing(project,grid,mesh,Monitor);
            filtration.Prepare();
        }

        public void Step()
        {
            Monitor.BeginIteration();

            #region Injection

            Monitor.BeginInjection();
            var injectedParticles = emitter.Inject();

            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                var cell = grid.FindCell(injectedParticles[i].X, injectedParticles[i].Y);
                var id = particles.Add(injectedParticles[i]);
                particles.SetParticleCell(id, cell);
                injectedParticlesId[i] = id;
            }
            Monitor.EndInjection();

            #endregion
            
            filtration.Apply();

            #region Poisson

            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.FdmMatrix, vector);
            Monitor.EndPoissonSolve();

            #endregion

            Gradient.Calculate(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);
            mesh.ResetDensity();

            #region Interpolation

            Monitor.BeginInterpolation();
            particles.ResetForces();
            interpolator.InterpolateForces();
            Monitor.EndInterpolation();

            #endregion

            #region Integration

            Monitor.BeginIntegration();
            for (var i = 0; i < emitter.ParticlesCount; i++)
                mover.Prepare(particles, injectedParticlesId[i], h);
            Monitor.EndIntegration();
            Trajectories.Clear();

            while (particles.Count > 0) MoveParticles();

            #endregion
            Monitor.Epsilon = ArrayExtension.Epsilon(mesh.Density, density);
            //Monitor.Epsilon = Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, grid.N, grid.M, Residue(mesh.Density, density));

            if(Monitor.Iteration != 0)
            mesh.Density = ArrayExtension.Sum(ArrayExtension.MultiplyVectorOnScalar(density, 1.0 - project.Properties.Relaxation),
                ArrayExtension.MultiplyVectorOnScalar(mesh.Density, project.Properties.Relaxation));

            density = mesh.Density.Clone() as double[];
            particles.Reset();
            Monitor.EndIteration();

        }
        public static double[] Residue(double[] lhs, double[] rhs)
        {
            var result = new double[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                result[i] = rhs[i] - lhs[i];
            }
            return result;
        }
        private void MoveParticles()
        {
            Monitor.ContinueInterpolation();
            particles.ResetForces();
            interpolator.InterpolateDensity();
            interpolator.InterpolateForces();
            Monitor.EndInterpolation();

            //only 20nth point on the trajectory plot
            //if (trajectoryInterval == 20) trajectoryInterval = 0; //todo вынести в проект
            //else trajectoryInterval++;

            Monitor.ContinueIntegration();
            foreach (var index in particles.EnumerateIndexes())
            {
                mover.Step(particles, index, h);

                //if (trajectoryInterval == 0) Trajectories.Add(new Tuple<int, double, double>(index, particles.Get(Field.X, index), particles.Get(Field.Y, index)));
                if (!backscattering && grid.IsOutOfGrid(particles.Get(Field.X, index), particles.Get(Field.Y, index)))
                {
                    particles.RemoveAt(index);
                }
                else
                {
                    if (backscattering && grid.IsOutOfGrid(particles.Get(Field.X, index), particles.Get(Field.Y, index)))
                    {
                        if (project.Backscattering.Random)
                        {
                            alfa = normalAlfa.Sample();
                            beta = normalBeta.Sample();
                        }

                        particles.Set(Field.X, index, particles.Get(Field.PrevX, index));
                        particles.Set(Field.Y, index, particles.Get(Field.PrevY, index));
                        particles.Multiply(Field.Px, index, -beta);
                        particles.Multiply(Field.Py, index, beta);
                        particles.Multiply(Field.Q, index, alfa);
                        if (particles.Get(Field.Q, index) > 0.05 * emitter.ParticleCharge) particles.RemoveAt(index);
                    }

                    var cell = grid.FindCell(particles.Get(Field.X, index), particles.Get(Field.Y, index));
                    particles.SetParticleCell(index, cell);
                }
            }
            Monitor.EndIntegration();
        }
    }
}