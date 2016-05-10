using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace PICSolver.Poisson
{
    /// <summary>
    /// Defines the interface for <see cref="T:MathNet.Numerics.LinearAlgebra.Solvers.IIterativeSolver`1"/> classes that solve the matrix equation Ax = b in
    ///             an iterative manner.
    /// 
    /// </summary>
    public interface IIterativeSolver<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        ///             solution vector and x is the unknown vector.
        /// 
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param><param name="input">The solution vector, <c>b</c></param><param name="result">The result vector, <c>x</c></param><param name="iterator">The iterator to use to control when to stop iterating.</param><param name="preconditioner">The preconditioner to use for approximations.</param>
        void Solve(Matrix<T> matrix, Vector<T> input, Vector<T> result, Iterator<T> iterator, IPreconditioner<T> preconditioner);
    }
}
