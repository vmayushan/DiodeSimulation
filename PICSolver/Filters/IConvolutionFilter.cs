namespace PICSolver.Filters
{
    public interface IConvolutionFilter
    {
        string FilterName { get; }

        double Factor { get; }

        double[,] FilterMatrix { get; }
    }
}