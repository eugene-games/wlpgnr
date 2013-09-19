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
    public class FormulaTreeGeneratorTests
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
            IEnumerable<string> expectedFromSymbols = new[] { "V", "C", "InfGuard", "RegOp2Operands", "OpNode", "OpOrConstOperands" };
            expectedFromSymbols = expectedFromSymbols.Concat(operators.GroupBy(op => op.Arity).Select(g => "Op" + g.Key + "Node"))
                .Concat(operators.Where(op => op.Arity > 0).Select(op => op.Name + "Node"));
            
            expectedFromSymbols = expectedFromSymbols.OrderBy(s => s);

            Random random = RandomMock.Setup(EnumerableExtensions.Repeat(i => i * 0.1, 10));
            IDictionary<int, double> arityAndOpNodesProbabilityMap = new Dictionary<int, double> { { 1, 0.4 }, { 2, 0.3 }, { 3, 0.2 }, { 4, 0.2 } };
            Grammar<Operator> grammar = FormulaTreeGenerator.CreateGrammar(operators, () => 0, 1, random, 0.3, arityAndOpNodesProbabilityMap);
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
                "sin sin abs mul x 1");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), 
                OperatorsLibrary.Sum, OperatorsLibrary.Mul, OperatorsLibrary.Sin, OperatorsLibrary.Abs, OperatorsLibrary.IfG0 }, createConstans, 6,
                "sin sin abs mul sin x 2");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Pow, OperatorsLibrary.IfG }, createConstans, 6,
                "atan pow pow ifg atan pow y y ifg z x x x tanh pow z 3 atan pow y y tanh 1 ifg atan pow y y ifg ifg x x x y pow 2 tanh z atan pow y y ifg z x x x tanh pow z 3 atan pow y y");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Div, OperatorsLibrary.Max }, createConstans, 4,
                    "atan div max y z max x x");

            TestGenerate(new Operator[] { new Variable("x"), new Variable("y"), new Variable("z"),
                OperatorsLibrary.Mod}, createConstans, 2,
                    "mod x sum abs y 0.01");
        }

        private static void TestGenerate(IEnumerable<Operator> operators, Func<double> createConstant,  int minimalTreeDepth, string expectedSerializedTree)
        {
            Random random = RandomMock.Setup(EnumerableExtensions.Repeat(i => i * 0.1, 10));
            IDictionary<int, double> arityAndOpNodesProbabilityMap = new Dictionary<int, double> { { 1, 0.4 }, { 2, 0.3 }, { 3, 0.2 }, { 4, 0.2 } };
            FormulaTree formulaTree = FormulaTreeGenerator.Generate(operators, createConstant, minimalTreeDepth, random, 0.3, arityAndOpNodesProbabilityMap);
            Assert.AreEqual(expectedSerializedTree, FormulaTreeSerializer.Serialize(formulaTree).ToLower());
        }

        [Test]
        public void TestNormalizeOpNodeProbabilities()
        {
            IEnumerable<Operator> operators = new Operator[] {OperatorsLibrary.Sin, OperatorsLibrary.Pow};
            IDictionary<int, double> arityAndProbabiltyMap = new Dictionary<int, double>{{1, 30}, {2, 20}, {3, 30}, {4, 10}};
            double[] expectedOpNodeProbabilities = new [] {0.6, 0.4};
            double[] opNodeProbabilities = FormulaTreeGenerator.NormalizeOpNodeProbabilities(operators, arityAndProbabiltyMap).ToArray();
            CollectionAssert.AreEqual(expectedOpNodeProbabilities, opNodeProbabilities);
        }
    }
}
