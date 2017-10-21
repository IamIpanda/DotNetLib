using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace IamI.Lib.WPF
{
    /// <summary>
    /// 窗口全屏控制类。
    /// </summary>
    public static class FullScreen
    {
        private static Window _full_window;
        private static WindowState _full_window_state;
        private static WindowStyle _full_window_style;
        private static bool _full_window_top_most;
        private static ResizeMode _full_window_resize_mode;
        private static Rect _full_window_rect;

        /// <summary>
        /// 进入全屏
        /// </summary>
        /// <param name="window"></param>
        /// <param name="save_message">指示是否保存窗口位置信息</param>
        public static void GoFullscreen(this Window window, bool save_message = true)
        {
            //已经是全屏
            if (window.IsFullscreen()) return;

            //存储窗体信息
            _full_window_state = window.WindowState;
            _full_window_style = window.WindowStyle;
            _full_window_top_most = window.Topmost;
            if (save_message)
            {
                _full_window_resize_mode = window.ResizeMode;
                _full_window_rect.X = window.Left;
                _full_window_rect.Y = window.Top;
                _full_window_rect.Width = window.Width;
                _full_window_rect.Height = window.Height;
            }
            else
            {
                _full_window_resize_mode = ResizeMode.NoResize;
                _full_window_rect = Rect.Empty;
            }

            //变成无边窗体
            window.WindowState = WindowState.Normal;//假如已经是Maximized，就不能进入全屏，所以这里先调整状态
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.Topmost = true; //最大化后总是在最上面

            //获取窗口句柄
            var handle = new WindowInteropHelper(window).Handle;
            //获取当前显示器屏幕
            var screen = Screen.FromHandle(handle);

            //调整窗口最大化,全屏的关键代码就是下面3句
            window.MaxWidth = screen.Bounds.Width;
            window.MaxHeight = screen.Bounds.Height;
            window.WindowState = WindowState.Maximized;

            //解决切换应用程序的问题
            window.Activated += Window_Activated;
            window.Deactivated += Window_Deactivated;

            //记住成功最大化的窗体
            _full_window = window;
        }

        private static void Window_Deactivated(object sender, EventArgs e)
        {
            if (sender is Window window) window.Topmost = false;
        }

        private static void Window_Activated(object sender, EventArgs e)
        {
            if (sender is Window window) window.Topmost = true;
        }

        /// <summary>
        /// 退出全屏
        /// </summary>
        /// <param name="window"></param>
        /// <param name="load_message">指示是否读取窗口信息。</param>
        public static void ExitFullscreen(this Window window, bool load_message = true)
        {
            //已经不是全屏无操作
            if (!window.IsFullscreen()) return;
            //恢复窗口先前信息，这样就退出了全屏
            window.Topmost = _full_window_top_most;
            window.WindowStyle = _full_window_style;
            window.ResizeMode = ResizeMode.CanResize;//设置为可调整窗体大小
            if (load_message)
            {
                if (_full_window_rect == Rect.Empty)
                    Basic.Log.Logger.Default.Warning("Fullscreen Want to load window message while nothing in it.");
                else
                {
                    window.Left = _full_window_rect.Left;
                    window.Width = _full_window_rect.Width;
                    window.Top = _full_window_rect.Top;
                    window.Height = _full_window_rect.Height;
                    window.WindowState = _full_window_state;//恢复窗口状态信息    
                }
            }
            window.ResizeMode = _full_window_resize_mode;//恢复窗口可调整信息

            //移除不需要的事件
            window.Activated -= Window_Activated;
            window.Deactivated -= Window_Deactivated;
            _full_window = null;
        }

        /// <summary>
        /// 返回一个值，指示窗体是否在全屏状态
        /// </summary>
        /// <param name="window">要获得状态的窗口。</param>
        /// <returns></returns>
        public static bool IsFullscreen(this Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            return Equals(_full_window, window);
        }
    }
}