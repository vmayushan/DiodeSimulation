using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Solvers;
using PICSolver.Abstract;
using Constants = PICSolver.Domain.Constants;

namespace PICSolver.Poisson
{
    public class Poisson2DFdmSolver : IFieldSolver
    {

        private IPreconditioner<double> preconditioner;
        private IIterativeSolver<double> solver;
        private Iterator<double> monitor;
        private readonly BoundaryConditions boundary;
        private readonly double dx;
        private readonly double dy;
        private readonly int m;
        private readonly int n;
        private readonly double[] x;
        private readonly double[] y;
        private Vector<double> result;
        public Matrix<double> FdmMatrix { get; set; }

        public Poisson2DFdmSolver(IGrid2D grid, BoundaryConditions boundary)
        {
            this.boundary = boundary;
            n = grid.N;
            m = grid.M;
            x = grid.X;
            y = grid.Y;
            //@todo fix this
            dx = grid.Hy;
            dy = grid.Hx;

            InitializeSolver();
        }

        public Matrix<double> BuildMatrix()
        {
            var size = n * m;

            var matrix = Matrix<double>.Build.Sparse(size, size);

            var invDx = 1 / dx;
            var invDy = 1 / dy;
            var invDxDx = 1 / (dx * dx);
            var invDyDy = 1 / (dy * dy);
            var invDxDy = -2 / (dx * dx) - 2 / (dy * dy);


            //Определение коэффициентов и свободных членов СЛАУ, соответствующих граничным условиям по оси X 
            for (var j = 0; j < m; j++)
            {
                //left boundary
                if (boundary.Left.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix.At(j, j, 1.0);
                }
                else if (boundary.Left.Type == BoundaryConditionType.Neumann)
                {
                    matrix.At(j, m + j, invDx);
                    matrix.At(j, j, -invDx);
                }

                //right boundary
                if (boundary.Right.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix.At(m * (n - 1) + j, m * (n - 1) + j, 1.0);
                }
                else if (boundary.Right.Type == BoundaryConditionType.Neumann)
                {
                    matrix.At(m * (n - 1) + j, m * (n - 1) + j, invDx);
                    matrix.At(m * (n - 1) + j, m * (n - 2) + j, -invDx);
                }
            }
            //Определение коэффициентов и свободных членов СЛАУ, соответствующих граничным условиям по оси Y 
            for (var i = 1; i < n - 1; i++)
            {
                //bottom boundary
                if (boundary.Bottom.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix.At(m * i, m * i, 1.0);
                }
                else if (boundary.Bottom.Type == BoundaryConditionType.Neumann)
                {
                    matrix.At(m * i, m * i, -invDy);
                    matrix.At(m * i, m * i + 1, invDy);
                }

                //top boundary
                if (boundary.Top.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix.At(m * i + m - 1, m * i + m - 1, 1.0);
                }
                else if (boundary.Top.Type == BoundaryConditionType.Neumann)
                {
                    matrix.At(m * i + m - 1, m * i + m - 1, invDy);
                    matrix.At(m * i + m - 1, m * i + m - 2, -invDy);
                }
            }
            //Определение коэффициентов СЛАУ, соответствующих внутренним точкам области
            for (var i = 1; i < n - 1; i++)
            {
                for (var j = 1; j < m - 1; j++)
                {
                    matrix.At(m * i + j, m * i + j, invDxDy);

                    matrix.At(m * i + j, m * (i + 1) + j, invDxDx);
                    matrix.At(m * i + j, m * (i - 1) + j, invDxDx);
                    matrix.At(m * i + j, m * i + j + 1, invDyDy);
                    matrix.At(m * i + j, m * i + j - 1, invDyDy);
                }
            }
            preconditioner.Initialize(matrix);
            return matrix;
        }

        public Vector<double> BuildVector(IMesh mesh)
        {
            var vector = Vector<double>.Build.Dense(n * m);

            for (var i = 1; i < n - 1; i++)
            {
                for (var j = 1; j < m - 1; j++)
                {
                    vector[m * i + j] = -mesh.GetDensity(n * j + i) / Constants.VacuumPermittivity;
                }
            }
            VectorBoundaries(vector);
            return vector;
        }

        private bool TrySolve(Matrix<double> matrix, Vector<double> right)
        {
            try
            {
                solver.Solve(matrix, right, result, monitor, preconditioner);
            }
            catch (NumericalBreakdownException)
            {
                return false;
            }
            return true;
        }

        public double[] Solve(Matrix<double> matrix, Vector<double> right)
        {
            monitor.Reset();
            for (int i = 0; ; i++)
            {
                if (i == 2) throw new ApplicationException();
                if (TrySolve(matrix, right)) break;
            }

            var newResult = new double[result.Count];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    newResult[n * j + i] = result.At(m * i + j);
                }
            }
            return newResult;
        }

        private void InitializeSolver()
        {
            Control.UseSingleThread();
            Control.UseNativeOpenBLAS();
            Control.MaxDegreeOfParallelism = 1;
            //Control.UseNativeMKL();
            result = Vector<double>.Build.Dense(n * m);
            var iterationCountStopCriterion = new IterationCountStopCriterion<double>(1000);
            var residualStopCriterion = new ResidualStopCriterion<double>(1e-7);
            
            monitor = new Iterator<double>(iterationCountStopCriterion, residualStopCriterion);
            solver = new BiCgStab();
            preconditioner = new MILU0Preconditioner();
        }

        private void VectorBoundaries(IList<double> vector)
        {
            for (var j = 0; j < m; j++)
            {
                vector[j] = boundary.Left.Value(y[j]);
                vector[m * (n - 1) + j] = boundary.Right.Value(y[j]);
            }
            for (var i = 1; i < n - 1; i++)
            {
                vector[m * i] = boundary.Bottom.Value(x[i]);
                vector[m * i + m - 1] = boundary.Top.Value(x[i]);
            }
        }
    }
}