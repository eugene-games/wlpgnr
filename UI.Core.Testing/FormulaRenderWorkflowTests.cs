﻿using System;
using System.Collections.Generic;
using MbUnit.Framework;
using Moq;
using WallpaperGenerator.Formulas;
using WallpaperGenerator.Formulas.Operators;
using WallpaperGenerator.Utilities;
using WallpaperGenerator.Utilities.Testing;

namespace WallpaperGenerator.UI.Core.Testing
{
    [TestFixture]
    public class FormulaRenderWorkflowTests
    {
        private Random _random;
        private FormulaRenderWorkflow _workflow;

        [SetUp]
        public void SetUp()
        {
            _random = RandomMock.Setup(EnumerableExtensions.Repeat(i => i * 0.1, 10));
            FormulaRenderArgumentsGenerationParams generationParams = new FormulaRenderArgumentsGenerationParams
            {
                DimensionCountBounds = new Bounds<int>(2,3),
                MinimalDepthBounds = new Bounds<int>(4, 4),
                OperatorAndMaxProbabilityBoundsMap = new Dictionary<Operator, Bounds>{ {OperatorsLibrary.Sum, new Bounds(1, 1)}, {OperatorsLibrary.Sin, new Bounds(1, 1)} },
                FirstPredefinedFormulaRenderingArgumentsCount = 0
            };

            Mock<FormulaGoodnessAnalyzer> formulaGoodnessAnalyzerMock = new Mock<FormulaGoodnessAnalyzer>(0, 0);
            formulaGoodnessAnalyzerMock.Setup(fa => fa.Analyze(It.IsAny<FormulaTree>())).Returns(false);

            _workflow = new FormulaRenderWorkflow(generationParams, new Size(3, 3), formulaGoodnessAnalyzerMock.Object, _random);
        }

        [Test]
        public void TestGenerateFormulaRenderArguments()
        {
            FormulaRenderArguments args = _workflow.GenerateFormulaRenderArguments();
            const string expectedArgsString = "-3.2,0;9.6,25.6\r\n0,0,0,0;0,0,0,0;0,0.72,1.92,0\r\nSum Sum Sin x0 Sum x1 x1 Sum Sin x0 Sum x1 x1";
            Assert.AreEqual(expectedArgsString, args.ToString());
            Assert.AreEqual(expectedArgsString, _workflow.FormulaRenderArguments.ToString());
        }

        [Test]
        public void TestChangeColors()
        {
            _workflow.GenerateFormulaRenderArguments();
            FormulaRenderArguments args = _workflow.ChangeColors();
            const string expectedArgsString = "-3.2,0;9.6,25.6\r\n0,0,0,0;0,0.72,1.92,0;0,0,0,0\r\nSum Sum Sin x0 Sum x1 x1 Sum Sin x0 Sum x1 x1";
            Assert.AreEqual(expectedArgsString, args.ToString());
            Assert.AreEqual(expectedArgsString, _workflow.FormulaRenderArguments.ToString());
        }

        [Test]
        public void TestTransformRanges()
        {
            _workflow.GenerateFormulaRenderArguments();
            FormulaRenderArguments args = _workflow.TransformRanges();
            const string expectedArgsString = "-0.24,0.76;-3.6,1.68\r\n0,0,0,0;0,0,0,0;0,0.72,1.92,0\r\nSum Sum Sin x0 Sum x1 x1 Sum Sin x0 Sum x1 x1";
            Assert.AreEqual(expectedArgsString, args.ToString());
            Assert.AreEqual(expectedArgsString, _workflow.FormulaRenderArguments.ToString());
        }

        [Test]
        public void TestRenderFormula()
        {
            Assert.IsFalse(_workflow.IsImageReady);
            FormulaRenderResult formulaRenderResult = _workflow.RenderFormula(true);
            Assert.IsNotNull(_workflow.FormulaRenderArguments);
            Assert.IsNotNull(formulaRenderResult.Image);
            Assert.AreNotEqual(TimeSpan.Zero, formulaRenderResult.ElapsedTime);
        }

        [Test]
        public void TestState()
        {
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            _workflow.GenerateFormulaRenderArguments();
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            _workflow.ChangeColors();
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            _workflow.TransformRanges();
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            FormulaRenderResult formulaRenderResult = _workflow.RenderFormula(false);
            Assert.IsNotNull(formulaRenderResult);
            Assert.IsFalse(_workflow.IsImageRendering);
            Assert.IsTrue(_workflow.IsImageReady);
            _workflow.ChangeColors();
            Assert.IsTrue(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            _workflow.TransformRanges();
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
            formulaRenderResult = _workflow.RenderFormula(false);
            Assert.IsNotNull(formulaRenderResult);
            _workflow.GenerateFormulaRenderArguments();
            Assert.IsFalse(_workflow.IsImageReady);
            Assert.IsFalse(_workflow.IsImageRendering);
        }

        [Test]
        public void TestGenerateFormulaRenderArgumentsWithPredefinedFormulas()
        {
            _workflow.GenerationParams.FirstPredefinedFormulaRenderingArgumentsCount = 1;
            _workflow.GenerationParams.RepeatPredefinedFormulaRenderingArgumentsAfterEvery = 2;
            _workflow.GenerationParams.PredefinedFormulaRenderingFormulaRenderArgumentStrings = new[] {
@"0,10;0,10
0,0,0,0;0,0,0,0;1,1,1,0
Sum Cos x Sin y"};

            FormulaRenderArguments args = _workflow.GenerateFormulaRenderArguments();
            Assert.AreEqual("-0.24,0.76;-3.6,1.68\r\n0,0,0,0;0,0,0,0;1,1,1,0\r\nSum Cos x Sin y", args.ToString());

            args = _workflow.GenerateFormulaRenderArguments();
            Assert.AreEqual("-18,8.4;-20,20\r\n2.52,-5.4,-0.36,0.09;1.2,3.2,0,0.06;0.4,1.68,-3.6,0.03\r\nSum Sum Sin x0 Sum x1 x1 Sin Sin x0", args.ToString());

            args = _workflow.GenerateFormulaRenderArguments();
            Assert.AreEqual("1.2,5.04;-10.8,-0.72\r\n0.4,1.68,-3.6,0.03;0,0.72,1.92,0;0,0,0,0\r\nSum Cos x Sin y", args.ToString());

            args = _workflow.GenerateFormulaRenderArguments();
            Assert.AreEqual("-2.56,0\r\n1.2,3.2,0,0.06;0.4,1.68,-3.6,0.03;0,0.72,1.92,0\r\nSum Sin Sin x1 4", args.ToString());
        }
    }
}
