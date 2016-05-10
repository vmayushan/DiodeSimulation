using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using PICSolver.Abstract;
using PICSolver.Monitor;
using SimulationTool.Commands;
using HeatMapSeries = OxyPlot.Series.HeatMapSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LinearColorAxis = OxyPlot.Axes.LinearColorAxis;
using PlotType = PICSolver.Monitor.PlotType;

namespace SimulationTool.ViewModel
{
    public class PlotViewModel : WorkspaceViewModel
    {
        public PlotViewModel(PICPlot plot)
        {
            PICPlot = plot;
            PlotModel = new PlotModel();
            var backgroundBrush = (Brush)ThemeManager.DetectAppStyle(Application.Current).Item1.Resources["WindowBackgroundBrush"];
            PlotModel.Background = backgroundBrush.ToOxyColor();

            switch (plot.PlotType)
            {
                case PlotType.Heatmap:
                    PlotModel.Axes.Add(new LinearColorAxis
                    {
                        HighColor = OxyColors.White,
                        LowColor = OxyColors.White,
                        Position = AxisPosition.Right,
                        Palette = OxyPalettes.Jet(1000)
                    });
                    PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
                    PlotModel.Axes.Add(new LinearAxis() { });
                    HeatMapSeries = new HeatMapSeries
                    {
                        X0 = 0,
                        X1 = 0.1,
                        Y0 = 0,
                        Y1 = 0.1,
                        Interpolate = true,
                        Data = new double[2, 2]
                    };
                    PlotModel.Series.Add(HeatMapSeries);
                    HeatMapSeries.Invalidate();
                    PlotModel.InvalidatePlot(true);

                    OnPropertyChanged("PlotModel");
                    break;
                case PlotType.Line:
                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                case PlotType.Epsilon:
                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                case PlotType.Trajectories:
                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IPICSolver Solver { get; set; }

        public PlotModel PlotModel { get; private set; }

        public HeatMapSeries HeatMapSeries { get; private set; }

        public PICPlot PICPlot { get; set; }

        private DelegateCommand exportMatlabCommand;

        public ICommand ExportMatlabCommand
            => exportMatlabCommand ?? (exportMatlabCommand = new DelegateCommand(ExportMatlab, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand exportJsonCommand;

        public ICommand ExportJsonCommand
            => exportJsonCommand ?? (exportJsonCommand = new DelegateCommand(ExportJson, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand exportSvgCommand;

        public ICommand ExportSvgCommand
            => exportSvgCommand ?? (exportSvgCommand = new DelegateCommand(ExportSvg, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand exportPngCommand;

        public ICommand ExportPngCommand
            => exportPngCommand ?? (exportPngCommand = new DelegateCommand(ExportPng, () => Solver?.Monitor?.Status != PICStatus.Computing));

        public void ExportMatlab()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Matlab matrix file (*.mat)|*.mat",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (saveFileDialog.ShowDialog() != true) return;

            var matrixName = "plot" + Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
            switch (PICPlot.PlotType)
            {
                case PlotType.Heatmap:
                    MatlabWriter.Write(saveFileDialog.FileName, Matrix<double>.Build.DenseOfArray(HeatMapSeries.Data), matrixName);
                    break;
                case PlotType.Line:
                    var plotLine = PlotModel.Series.First() as OxyPlot.Series.LineSeries;
                    if (plotLine == null) return;
                    var m1 = Vector<double>.Build.DenseOfArray(plotLine.Points.Select(x => x.X).ToArray()).ToRowMatrix();
                    var m2 = Vector<double>.Build.DenseOfArray(plotLine.Points.Select(x => x.Y).ToArray()).ToRowMatrix();
                    MatlabWriter.Write(saveFileDialog.FileName, new[] { m1, m2 }, new[] { matrixName + "x", matrixName + "y" });
                    break;
                case PlotType.Epsilon:
                    var plotEpsilon = PlotModel.Series.First() as OxyPlot.Series.LineSeries;
                    if (plotEpsilon == null) return;
                    var p1 = Vector<double>.Build.DenseOfArray(plotEpsilon.Points.Select(x => x.X).ToArray()).ToRowMatrix();
                    var p2 = Vector<double>.Build.DenseOfArray(plotEpsilon.Points.Select(x => x.Y).ToArray()).ToRowMatrix();
                    MatlabWriter.Write(saveFileDialog.FileName, new[] { p1, p2 }, new[] { matrixName + "x", matrixName + "y" });
                    break;
                case PlotType.Trajectories:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ExportJson()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (saveFileDialog.ShowDialog() != true) return;

            switch (PICPlot.PlotType)
            {
                case PlotType.Heatmap:
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(HeatMapSeries.Data));
                    break;
                case PlotType.Line:
                    var plotLine = PlotModel.Series.First() as OxyPlot.Series.LineSeries;
                    if (plotLine == null) return;
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(plotLine.Points));
                    break;
                case PlotType.Epsilon:
                    var plotEpsilon = PlotModel.Series.First() as OxyPlot.Series.LineSeries;
                    if (plotEpsilon == null) return;
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(plotEpsilon.Points));
                    break;
                case PlotType.Trajectories:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ExportSvg()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Image (*.svg)|*.svg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (saveFileDialog.ShowDialog() != true) return;

            var exporter = new OxyPlot.Wpf.SvgExporter { Width = 600, Height = 400 };
            var data = exporter.ExportToString(PlotModel);
            File.WriteAllText(saveFileDialog.FileName, data);
        }
        public void ExportPng()
        {
            //var saveFileDialog = new SaveFileDialog
            //{
            //    Filter = "Image (*.png)|*.png",
            //    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            //};
            //if (saveFileDialog.ShowDialog() != true) return;


           // using (var stream = File.Create(saveFileDialog.FileName))
            using (var stream = File.Create(Solver.Monitor.Iteration + ".png"))
            {
                PngExporter.Export(PlotModel, stream, 800, 600, Brushes.White.ToOxyColor());
            }


        }
    }
}
