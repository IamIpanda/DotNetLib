using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IamI.Lib.WPF.Views
{
    public class PaddingPanel : ContentControl
    {
        static PaddingPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(typeof(PaddingPanel)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(PaddingPanel),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        public static readonly DependencyProperty RowLengthProperty = DependencyProperty.Register(
            "RowLength", typeof(GridLength), typeof(PaddingPanel),
            new PropertyMetadata(new GridLength(10, GridUnitType.Star)));

        /// <summary>
        /// 获取或设置此控件的内含控件的高。
        /// </summary>
        public GridLength RowLength
        {
            get { return (GridLength) GetValue(RowLengthProperty); }
            set { SetValue(RowLengthProperty, value); }
        }

        public static readonly DependencyProperty ColumnLengthProperty = DependencyProperty.Register(
            "ColumnLength", typeof(GridLength), typeof(PaddingPanel),
            new PropertyMetadata(new GridLength(10, GridUnitType.Star)));

        /// <summary>
        /// 获取或设置此控件的内含控件的宽。
        /// </summary>
        public GridLength ColumnLength
        {
            get { return (GridLength) GetValue(ColumnLengthProperty); }
            set { SetValue(ColumnLengthProperty, value); }
        }

    }
}