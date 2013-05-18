﻿namespace WallpaperGenerator.Formulas.Operators.Arithmetic
{
    public class Sub : BinaryOperator
    {
        protected override double EvaluateCore(params double[] operands)
        {
            return operands[0] - operands[1];
        }
    }
}
