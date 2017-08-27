namespace IamI.Lib.Basic.Calculator
{
    /// <summary>
    /// 方便调用计算器的类。
    /// </summary>
    public static class Calculator
    {
        /// <summary>
        /// 根据给定的字符串，创建一个计算器结构。
        /// </summary>
        /// <param name="expression_string">要转化为计算器的字符串</param>
        /// <returns>字符串对应的就计算器结构</returns>
        public static CalculatorMachine GetCalculatorMachine(string expression_string) { return SequentialExpression.FromString(expression_string).ToPreorderExpression().ToCalculateMachine(); }

        /// <summary>
        /// 立即计算给定的字符串值
        /// </summary>
        /// <param name="expression_string">要结算的字符串</param>
        /// <returns>字符串对应的值。</returns>
        public static double ResolveValue(string expression_string) { return GetCalculatorMachine(expression_string).Resolve(); }
    }
}