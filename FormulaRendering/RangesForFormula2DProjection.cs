﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WallpaperGenerator.Formulas;
using WallpaperGenerator.Utilities;

namespace WallpaperGenerator.FormulaRendering
{
    public class RangesForFormula2DProjection
    {
        public int XCount { get; private set; }

        public int YCount { get; private set; }

        public Range[] Ranges { get; private set; }

        public RangesForFormula2DProjection(int xCount, int yCount, IEnumerable<Range> ranges)
        {
            XCount = xCount;
            YCount = yCount;
            Ranges = ranges.Select((r, i) => new Range(r.Start, r.Step, i%2 == 0 ? XCount : YCount)).ToArray();
        }

        public override string ToString()
        {
            string[] countStrings = new [] { XCount.ToString(CultureInfo.InvariantCulture), YCount.ToString(CultureInfo.InvariantCulture) };
            IEnumerable<string> rangeStrings = Ranges.Select(r => r.ToString(true));
            return string.Join(";", countStrings.Concat(rangeStrings).ToArray());
        }

        public static RangesForFormula2DProjection FromString(string value)
        {
            string[] rangeStrings = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            int xCount = int.Parse(rangeStrings[0]);
            int yCount = int.Parse(rangeStrings[1]);
            IEnumerable<Range> ranges = rangeStrings.Skip(2).Select(Range.FromString);
            return new RangesForFormula2DProjection(xCount, yCount, ranges);
        }

        public static RangesForFormula2DProjection CreateRandom(Random random, int variableCount,
            int xRangeCount, int yRangeCount, int rangeLowBound, int rangeHighBound)
        {
            random.RandomlyShrinkBounds(ref rangeLowBound, ref rangeHighBound);
            IEnumerable<Range> ranges = Enumerable.Repeat(1, variableCount).
                Select(i => Range.CreateRandom(random, i % 2 == 0 ? xRangeCount : yRangeCount, rangeLowBound, rangeHighBound));
            return new RangesForFormula2DProjection(xRangeCount, yRangeCount, ranges);
        }
    }
}