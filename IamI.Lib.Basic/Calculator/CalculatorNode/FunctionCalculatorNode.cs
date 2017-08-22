using System;

namespace IamI.Lib.Basic.Calculator.CalculatorNode
{
    public class FunctionCalculatorNode : CalculatorNode
    {
        public static readonly string[] LegalFunctions = {"sin", "cos", "ln", "lg", "exp", "sqrt", "trun", "abs"};
        public override int ChildNodeCount => 1;
        public string Function { get; set; } = "";

        public override double Resolve(params double[] parameters)
        {
            return ResolveFunction(parameters[0], Function);
        }
        
        public override string ToString()
        {
            return $"[{Function}]";
        }

        /// <summary>
        /// 根据函数名和参数，计算函数结果。
        /// </summary>
        /// <param name="value">函数的参数</param>
        /// <param name="function">函数名</param>
        /// <returns>计算结果</returns>
        public static double ResolveFunction(double value, string function)
        {
            switch (function)
            {
                case "sin": return Math.Sin(value);
                case "cos": return Math.Cos(value);
                case "ln": return Math.Log(value);
                case "lg": return Math.Log10(value);
                case "exp": return Math.Exp(value);
                case "sqrt": return Math.Sqrt(value);
                case "trun": return Math.Truncate(value);
                case "abs": return Math.Abs(value);
                default: return 0;
            }
        }

    }
}