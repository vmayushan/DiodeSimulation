using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace PICSolver.Poisson
{
    public interface IPreconditioner<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// 
        /// </summary>
        /// <param name="matrix">The matrix on which the preconditioner is based.</param>
        void Initialize(Matrix<T> matrix);
        /// <summary>
        /// Approximates the solution to the matrix equation <b>Mx = b</b>.
        /// 
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param><param name="lhs">The left hand side vector. Also known as the result vector.</param>
        void Approximate(DenseVector rhs, DenseVector lhs);
    }
}
