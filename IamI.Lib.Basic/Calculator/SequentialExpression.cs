using System.Collections.Generic;
using System.Linq;
using System.Text;
using IamI.Lib.Basic.Calculator.CalculatorNode;

namespace IamI.Lib.Basic.Calculator
{
    /// <summary>
    /// 对中序表达式的封装。
    /// </summary>
    public class SequentialExpression
    {
        public List<CalculatorNode.CalculatorNode> Nodes { get; internal set; }

        private SequentialExpression() { }

        /// <summary>
        /// 将此中序表达式转化为前序表达式。
        /// 此为对 PreorderExpression.FromSeqientalExpression 的转发。
        /// </summary>
        /// <returns>所对应的前序表达式</returns>
        public PreorderExpression ToPreorderExpression() { return PreorderExpression.FromSeqientialExpression(this); }

        public override string ToString() { return string.Join(" ", Nodes.Select(node => node.ToString()).ToArray()); }

        /// <summary>
        /// 从字符串中初始化中序表达式。
        /// </summary>
        /// <param name="expression">表达式字符串</param>
        /// <returns>对应的中序表达式。</returns>
        public static SequentialExpression FromString(string expression)
        {
            expression = expression.Replace(" ", "");
            var charArray = expression.ToCharArray();
            var nodes = new List<CalculatorNode.CalculatorNode>();
            for (var index = 0; index < charArray.Length;)
            {
                var _char = charArray[index];
                if (_char >= '0' && _char <= '9')
                {
                    double value = _char - '0';
                    double decimal_point = 2;
                    while (++index < charArray.Length)
                    {
                        _char = charArray[index];
                        if (_char >= '0' && _char <= '9')
                            if (decimal_point > 1) value = value * 10 + _char - '0';
                            else value = value + (decimal_point /= 10);
                        else if (_char == '.') decimal_point = 1;
                        else break;
                    }
                    nodes.Add(new NumberCalculatorNode {Value = value});
                }
                else if (OperatorCalculatorNode.LegalOperators.Contains(_char))
                {
                    nodes.Add(new OperatorCalculatorNode {Operator = _char});
                    index++;
                }
                else if (IsValidVariableNameChar(_char))
                {
                    var builder = new StringBuilder(_char.ToString());
                    while (++index < charArray.Length)
                    {
                        _char = charArray[index];
                        if (IsValidVariableNameChar(_char)) builder.Append(_char);
                        else break;
                    }
                    var name = builder.ToString();
                    if (FunctionCalculatorNode.LegalFunctions.Contains(name)) nodes.Add(new FunctionCalculatorNode {Function = name});
                    else nodes.Add(new VariableCalculatorNode {VariableName = name});
                }
                else Log.Logger.Default.Warning($"Can't realize the char '{_char}' when analyze the expression \"{expression}\". Will be Ignored.");
            }
            return new SequentialExpression {Nodes = nodes};
        }

        /// <summary>
        /// 判断此字符是否一个合法的变量名。
        /// </summary>
        /// <param name="_char">变量名中所包含的字符</param>
        /// <returns></returns>
        public static bool IsValidVariableNameChar(char _char) { return _char >= 'a' && _char < 'z' || _char >= 'A' && _char <= 'Z' || _char >= '0' && _char <= '9' || _char == '_' || _char == '@'; }
    }
}