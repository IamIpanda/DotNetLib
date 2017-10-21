using System.Windows;
using System.Windows.Controls;

namespace IamI.Lib.WPF.Views
{
    /// <summary>
    ///     使用 GridLength 作为 Padding 的容器类。
    /// </summary>
    public class PaddingPanel : ContentControl
    {
        public static readonly DependencyProperty RowLengthProperty = DependencyProperty.Register(
            "RowLength", typeof(GridLength), typeof(PaddingPanel),
            new PropertyMetadata(new GridLength(10, GridUnitType.Star)));

        public static readonly DependencyProperty ColumnLengthProperty = DependencyProperty.Register(
            "ColumnLength", typeof(GridLength), typeof(PaddingPanel),
            new PropertyMetadata(new GridLength(10, GridUnitType.Star)));

        static PaddingPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(typeof(PaddingPanel)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        /// <summary>
        ///     获取或设置此控件的内含控件的高。
        /// </summary>
        public GridLength RowLength
        {
            get => (GridLength) GetValue(RowLengthProperty);
            set => SetValue(RowLengthProperty, value);
        }

        /// <summary>
        ///     获取或设置此控件的内含控件的宽。
        /// </summary>
        public GridLength ColumnLength
        {
            get => (GridLength) GetValue(ColumnLengthProperty);
            set => SetValue(ColumnLengthProperty, value);
        }
    }
}