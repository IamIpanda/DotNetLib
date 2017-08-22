using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace IamI.Lib.WPF.Strengthen.Animtion
{
    /// <summary>
    /// 对 Gridlength 作动画变换的类。
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);
        
        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

        public GridLength From
        {
            get { return (GridLength) GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));

        public GridLength To
        {
            get { return (GridLength) GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public override object GetCurrentValue(object defaultOriginValue,
            object defaultDestinationValue, AnimationClock animationClock)
        {
            var fromVal = ((GridLength)GetValue(FromProperty)).Value;
            if (fromVal == 1.0)
                fromVal = ((GridLength)defaultOriginValue).Value;
            var toVal = ((GridLength)GetValue(ToProperty)).Value;
            // 若根本不在动画中，返回初始值
            if (animationClock.CurrentProgress == null) return From;
            // 动画结算
            return fromVal > toVal
                ? new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, To.GridUnitType)
                : new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, To.GridUnitType);
        }
    }
}