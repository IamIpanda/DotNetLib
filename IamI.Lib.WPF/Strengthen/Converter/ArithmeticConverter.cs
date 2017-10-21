using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace IamI.Lib.WPF.Strengthen.Converter
{
    /// <summary>
    ///     对值进行简单一步加减乘除的变换类。
    /// </summary>
    public class ArithmeticConverter : IValueConverter
    {
        private const string ARITHMETIC_PARSE_EXPRESSION = "([+\\-*/]{1,1})\\s{0,}(\\-?[\\d\\.]+)";
        private readonly Regex _arithmetic_regex = new Regex(ARITHMETIC_PARSE_EXPRESSION);

        object IValueConverter.Convert(object value, Type target_type, object parameter, CultureInfo culture)
        {
            if (!(value is double) || parameter == null) return null;
            var param = parameter.ToString();

            if (param.Length <= 0) return null;
            var match = _arithmetic_regex.Match(param);
            if (match.Groups.Count != 3) return null;
            var operation = match.Groups[1].Value.Trim();
            var numeric_value = match.Groups[2].Value;

            if (!double.TryParse(numeric_value, out var number)) return null;
            var value_as_double = (double) value;

            switch (operation)
            {
                case "+":
                    return value_as_double + number;
                case "-":
                    return value_as_double - number;
                case "*":
                    return value_as_double * number;
                case "/":
                    return value_as_double / number;
                default:
                    return value_as_double;
            }
        }

        object IValueConverter.ConvertBack(object value, Type target_type, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}