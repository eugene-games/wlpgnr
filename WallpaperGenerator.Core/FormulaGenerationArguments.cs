﻿using System;
using System.Collections.Generic;
using System.Linq;
using WallpaperGenerator.Formulas;
using WallpaperGenerator.Utilities;

namespace WallpaperGenerator.UI.Core
{
    public class FormulaGenerationArguments
    {
        public int DimensionsCount { get; set; }
        public int MinimalDepth { get; set; }
        public double ConstantProbability { get; set; }
        public double LeafProbability { get; set; }
        public IDictionary<Operator, double> OperatorAndProbabilityMap { get; set; }
        public Func<double> CreateConstant { get; set; }

        public static FormulaGenerationArguments CreateRandom(Random random, Bounds<int> dimensionCountBounds, Bounds<int> minimalDepthBounds,
            Bounds leafProbabilityBounds, Bounds constantProbabilityBounds, Bounds constantBounds, IDictionary<Operator, Bounds> operatorAndMaxProbabilityBoundsMap,
            Bounds unaryVsBinaryOperatorsProbabilityBounds)
        {
            Dictionary<Operator, double> operatorAndProbabilityMap = operatorAndMaxProbabilityBoundsMap.ToDictionary(e => e.Key, e => random.Next(e.Value));
            double ubp = random.Next(unaryVsBinaryOperatorsProbabilityBounds);
            double ups = operatorAndProbabilityMap.Where(e => e.Key.Arity == 1).Sum(e => e.Value);
            double bps = operatorAndProbabilityMap.Where(e => e.Key.Arity == 2).Sum(e => e.Value);
            double correctionCoef = ubp / (1 - ubp) * bps / ups;
            operatorAndProbabilityMap.Select(e => e.Key).Where(op => op.Arity == 1).ToArray().
                ForEach(op => operatorAndProbabilityMap[op] *= correctionCoef);

            return new FormulaGenerationArguments
            {
                DimensionsCount = random.Next(dimensionCountBounds),
                MinimalDepth = random.Next(minimalDepthBounds),
                LeafProbability = random.Next(leafProbabilityBounds),
                ConstantProbability = random.Next(constantProbabilityBounds),
                OperatorAndProbabilityMap = operatorAndMaxProbabilityBoundsMap.ToDictionary(e => e.Key, e => random.Next(e.Value)),
                CreateConstant = () =>
                {
                    double c = Math.Round(random.Next(constantBounds), 2);
                    return Math.Abs(c - 0) < 0.01 ? 0.01 : c;
                }
            };
        }
    }
}
