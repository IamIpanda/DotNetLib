using System.Collections.Generic;
using System.Linq;
using IamI.Lib.Basic.Calculator.CalculatorNode;

namespace IamI.Lib.Basic.Calculator
{
    /// <summary>
    /// ǰ����ʽ�ķ�װ��
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
        /// �������б����ڵ㡣
        /// </summary>
        protected void BuildVariableRelationship()
        {
            Variables.Clear();
            Nodes.OfType<VariableCalculatorNode>().ToList().ForEach(node => Variables.Add(node.VariableName, node));
        }

        /// <summary>
        /// �����б����ڵ�ȫ������Ϊ0��
        /// </summary>
        public void InitializeVariables() { Nodes.OfType<VariableCalculatorNode>().ToList().ForEach(node => node.Value = 0); }

        /// <summary>
        /// ���ո��������ƣ����ñ����ڵ��ֵ��
        /// </summary>
        /// <param name="variable_name">�����ڵ������</param>
        /// <param name="value">�����ڵ��ֵ</param>
        public void SetVariable(string variable_name, double value)
        {
            if (Variables.ContainsKey(variable_name)) Variables[variable_name].Value = value;
            else Log.Logger.Default.Warning($"Trying to set a variable named {variable_name} = {value} in calculator, but it doesn't exists. Nothing done.");
        }

        /// <summary>
        /// ���ո����Ķ�����ƣ����ñ����ڵ��ֵ��
        /// </summary>
        /// <param name="values">�����ڵ�����ƺͱ����ڵ��ֵ�Ķ�Ӧ��</param>
        public void SetVariables(Dictionary<string, double> values)
        {
            InitializeVariables();
            values.ToList().ForEach(key_value_pair => this[key_value_pair.Key] = key_value_pair.Value);
        }

        /// <summary>
        /// �������ñ����ڵ㡣
        /// ������Щ�����ڵ�����ơ�
        /// </summary>
        /// <param name="values">����ֵ�б�</param>
        public void SetVariables(IEnumerable<double> values)
        {
            var variable_node_list = Variables.Values.ToList();
            var value_list = values.ToList();
            for (var i = 0; i < variable_node_list.Count && i < value_list.Count; i++) variable_node_list[i].Value = value_list[i];
        }

        /// <summary>
        /// ���ո��������ƣ����ñ����ڵ��ֵ��
        /// �÷����� @SetVariable ��ת����
        /// </summary>
        /// <param name="variable_name">�����ڵ������</param>
        public double this[string variable_name] { set { SetVariable(variable_name, value); } }

        /// <summary>
        /// ����ǰ����ʽת��Ϊ��������
        /// </summary>
        /// <returns>��ǰ����ʽ�ļ�������װ��</returns>
        public CalculatorMachine ToCalculateMachine() { return CalculatorMachine.FromPreorderExpression(this); }

        /// <summary>
        /// ��һ��������ʽת��Ϊ�沨�����ʽ��
        /// </summary>
        /// <param name="sequential_expression">Ҫת����������ʽ</param>
        /// <returns>��Ӧ���沨�����ʽ�ṹ</returns>
        /// <remarks>
        /// ����׺���ʽת���ɺ�׺���ʽ�㷨��
        /// 1����������ɨ��һ��׺���ʽ��
        /// 2 ������ȡ���ǲ����������жϸò����������ͣ������ò����������������ջ
        /// 3������ȡ���������
        /// (1) �������Ϊ������"("����ֱ�Ӵ����������ջ��
        /// (2) �������Ϊ������")"��������������ջ�е����������������ջ��ֱ������������Ϊֹ��
        /// (3) �������Ϊ�������������
        /// (a) ���������ջջ���������Ϊ���ţ���ֱ�Ӵ����������ջ��
        /// (b) �����������ջջ������������ȼ��߻���ȣ���ֱ�Ӵ����������ջ��
        /// (c) �����������ջջ������������ȼ��ͣ������ջ�����������������ջ��������ǰ�����ѹ���������ջ��
        /// 4�������ʽ��ȡ��ɺ��������ջ�����������ʱ��������ȡ�����������������ջ��ֱ���������ջΪ�ա�
        /// </remarks>
        public static PreorderExpression FromSeqientialExpression(SequentialExpression sequential_expression)
        {
            var operator_stack = new Stack<CalculatorNode.CalculatorNode>();
            var number_stack = new Stack<CalculatorNode.CalculatorNode>();
            operator_stack.Push(new OperatorCalculatorNode() {Operator = ' '});
            foreach (var node in sequential_expression.Nodes)
            {
                // Ϊ��ʵ�ֺ�������������ܣ�����������һ�����������
                if (node is OperatorCalculatorNode || node is FunctionCalculatorNode)
                {
                    // ���ں�������������ԣ�������һ������������������Ǻ������������Ҫ��
                    // �������������ȼ���Ч��
                    var node_operator = (node as OperatorCalculatorNode)?.Operator ?? ' ';
                    switch (node_operator)
                    {
                        // ������ֱѹջ
                        case '(':
                            operator_stack.Push(node);
                            break;
                        // �����ŵ�ջֱ������������Ϊֹ��
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
                        // ������ʱ
                        default:
                            var operator_stack_top_node = operator_stack.Peek();
                            var current_node_rank = node.Rank();
                            var top_node_rank = operator_stack_top_node.Rank();
                            // ջ��Ϊ�����Ż�����ȼ��ĳ��ϣ�ֱѹջ
                            if (operator_stack_top_node.IsLeftBracket() || current_node_rank >= top_node_rank) operator_stack.Push(node);
                            // ���򣬵�ջֱ������������Ȼ��ѹջ
                            else
                            {
                                while (operator_stack.Peek().Rank() >= current_node_rank) number_stack.Push(operator_stack.Pop());
                                operator_stack.Push(node);
                            }
                            break;
                    }
                }
                else
                    // ������ֱѹջ
                    number_stack.Push(node);
            }
            // ʣ��Ĳ�����ѹջ
            // ReSharper disable once InvertIf
            if (operator_stack.Count != 0)
                // �������и��ղ�����������ջ
                while (operator_stack.Count > 1) number_stack.Push(operator_stack.Pop());
            return new PreorderExpression(new Stack<CalculatorNode.CalculatorNode>(number_stack));
        }
    }
}