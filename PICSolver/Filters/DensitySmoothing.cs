using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using PICSolver.Abstract;
using PICSolver.Extensions;
using PICSolver.Monitor;
using PICSolver.Project;

namespace PICSolver.Filters
{
    public class DensitySmoothing
    {
        private SmoothSpline smoothSpline;
        private double normalizationAverage;
        private FFTWFilter fftwFilter;
        private readonly PICProject project;
        private IConvolutionFilter convolutionFilter;
        private readonly IGrid2D grid;
        private readonly IMesh mesh;
        private readonly PICMonitor monitor;

        public DensitySmoothing(PICProject project, IGrid2D grid, IMesh mesh, PICMonitor monitor)
        {
            this.project = project;
            this.grid = grid;
            this.mesh = mesh;
            this.monitor = monitor;
        }
        public void Prepare()
        {
            smoothSpline = new SmoothSpline(grid.N, grid.M);
            if (project.Filtration.FourierFilterEnabled)
            {
                fftwFilter = new FFTWFilter(mesh.Density.Length);
                var horizontal = Vector<double>.Build.DenseOfArray(Window.Gauss(128, project.Filtration.SplineSmoothingParameter)); //тут гаусс был
                var vertical = Vector<double>.Build.DenseOfArray(Window.Dirichlet(128)); // а тут дирихле
                var matrix = vertical.ToColumnMatrix() * horizontal.ToRowMatrix();
                //var matrix = vertical.ToColumnMatrix() * vertical.ToRowMatrix();
                fftwFilter.FFTFilter = ArrayExtension.FlattenArray(matrix.ToArray(), grid.N, grid.M);

                //var m1 = Vector<double>.Build.DenseOfArray(Window.Gauss(128, 0.1)).ToRowMatrix();
                //var m2 = Vector<double>.Build.DenseOfArray(Window.Gauss(128, 0.05)).ToRowMatrix();
                //MatlabWriter.Write("filters.mat", new[] { m1, m2 }, new[] { "ofive", "oofive" });

            }
            if (project.Filtration.ConvolutionFilterEnabled)
            {
                convolutionFilter =
                    (IConvolutionFilter)
                        Activator.CreateInstance("PICSolver",
                            "PICSolver.Filters." + project.Filtration.ConvolutionFilterName).Unwrap();
            }
        }
        public void Apply()
        {
            for (int i = 1; i < grid.N - 1; i++)
            {
                for (int j = 0; j < grid.M; j++)
                {
                    var d2Rho = mesh.Density[Index.At(i + 1, j, grid.N, grid.M)] + mesh.Density[Index.At(i - 1, j, grid.N, grid.M)] - 2 * mesh.Density[Index.At(i, j, grid.N, grid.M)];
                    if (d2Rho > 0)
                        mesh.Density[Index.At(i, j, grid.N, grid.M)] = (mesh.Density[Index.At(i + 1, j, grid.N, grid.M)] + mesh.Density[Index.At(i - 1, j, grid.N, grid.M)]) / 2;
                }
            }

            if (project.Filtration.ConvolutionFilterEnabled)
            {
                mesh.Density = mesh.Density.ConvolutionFilter(convolutionFilter, grid.N, grid.M);
            }
            if (project.Filtration.NormalizationEnabled)
            {
                normalizationAverage = mesh.Density.Average();
            }
            if (project.Filtration.SplineSmoothingEnabled)
            {
                smoothSpline.Smooth(mesh.Density, project.Filtration.SplineSmoothingParameter);
                //MatlabWriter.Write("spl5.mat", new[] { Matrix<double>.Build.DenseOfArray(ArrayExtension.RectangleArray(mesh.Density, grid.N, grid.M)) }, new[] { "su5" });
            }
            if (project.Filtration.FourierFilterEnabled)
            {
                fftwFilter.Execute(mesh.Density, grid.N, grid.M, monitor.Rho);
                #region old
                //private Complex[,] fourierImage;
                //private Complex[,] fourierImageShifted;

                //fourierImage = new Complex[grid.N, grid.M];
                //fourierImageShifted = new Complex[grid.N, grid.M];

                //for (int i = 0; i < grid.N; i++)
                //{
                //    if (i < 54 || i > 74)
                //    {
                //        matrix[63, i] = 0.0;
                //        matrix[64, i] = 0.0;
                //        matrix[65, i] = 0.0;
                //        matrix[66, i] = 0.0;
                //    }
                //}

                //var before = Matrix<double>.Build.DenseOfArray(ArrayExtension.RectangleArray(mesh.Density, grid.N, grid.M));
                //var after = Matrix<double>.Build.DenseOfArray(ArrayExtension.RectangleArray(mesh.Density, grid.N, grid.M));
                //MatlabWriter.Write("new.mat", new[] { before, after }, new[] { "bef", "aft" });
                //Emgu.CV.Matrix<double> input = new Emgu.CV.Matrix<double>(Helpers.RectangleArray(mesh.Density, grid.N, grid.M));
                //var input2 = input.Convert<float>();
                //Emgu.CV.Matrix<float> output = new Emgu.CV.Matrix<float>(grid.N, grid.M);
                //  CvInvoke.BilateralFilter(input2, output, 10, 10, 10);
                // CvInvoke.BoxFilter(input2,output,DepthType.Cv32F,new Size(30,30),new Point(-1,-1)  );
                //CvInvoke.MedianBlur(input2,output,3);
                //CvInvoke.GaussianBlur(input2,output,new Size(33,33),100,100 );
                // var output2 = output.Convert<double>();
                // mesh.Density = Helpers.FlattenArray2(output2.Data, grid.N, grid.M);

                //ArrayExtension.FlattenComplexArray(fourierImageShifted, grid.N, grid.M, Monitor.TestData, x => x.Im);

                //var watch2 = Stopwatch.StartNew();
                //ArrayExtension.RectangleComplexArray(mesh.Density, grid.N, grid.M, fourierImage);
                //FourierTransform.DFT2(fourierImage, FourierTransform.Direction.Forward);
                //FourierFilter.FFTShift(fourierImage, fourierImageShifted);
                //FourierFilter.ApplyMatrixFilter(fourierImageShifted, filterMatrix);
                //FourierFilter.RemoveFFTShift(fourierImageShifted, fourierImage);
                //FourierTransform.DFT2(fourierImage, FourierTransform.Direction.Backward);
                //ArrayExtension.FlattenComplexArray(fourierImage, grid.N, grid.M, mesh.Density, x => x.Re);
                //watch2.Stop();
                //Debug.WriteLine("aforge " + watch2.ElapsedMilliseconds);
                #endregion
            }
            if (project.Filtration.NormalizationEnabled)
            {
                normalizationAverage /= mesh.Density.Average();
                if (!double.IsNaN(normalizationAverage))
                {
                    for (var i = 0; i < mesh.Density.Length; i++)
                    {
                        mesh.Density[i] *= normalizationAverage;
                    }
                }
            }
        }
    }
}
