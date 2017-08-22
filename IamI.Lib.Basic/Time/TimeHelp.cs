using System;
using System.Collections.Generic;

namespace IamI.Lib.Basic.Time
{
    public static class TimeHelp
    {
        /// <summary>
        /// 分钟的字符后缀
        /// </summary>
        public static readonly List<string> minute_names = new List<string> { "m", "min", "mins", "minute", "minutes", "分", "分钟" };
        /// <summary>
        /// 秒钟的字符后缀
        /// </summary>
        public static readonly List<string> second_names = new List<string> { "s", "sec", "secs", "second", "seconds", "秒", "秒钟" };

        /// <summary>
        /// 把包含后缀在内的字符串转换为 Timespan。
        /// </summary>
        /// <param name="timespan_string">要转换的字符串</param>
        /// <returns>转换后的 Timespan</returns>
        public static TimeSpan ToTimespan(this string timespan_string)
        {
            var num = 0;
            timespan_string = timespan_string.Trim().ToLower();
            var second_name = second_names.Find((s) => timespan_string.EndsWith(s));
            if (second_name != null) timespan_string = timespan_string.Substring(0, timespan_string.Length - second_name.Length);
            timespan_string = timespan_string.Trim();
            var minute_name = minute_names.Find((m) => timespan_string.Contains(m));
            if (minute_name != null)
            {
                var components = timespan_string.Split(new [] { minute_name }, StringSplitOptions.None);
                num = Convert.ToInt32(components[0]) * 60 + (components.Length >= 2 ? (int.TryParse(components[1], out int i) ? i : 0) : 0);
            }
            else
                num = int.TryParse(timespan_string, out int j) ? j : 0;
            return TimeSpan.FromSeconds(num);
        }

        /// <summary>
        /// 基准时间阵列
        /// </summary>
        private static readonly List<TimeSpan> BasicTimes = new List<TimeSpan>
        {
            new TimeSpan(1, 0, 0, 0),
            new TimeSpan(0, 10, 0, 0),
            new TimeSpan(0, 1, 0, 0),
            new TimeSpan(0, 0, 10, 0),
            new TimeSpan(0, 0, 1, 0),
            new TimeSpan(0, 0, 0, 10),
            new TimeSpan(0, 0, 0, 1),
            new TimeSpan(1)
        };
        
        /// <summary>
        /// 将 Timespan 乘以某个值
        /// </summary>
        /// <param name="span">被乘 Timespan</param>
        /// <param name="factor">乘数</param>
        /// <returns>计算后的 Timespan</returns>
        public static TimeSpan Times(this TimeSpan span, int factor)
        {
            return new TimeSpan(span.Ticks * factor);
        }

        /// <summary>
        /// 获得 Timespan 的某一位。
        /// 第 0 位：日期
        /// 第 1 位：小时的十位
        /// 第 2 位：小时的个位
        /// 第 3 位：分钟的十位
        /// 第 4 位：分钟的个位
        /// 第 5 位：秒钟的十位
        /// 第 6 位：秒钟的个位
        /// 第 7 位：毫秒计数
        /// </summary>
        /// <param name="span"></param>
        /// <param name="bit">要获得的位数</param>
        /// <returns>所请求的位</returns>
        public static int GetBit(this TimeSpan span, int bit)
        {
            switch (bit)
            {
                case 0: return span.Days;
                case 1: return span.Hours / 10;
                case 2: return span.Hours % 10;
                case 3: return span.Minutes / 10;
                case 4: return span.Minutes % 10;
                case 5: return span.Seconds / 10;
                case 6: return span.Seconds % 10;
                case 7: return span.Milliseconds;
                default: return 0;
            }
        }

        /// <summary>
        /// 设置 Timespan 的某一位。
        /// </summary>
        /// <param name="span"></param>
        /// <param name="bit">要设置的位</param>
        /// <param name="value">要设置的值</param>
        /// <returns>设置后的 Timespan</returns>
        public static TimeSpan SetBit(this TimeSpan span, int bit, int value)
        {
            return span + BasicTimes[bit].Times(value - span.GetBit(bit));
        }
        
        /// <summary>
        /// 调整 Timespan 的某一位。
        /// 在原有的值上进行加和减。
        /// </summary>
        /// <param name="span"></param>
        /// <param name="bit">要调整的位</param>
        /// <param name="value">要加减的值</param>
        /// <returns>设置后的 Timespan</returns>
        public static TimeSpan AdjustBit(this TimeSpan span, int bit, int value)
        {
            var time = span + BasicTimes[bit].Times(value);
            return time.TotalSeconds < 0 ? TimeSpan.Zero : time;
        }

    }
}