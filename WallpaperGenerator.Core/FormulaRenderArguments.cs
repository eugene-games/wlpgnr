﻿using System;
using WallpaperGenerator.FormulaRendering;
using WallpaperGenerator.Formulas;

namespace WallpaperGenerator.UI.Core
{
    public class FormulaRenderArguments
    {
        public FormulaTree FormulaTree { get; private set; }
        public RangesForFormula2DProjection Ranges { get; private set; }
        public ColorTransformation ColorTransformation { get; private set; }
        public int WidthInPixels { get; private set; }
        public int HeightInPixels { get; private set; }

        public FormulaRenderArguments(FormulaTree formulaTree, RangesForFormula2DProjection ranges, ColorTransformation colorTransformation)
        {
            FormulaTree = formulaTree;
            Ranges = ranges;
            ColorTransformation = colorTransformation;
            WidthInPixels = ranges.XCount;
            HeightInPixels = ranges.YCount;
        }

        public override string ToString()
        {            
            string formulaString = FormulaTreeSerializer.Serialize(FormulaTree);
            string variableRangesString = Ranges.ToString();
            string colorTransformationString = ColorTransformation.ToString();
            string[] lines = { variableRangesString, colorTransformationString, formulaString };
            return string.Join("\r\n", lines);
        }

        public static FormulaRenderArguments FromString(string value)
        {
            string[] lines = value.Split(new[] { "\r\n" }, 3, StringSplitOptions.RemoveEmptyEntries);
            RangesForFormula2DProjection ranges = RangesForFormula2DProjection.FromString(lines[0]);
            ColorTransformation colorTransformation = ColorTransformation.FromString(lines[1]);
            FormulaTree formulaTree = FormulaTreeSerializer.Deserialize(lines[2]);
            return new FormulaRenderArguments(formulaTree, ranges, colorTransformation);
        }
    }
}
