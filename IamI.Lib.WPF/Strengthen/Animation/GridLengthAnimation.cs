using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace IamI.Lib.WPF.Strengthen.Animation
{
    /// <summary>
    ///     对 Gridlength 作动画变换的类。
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength),
            typeof(GridLengthAnimation));

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength),
            typeof(GridLengthAnimation));

        public override Type TargetPropertyType => typeof(GridLength);

        public GridLength From
        {
            get => (GridLength) GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength To
        {
            get => (GridLength) GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object default_origin_value, object default_destination_value, AnimationClock animation_clock)
        {
            var from_val = ((GridLength) GetValue(FromProperty)).Value;
            if (from_val == 1.0)
                from_val = ((GridLength) default_origin_value).Value;
            var to_val = ((GridLength) GetValue(ToProperty)).Value;
            // 若根本不在动画中，返回初始值
            if (animation_clock.CurrentProgress == null) return From;
            // 动画结算
            return from_val > to_val
                ? new GridLength((1 - animation_clock.CurrentProgress.Value) * (from_val - to_val) + to_val,
                    To.GridUnitType)
                : new GridLength(animation_clock.CurrentProgress.Value * (to_val - from_val) + from_val, To.GridUnitType);
        }
    }
}