﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WallpaperGenerator.Utilities
{
    public static class MathUtilities
    {
        public static double MathExpectation(float[] values) 
        {
            double sum = values.Sum();
            return sum / values.Length;
        }

        public static double Variance(float[] values)
        {
            if (values.Length == 1)
                return 0;

            double sum = values.Sum();
            double sumOfSquares = values.Sum(t => t * t);

            return (sumOfSquares - sum * sum / values.Length) / (values.Length - 1);
        }

        public static double StandardDeviation(float[] values)
        {
            double varianse = Variance(values);
            return Math.Sqrt(varianse);
        }

        public static double Map(double value, double rangeStart, double rangeEnd, double range,
            double mappedRangeStart, double mappedRangeEnd, double mappedRange, double scale)
        {
            if (value < rangeStart)
                value = rangeStart;
            else if (value > rangeEnd)
                value = rangeEnd;

            return (value - rangeStart) * scale + mappedRangeStart;
        }

        public static IEnumerable<double> Normalize(IEnumerable<double> numbers)
        {
            double sum = numbers.Sum();
            return numbers.Select(n => n/sum);
        }

        //public static double Sum(double[] values, )
    }
}
