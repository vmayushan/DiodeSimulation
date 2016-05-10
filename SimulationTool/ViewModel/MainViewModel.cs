using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Wpf;
using PICSolver;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Monitor;
using PICSolver.Project;
using SimulationTool.Commands;
using SimulationTool.Helpers;
using SimulationTool.View;
using IDialogCoordinator = SimulationTool.Dialogs.IDialogCoordinator;
using LineSeries = OxyPlot.Series.LineSeries;
using PlotType = PICSolver.Monitor.PlotType;

namespace SimulationTool.ViewModel
{
    //при нажатии на создание новой вкладки 
    //открывается окно с параметрами
    //кол-во графиков (может быть)
    //типы графиков
    //цветовая схема
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator dialogCoordinator;
        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        private PICProject Project { get; set; }
        private string ProjectPath { get; set; }
        private IPICSolver Solver { get; set; }
        private CancellationTokenSource TokenSource { get; set; }

        public void AddPlot(PICPlot plot)
        {
            string header;
            switch (plot.PlotType)
            {
                case PlotType.Line:
                    header = $"{plot.PlotSource.Description()} {plot.PlotType.Description()}";
                    break;
                case PlotType.Trajectories:
                    header = plot.PlotType.Description();
                    break;
                case PlotType.Epsilon:
                    header = plot.PlotType.Description();
                    break;
                default:
                    header = plot.PlotSource.Description();
                    break;
            }
            var workspace = new PlotViewModel(plot) {Header = header, Solver = Solver};
            Workspaces.Add(workspace);
            SelectedIndex = Workspaces.IndexOf(workspace);
        }

