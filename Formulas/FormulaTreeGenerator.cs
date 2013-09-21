﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WallpaperGenerator.Formulas.Operators;
using WallpaperGenerator.Formulas.Operators.Arithmetic;
using WallpaperGenerator.Formulas.Operators.Conditionals;
using WallpaperGenerator.Formulas.Operators.Trigonometric;
using WallpaperGenerator.Utilities;
using WallpaperGenerator.Utilities.DataStructures.Trees;
using WallpaperGenerator.Utilities.DataStructures.Trees.TreeGenerating;
using WallpaperGenerator.Utilities.FormalGrammar;
using WallpaperGenerator.Utilities.FormalGrammar.RuleSelectors;
using WallpaperGenerator.Utilities.FormalGrammar.Rules;

namespace WallpaperGenerator.Formulas
{
    public static class FormulaTreeGenerator
    {
        public static FormulaTree Generate(IDictionary<Operator, double> operatorAndProbabilityMap, Func<double> createConstant, int dimensionsCount, int minimalDepth,
            Random random, double constOrVarProbability, double constantProbability, IDictionary<int, double> arityAndOpNodeProbabiltyMap)
        {
            IEnumerable<string> variableNames = EnumerableExtensions.Repeat(i => "x" + i.ToString(CultureInfo.InvariantCulture), dimensionsCount);
            IEnumerable<Operator> variables = variableNames.Select(n => new Variable(n));
            variables.ForEach(v => operatorAndProbabilityMap.Add(v, 1));
            return Generate(operatorAndProbabilityMap, createConstant, minimalDepth, random, constOrVarProbability, constantProbability, arityAndOpNodeProbabiltyMap);
        }

        public static FormulaTree Generate(IDictionary<Operator, double> operatorAndProbabilityMap, Func<double> createConstant, int minimalDepth, Random random,
            double constOrVarProbability, double constantProbability, IDictionary<int, double> arityAndOpNodeProbabiltyMap)
        {
            Grammar<Operator> grammar = CreateGrammar(operatorAndProbabilityMap, createConstant, minimalDepth, random, constOrVarProbability, constantProbability, arityAndOpNodeProbabiltyMap); 
            TreeNode<Operator> treeRoot = TreeGenerator.Generate(grammar, "OpNode", op => op.Arity);
            return new FormulaTree(treeRoot);
        }

