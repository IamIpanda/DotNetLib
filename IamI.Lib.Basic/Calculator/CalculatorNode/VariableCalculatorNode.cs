namespace IamI.Lib.Basic.Calculator.CalculatorNode
{
    public class VariableCalculatorNode : CalculatorNode
    {
        public string VariableName { get; set; } = "";
        public double Value { get; set; } = 0d;
        public override double Resolve(params double[] parameters)
        {
            return Value;
        }

        public override string ToString()
        {
            return $"[{VariableName} = {Value}]";
        }
    }
}