﻿using System.Collections.Generic;
using WallpaperGenerator.Formulas;
using WallpaperGenerator.Formulas.Operators;
using WallpaperGenerator.Utilities;

namespace WallpaperGenerator.UI.Core
{
    public class FormulaRenderArgumentsGenerationParams
    {
        public int WidthInPixels = 360;
        public int HeightInPixels = 640;
        public Bounds<int> DimensionCountBounds = new Bounds<int>(4, 15);
        public Bounds<int> MinimalDepthBounds = new Bounds<int>(8, 13);
        public Bounds ConstantBounds = new Bounds(-10, 10);
        public Bounds ConstantProbabilityBounds = new Bounds(0, 0.5);
        public Bounds LeafProbabilityBounds = new Bounds(0, 0.25);
        public Bounds RangeBounds = new Bounds(-40, 40);
        public Bounds ColorChannelPolinomialTransformationCoefficientBounds = new Bounds(-10, 10);
        public double ColorChannelZeroProbabilty = 0.1;
        public Bounds UnaryVsBinaryOperatorsProbabilityBounds = new Bounds(0.01, 0.01);

        public IDictionary<Operator, Bounds> OperatorAndMaxProbabilityBoundsMap = new Dictionary<Operator, Bounds>
        {
            {OperatorsLibrary.Sum, new Bounds(0, 1)},
            {OperatorsLibrary.Sub, new Bounds(0, 1)},
            //{OperatorsLibrary.Mul, 0.25},
            //{OperatorsLibrary.Div, 0.25},
            //{OperatorsLibrary.Max, 0.25},
            //{OperatorsLibrary.Pow, 0.2},

            //{OperatorsLibrary.Abs, 0.7},
            {OperatorsLibrary.Sin, new Bounds(0, 1)},
            {OperatorsLibrary.Cos, new Bounds(0, 1)},
            //{OperatorsLibrary.Atan, new Bounds(0, 1)},
            {OperatorsLibrary.Ln, new Bounds(0, 1)},
            //{OperatorsLibrary.Sqrt,0.25},
            {OperatorsLibrary.Cbrt,new Bounds(0, 1)},
            //{OperatorsLibrary.Pow2, 0.2},
            //{OperatorsLibrary.Pow3, 0.2},
            
        }; 
    }
}
