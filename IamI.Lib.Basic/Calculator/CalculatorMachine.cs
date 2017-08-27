using System;
using System.Collections.Generic;
using System.Linq;

namespace IamI.Lib.Basic.Calculator
{
    /// <summary>
    /// 对计算器的封装。
    /// 是对包含变量的情形的一个方便的封装。
    /// </summary>
    public class CalculatorMachine
    {
        /// <summary>
        /// 此计算器中所包含的前序表达式。
        /// </summary>
        public PreorderExpression Expression { get; internal set; }

        private CalculatorMachine() { }

        /// <summary>
        /// 按照给定的多个名称，设置变量节点的值。
        /// 此为对 Expression.SetVariables 的转发。
        /// </summary>
        /// <param name="variables">变量节点的名称和变量节点的值的对应表。</param>
        public void SetVariables(Dictionary<string, double> variables) { Expression.SetVariables(variables); }

        /// <summary>
        /// 依次设置变量节点，忽略那些变量节点的名称。
        /// 此为对 Expression.SetVariables 的转发。
        /// </summary>
        /// <param name="variables">变量值列表。</param>
        public void SetVariables(IEnumerable<double> variables) { Expression.SetVariables(variables); }

        /// <summary>
        /// 结算此计算器。
        /// </summary>
        /// <returns>结算结果</returns>
        public double Resolve()
        {
            var expression_copy = new Stack<CalculatorNode.CalculatorNode>(Expression.Nodes.Reverse());
            var calculating_node = new Stack<double>();
            while (expression_copy.Count > 0) calculating_node.Push(expression_copy.Pop().Resolve(calculating_node));
            return calculating_node.Pop();
        }

        /// <summary>
        /// 依次设置给定名称的变量，然后结算此计算器。
        /// </summary>
        /// <param name="variables">变量节点的名称和变量节点的值的对应表</param>
        /// <returns>结算结果</returns>
        public double Resolve(Dictionary<string, double> variables)
        {
            SetVariables(variables);
            return Resolve();
        }

        /// <summary>
        /// 依次设置变量节点，然后结算此计算器。
        /// </summary>
        /// <param name="variables">变量值列表</param>
        /// <returns>结算结果</returns>
        public double Resolve(IEnumerable<double> variables)
        {
            SetVariables(variables);
            return Resolve();
        }

        /// <summary>
        /// 依次设置变量节点，然后结算此计算器。
        /// </summary>
        /// <param name="value1">第一个变量</param>
        /// <param name="other_variables">其他变量的变量值列表</param>
        /// <returns>结算结果</returns>
        public double Resolve(double value1, IEnumerable<double> other_variables)
        {
            var variables = new List<double> {value1};
            variables.AddRange(other_variables);
            return Resolve(variables);
        }

        /// <summary>
        /// 依次设置变量节点，然后结算此计算器。
        /// 此为对 Resolve 的分类转发。
        /// </summary>
        /// <param name="variables">变量值列表</param>
        public double this[params double[] variables] => variables.Length == 0 ? Resolve() : Resolve(variables);

        /// <summary>
        /// 依次设置给定名称的变量，然后结算此计算器。
        /// 此为对 Resolve 的转发。
        /// </summary>
        /// <param name="variables">变量节点的名称和变量节点的值的对应表</param>
        public double this[Dictionary<string, double> variables] => Resolve(variables);

        /// <summary>
        /// 从前序表达式构造计算器。
        /// </summary>
        /// <param name="expression">前序表达式</param>
        /// <returns>对应的计算器封装</returns>
        public static CalculatorMachine FromPreorderExpression(PreorderExpression expression) { return new CalculatorMachine {Expression = expression}; }
    }
}