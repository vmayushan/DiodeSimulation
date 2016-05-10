using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Emitter;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.ElectricField;
using PICSolver.Filters;
using PICSolver.Mesh;
using PICSolver.Monitor;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Project;
using PICSolver.Storage;

namespace PICSolver
{
    public class PICSolver2D : IPICSolver
    {
        private BoundaryConditions boundaryConditions;
        private IEmitter emitter;
        private IGrid2D grid;
        private double h;
        private IInterpolationScheme interpolator;
        private IMesh mesh;
        private IMover mover;
        private IParticleStorage<Particle> particles;
        private IFieldSolver poissonSolver;
        private double step;
        private double u;
        private bool backscattering;
        private double alfa;
        private double beta;
        public static int a = 5;
        private Normal normalAlfa;
        private Normal normalBeta;
        private PICProject project;
        private DensitySmoothing filtration;

        public PICMonitor Monitor { get; set; }
        public List<Tuple<int, double, double>> Trajectories { get; set; }
        public IMesh GetMesh()
        {
            return mesh;
        }
        public void Prepare(PICProject proj)
        {
            this.project = proj;
            backscattering = project.Backscattering.IsEnabled;
            alfa = project.Backscattering.Alfa;
            beta = project.Backscattering.Beta;
            step = project.Properties.Step;
            u = project.Diode.Voltage;
            particles = new ParticleArrayStorageModified<Particle>(1000000);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => u, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(project.Emitter.Left, project.Emitter.Bottom, project.Emitter.Right, project.Emitter.Top, project.Emitter.ParticlesCount, 0, 0, project.Emitter.CurrentDensity, step, project.Emitter.EmissionType);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(project.Properties.GridN, project.Properties.GridM, 0, project.Diode.Length, 0, project.Diode.Height);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCell(particles, grid, mesh, true);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.FdmMatrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles,this);
            if (project.Backscattering.Random)
            {
                normalAlfa = new Normal(project.Backscattering.Alfa, (project.Backscattering.Alfa / 3));
                normalBeta = new Normal(project.Backscattering.Beta, (project.Backscattering.Beta / 3));
            }
            filtration = new DensitySmoothing(project, grid, mesh, Monitor);
            filtration.Prepare();
        }

        public void Step()
        {
            Monitor.BeginIteration();
            var injectedParticles = emitter.Inject();
            var injectedParticlesId = new int[emitter.ParticlesCount];
            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                var cell = grid.FindCell(injectedParticles[i].X, injectedParticles[i].Y);
                var id = particles.Add(injectedParticles[i]);
                particles.SetParticleCell(id, cell);
                injectedParticlesId[i] = id;
            }

            Monitor.BeginInterpolation();
            mesh.ResetDensity();
            interpolator.InterpolateDensity();
            Monitor.EndInterpolation();

            filtration.Apply();
            
            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.FdmMatrix, vector);
            Monitor.EndPoissonSolve();


            Gradient.Calculate(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);

            Monitor.ContinueInterpolation();
            particles.ResetForces();
            interpolator.InterpolateForces();
            Monitor.EndInterpolation();

            Monitor.BeginIntegration();
            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                mover.Prepare(particles, injectedParticlesId[i], h);
            }

            foreach (var index in particles.EnumerateIndexes())
            {
                mover.Step(particles, index, h);

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
                        particles.Multiply(Field.Py, index, -beta);
                        particles.Multiply(Field.Q, index, alfa);
                        if (particles.Get(Field.Q, index) > 0.05 * emitter.ParticleCharge) particles.RemoveAt(index);
                    }

                    var cell = grid.FindCell(particles.Get(Field.X, index), particles.Get(Field.Y, index));
                    particles.SetParticleCell(index, cell);
                }
            }
            Monitor.EndIntegration();
            Monitor.EndIteration();
        }
    }
}