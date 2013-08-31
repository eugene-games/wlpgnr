﻿using System;
using System.Collections.Generic;
using System.Linq;
using WallpaperGenerator.Utilities;
using System.Globalization;
using WallpaperGenerator.Formulas.Operators;
using WallpaperGenerator.Utilities.DataStructures.Trees;

namespace WallpaperGenerator.Formulas.FormulaTreeGeneration
{
    public class FormulaTreeGenerator
    {
        private readonly Random _random;
        private readonly FormulaTreeNodeFactory _formulaTreeNodeFactory;
        private readonly OperatorsAndOperandsConstantAcceptanceRules _operatorsAndOperandsConstantAcceptanceRules;

        public FormulaTreeGenerator(Random random)
            : this(random, new FormulaTreeNodeFactory(random), new DefaultOperatorsAndOperandsConstantAcceptanceRules())
        {
        }

        public FormulaTreeGenerator(Random random, FormulaTreeNodeFactory formulaTreeNodeFactory,
            OperatorsAndOperandsConstantAcceptanceRules operatorsAndOperandsConstantAcceptanceRules)
        {
            _random = random;
            _formulaTreeNodeFactory = formulaTreeNodeFactory;
            _operatorsAndOperandsConstantAcceptanceRules = operatorsAndOperandsConstantAcceptanceRules;
        }

        public TreeNode<Operator> CreateRandomFormulaTree(int dimensionsCount, int variablesCount, int constantsCount, 
            int unaryOperatorsCountForFormulaDiluting, IEnumerable<Operator> operatorsLibrary)
        {
            if (dimensionsCount < 0)
                throw new ArgumentException("Dimensions count can't be less then 0.");

            if (variablesCount < dimensionsCount)
                throw new ArgumentException("Variables count can't be less then dimensions count.");

            if (constantsCount < 0)
                throw new ArgumentException("Constants count can't be less then 0.");

            if (unaryOperatorsCountForFormulaDiluting < 0)
                throw new ArgumentException("Unary operators count can't be less then 0.");

            if (variablesCount + constantsCount < 0)
                throw new ArgumentException("Sum of variables count and constants count can't be less then zero.");

            if (!operatorsLibrary.Any())
                throw new ArgumentException("Operators library can't be empty.");
            
            if (variablesCount + constantsCount > 1 && operatorsLibrary.All(op => op.Arity != 2))
                throw new ArgumentException("If sum of variables count and constants more than 1 then operators library should contain binary operators.");

            if (unaryOperatorsCountForFormulaDiluting > 0 && operatorsLibrary.All(op => op.Arity != 1))
                throw new ArgumentException("If unary operators count for formula diluting is more then 0 then operators library should contina unary operators");

            return CreateRandomFormulaTreeCore(dimensionsCount, variablesCount, constantsCount, unaryOperatorsCountForFormulaDiluting, operatorsLibrary);
        }

        private TreeNode<Operator> CreateRandomFormulaTreeCore(int dimensionsCount, int variablesCount, int constantsCount, 
            int unaryOperatorsCountForFormulaDiluting, IEnumerable<Operator> operatorsLibrary)
        {                        
            int zeroOperatorsCount = variablesCount + constantsCount;
            int availableBinaryOperatorsCount = operatorsLibrary.Count(op => op.Arity == 2);
            int availableTernaryOperatorsCount = operatorsLibrary.Count(op => op.Arity == 3);
            double ternaryVsBinaryOperatorOccurenceProbability = (double)availableTernaryOperatorsCount /
                (availableBinaryOperatorsCount + availableTernaryOperatorsCount);

            int[] operatorsAritySequence = GetNonZeroOperatorsAritySequence(zeroOperatorsCount,
                unaryOperatorsCountForFormulaDiluting, ternaryVsBinaryOperatorOccurenceProbability).ToArray();
            IEnumerable<Operator> nonZeroArityOperators = operatorsAritySequence.Select(a => operatorsLibrary.Where(op => op.Arity == a).TakeRandom(_random));

            IEnumerable<string> variableNames = EnumerableExtensions.Repeat(i => "x" + i.ToString(CultureInfo.InvariantCulture), dimensionsCount);
            IEnumerable<Operator> availableVariables = variableNames.Select(n => new Variable(n)).Cast<Operator>().ToArray();
            IEnumerable<Operator> variablesPart = EnumerableExtensions.Repeat(i => availableVariables.TakeRandom(_random), variablesCount - dimensionsCount);
            IEnumerable<Operator> variables = availableVariables.Concat(variablesPart);

            IEnumerable<Operator> constants = EnumerableExtensions.Repeat(i => (Operator)(new Constant(_random.Next(1, 20))), constantsCount);
            IEnumerable<Operator> zeroArityOperators = variables.Concat(constants).Randomize(_random);

            return CreateFormulaTree(zeroArityOperators, nonZeroArityOperators);
        }

        public TreeNode<Operator> CreateFormulaTree(IEnumerable<Operator> zeroArityOperators, IEnumerable<Operator> nonZeroArityOperators)
        {
            if (!zeroArityOperators.Any())
                throw new ArgumentException("Zero-arity operators enumeration can't be empty.");
            
            // TODO: Count this for operators of any arity.
            if (nonZeroArityOperators.Count(op => op.Arity == 2)+ 
                nonZeroArityOperators.Count(op => op.Arity == 3)*2 + 1 != zeroArityOperators.Count())
                throw new ArgumentException("Number of zero and non-zero -arity operators is not balanced.");

            Queue<TreeNode<Operator>> nodes = new Queue<TreeNode<Operator>>(zeroArityOperators.Select(op => new TreeNode<Operator>(op)));
            foreach (Operator op in nonZeroArityOperators)
            {
                TreeNode<Operator> node = _formulaTreeNodeFactory.Create(op, nodes.Dequeue(op.Arity));
                nodes.Enqueue(node);
            }

            return nodes.Dequeue();
        }

        public IEnumerable<int> GetNonZeroOperatorsAritySequence(int zeroArityOperatorsCount, int unaryOperatorsCount, 
            double ternaryVsBinaryOperatorOccurenceProbability)
        {
            if (zeroArityOperatorsCount < 1)
                throw new ArgumentException("Zero arity operators can't be less then 1.");

            if (unaryOperatorsCount < 0)
                throw new ArgumentException("Zero arity operators can't be less then 0.");

            if (ternaryVsBinaryOperatorOccurenceProbability > 0.99)
                throw new ArgumentException("Ternary VS binary operator occurence probability can't be more then 0.99.");

            while (zeroArityOperatorsCount > 1)
            {
                double unaryOperatorOccurenceProbability = (double)unaryOperatorsCount / (zeroArityOperatorsCount + unaryOperatorsCount);
                int arity = _random.GetRandomBetweenTwo(2, 1, unaryOperatorOccurenceProbability);
                if (arity == 2 && zeroArityOperatorsCount > 2)
                {
                    arity = _random.GetRandomBetweenTwo(2, 3, ternaryVsBinaryOperatorOccurenceProbability);
                }
                   
                yield return arity;
                zeroArityOperatorsCount -= (arity - 1);
                if (arity == 1)
                    unaryOperatorsCount--;
            }

            foreach (int i in Enumerable.Repeat(1, unaryOperatorsCount))
                yield return i;
        }
    }
}
