using System;
using AForge.Math;

namespace PICSolver.Filters
{
    public static class Convolution
    {
        /// <summary>
        /// Applies convolution filter on rectangle complex array
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Complex[,] ConvolutionFilter<TFilter>(this Complex[,] source, TFilter filter) where TFilter : IConvolutionFilter
        {
            int filterWidth = filter.FilterMatrix.GetLength(1);
            int filterHeight = filter.FilterMatrix.GetLength(0);

            int sourceWidth = source.GetLength(1);
            int sourceHeight = source.GetLength(0);

            Complex[,] result = source.Clone() as Complex[,];

            int filterOffsetWidth = (filterWidth - 1) / 2;
            int filterOffsetHeight = (filterHeight - 1) / 2;

            for (int offsetY = filterOffsetHeight; offsetY < sourceHeight - filterOffsetHeight; offsetY++)
            {
                for (int offsetX = filterOffsetWidth; offsetX < sourceWidth - filterOffsetWidth; offsetX++)
                {

                    Complex temp = Complex.Zero;
                    for (int filterY = -filterOffsetHeight; filterY <= filterOffsetHeight; filterY++)
                    {
                        for (int filterX = -filterOffsetWidth; filterX <= filterOffsetWidth; filterX++)
                        {
                            temp += source[filterY + offsetY, filterX + offsetX] *
                                    filter.FilterMatrix[filterY + filterOffsetHeight, filterX + filterOffsetWidth];
                        }
                    }
                    temp *= filter.Factor;

                    result[offsetY, offsetX] = temp;
                }
            }
            return result;
        }


        /// <summary>
        /// Applies convolution filter to double flatten array
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <param name="sourceHeight"></param>
        /// <param name="sourceWidth"></param>
        /// <returns></returns>
        public static double[] ConvolutionFilter<TFilter>(this double[] source, TFilter filter, int sourceHeight, int sourceWidth)
            where TFilter : IConvolutionFilter
        {
            int filterWidth = filter.FilterMatrix.GetLength(1);
            int filterHeight = filter.FilterMatrix.GetLength(0);

            double[] result = source.Clone() as double[];

            int filterOffsetWidth = (filterWidth - 1) / 2;
            int filterOffsetHeight = (filterHeight - 1) / 2;

            for (int offsetY = filterOffsetHeight; offsetY < sourceHeight - filterOffsetHeight; offsetY++)
            {
                for (int offsetX = filterOffsetWidth; offsetX < sourceWidth - filterOffsetWidth; offsetX++)
                {

                    double temp = 0;
                    for (int filterY = -filterOffsetHeight; filterY <= filterOffsetHeight; filterY++)
                    {
                        for (int filterX = -filterOffsetWidth; filterX <= filterOffsetWidth; filterX++)
                        {
                            temp += source[sourceHeight * (filterY + offsetY) + filterX + offsetX] *
                                    filter.FilterMatrix[filterY + filterOffsetHeight, filterX + filterOffsetWidth];
                        }
                    }
                    temp *= filter.Factor;
                    result[sourceHeight * offsetY + offsetX] = temp;
                }
            }
            return result;
        } 

        /// <summary>
        /// Calculates gaussian kernel with weight
        /// </summary>
        /// <param name="length"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static double[,] Calculate(int length, double weight)
        {
            double[,] kernel = new double[length, length];
            double sumTotal = 0;
            int kernelRadius = length / 2;

            double calculatedEuler = 1.0 /
            (2.0 * Math.PI * Math.Pow(weight, 2));


            for (int filterY = -kernelRadius;
                 filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius;
                    filterX <= kernelRadius; filterX++)
                {
                    var distance = ((filterX * filterX) +
                                       (filterY * filterY)) /
                                      (2 * (weight * weight));


                    kernel[filterY + kernelRadius,
                           filterX + kernelRadius] =
                           calculatedEuler * Math.Exp(-distance);


                    sumTotal += kernel[filterY + kernelRadius,
                                       filterX + kernelRadius];
                }
            }


            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    kernel[y, x] = kernel[y, x] *
                                   (1.0 / sumTotal);
                }
            }


            return kernel;
        }
    }
}