﻿namespace WallpaperGenerator.Formulas.Operators.Arithmetic
{
    public class Pow2 : UnaryOperator
    {
        protected override double EvaluateCore(params double[] operands)
        {
            return operands[0] * operands[0];
        }
    }
}