        public static Grammar<Operator> CreateGrammar(IDictionary<Operator, double> operatorAndProbabilityMap, Func<double> createConstant, int minimalDepth, Random random,
            double constOrVarProbability, double constantProbability, IDictionary<int, double> arityAndOpNodeProbabiltyMap)
        {
            List<Operator> operatorsWithZeroProbability = operatorAndProbabilityMap.Where(e => e.Value.Equals(0)).Select(e => e.Key).ToList();
            foreach (Operator op in operatorsWithZeroProbability)
            {
                operatorAndProbabilityMap.Remove(op);
            }

            IEnumerable<Operator> operators = operatorAndProbabilityMap.Keys;

            if (!operators.OfType<Variable>().Any())
                throw new ArgumentException("Operators should have at least one variable.", "operatorAndProbabilityMap");

            // V -> x1|x2|...d
            // C -> c1|c2|...
            // Op0Node -> V|C

            // AbsNode -> abs OpNode
            // SqrtNode -> sqrt OpNode
            // CbrtNode -> cbrt OpNode
            // SinNode -> sin OpNode
            // CosNode -> cos OpNode
            // AtanNode -> atan OpNode
            // TanhNode -> tanh OpNode
            // InfGuard -> atan|tanh
            // Pow2Node -> InfGuard pow2 OpNode
            // Pow3Node -> InfGuard pow3 OpNode
            // LnNode -> InfGuard ln OpNode
            // SinhNode -> InfGuard sinh OpNode
            // CoshNode -> InfGuard cosh OpNode
            // Op1Node -> AbsNode|SqrtNode|CbrtNode|SinNode|CosNode|AtanNode|TanhNode|Pow2Node|Pow3Node|LnNode|SinhNode|CoshNode

            // OpOrOp0NodeOperands -> (OpNode Op0Node)|(Op0Node OpNode)
            // OpOrVNodeOperands -> (OpNode OpOrVNode)|(OpOrVNode OpNode)
            // OpOrVNode -> OpNode|V
            // RegOp2Operands -> (OpNode OpNode)|OpOrOp0NodeOperands
            // SumNode -> sum RegOp2Operands
            // SubNode -> sub RegOp2Operands
            // MulNode -> mul RegOp2Operands
            // DivNode -> (InfGuard div OpNode OpNode)|(div OpOrOp0NodeOperands)
            // PowNode -> (InfGuard pow RegOp2Operands)|(pow (OpNode InfGuard Op0Node)|(Op0Node InfGuard OpNode))
            // MaxNode -> max (OpNode OpOrVNode)|(OpOrVNode OpNode)
            // ModNode -> mod (OpNode sum abs OpNode 0.01)|OpOrOp0NodeOperands
            // Op2Node -> DivNode|PowNode|MaxNode|ModNode

            // Ifg0Node -> ifg0 OpNode OpNode OpNode
            // Op3Node -> Ifg0Node

            // IfgNode -> ifg OpNode OpNode OpNode OpNode  
            // Op4Node -> IfgNode

            // OpNode -> V|Op1Node|Op2Node|Op3Node|Op4Node

            SymbolsSet<Operator> s = CreateSymbols(operators);

            IEnumerable<Symbol<Operator>> opArityNodeSymbols = GetOpArityNodeSymbolNames(operators).Select(n => s[n]).ToArray();
            IEnumerable<double> opNodesProbabilities = NormalizeOpNodeProbabilities(operators, arityAndOpNodeProbabiltyMap);
            Func<IEnumerable<Rule<Operator>>, RuleSelector<Operator>> createConstOrVarRuleSelector = 
                rs => new RandomRuleSelector<Operator>(random, rs, new[] { 1 - constOrVarProbability, constOrVarProbability });
            List<Rule<Operator>> rules = new List<Rule<Operator>>
            {
                new Rule<Operator>(s["C"], () => new[] { new Symbol<Operator>(new Constant(createConstant()))}),
                new OrRule<Operator>(s["V"], rs => new RandomRuleSelector<Operator>(random, rs), operators.OfType<Variable>().Select(v => s[v.Name])),
                
                new OrRule<Operator>(s["Op0Node"], 
                    rs => new RandomRuleSelector<Operator>(random, rs, new[] { 1 - constantProbability, constantProbability }),
                    new []{ s["V"], s["C"] }), 

                new OrRule<Operator>(s["InfGuard"], rs => new RandomRuleSelector<Operator>(random, rs),
                    new []{s[GetOpSymbolName(OperatorsLibrary.Atan)], s[GetOpSymbolName(OperatorsLibrary.Tanh)]}), 
                
                // OpOrVNodeOperands -> (OpNode OpOrVNode)|(OpOrVNode OpNode)
                new OrRule<Operator>(s["OpOrVNodeOperands"], 
                    rs => new RandomRuleSelector<Operator>(random, rs),
                    new Rule<Operator>(new[]{s["OpNode"], s["OpOrVNode"]}), 
                    new Rule<Operator>(new[]{s["OpOrVNode"], s["OpNode"]})),

                // OpOrOp0NodeOperands -> (OpNode Op0Node)|(Op0Node OpNode)
                new OrRule<Operator>(s["OpOrOp0NodeOperands"], 
                    rs => new RandomRuleSelector<Operator>(random, rs),
                    new Rule<Operator>(new[]{s["OpNode"], s["Op0Node"]}), 
                    new Rule<Operator>(new[]{s["Op0Node"], s["OpNode"]})),

                // OpOrVNode -> OpNode|V
                new OrRule<Operator>(s["OpOrVNode"], 
                    createConstOrVarRuleSelector,
                    new[]{s["OpNode"], s["V"]}),

                // RegOp2Operands -> (OpNode OpNode)|OpOrOp0NodeOperands
                new OrRule<Operator>(s["RegOp2Operands"], 
                    createConstOrVarRuleSelector,
                    new Rule<Operator>(new[]{s["OpNode"], s["OpNode"]}), 
                    new Rule<Operator>(new[]{s["OpOrOp0NodeOperands"]})), 
                
                new OrRule<Operator>(s["OpNode"], 
                    rs => new TreeGeneratingRuleSelector<Operator>(minimalDepth, rs, rls => new RandomRuleSelector<Operator>(random, rls, opNodesProbabilities)),
                    new []{ s["V"] }.Concat(opArityNodeSymbols.Skip(1))), 
            };

            // AbsNode -> abs OpNode, 
            // ...
            rules.AddRange(CreateOpNodeRules(operators, 
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s[GetOpSymbolName(op)], s["OpNode"] }),
                typeof(Abs), typeof(Sqrt), typeof(Cbrt), typeof(Sin), typeof(Cos), typeof(Atan), typeof(Tanh)));

