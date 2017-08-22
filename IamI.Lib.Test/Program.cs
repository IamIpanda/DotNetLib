using System;
using System.Collections.Generic;
using IamI.Lib.Basic.Calculator;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Logger.Default.Debug("start");
            var machine = Calculator.GetCalculatorMachine("a ^ 2 + 6");
            for (var i = 2; i < 10; i++)
                Logger.Default.Debug(machine[i].ToString());

            Logger.Default.Info(Calculator.ResolveValue("2+3*4^5+sin(30)").ToString());
            Logger.Default.Debug("finished");
        }
    }
}