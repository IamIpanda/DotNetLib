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
        private static Window fullWindow;
        private static WindowState fullWindowState;
        private static WindowStyle fullWindowStyle;
        private static bool fullWindowTopMost;
        private static ResizeMode fullWindowResizeMode;
        private static Rect fullWindowRect;

        /// <summary>
        /// 进入全屏
        /// </summary>
        /// <param name="window"></param>
        /// <param name="saveMessage">指示是否保存窗口位置信息</param>
        public static void GoFullscreen(this Window window, bool saveMessage = true)
        {
            //已经是全屏
            if (window.IsFullscreen()) return;

            //存储窗体信息
            fullWindowState = window.WindowState;
            fullWindowStyle = window.WindowStyle;
            fullWindowTopMost = window.Topmost;
            if (saveMessage)
            {
                fullWindowResizeMode = window.ResizeMode;
                fullWindowRect.X = window.Left;
                fullWindowRect.Y = window.Top;
                fullWindowRect.Width = window.Width;
                fullWindowRect.Height = window.Height;
            }
            else
            {
                fullWindowResizeMode = ResizeMode.NoResize;
                fullWindowRect = Rect.Empty;
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
            fullWindow = window;
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
        /// <param name="loadMessage">指示是否读取窗口信息。</param>
        public static void ExitFullscreen(this Window window, bool loadMessage = true)
        {
            //已经不是全屏无操作
            if (!window.IsFullscreen()) return;
            //恢复窗口先前信息，这样就退出了全屏
            window.Topmost = fullWindowTopMost;
            window.WindowStyle = fullWindowStyle;
            window.ResizeMode = ResizeMode.CanResize;//设置为可调整窗体大小
            if (loadMessage)
            {
                if (fullWindowRect == Rect.Empty)
                    Basic.Log.Logger.Default.Warning("Fullscreen Want to load window message while nothing in it.");
                else
                {
                    window.Left = fullWindowRect.Left;
                    window.Width = fullWindowRect.Width;
                    window.Top = fullWindowRect.Top;
                    window.Height = fullWindowRect.Height;
                    window.WindowState = fullWindowState;//恢复窗口状态信息    
                }
            }
            window.ResizeMode = fullWindowResizeMode;//恢复窗口可调整信息

            //移除不需要的事件
            window.Activated -= Window_Activated;
            window.Deactivated -= Window_Deactivated;
            fullWindow = null;
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
            return Equals(fullWindow, window);
        }
    }
}