            // Pow2Node -> InfGuard pow2 OpNode
            // ...
            rules.AddRange(CreateOpNodeRules(operators, 
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s["InfGuard"], s[GetOpSymbolName(op)], s["OpNode"] }),
                typeof(Pow2), typeof(Pow3), typeof(Ln), typeof(Sinh), typeof(Cosh)));

            // SumNode -> sum RegOp2Operands
            // ...
            rules.AddRange(CreateOpNodeRules(operators, 
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s[GetOpSymbolName(op)], s["RegOp2Operands"] }),
                typeof(Sum), typeof(Sub), typeof(Mul)));

            // DivNode -> (InfGuard div OpNode OpNode)|(div OpOrOp0NodeOperands)
            rules.AddRange(CreateOpNodeRules(operators,
                op =>
                    new OrRule<Operator>(s[GetOpNodeSymbolName(op)],
                        createConstOrVarRuleSelector,
                        new Rule<Operator>(new[] { s["InfGuard"], s[GetOpSymbolName(op)], s["OpNode"], s["OpNode"] }),
                        new Rule<Operator>(new[] { s[GetOpSymbolName(op)], s["OpOrOp0NodeOperands"] })),
                typeof(Div)));

            // PowNode -> (InfGuard pow RegOp2Operands)|(pow (OpNode InfGuard Op0Node)|(Op0Node InfGuard OpNode))
            rules.AddRange(CreateOpNodeRules(operators,
                op => new OrRule<Operator>(s[GetOpNodeSymbolName(op)],
                    createConstOrVarRuleSelector,
                    new Rule<Operator>(new[] { s["InfGuard"], s[GetOpSymbolName(op)], s["RegOp2Operands"] }),
                    new AndRule<Operator>(new Rule<Operator>(new[]{s[GetOpSymbolName(op)]}),
                        new OrRule<Operator>(rs => new RandomRuleSelector<Operator>(random, rs),
                            new Rule<Operator>(new[] { s["OpNode"], s["InfGuard"], s["Op0Node"] }),
                            new Rule<Operator>(new[] { s["Op0Node"], s["InfGuard"], s["OpNode"] })))),
                typeof(Pow)));

            // ModNode -> mod (OpNode sum abs OpNode 0.01)|OpOrOp0NodeOperands
            rules.AddRange(CreateOpNodeRules(operators,
                op => new AndRule<Operator>(s[GetOpNodeSymbolName(op)], 
                    new Rule<Operator>(new[] { s[GetOpSymbolName(op)] }),
                    new OrRule<Operator>(createConstOrVarRuleSelector,
                        new Rule<Operator>(new[] { s["OpNode"], s[GetOpSymbolName(OperatorsLibrary.Sum)], s[GetOpSymbolName(OperatorsLibrary.Abs)], s["OpNode"], new Symbol<Operator>(new Constant(0.01))}),
                        new Rule<Operator>(new[] { s["OpOrOp0NodeOperands"] }))),
                typeof(Mod)));

            // MaxNode -> max OpOrVNodeOperands
            rules.AddRange(CreateOpNodeRules(operators,
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s[GetOpSymbolName(op)], s["OpOrVNodeOperands"] }),
                typeof(Max)));

            // Ifg0Node -> ifg0 OpNode OpNode OpNode
            rules.AddRange(CreateOpNodeRules(operators,
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s[GetOpSymbolName(op)], s["OpNode"], s["OpNode"], s["OpNode"] }),
                typeof(IfG0)));

            // IfgNode -> ifg OpNode OpNode OpNode OpNode 
            rules.AddRange(CreateOpNodeRules(operators,
                op => new Rule<Operator>(s[GetOpNodeSymbolName(op)], new[] { s[GetOpSymbolName(op)], s["OpNode"], s["OpNode"], s["OpNode"], s["OpNode"] }),
                typeof(IfG)));

            // Op1Node -> AbsNode|SqrtNode|...
            // Op2Node -> DivNode|PowNode|...
            // ...
            IEnumerable<IGrouping<int, Operator>> operatorsByArity = operators.GroupBy(op => op.Arity).Where(g => g.Key > 0 && g.Any());
            rules.AddRange(operatorsByArity.Select(g =>
                new OrRule<Operator>(s[GetOpArityNodeSymbolName(g.Key)], 
                    rls => new RandomRuleSelector<Operator>(random, rls, g.Select(op => operatorAndProbabilityMap[op])), 
                    g.Select(op => s[GetOpNodeSymbolName(op)]))));
            
            return new Grammar<Operator>(rules);
        }

        private static IEnumerable<Rule<Operator>> CreateOpNodeRules(IEnumerable<Operator> operators,
            Func<Operator, Rule<Operator>> createRule, params Type[] operatorTypes)
        {
            IEnumerable<Operator> availableOperators = operators.Where(op => operatorTypes.Contains(op.GetType()));
            return availableOperators.Select(createRule);
        }
       
        private static SymbolsSet<Operator> CreateSymbols(IEnumerable<Operator> operators)
        {
            List<string> nonTerminalsNames = new List<string> 
            { 
                "V", "C", "InfGuard", "RegOp2Operands", "OpOrOp0NodeOperands", "OpOrVNodeOperands", "OpOrVNode", "OpNode"
            };

            // Op0Node, Op1Node, ...
            nonTerminalsNames.AddRange(GetOpArityNodeSymbolNames(operators));

            // SumNode, SinNode, ...
            nonTerminalsNames.AddRange(operators.Where(op => op.Arity > 0).Select(GetOpNodeSymbolName));
            
            IEnumerable<Operator> terminalGuards = new Operator[] {OperatorsLibrary.Atan, OperatorsLibrary.Tanh, OperatorsLibrary.Sum, OperatorsLibrary.Abs};
            IEnumerable<Symbol<Operator>> terminals = operators.Concat(terminalGuards).Distinct().Select(op => new Symbol<Operator>(op, GetOpSymbolName(op)));
            IEnumerable<Symbol<Operator>> nonTerminals = nonTerminalsNames.Select(n => new Symbol<Operator>(n));
            return new SymbolsSet<Operator> { terminals, nonTerminals };
        }

        private static IEnumerable<string> GetOpArityNodeSymbolNames(IEnumerable<Operator> operators)
        {
            IEnumerable<int> availableArities = operators.Select(op => op.Arity).Distinct().OrderBy(a => a);
            return availableArities.Select(GetOpArityNodeSymbolName);
        }

        private static string GetOpArityNodeSymbolName(int arity)
        {
            return "Op" + arity + "Node";
        }

        private static string GetOpNodeSymbolName(Operator op)
        {
            return GetOpSymbolName(op) + "Node";
        }

        private static string GetOpSymbolName(Operator op)
        {
            return op.Name;
        }

        public static IEnumerable<double> NormalizeOpNodeProbabilities(IEnumerable<Operator> operators, 
            IEnumerable<KeyValuePair<int, double>> arityAndProbabiltyMap)
        {
            IEnumerable<double> probabilities = arityAndProbabiltyMap.Where(e => operators.Any(op => op.Arity == e.Key)).Select(e => e.Value);
            return MathUtilities.Normalize(probabilities);
        }
    }
}
