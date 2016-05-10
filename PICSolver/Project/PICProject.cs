using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Poisson;

namespace PICSolver.Project
{
    public class PICProject
    {
        public static PICProject Default => new PICProject
        {
            Method = PICMethod.Iterative,
            Backscattering = new Backscattering
            {
                IsEnabled = false,
                Alfa = 0.8,
                Beta = 0.3,
                Random = true
            },
            Diode = new Diode
            {
                Height = 0.1,
                Length = 0.1,
                Voltage = 100000
            },
            Properties = new PICSolver.Project.Properties
            {
                Step = 1E-12,
                GridM = 128,
                GridN = 128,
                Relaxation = 0.1,
                TracksCount = 10,
                Iterations = 400,
                Parallel = false
            },
            Emitter = new Emitter
            {
                Left = 0,
                Right = 0,
                Top = 0.06,
                Bottom = 0.04,
                ParticlesCount = 500,
                CurrentDensity = -1.9*Constants.ChildLangmuirCurrent(0.1, 100000),
                EmissionType = EmissionType.QuietStart
            },
            Filtration = new Filtration
            {
                NormalizationEnabled = false,
                ConvolutionFilterEnabled = false,
                ConvolutionFilterName = "Gaussian7X7VerticalBlurFilter",
                FourierFilterEnabled = false,
                FourierHorizontalFilterName = "",
                FourierVerticalFilterName = "",
                SplineSmoothingEnabled = false,
                SplineSmoothingParameter = 0.1
            }
        };
        public Diode Diode { get; set; }
        public Emitter Emitter { get; set; }
        public Properties Properties { get; set; }
        public Backscattering Backscattering { get; set; }
        public PICMethod Method { get; set; }
        public Filtration Filtration { get; set; }
        public BoundaryConditions BoundaryConditions { get; set; }


    }
    public class Diode
    {
        public double Length { get; set; }
        public double Height { get; set; }
        public int Voltage { get; set; }
    }

    public class Emitter
    {
        public double Bottom { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public int ParticlesCount { get; set; }
        public double CurrentDensity { get; set; }
        public EmissionType EmissionType { get; set; }
    }

    public class Properties
    {
        public double Relaxation { get; set; }
        public int GridN { get; set; }
        public int GridM { get; set; }
        public double Step { get; set; }
        public int TracksCount { get; set; }
        public int Iterations { get; set; }
        public bool Parallel { get; set; }
    }

    public class Backscattering
    {
        public bool IsEnabled { get; set; }
        public double Alfa { get; set; }
        public double Beta { get; set; }
        public bool Random { get; set; }
    }

    public class Filtration
    {
        public bool SplineSmoothingEnabled { get; set; }
        public double SplineSmoothingParameter { get; set; }
        public bool ConvolutionFilterEnabled { get; set; }
        public string ConvolutionFilterName { get; set; }
        public bool FourierFilterEnabled { get; set; }
        public string FourierHorizontalFilterName { get; set; }
        public string FourierVerticalFilterName { get; set; }
        public bool NormalizationEnabled { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PICMethod
    {
        ParticleInCell,
        Iterative
    }
}
