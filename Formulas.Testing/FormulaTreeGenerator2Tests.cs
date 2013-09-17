﻿using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using WallpaperGenerator.Formulas.Operators;
using WallpaperGenerator.Utilities;
using WallpaperGenerator.Utilities.FormalGrammar;
using WallpaperGenerator.Utilities.Testing;

namespace WallpaperGenerator.Formulas.Testing
{
    [TestFixture]
    public class FormulaTreeGenerator2Tests
    {
        [Test]
        public void TestCreateGrammar()
        {
            TestCreateGrammar(new[] { new Variable("x"), new Variable("y") });
            TestCreateGrammar(new Operator[] { new Variable("x"), new Variable("y"), OperatorsLibrary.Abs, OperatorsLibrary.Sin });
            TestCreateGrammar(new Operator[] { new Variable("x"), OperatorsLibrary.Abs, OperatorsLibrary.Sin, OperatorsLibrary.Sum });
            TestCreateGrammar(new Operator[] { new Variable("x"), OperatorsLibrary.Abs, OperatorsLibrary.IfG0 });
            TestCreateGrammar(new Operator[] { new Variable("x"), OperatorsLibrary.IfG });
            TestCreateGrammar(new Operator[] { new Variable("x"), OperatorsLibrary.Abs, OperatorsLibrary.Sin, OperatorsLibrary.Sum });
            TestCreateGrammar(new Operator[] { new Variable("x"), OperatorsLibrary.Abs, OperatorsLibrary.Sin, OperatorsLibrary.Sum,
                OperatorsLibrary.Pow, OperatorsLibrary.IfG, OperatorsLibrary.Max, OperatorsLibrary.Mod, OperatorsLibrary.IfG0 });

            try
            {
                TestCreateGrammar(new Operator[] { OperatorsLibrary.Abs, OperatorsLibrary.Sin });
                Assert.Fail(typeof(ArgumentException).Name + " is expected.");
            }
            catch (ArgumentException)
            {
            }
        }

        private static void TestCreateGrammar(IEnumerable<Operator> operators)
        {
            IEnumerable<string> expectedFromSymbols = new [] { "V", "C", "InfGuard", "RegOp2Operands", "OpNode", "NoConstOpNode" };
            expectedFromSymbols = expectedFromSymbols.Concat(operators.GroupBy(op => op.Arity).Select(g => "Op" + g.Key + "Node"))
                .Concat(operators.Where(op => op.Arity > 0).Select(op => op.Name + "Node"));
            
            expectedFromSymbols = expectedFromSymbols.OrderBy(s => s);

            Random random = RandomMock.Setup(EnumerableExtensions.Repeat(i => i * 0.1, 10));
            IDictionary<int, double> arityAndOpNodesProbabilityMap = new Dictionary<int, double> { { 1, 0.4 }, { 2, 0.3 }, { 3, 0.2 }, { 4, 0.2 } };
            Grammar<Operator> grammar = FormulaTreeGenerator2.CreateGrammar(operators, () => 0, 1, random, 0.3, arityAndOpNodesProbabilityMap);
            IEnumerable<string> fromSymbols = grammar.Rules.Select(r => r.From.Name).OrderBy(s => s);
            CollectionAssert.AreEqual(expectedFromSymbols.ToArray(), fromSymbols.ToArray());
        }

        [Test]
        public void TestGenerate()
        {
            Func<double> createConstans = new EnumerableNext<double>(new double[] { 1, 2, 3 }.Repeat()).Next;

            TestGenerate(new[] { new Variable("x"), new Variable("y") }, createConstans, 3,
                "x");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), OperatorsLibrary.Abs, OperatorsLibrary.Sin }, createConstans, 5,
                "abs abs sin sin y");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), 
                OperatorsLibrary.Sum, OperatorsLibrary.Mul, OperatorsLibrary.Sin, OperatorsLibrary.Abs }, createConstans, 5,
                "sin sin abs mul y x");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), 
                OperatorsLibrary.Sum, OperatorsLibrary.Mul, OperatorsLibrary.Sin, OperatorsLibrary.Abs, OperatorsLibrary.IfG0 }, createConstans, 6,
                "sin sin abs mul ifg0 x x x abs y");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Pow, OperatorsLibrary.IfG }, createConstans, 6,
                "atan pow pow ifg atan pow y z ifg x x x x pow 1 tanh z atan pow y z tanh ifg x x x y pow ifg atan pow y z ifg x x x x pow 2 tanh z atan pow y z tanh ifg x x x y");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Div, OperatorsLibrary.Max }, createConstans, 4,
                    "atan div max y z max x x");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Mod}, createConstans, 2,
                    "mod x sum abs y 0.0001");
        }

        private static void TestGenerate(IEnumerable<Operator> operators, Func<double> createConstant,  int minimalTreeDepth, string expectedSerializedTree)
        {
            Random random = RandomMock.Setup(EnumerableExtensions.Repeat(i => i * 0.1, 10));
            IDictionary<int, double> arityAndOpNodesProbabilityMap = new Dictionary<int, double> { { 1, 0.4 }, { 2, 0.3 }, { 3, 0.2 }, { 4, 0.2 } };
            FormulaTree formulaTree = FormulaTreeGenerator2.Generate(operators, createConstant, minimalTreeDepth, random, 0.3, arityAndOpNodesProbabilityMap);
            Assert.AreEqual(expectedSerializedTree, FormulaTreeSerializer.Serialize(formulaTree).ToLower());
        }
    }
}
