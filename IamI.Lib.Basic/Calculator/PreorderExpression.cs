using System.Collections.Generic;
using System.Linq;
using IamI.Lib.Basic.Calculator.CalculatorNode;

namespace IamI.Lib.Basic.Calculator
{
    /// <summary>
    /// 前序表达式的封装类
    /// </summary>
    public class PreorderExpression
    {
        public Stack<CalculatorNode.CalculatorNode> Nodes { get; set; }

        public Dictionary<string, VariableCalculatorNode> Variables { get; } = new Dictionary<string, VariableCalculatorNode>();

        private PreorderExpression(Stack<CalculatorNode.CalculatorNode> nodes)
        {
            Nodes = nodes;
            BuildVariableRelationship();
        }

        /// <summary>
        /// 索引所有变量节点。
        /// </summary>
        protected void BuildVariableRelationship()
        {
            Variables.Clear();
            Nodes.OfType<VariableCalculatorNode>().ToList().ForEach(node => Variables.Add(node.VariableName, node));
        }

        /// <summary>
        /// 将所有变量节点全部设置为0。
        /// </summary>
        public void InitializeVariables() { Nodes.OfType<VariableCalculatorNode>().ToList().ForEach(node => node.Value = 0); }

        /// <summary>
        /// 按照给定的名称，设置变量节点的值。
        /// </summary>
        /// <param name="variable_name">变量节点的名称</param>
        /// <param name="value">变量节点的值</param>
        public void SetVariable(string variable_name, double value)
        {
            if (Variables.ContainsKey(variable_name)) Variables[variable_name].Value = value;
            else Log.Logger.Default.Warning($"Trying to set a variable named {variable_name} = {value} in calculator, but it doesn't exists. Nothing done.");
        }

        /// <summary>
        /// 按照给定的多个名称，设置变量节点的值。
        /// </summary>
        /// <param name="values">变量节点的名称和变量节点的值的对应表。</param>
        public void SetVariables(Dictionary<string, double> values)
        {
            InitializeVariables();
            values.ToList().ForEach(key_value_pair => this[key_value_pair.Key] = key_value_pair.Value);
        }

        /// <summary>
        /// 依次设置变量节点。
        /// 忽略那些变量节点的名称。
        /// </summary>
        /// <param name="values">变量值列表</param>
        public void SetVariables(IEnumerable<double> values)
        {
            var variable_node_list = Variables.Values.ToList();
            var value_list = values.ToList();
            for (var i = 0; i < variable_node_list.Count && i < value_list.Count; i++) variable_node_list[i].Value = value_list[i];
        }

        /// <summary>
        /// 按照给定的名称，设置变量节点的值。
        /// 该方法是 @SetVariable 的转发。
        /// </summary>
        /// <param name="variable_name">变量节点的名称</param>
        public double this[string variable_name] { set { SetVariable(variable_name, value); } }

        /// <summary>
        /// 将此前序表达式转化为计算器。
        /// </summary>
        /// <returns>此前序表达式的计算器封装，</returns>
        public CalculatorMachine ToCalculateMachine() { return CalculatorMachine.FromPreorderExpression(this); }

        /// <summary>
        /// 将一个中序表达式转化为逆波兰表达式。
        /// </summary>
        /// <param name="sequential_expression">要转化的中序表达式</param>
        /// <returns>对应的逆波兰表达式结构</returns>
        /// <remarks>
        /// 将中缀表达式转换成后缀表达式算法：
        /// 1、从左至右扫描一中缀表达式。
        /// 2 、若读取的是操作数，则判断该操作数的类型，并将该操作数存入操作数堆栈
        /// 3、若读取的是运算符
        /// (1) 该运算符为左括号"("，则直接存入运算符堆栈。
        /// (2) 该运算符为右括号")"，则输出运算符堆栈中的运算符到操作数堆栈，直到遇到左括号为止。
        /// (3) 该运算符为非括号运算符：
        /// (a) 若运算符堆栈栈顶的运算符为括号，则直接存入运算符堆栈。
        /// (b) 若比运算符堆栈栈顶的运算符优先级高或相等，则直接存入运算符堆栈。
        /// (c) 若比运算符堆栈栈顶的运算符优先级低，则输出栈顶运算符到操作数堆栈，并将当前运算符压入运算符堆栈。
        /// 4、当表达式读取完成后运算符堆栈中尚有运算符时，则依序取出运算符到操作数堆栈，直到运算符堆栈为空。
        /// </remarks>
        public static PreorderExpression FromSeqientialExpression(SequentialExpression sequential_expression)
        {
            var operator_stack = new Stack<CalculatorNode.CalculatorNode>();
            var number_stack = new Stack<CalculatorNode.CalculatorNode>();
            operator_stack.Push(new OperatorCalculatorNode() {Operator = ' '});
            foreach (var node in sequential_expression.Nodes)
            {
                // 为了实现函数类操作符功能，将函数当做一个运算符处理。
                if (node is OperatorCalculatorNode || node is FunctionCalculatorNode)
                {
                    // 对于函数类操作符而言，它仅是一个非括号运算符，而是何运算符并不重要。
                    // 而仅有它的优先级生效。
                    var node_operator = (node as OperatorCalculatorNode)?.Operator ?? ' ';
                    switch (node_operator)
                    {
                        // 左括号直压栈
                        case '(':
                            operator_stack.Push(node);
                            break;
                        // 右括号弹栈直到看见左括号为止。
                        case ')':
                            var last_node = operator_stack.Pop();
                            while (!last_node.IsLeftBracket())
                            {
                                number_stack.Push(last_node);
                                if (operator_stack.Count == 0)
                                {
                                    Log.Logger.Default.Warning("Not enough left bracket for right bracket.");
                                    break;
                                }
                                last_node = operator_stack.Pop();
                            }
                            break;
                        // 操作符时
                        default:
                            var operator_stack_top_node = operator_stack.Peek();
                            var current_node_rank = node.Rank();
                            var top_node_rank = operator_stack_top_node.Rank();
                            // 栈顶为左括号或高优先级的场合，直压栈
                            if (operator_stack_top_node.IsLeftBracket() || current_node_rank >= top_node_rank) operator_stack.Push(node);
                            // 否则，弹栈直到满足条件，然后压栈
                            else
                            {
                                while (operator_stack.Peek().Rank() >= current_node_rank) number_stack.Push(operator_stack.Pop());
                                operator_stack.Push(node);
                            }
                            break;
                    }
                }
                else
                    // 操作数直压栈
                    number_stack.Push(node);
            }
            // 剩余的操作符压栈
            // ReSharper disable once InvertIf
            if (operator_stack.Count != 0)
                // 最下面有个空操作符，不进栈
                while (operator_stack.Count > 1) number_stack.Push(operator_stack.Pop());
            return new PreorderExpression(new Stack<CalculatorNode.CalculatorNode>(number_stack));
        }
    }
}