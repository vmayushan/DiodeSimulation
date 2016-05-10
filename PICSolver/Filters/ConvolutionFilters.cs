namespace PICSolver.Filters
{
        public class SoftenFilter : IConvolutionFilter
        {
            public string FilterName => "SoftenFilter";
            public double Factor { get; } = 1.0 / 2.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {0, 0.5, 0,},
            {0, 1, 0,},
            {0, 0.5, 0,},
            };
        }

        public class Gaussian3X3BlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian3X3BlurFilter";
            public double Factor { get; } = 1.0 / 16.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1, 2, 1,},
            {2, 4, 2,},
            {1, 2, 1,},
            };
        }

        public class TestFilter : IConvolutionFilter
        {
            public string FilterName => "TestFilter";
            public double Factor { get; } = 1.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {0, 0, 0,},
            {0, 1.0, 0,},
            {0, 0, 0},
            };
        }

        public class Gaussian5X5BlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 159.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {2, 04, 05, 04, 2,},
            {4, 09, 12, 09, 4,},
            {5, 12, 15, 12, 5,},
            {4, 09, 12, 09, 4,},
            {2, 04, 05, 04, 2,},
            };
        }

        public class Sharpen3X3Filter : IConvolutionFilter
        {
            public string FilterName => "Sharpen3X3Filter";
            public double Factor => 1.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {0, -1, 0,},
            {-1, 5, -1,},
            {0, -1, 0,},
            };
        }

        public class HorizontalFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 24.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {{2, 5, 10, 5, 2,}};
        }

        public class MeanFilter : IConvolutionFilter
        {
            public string FilterName => "Sharpen3X3Filter";
            public double Factor => 9.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1, 1, 1,},
            {1, 1, 1,},
            {1, 1, 1,},
            };
        }

        public class MeanFilterVertical : IConvolutionFilter
        {
            public string FilterName => "Sharpen3X3Filter";
            public double Factor => 9.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1},
            {1},
            {1 },
            };
        }

        public class Gaussian5X5BlurFilter2 : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 256.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1, 4, 6, 4, 1,},
            {4, 16, 24, 16, 4,},
            {6, 24, 36, 24, 6,},
            {1, 16, 24, 16, 4,},
            {1, 4, 6, 4, 1,},
            };
        }
        public class Gaussian7X7BlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 1003.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {0,0,1,2,1,0,0},
            {0,3,13,22,13,3,0},
            {1,13,59,97,59,13,1},
            {2,22,97,159,97,22,2},
            {1,13,59,97,59,13,1},
            {0,3,13,22,13,3,0},
            {0,0,1,2,1,0,0},
            };
        }
        public class Gaussian7X7HorizontalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 243.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1,13,59,97,59,13,1},
            };
        }
        public class Gaussian7X7VerticalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 243.0;

            public double[,] FilterMatrix { get; } = new double[,]
           {
            {1}, { 13}, { 59}, { 97}, { 59}, { 13}, { 1},
           };
        }
        public class Gaussian9X9HorizontalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 499.0;

            public double[,] FilterMatrix { get; } = new double[,]
            {
            {1,13,59,97,159,97,59,13,1},
            };
        }
        public class Gaussian9X9VerticalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 499.0;

            public double[,] FilterMatrix { get; } = new double[,]
           {
            {1}, { 13}, { 59}, { 97}, { 159}, { 97}, { 59}, { 13}, { 1},
           };
        }
        public class Gaussian3X3VerticalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 4;

            public double[,] FilterMatrix { get; } = new double[,]
           {
            {1}, { 2}, { 1},
           };
        }
        public class VerticalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 3;

            public double[,] FilterMatrix { get; } = new double[,]
           {
            {1}, { 1}, { 1},
           };
        }

        public class CustomFilter : IConvolutionFilter
        {
            public string FilterName => "CustomFilter";
            public double Factor => 1.0;

            public CustomFilter(double[,] matrix)
            {
                this.FilterMatrix = matrix;
            }
            public double[,] FilterMatrix { get; }
        }
        public class Gaussian11X11VerticalBlurFilter : IConvolutionFilter
        {
            public string FilterName => "Gaussian5X5BlurFilter";
            public double Factor { get; } = 1.0 / 914.0;

            public double[,] FilterMatrix { get; } = new double[,]
           {
            {1}, { 13}, { 59}, { 97}, { 159}, { 256}, { 159}, { 97}, { 59}, { 13}, { 1},
           };
        }
    
}
