using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using Newtonsoft.Json;
using PICSolver;
using PICSolver.Abstract;
using PICSolver.Project;

namespace ConsolePIC
{
    class Program
    {
        static void Main(string[] args)
        {
            //for (double relaxation = 0.1; relaxation >= 0; relaxation -= 0.02)
            //{
            //    var dump = new List<double>(200);
            //    Console.WriteLine("Solving started with relaxation = {0}", relaxation);
            //    PICProject Project = PICProject.Default;
            //    Project.Filtration.SplineSmoothingParameter = relaxation;
            //    IPICSolver Solver = Project.Method == PICMethod.ParticleInCell ? (IPICSolver)new PICSolver2D() : new IterativeSolver2D();
            //    Solver.Prepare(Project);
            //    do
            //    {
            //        Solver.Step();
            //        dump.Add(Solver.Monitor.Epsilon);
            //    } while (Solver.Monitor.Iteration < Project.Properties.Iterations);
            //    File.WriteAllLines("s" + relaxation + ".txt", dump.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            //    Console.WriteLine("Solved with {0} iteration for {1}", Solver.Monitor.Iteration, Solver.Monitor.TotalTime);
            //}
            //PICProject Project = PICProject.Default;
            //Project.Properties.Parallel = true;
            //IPICSolver Solver = Project.Method == PICMethod.ParticleInCell ? (IPICSolver)new PICSolver2D() : new IterativeSolver2D();
            //Solver.Prepare(Project);
            //Solver.Step();
            //Console.WriteLine(Solver.Monitor.IterationTime);

            //PICProject Project2 = PICProject.Default;
            //Project2.Properties.Parallel = false;
            //IPICSolver Solver2 = Project.Method == PICMethod.ParticleInCell ? (IPICSolver)new PICSolver2D() : new IterativeSolver2D();
            //Solver2.Prepare(Project2);
            //Solver2.Step();
            //Console.WriteLine(Solver2.Monitor.IterationTime);

            //Console.ReadLine();



            var origin = JsonConvert.DeserializeObject<double[]>(File.ReadAllText("rho_flatten.json"));



            foreach(var relaxation in new double[] { 0.7,0.8,0.9, 1.0 })
            {
                PICProject project = PICProject.Default;
                project.Properties.Relaxation = relaxation;
                var dumpEpsilon = new List<double>(project.Properties.Iterations);
               // var dumpNorm = new List<double>(Project.Properties.Iterations);
                IPICSolver solver = project.Method == PICMethod.ParticleInCell ? (IPICSolver)new PICSolver2D() : new IterativeSolver2D();
                solver.Prepare(project);
                do
                {
                    solver.Step();
                   // var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, 128, 128,Residue(origin, Solver.GetMesh().Density));
                    dumpEpsilon.Add(solver.Monitor.Epsilon);
                    if (solver.Monitor.Epsilon < 1E-10) break;
                } while (solver.Monitor.Iteration < project.Properties.Iterations);
                File.WriteAllLines("eps"+ relaxation + ".txt", dumpEpsilon.Select(x => x.ToString(CultureInfo.InvariantCulture)));
                //File.WriteAllLines("tnorm.txt", dumpNorm.Select(x => x.ToString(CultureInfo.InvariantCulture)));
                Console.WriteLine("Solved with {0} iteration for {1} relaxation {2}", solver.Monitor.Iteration, solver.Monitor.TotalTime, relaxation);
            }

            //File.WriteAllText("rho_flatten.json", JsonConvert.SerializeObject(Solver.GetMesh().Density));
            //File.WriteAllText("ex_flatten.json", JsonConvert.SerializeObject(Solver.GetMesh().Ex));
            //File.WriteAllText("ey_flatten.json", JsonConvert.SerializeObject(Solver.GetMesh().Ey));
            //File.WriteAllText("potential_flatten.json", JsonConvert.SerializeObject(Solver.GetMesh().Potential));
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
    }
}
