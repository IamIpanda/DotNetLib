using System;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace IamI.Lib.Basic.Calculator.CalculatorNode
{
    /// <summary>
    /// 运算符节点。
    /// </summary>
    public class OperatorCalculatorNode : CalculatorNode
    {
        /// <summary>
        /// 所有合法运算符的集合。
        /// </summary>
        public static readonly char[] LegalOperators = {'+', '-', '*', '/', '%', '^', '(', ')', ','};

        public override int ChildNodeCount => 2;

        /// <summary>
        /// 该节点的运算符。
        /// </summary>
        public char Operator { get; set; } = ' ';

        /// <summary>
        /// 该节点运算符的优先级。
        /// </summary>
        public int Rank => OperatorRank(Operator);

        public override double Resolve(params double[] parameters) { return Calculate(parameters[0], parameters[1]); }

        /// <summary>
        /// 根据给定的参数，用本节点的运算符计算结果。
        /// </summary>
        /// <param name="x">运算参数 1</param>
        /// <param name="y">运算参数 2</param>
        /// <returns>计算结果</returns>
        public double Calculate(double x, double y) { return CalculateOperator(x, y, Operator); }

        public override string ToString() { return $"[{Operator}]"; }

        /// <summary>
        /// 根据给定的参数和运算符，计算结果。
        /// </summary>
        /// <param name="x">运算参数 1</param>
        /// <param name="y">运算参数 2</param>
        /// <param name="_operator">运算符</param>
        /// <returns>计算结果</returns>
        public static double CalculateOperator(double x, double y, char _operator)
        {
            if (_operator == '/' && y == 0 || _operator == '%' && y == 0) Log.Logger.Default.Warning("Divided by zero in calculator.");
            else if (_operator == '(' || _operator == ')') Log.Logger.Default.Warning("Trying to calculate a operator '(' or ')'.");
            switch (_operator)
            {
                case '+':
                    return x + y;
                case '-':
                    return y - x;
                case '*':
                    return x * y;
                case '/':
                    return y / x;
                case '%':
                    return y % x;
                case '^':
                    return Math.Pow(y, x);
                case '(':
                case ')':
                    return x;
                case ',':
                    return y;
                default:
                    Log.Logger.Default.Warning($"Unknown operator in calculator: {_operator}. Would return 0.");
                    return 0;
            }
        }

        /// <summary>
        /// 获取运算符所对应的优先级。
        /// </summary>
        /// <remarks>
        /// (，, 的优先级为 1。
        /// +, - 的优先级为 3。
        /// *，/，% 的优先级为 5。
        /// ^ 的优先级为 7。
        /// ) 的优先级为 8。
        /// 空格和其他字符的优先级为 0。
        /// </remarks>
        /// <param name="_operator">要获得优先级的运算符</param>
        /// <returns>运算符所对应的优先级。</returns>
        public static int OperatorRank(char _operator)
        {
            switch (_operator)
            {
                case '+':
                case '-':
                    return 3;
                case '*':
                case '/':
                case '%':
                    return 5;
                case '^':
                    return 7;
                case '(':
                case ',':
                    return 1;
                case ')':
                    return 8;
                case ' ':
                    return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 获取节点所对应的优先级。
        /// </summary>
        /// <remarks>
        /// 函数节点的优先级均为 8。
        /// 运算符节点的优先级为运算符对应的优先级。
        /// 其他节点的优先级均为 0。
        /// </remarks>
        /// <param name="node">要获得优先级的节点</param>
        /// <returns>节点所对应的优先级</returns>
        public static int OperatorRank(CalculatorNode node)
        {
            if (node is FunctionCalculatorNode) return 8;
            var operator_calculator_node = node as OperatorCalculatorNode;
            return operator_calculator_node != null ? OperatorRank(operator_calculator_node.Operator) : 0;
        }

        /// <summary>
        /// 判断此节点是否左括号。
        /// </summary>
        /// <param name="node">要判断的节点</param>
        /// <returns>一个值，指示此节点是否优先级。</returns>
        public static bool IsLeftBracket(CalculatorNode node)
        {
            var operator_calculator_node = node as OperatorCalculatorNode;
            return operator_calculator_node != null && operator_calculator_node.Operator == '(';
        }
    }

    public static class OperatorCalculatorNodeHelp
    {
        /// <summary>
        /// 获取节点所对应的优先级。
        /// 此为对 OperatorCalculator.OperatorRank 的转发。
        /// </summary>
        /// <param name="node">要获得优先级的节点</param>
        /// <returns>节点所对应的优先级</returns>
        public static int Rank(this CalculatorNode node) { return OperatorCalculatorNode.OperatorRank(node); }

        /// <summary>
        /// 判断此节点是否左括号。
        /// 此为对OperatorCalculator.IsLeftBracket的转发。
        /// </summary>
        /// <param name="node">要判断的节点</param>
        /// <returns>一个值，指示此节点是否优先级。</returns>
        public static bool IsLeftBracket(this CalculatorNode node) { return OperatorCalculatorNode.IsLeftBracket(node); }
    }
}