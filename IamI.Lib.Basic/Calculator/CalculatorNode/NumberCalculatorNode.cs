namespace IamI.Lib.Basic.Calculator.CalculatorNode
{
    /// <summary>
    ///     数值节点
    /// </summary>
    public class NumberCalculatorNode : CalculatorNode
    {
        /// <summary>
        ///     该节点的值。
        /// </summary>
        public double Value { get; set; } = 0;

        public override double Resolve(params double[] parameters)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}