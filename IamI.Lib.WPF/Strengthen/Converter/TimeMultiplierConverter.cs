using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using IamI.Lib.Basic.Time;

namespace IamI.Lib.WPF.Strengthen.Converter
{
    public class TimeMultiplierConverter : IValueConverter
    {
        /// <summary>
        /// 基准单位时间
        /// </summary>
        public TimeSpan BasicTimeSpan { get; set; } = new TimeSpan(0, 0, 0, 0, 200);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return new TimeSpan(0);
            var param = parameter.ToString();
            var factor = int.Parse(param);
            return Times(factor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 返回基准时间 X 给定因子
        /// </summary>
        /// <param name="factor">乘数因子</param>
        /// <returns>乘积时间</returns>
        public virtual TimeSpan Times(int factor)
        {
            return BasicTimeSpan.Times(factor);
        }
        
        /// <summary>
        /// Times 的转发
        /// </summary>
        /// <param name="factor">乘数因子</param>
        /// <returns>乘积时间</returns>
        public TimeSpan this[int factor] => Times(factor);
    }
}