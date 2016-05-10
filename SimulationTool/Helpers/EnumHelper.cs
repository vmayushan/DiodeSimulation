using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using OxyPlot.Axes;

namespace SimulationTool.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the description of a specific enum value.
        /// </summary>
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            // If no description is found, best guess is to generate it by replacing underscores with spaces
            if (!nAttributes.Any())
            {
                TextInfo oTi = CultureInfo.CurrentCulture.TextInfo;
                return oTi.ToTitleCase(oTi.ToLower(eValue.ToString().Replace("_", " ")));
            }

            return (nAttributes.First() as DescriptionAttribute).Description;
        }

        /// <summary>
        /// Returns an enumerable collection of all values and descriptions for an enum type.
        /// </summary>
        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            return Enum.GetValues(typeof(TEnum)).Cast<Enum>().Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
        }

        public static void ResetZoomFactor(this Axis axis)
        {

            const int minimumOffset = 25;
            const int maximumOffset = 10;

            var oldMinimum = axis.Minimum;
            var oldMaxium = axis.Maximum;

            axis.Minimum = double.NaN;
            axis.Maximum = double.NaN;

            axis.Reset();

            if (Math.Abs(axis.Minimum - oldMinimum) > double.Epsilon)
            {
                axis.Minimum -= minimumOffset;
            }

            if (Math.Abs(axis.Maximum - oldMaxium) > double.Epsilon)
            {
                axis.Maximum += maximumOffset;
            }
        }
    }

    public class ValueDescription
    {
        public Enum Value { get; set; }
        public string Description { get; set; }
    }
}