        public static double[,] RectangleArrayResidue(double[,] lhs, double[,] rhs, int n, int m)
        {
            var result = new double[n, m];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    result[i, j] = Math.Abs(lhs[i, j] - rhs[i, j]);
                }
            }
            return result;
        }

        public void UpdatePlots(PICMonitor monitor)
        {
            Task.Run(() =>
            {
                foreach (var model in Workspaces.Where(p => p.GetType() == typeof (PlotViewModel)).Cast<PlotViewModel>()
                    )
                {
                    if (model.PlotModel == null) continue;
                    switch (model.PICPlot.PlotType)
                    {
                        case PlotType.Heatmap:
                            switch (model.PICPlot.PlotSource)
                            {
                                case PlotSource.Density:
                                    //model.HeatMapSeries.Data = RectangleArrayResidue(monitor.Rho, TestRho, 100, 100);
                                    model.HeatMapSeries.Data = monitor.Rho;
                                    break;
                                case PlotSource.Potential:
                                    model.HeatMapSeries.Data = monitor.Potential;
                                    break;
                                case PlotSource.ElectricFieldX:
                                    model.HeatMapSeries.Data = monitor.Ex;
                                    break;
                                case PlotSource.ElectricFieldY:
                                    model.HeatMapSeries.Data = monitor.Ey;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            model.HeatMapSeries.Invalidate();
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        case PlotType.Line:
                            var lineSeries = new LineSeries {MarkerType = MarkerType.Circle};
                            var data = monitor.GetLine(model.PICPlot.PlotSource, model.PICPlot.LinePlotAlignment,
                                model.PICPlot.LinePlotСoordinate);
                            for (var i = 0; i < data.Length; i++)
                                lineSeries.Points.Add(new DataPoint(monitor.GridX[i], data[i]));
                            model.PlotModel.Series.Clear();
                            model.PlotModel.Series.Add(lineSeries);
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        case PlotType.Trajectories:
                            model.PlotModel.Series.Clear();
                            foreach (var trajectory in  monitor.Trajectories)
                            {
                                var series = new LineSeries
                                {
                                    MarkerType = MarkerType.None,
                                    LineStyle = LineStyle.Solid,
                                    Color = OxyColors.Black,
                                    StrokeThickness = 1,
                                    Smooth = false
                                };
                                foreach (var point in trajectory)
                                    series.Points.Add(new DataPoint(point.Item2, point.Item3));
                                model.PlotModel.Series.Add(series);
                            }
                            //bgg
                            var series2 = new LineSeries
                            {
                                MarkerType = MarkerType.None,
                                LineStyle = LineStyle.Solid,
                                Color = OxyColors.White,
                                StrokeThickness = 1,
                                Selectable = false
                            };
                            series2.Points.Add(new DataPoint(0, 0));
                            series2.Points.Add(new DataPoint(0, 0.1));
                            model.PlotModel.Series.Add(series2);
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        case PlotType.Epsilon:
                            if (!model.PlotModel.Series.Any())
                            {
                                var epsilonSeries = new LineSeries {MarkerType = MarkerType.Circle};
                                model.PlotModel.Series.Add(epsilonSeries);
                            }
                            var epsSeries = model.PlotModel.Series.First() as LineSeries;
                            epsSeries?.Points.Add(new DataPoint(monitor.Iteration, monitor.Epsilon));

                            foreach (var axis in model.PlotModel.Axes)
                            {
                                axis.ResetZoomFactor();
                            }
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            });
        }

        public void UpdateTheme()
        {
            foreach (var model in Workspaces.Where(p => p.GetType() == typeof (PlotViewModel)).Cast<PlotViewModel>())
            {
                if (model.PlotModel == null) continue;
                var backgroundBrush =
                    (Brush) ThemeManager.DetectAppStyle(Application.Current).Item1.Resources["WindowBackgroundBrush"];
                model.PlotModel.Background = backgroundBrush.ToOxyColor();
                model.PlotModel.InvalidatePlot(true);
            }
        }

        private void NewProject()
        {
        }

        private void OpenProject()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Json Project (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (!File.Exists(openFileDialog.FileName)) return;
                var json = File.ReadAllText(openFileDialog.FileName);
                var project = JsonConvert.DeserializeObject<PICProject>(json);
                if (project == null) return;
                Project = project;
                ProjectPath = openFileDialog.FileName;
            }
        }

        private void SaveProject()
        {
            if (ProjectPath == null || !File.Exists(ProjectPath)) return;
            var json = JsonConvert.SerializeObject(Project);
            File.WriteAllText(ProjectPath, json);
        }

        private void SaveAsProject()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json Project (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                ProjectPath = saveFileDialog.FileName;
                var json = JsonConvert.SerializeObject(Project);
                File.WriteAllText(saveFileDialog.FileName, json);
            }
        }

        private void About()
        {
        }

        #region Constructor

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            this.dialogCoordinator = dialogCoordinator;
            Workspaces = new ObservableCollection<WorkspaceViewModel>();
            Workspaces.CollectionChanged += Workspaces_CollectionChanged;

            // create accent color menu items for the demo
            AccentColors = ThemeManager.Accents
                .Select(
                    a =>
                        new AccentColorMenuData
                        {
                            Name = a.Name,
                            ColorBrush = a.Resources["AccentColorBrush"] as Brush,
                            Model = this
                        })
                .ToList();

            // create metro theme color menu items for the demo
            AppThemes = ThemeManager.AppThemes
                .Select(
                    a =>
                        new AppThemeMenuData
                        {
                            Name = a.Name,
                            BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                            ColorBrush = a.Resources["WhiteColorBrush"] as Brush,
                            Model = this
                        })
                .ToList();
            //open epsilon by default
            AddPlot(new PICPlot
            {
                PlotSource = PlotSource.Density,
                PlotType = PlotType.Epsilon,
                LinePlotAlignment = LinePlotAlignment.Horizontal,
                LinePlotСoordinate = 0
            });
        }

        private void LoadProject()
        {
            Project = PICProject.Default;
            Solver = Project.Method == PICMethod.ParticleInCell
                ? (IPICSolver) new PICSolver2D()
                : new IterativeSolver2D();

            foreach (var plotWorkpace in Workspaces.OfType<PlotViewModel>())
            {
                plotWorkpace.Solver = Solver;
            }

            try
            {
                Solver.Prepare(Project);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Run()
        {
            try
            {
                TokenSource?.Dispose();

                TokenSource = new CancellationTokenSource();
                Task.Run(() =>
                {
                    if (Project == null) LoadProject();
                    if (Project == null) throw new ApplicationException("Project loading failed");
                    Solver.Monitor.Status = PICStatus.Computing;
                    for (var i = 0; i < Project.Properties.Iterations; i++)
                    {
                        if (TokenSource.IsCancellationRequested) TokenSource.Token.ThrowIfCancellationRequested();
                        Step();
                    }
                }, TokenSource.Token).ContinueWith(prevTask => { Solver.Monitor.Status = PICStatus.Pause; });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OneStep()
        {
            Task.Run(() =>
            {
                if (Project == null) LoadProject();
                Solver.Monitor.Status = PICStatus.Computing;
                Step();
            }).ContinueWith(prevTask => { Solver.Monitor.Status = PICStatus.Pause; });
        }

        private void Step()
        {
            Solver.Step();
            Epsilon = Solver.Monitor.Epsilon;
            Iteration = Solver.Monitor.Iteration;
            Time = Solver.Monitor.TotalTime;
            TimeInterpolation = Solver.Monitor.TimeInterpolation;
            TimeIntegration = Solver.Monitor.TimeIntegrator;
            TimePoisson = Solver.Monitor.TimePoisson;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                new Action(() => { UpdatePlots(Solver.Monitor); }));
        }

        private void Abort()
        {
            var json = JsonConvert.SerializeObject(Solver.Monitor.Rho);
            File.WriteAllText("rho.json", json);
            TokenSource.Cancel();
        }

        #endregion

        #region Event Handlers

        private void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= OnWorkspaceRequestClose;
        }

        private void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            CloseWorkspace();
        }

        #endregion

        #region Commands

        private DelegateCommand exitCommand;

        public ICommand ExitCommand
            => exitCommand ?? (exitCommand = new DelegateCommand(() => Application.Current.Shutdown()));

        private DelegateCommand runCommand;

        public ICommand RunCommand
            =>
                runCommand ??
                (runCommand = new DelegateCommand(Run, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand abortCommand;

        public ICommand AbortCommand
            =>
                abortCommand ??
                (abortCommand = new DelegateCommand(Abort, () => Solver?.Monitor?.Status == PICStatus.Computing));

        private DelegateCommand stepCommand;

        public ICommand StepCommand
            =>
                stepCommand ??
                (stepCommand = new DelegateCommand(OneStep, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand openProjectCommand;

        public ICommand OpenProjectCommand
            =>
                openProjectCommand ??
                (openProjectCommand =
                    new DelegateCommand(OpenProject, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand newProjectCommand;

        public ICommand NewProjectCommand
            =>
                newProjectCommand ??
                (newProjectCommand =
                    new DelegateCommand(NewProject, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand saveProjectCommand;

        public ICommand SaveProjectCommand
            => saveProjectCommand ?? (saveProjectCommand = new DelegateCommand(SaveProject));

        private DelegateCommand saveAsProjectCommand;

        public ICommand SaveAsProjectCommand
            => saveAsProjectCommand ?? (saveAsProjectCommand = new DelegateCommand(SaveAsProject));

        private DelegateCommand newWorkspaceCommand;

        public ICommand NewWorkspaceCommand
            => newWorkspaceCommand ?? (newWorkspaceCommand = new DelegateCommand(RunCustomFromVm));

        private DelegateCommand aboutCommand;
        public ICommand AboutCommand => aboutCommand ?? (aboutCommand = new DelegateCommand(About));

        private DelegateCommand projectPropertiesCommand;

        public ICommand ProjectPropertiesCommand
            =>
                projectPropertiesCommand ??
                (projectPropertiesCommand =
                    new DelegateCommand(NewProjectWorkspace,
                        () => Workspaces == null || !Workspaces.Any(x => x is PreferencesViewModel)));

        private void NewProjectWorkspace()
        {
            var workspace = new PreferencesViewModel {Header = "Properties"};
            Workspaces.Add(workspace);
            SelectedIndex = Workspaces.IndexOf(workspace);
        }

        private DelegateCommand closeWorkspaceCommand;

        public ICommand CloseWorkspaceCommand
            =>
                closeWorkspaceCommand ??
                (closeWorkspaceCommand = new DelegateCommand(CloseWorkspace, () => Workspaces?.Count > 0));

        private void CloseWorkspace()
        {
            Workspaces.RemoveAt(SelectedIndex);
            SelectedIndex = 0;
        }


        private DelegateCommand onGitHub;
        public ICommand OnGitHub => onGitHub ?? (onGitHub = new DelegateCommand(LaunchOnGitHub));

        private void LaunchOnGitHub()
        {
            Process.Start("https://github.com/vlad294/PICSolver");
        }

        private async void RunCustomFromVm()
        {
            var customDialog = new CustomDialog {Title = "New plot"};

            var addPlotDialogViewModel =
                new AddPlotDialogViewModel(instance => { dialogCoordinator.HideMetroDialogAsync(this, customDialog); })
                {
                    MainViewModel = this
                };
            customDialog.Content = new AddPlot {DataContext = addPlotDialogViewModel};
            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        #endregion

        #region Public Members

        public ObservableCollection<WorkspaceViewModel> Workspaces { get; set; }

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        private double epsilon;

        public double Epsilon
        {
            get { return epsilon; }
            set
            {
                epsilon = value;
                OnPropertyChanged(nameof(Epsilon));
            }
        }

        private int iteration;

        public int Iteration
        {
            get { return iteration; }
            set
            {
                iteration = value;
                OnPropertyChanged(nameof(Iteration));
            }
        }

        private long time;

        public long Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }


        private long timePoisson;

        public long TimePoisson
        {
            get { return timePoisson; }
            set
            {
                timePoisson = value;
                OnPropertyChanged(nameof(TimePoisson));
            }
        }

        private long timeInterpolation;

        public long TimeInterpolation
        {
            get { return timeInterpolation; }
            set
            {
                timeInterpolation = value;
                OnPropertyChanged(nameof(TimeInterpolation));
            }
        }

        private long timeIntegration;

        public long TimeIntegration
        {
            get { return timeIntegration; }
            set
            {
                timeIntegration = value;
                OnPropertyChanged(nameof(TimeIntegration));
            }
        }
        #endregion
    }

    public class AccentColorMenuData
    {
        private ICommand changeAccentCommand;
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
        public MainViewModel Model { get; set; }

        public ICommand ChangeAccentCommand
            => changeAccentCommand ?? (changeAccentCommand = new DelegateCommand(DoChangeTheme));

        protected virtual void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
            Model.UpdateTheme();
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
            Model.UpdateTheme();
        }
    }
}