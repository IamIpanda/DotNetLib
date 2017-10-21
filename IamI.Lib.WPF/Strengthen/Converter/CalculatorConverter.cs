using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using IamI.Lib.Basic.Calculator;

namespace IamI.Lib.WPF.Strengthen.Converter
{
    public class CalculatorConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type target_type, object parameter, CultureInfo culture)
        {
            if (parameter == null || values == null) return 0;
            var calculator = Calculator.GetCalculatorMachine(parameter.ToString());
            var double_values = values.Select(value =>
                double.TryParse(value.ToString(), out var double_value) ? double_value : 0D);
            return calculator.Resolve(double_values);
        }

        public object[] ConvertBack(object value, Type[] target_types, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Calculator Converter doesn't support ConvertBack.");
        }

        public object Convert(object value, Type target_type, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return 0;
            var calculator = Calculator.GetCalculatorMachine(parameter.ToString());
            return double.TryParse(value.ToString(), out var param) ? 0 : calculator[param];
        }

        public object ConvertBack(object value, Type target_type, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Calculator Converter doesn't support ConvertBack.");
        }
    }
}