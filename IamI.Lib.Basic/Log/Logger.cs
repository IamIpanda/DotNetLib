using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace IamI.Lib.Basic.Log
{
    /// <summary>
    ///     记录日志所用的工具类。
    /// </summary>
    public class Logger
    {
        private readonly TextWriter _program_logger_text_writer;

        /// <summary>
        ///     创建记录器对象。
        ///     在Debug模式下，默认的等级为 LogLevel::Debug。
        ///     在Release模式下，默认的等级为 LogLevel::Info。
        /// </summary>
        /// <param name="output_name">日志文件的名称。如为空，此记录器不会将日志写进文件。</param>
        /// <param name="output_pattern">日志文件的后缀名。</param>
        public Logger(string output_name = "Log/Log", string output_pattern = ".log")
        {
            FileOutputName = output_name;
            FileOutputPattern = output_pattern;
#if DEBUG
            Level = LogLevel.Debug;
#else
            Level = LogLevel.Info;
#endif
            if (output_name == null) return;
            CreateLogFile();
            WriteLoggerHead();
            _program_logger_text_writer = new StreamWriter(FileOutputName + FileOutputPattern, true);
        }

        /// <summary>
        ///     获取或设置一个数值，指示日志文件的最大尺寸。
        /// </summary>
        public static long MaxLogSize { get; set; } = 5000000;

        /// <summary>
        ///     获取或设置此日志记录器的等级。
        ///     低于指定等级的日志会被忽略。
        /// </summary>
        public LogLevel Level { get; set; }

        public string FileOutputName { get; }
        public string FileOutputPattern { get; }

        /// <summary>
        ///     获取或设置显示的堆栈信息应向上追溯多少级。
        /// </summary>
        public int StackMessageLevel { get; set; } = 2;

        /// <summary>
        ///     获取或设置输出中的时间格式。
        /// </summary>
        public string TimeFormat { get; set; } = "yyyy-MM-dd hh:mm:ss.ffff";

        /// <summary>
        ///     获取或设置输出的整体样式。
        /// </summary>
        public string LineFormat { get; set; } = "[{0}][{1, 5}][{2}] {3}";

        /// <summary>
        ///     获取或设置记录器保存最近多少条日志。
        /// </summary>
        public int RecentMessageCount { get; set; } = 50;

        /// <summary>
        ///     获取最近由此对象记录的日志。
        /// </summary>
        public Queue<string> RecentMessage { get; } = new Queue<string>();

        /// <summary>
        ///     默认的记录器。
        /// </summary>
        public static Logger Default { get; } = new Logger();

        /// <summary>
        ///     每当此对象记录日志时触发。
        /// </summary>
        public event EventHandler<LoggerEventArgs> Logged;

        /// <summary>
        ///     创建日志文件。
        /// </summary>
        protected void CreateLogFile()
        {
            var path = Path.GetDirectoryName(FileOutputName);
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) dir.Create();

            if (!File.Exists(FileOutputName + FileOutputPattern))
                File.CreateText(FileOutputName + FileOutputPattern).Close();
            var info = new FileInfo(FileOutputName + FileOutputPattern);
            if (info.Length <= MaxLogSize) return;
            var ans = 1;
            while (File.Exists(FileOutputName + ans + FileOutputPattern)) ans++;
            info.CopyTo(FileOutputName + ans + FileOutputPattern);
            File.Create(FileOutputName + FileOutputPattern).Close();
        }

        /// <summary>
        ///     向日志文件写入文件头。
        /// </summary>
        protected void WriteLoggerHead()
        {
            _program_logger_text_writer?.WriteLine("");
            _program_logger_text_writer?.WriteLine("");
            _program_logger_text_writer?.WriteLine(
                "==================================================================");
            _program_logger_text_writer?.WriteLine(" Initialized On " + DateTime.Now);
            _program_logger_text_writer?.WriteLine(
                "==================================================================");
            _program_logger_text_writer?.Flush();
            System.Diagnostics.Debug.WriteLine("Logger started on " + DateTime.Now);
        }

        /// <summary>
        ///     格式化堆栈信息。
        /// </summary>
        /// <param name="frame">要格式化的堆栈页。</param>
        /// <returns>一个字符串，包含了格式化后的堆栈页信息。</returns>
        protected static string MarshalFrame(StackFrame frame)
        {
            return $"{Path.GetFileName(frame.GetFileName())}, L{frame.GetFileLineNumber()} in <{frame.GetMethod()}>";
        }

        /// <summary>
        ///     将日志入列保存。
        /// </summary>
        /// <param name="output_line">要保存的日志。</param>
        protected virtual void QueueMessage(string output_line)
        {
            RecentMessage.Enqueue(output_line);
            while (RecentMessage.Count > RecentMessageCount) RecentMessage.Dequeue();
        }

        /// <summary>
        ///     发送日志。
        /// </summary>
        /// <param name="message">日志的内容</param>
        /// <param name="log_level">日志的等级</param>
        /// <param name="stack_count">指出日志要向前追溯多少个调用栈页。</param>
        public void Log(string message, LogLevel log_level = LogLevel.Info, int stack_count = -1)
        {
            if (log_level < Level) return;
            if (stack_count < 0) stack_count = StackMessageLevel;
            var time_str = DateTime.Now.ToString(TimeFormat);
            var stack_str = MarshalFrame(new StackTrace(true).GetFrame(stack_count));
            var output_line = string.Format(LineFormat, time_str, log_level, stack_str, message);
            _program_logger_text_writer?.WriteLine(output_line);
            System.Diagnostics.Debug.WriteLine(output_line);
            QueueMessage(output_line);
            Logged?.Invoke(this, new LoggerEventArgs {Message = message, Level = log_level, Line = output_line});
        }


        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Debug。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Info。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Info(string message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Warning。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Warning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Warning。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Warn(string message)
        {
            Log(message, LogLevel.Warning);
        }

        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Error。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Error(string message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        ///     发送日志，并将等级标注为 LogLevel::Fatal。
        /// </summary>
        /// <param name="message">要发送的日志。</param>
        public void Fatal(string message)
        {
            Log(message, LogLevel.Fatal);
        }

        /// <summary>
        ///     强制把缓存中的消息写入到文件。
        /// </summary>
        public void Flush()
        {
            _program_logger_text_writer?.Flush();
        }

        /// <summary>
        ///     描述记录器消息的类。
        /// </summary>
        public class LoggerEventArgs : EventArgs
        {
            /// <summary>
            ///     发送给记录器的消息本体
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            ///     被打印在屏幕上的消息。
            /// </summary>
            public string Line { get; set; }

            /// <summary>
            ///     消息的等级。
            /// </summary>
            public LogLevel Level { get; set; }
        }
    }
}