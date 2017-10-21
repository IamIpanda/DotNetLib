using System;
using System.Collections.Generic;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.Basic.Calculator.CalculatorNode
{
    /// <summary>
    ///     计算器中的节点的抽象。
    /// </summary>
    public abstract class CalculatorNode : ICloneable
    {
        /// <summary>
        ///     指示该节点的结算需要多少个参数。
        /// </summary>
        public virtual int ChildNodeCount { get; } = 0;

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        ///     根据给定的值栈，结算此节点。
        /// </summary>
        /// <param name="doubleStack">计算根据的值栈</param>
        /// <returns>节点结算的结果</returns>
        public virtual double Resolve(Stack<double> doubleStack)
        {
            if (doubleStack.Count < ChildNodeCount)
            {
                Logger.Default.Warning(
                    "The value stack doesn't contains enough values to resolve. NaN would be returned.");
                return double.NaN;
            }
            var ParamNodes = new double[ChildNodeCount];
            for (var i = 0; i < ChildNodeCount; i++) ParamNodes[i] = doubleStack.Pop();
            return Resolve(ParamNodes);
        }

        /// <summary>
        ///     根据给定的参数，结算此节点。
        ///     在派生类中重写。
        /// </summary>
        /// <param name="parameters">计算使用的参数</param>
        /// <returns>节点计算的结果</returns>
        public abstract double Resolve(params double[] parameters);
    }
}