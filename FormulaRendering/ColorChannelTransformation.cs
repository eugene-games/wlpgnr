﻿using System;
using System.Linq;
using System.Globalization;
using WallpaperGenerator.Utilities;

namespace WallpaperGenerator.FormulaRendering
{
    public class ColorChannelTransformation
    {
        public double _polinomCoefficcientA;
        public double _polinomCoefficcientB;
        public double _polinomCoefficcientC;
        
        public Func<double, double> TransformationFunction { get; private set; }

        public double DispersionCoefficient { get; set; }

        public bool IsZero
        {
            get { return _polinomCoefficcientA.Equals(0) && _polinomCoefficcientB.Equals(0) && _polinomCoefficcientC.Equals(0); }
        }

        public ColorChannelTransformation(Func<double, double> transformationFunction, double dispersionCoefficient)
        {
            TransformationFunction = transformationFunction;
            DispersionCoefficient = dispersionCoefficient;
        }

        private ColorChannelTransformation(double a, double b, double c, double dispersionCoefficient)
        {
            _polinomCoefficcientA = a;
            _polinomCoefficcientB = b;
            _polinomCoefficcientC = c;
            TransformationFunction = v => v * v * v * a + v * v * b + v * c;
            DispersionCoefficient = dispersionCoefficient;
        }

        public static ColorChannelTransformation CreateRandomPolinomialChannelTransformation(Random random, Bounds coefficientBounds, double zeroChannelProbabilty)
        {
            double zeroChannel = random.NextDouble();
            if (zeroChannel < zeroChannelProbabilty)
                return new ColorChannelTransformation(0, 0, 0, 0);

            coefficientBounds = random.RandomlyShrinkBounds(coefficientBounds, 1);

            double a = Math.Round(random.NextDouble() * random.Next(coefficientBounds.Low, coefficientBounds.High), 2);
            double b = Math.Round(random.NextDouble() * random.Next(coefficientBounds.Low, coefficientBounds.High), 2);
            double c = Math.Round(random.NextDouble() * random.Next(coefficientBounds.Low, coefficientBounds.High), 2);

            if (a.Equals(0) && b.Equals(0) && c.Equals(0))
            {
                a = b = c = (coefficientBounds.High - coefficientBounds.Low) / 2.0;
            }

            double dispersionCoefficient = Math.Round(random.NextDouble() * 0.3, 2);
            return new ColorChannelTransformation(a, b, c, dispersionCoefficient);
        }

        public override string ToString()
        {
            double[] coefficcients = {_polinomCoefficcientA, _polinomCoefficcientB, _polinomCoefficcientC, DispersionCoefficient};
            return string.Join(",", coefficcients.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray());
        }

        public static ColorChannelTransformation FromString(string value)
        {
            string[] coeffficients = value.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            double a = DoubleUtilities.ParseInvariant(coeffficients[0]);
            double b = DoubleUtilities.ParseInvariant(coeffficients[1]);
            double c = DoubleUtilities.ParseInvariant(coeffficients[2]);
            double d = DoubleUtilities.ParseInvariant(coeffficients[3]);
            return new ColorChannelTransformation(a, b, c, d);
        }
    }
}
