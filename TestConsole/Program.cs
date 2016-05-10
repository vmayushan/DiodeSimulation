using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICSolver.Extensions;
using PICSolver.Poisson;
using MathNet.Numerics.Data.Text;
using PICSolver.Domain;
using System.Diagnostics;
using System.IO;
using AForge.Math;
using BM3D;
using MathNet.Numerics.LinearAlgebra.Double;

using Newtonsoft.Json;
using PICSolver.Grid;
using PICSolver.Abstract;
using PICSolver.Emitter;
using PICSolver.Storage;
using PICSolver;
using PICSolver.Filters;
using PICSolver.Interpolation;
using PICSolver.Project;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //придумать как "унифицировать" решение Пуассона, чтобы FDM можно было заменить на другой метод
            //var boundary = new BoundaryConditions();
            //var poisson = new Poisson2DFdmSolver(0, 1, 250, 0, 1, 250, boundary); 
            //var matrix = poisson.BuildMatrix();
            //var vector = poisson.BuildVector((x, y) => -2);
            //var result = poisson.Solve(matrix, vector);
            //var grad = ElectricField.Instance.Evaluate(result);
            //DelimitedWriter.Write("result.csv", result, ";");
            //Console.WriteLine(result);
            //Console.WriteLine(grad.Dx);
            //Console.WriteLine(grad.Dy);
            //Stopwatch s = new Stopwatch();
            //var n = 100000;
            //var storage = new ParticleArrayStorage<Particle>(2*n);
            //var list = new List<Particle>(2*n);
            //s.Start();
            //for (int i=0;i< n; i++)
            //{
            //    storage.Add(new Particle(3, 4, 0.5, 0.8, 7));
            //    storage[i] = new Particle(1, 1, 1, 1, 1);
            //}
            //s.Stop();
            //Console.WriteLine(s.ElapsedMilliseconds);
            //s.Start();
            //for (int i = 0; i < n; i++)
            //{
            //    list.Add(new Particle(3, 4, 0.5, 0.8, 7));
            //    storage[i] = new Particle(1, 1, 1, 1, 1);
            //}
            //s.Stop();
            //Console.WriteLine(s.ElapsedMilliseconds);


            //var boundary = new BoundaryConditions();
            //var storage = new ParticleArrayStorage<Particle>(2);
            //var grid = new Grid2D();
            //grid.InitializeGrid(11, 11, 0, 10, 0, 10);
            //storage.Add(new Particle(5.5, 7.5, 0, 0, 0, 0, 1));
            //var cell = grid.FindCell(5.5, 7.5);
            //storage.SetCell(0, cell);
            //var cic = new CloudInCell(storage, grid);
            //cic.InterpolateDensity();
            //var poissonSolver = new Poisson2DFdmSolver(grid, boundary);
            //var matrix = poissonSolver.BuildMatrix();
            //var vector = poissonSolver.BuildVector(grid);
            //var result = poissonSolver.Solve(matrix, vector);
            //ElectricField.Instance.Gradient(result, grid.Ex, grid.Ey, grid.Hx, grid.Hy);


            //var grad = ElectricField.Instance.Evaluate(result);
            //Console.WriteLine(result);

            //Console.WriteLine(grad.Dx);
            //grid.Ex.ToList().ForEach(x => Console.Write(x + " "));
            //Console.WriteLine();

            //Console.WriteLine(grad.Dy);
            //grid.Ey.ToList().ForEach(x => Console.Write(x + " "));
            //Console.WriteLine();
            //var em = new Emitter2D(0, 0, 0, 1, 10);
            //em.Inject();
            //var solver = new PICSolver2D();
            //solver.Prepare();
            //Console.WriteLine((int)Field.X);
            //Console.WriteLine((int)Field.Y);
            //Console.WriteLine((int)Field.Px);
            //Console.WriteLine((int)Field.Py);
            //Console.WriteLine((int)Field.Q);
            //Console.WriteLine((int)Field.Ex);
            //Console.WriteLine((int)Field.Ey);

            //var matrix = FdmMatrix<double>.Build.Dense(4, 3, (x, y) => 3*y + x + 1);
            //Console.WriteLine(matrix);
            //var matrix2 = FdmMatrix<double>.Build.Dense(3, 4, (x, y) => matrix[y,2-x]);
            //Console.WriteLine(matrix2);
            //var test = new List<Tuple<int, double, double>>();
            //test.Add(new Tuple<int, double, double>(1, 0.5, 0.3));


            //test.Add(new Tuple<int, double, double>(1, 0.52, 0.8));
            //var lookup = test.ToLookup(x => x.Item1);
            //var enume = lookup[1].Last();
            //Console.WriteLine(enume.Item3);

            //var solver = new PICTest();
            //solver.Test();
            //var matrix = new double[,] { { 12,14,41 },
            //                { 43,84,24},
            //                {2,1,43 } };
            //var result = matrix.ConvolutionFilter(new TestFilter());
            //Console.WriteLine(Matrix<double>.Build.DenseOfArray(result));
            // var a = Convolution.Calculate(7, 2);
            //  Console.WriteLine(Matrix<double>.Build.DenseOfArray(a));
            //Complex[,] arr = new Complex[2,2];
            //arr[0, 0] = new Complex(4,0);
            //arr[1, 1] = (Complex)5;
            //arr[0, 1] = (Complex)1;
            //arr[1, 0] = (Complex)10;
            //arr = Helpers.FFTShift(Helpers.FFTShift(arr));

            //Console.WriteLine(arr[0,0]);
            //Console.WriteLine(arr[1, 1]);
            //Console.WriteLine(arr[0, 1]);
            //Console.WriteLine(arr[1, 0]);
            //var n = 2;
            //var k = 2;
            //var p = new double[] { 1.0, 1.0 };

            //var collection = new List<Probability>(10000);
            //collection.Add(new Probability() { Index = 0, Level = 0, Parent = null, Value = 1.0 });
            //collection.Add(new Probability() { Index = 1, Level = 0, Parent = null, Value = 10.0 });
            //for (int i = 0; i < k; i++)
            //{
            //    GenerateLevel(collection, i, n);
            //}
            //var zz = collection.Where(c => c.Level == k).ToList();

            //for (int i = k; i >= 0; i--)
            //{
            //    foreach (var prob in collection.Where(c => c.Level == i))
            //    {
            //        var min2 = collection.Where(c => c.Parent == prob).OrderBy(c => c.Result);
            //        var min = min2.FirstOrDefault();
            //        if (min == null) prob.Result = F(prob.Value, p[prob.Index], 0);
            //        else prob.Result = F(prob.Value, p[prob.Index], min.Result);
            //    }
            //}

            //foreach (var prob in collection.Where(c => c.Level == 2))
            //{
            //    Console.WriteLine(prob.Result);
            //}
            //var grid = new Grid2D();
            //grid.InitializeGrid(11, 11, 0, 10, 0, 10);
            //var x = 0.5;
            //var x1 = 6;
            //var y = 0.5;
            //var y1 = 0.1;


            ////var watch = Stopwatch.StartNew();
            ////for (int i = 0; i < 1000000; i++)
            ////{
            ////    var intersect = new List<CloudInCellIterative.Test>();
            ////    intersect.Add(new CloudInCellIterative.Test() { X = x, Y = y });
            ////    intersect.AddRange(grid.X.Where(p => p > Math.Min(x, x1) && p < Math.Max(x, x1))
            ////        .Select(p => new CloudInCellIterative.Test() { X = p, Y = Helpers.LineY(x1, x, y1, y, p) })
            ////        .Concat(grid.Y.Where(p => p > Math.Min(y, y1) && p < Math.Max(y, y1))
            ////        .Select(p => new CloudInCellIterative.Test() { X = Helpers.LineX(x1, x, y1, y, p), Y = p }))
            ////        .OrderBy(p => p.X));
            ////    intersect.Add(new CloudInCellIterative.Test() { X = x1, Y = y1 });
            ////}
            ////Console.WriteLine(watch.ElapsedMilliseconds);

            ////intersect.ToList().ForEach(p => Console.WriteLine(p.X + " " + p.Y));
            //// Console.WriteLine("тестируем");

            ////watch.Restart();
            //Console.ReadLine();
            //for (int z = 0; z < 10000; z++)
            //{
            //    var test = new SortedList<double, double>(4) { { x, y } };
            //    for (int i = 0; i < grid.N; i++)
            //    {
            //        if (grid.X[i] > Math.Min(x, x1) && grid.X[i] < Math.Max(x, x1))
            //        {
            //            test.Add(grid.X[i], Helpers.LineY(x1, x, y1, y, grid.X[i]));
            //        }
            //    }
            //    for (int i = 0; i < grid.M; i++)
            //    {
            //        if (grid.Y[i] > Math.Min(y, y1) && grid.Y[i] < Math.Max(y, y1))
            //        {
            //            test.Add(Helpers.LineX(x1, x, y1, y, grid.Y[i]), grid.Y[i]);
            //        }
            //    }
            //    //grid.X.Where(p => p > Math.Min(x, x1) && p < Math.Max(x, x1)).ForEach(p => test.Add(p, Helpers.LineY(x1, x, y1, y, p)));
            //    //grid.Y.Where(p => p > Math.Min(y, y1) && p < Math.Max(y, y1)).ForEach(p => test.Add(Helpers.LineX(x1, x, y1, y, p), p));
            //    test.Add(x1, y1);
            //    //var intersect = new List<CloudInCellIterative.Test>();
            //    //intersect.Add(new CloudInCellIterative.Test() { X = x, Y = y });
            //    //intersect.AddRange(grid.X.Where(p => p > Math.Min(x, x1) && p < Math.Max(x, x1))
            //    //    .Select(p => new CloudInCellIterative.Test() { X = p, Y = Helpers.LineY(x1, x, y1, y, p) })
            //    //    .Concat(grid.Y.Where(p => p > Math.Min(y, y1) && p < Math.Max(y, y1))
            //    //    .Select(p => new CloudInCellIterative.Test() { X = Helpers.LineX(x1, x, y1, y, p), Y = p }))
            //    //    .OrderBy(p => p.X));
            //    //intersect.Add(new CloudInCellIterative.Test() { X = x1, Y = y1 });
            //    //var enumerator = test.GetEnumerator();
            //    //enumerator.MoveNext();
            //    //var left = enumerator.Current;
            //    //while (enumerator.MoveNext())
            //    //{
            //    //    var right = enumerator.Current;
            //    //    // Perform logic on the item
            //    //    left = right;

            //    //}
            //}
            //Console.WriteLine("все");
            //Console.WriteLine(watch.ElapsedMilliseconds);

            //test.ToList().ForEach(p => Console.WriteLine(p.Key + " "+ p.Value));

            //MWArray array  = MWArray.
            //try
            //{

            //    var json = File.ReadAllText(@"C:\Users\Владислав\Documents\Visual Studio 2015\Projects\PICSolver\SimulationTool\bin\Release\rho.json");
            //    var array = JsonConvert.DeserializeObject<double[,]>(json);
            //    Class1 class1 = new Class1();
            //    MWNumericArray first = new MWNumericArray(1);
            //    double[,] arr = new double[,]
            //    {
            //        {1, 0.1, 0.2, 0.3, 0.4, 0, 0.1, 0.2, 0.3, 0.4, 0, 0.1, 0.2, 0.3, 0.4, 0, 0.1, 0.2, 0.3, 0.4},
            //        {0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F},
            //         {0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F},
            //          {0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F, 0, 0.1F, 0.2F, 0.3F, 0.4F},
            //    };
            //    MWNumericArray second = new MWNumericArray(arr);
            //    MWNumericArray third = new MWNumericArray(25);
            //    var a = class1.BM3D_CFA(1, second,  third);
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine("Error: {0}", exception);
            //}

            //var matrix = new double[,] { { 12,14,41 },
            //                { 43,84,24},
            //                {2,1,43 } };
            //var result = matrix.MedianFilter(3, 1, x=>x);
            
            //Console.WriteLine(Matrix<double>.Build.DenseOfArray(matrix));
            //Console.WriteLine(Matrix<double>.Build.DenseOfArray(result));
            float i = 100;
            Console.WriteLine(i);
        }


        public static double C(double x, int i, int j)
        {
            if (j == i) return x - 1;
            return x + 1;
        }

        public static double F(double x, double p, double min)
        {
            return 3 - 5 * x * p + (1 - p * x) * min;
        }

        public static void GenerateLevel(List<Probability> collection, int level, int n)
        {
            foreach (var p in collection.Where(x => x.Level == level).ToArray())
            {
                for (int i = 0; i < n; i++)
                {
                    var newProbability = new Probability()
                    {
                        Level = (short)(level + 1),
                        Index = (short)i,
                        Parent = p,
                        Value = C(p.Value, i, p.Index)
                    };
                    collection.Add(newProbability);
                }
            }
        }
    }

    public class Probability
    {
        public double Value { get; set; }
        public short Level { get; set; }
        public short Index { get; set; }
        public Probability Parent { get; set; }
        public double Result { get; set; }
    }
}
