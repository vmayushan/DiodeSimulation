using System;
using System.Collections.Generic;
using PICSolver.Monitor;
using PICSolver.Project;

namespace PICSolver.Abstract
{
    // ReSharper disable once InconsistentNaming
    public interface IPICSolver
    {
        void Prepare(PICProject project);
        void Step();
        PICMonitor Monitor { get; set; }
        List<Tuple<int, double, double>> Trajectories { get; set; }
        IMesh GetMesh();
    }
}
