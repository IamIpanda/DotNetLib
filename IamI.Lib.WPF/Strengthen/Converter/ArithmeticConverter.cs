using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace IamI.Lib.WPF.Strengthen.Converter
{
    /// <summary>
    /// 对值进行简单一步加减乘除的变换类。
    /// </summary>
    public class ArithmeticConverter : IValueConverter
    {
        private const string ArithmeticParseExpression = "([+\\-*/]{1,1})\\s{0,}(\\-?[\\d\\.]+)";
        private readonly Regex arithmeticRegex = new Regex(ArithmeticParseExpression);
        
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double) || parameter == null) return null;
            var param = parameter.ToString();

            if (param.Length <= 0) return null;
            var match = arithmeticRegex.Match(param);
            if (match.Groups.Count != 3) return null;
            var operation = match.Groups[1].Value.Trim();
            var numericValue = match.Groups[2].Value;
            
            if (!double.TryParse(numericValue, out var number)) return null;
            var valueAsDouble = (double)value;

            switch (operation)
            {
                case "+":
                    return valueAsDouble + number;
                case "-":
                    return valueAsDouble - number;
                case "*":
                    return valueAsDouble * number;
                case "/":
                    return valueAsDouble / number;
                default:
                    return valueAsDouble;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}