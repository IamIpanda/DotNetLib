using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace IamI.Lib.WPF.Resource
{
    public static class AnimationHelp
    {
        /// <summary>
        ///     获取或设置动画资源的前缀。
        /// </summary>
        public static string AnimationResourcePrefix { get; set; } = "animation-";

        /// <summary>
        ///     获取或设置故事板资源的前缀。
        /// </summary>
        public static string StoryboardResourcePrefix { get; set; } = "storyboard-";

        /// <summary>
        ///     获取或设置执行动画成功时，返回的值。
        /// </summary>
        public static int ExecuteAnimationSuccessed { get; set; } = 0;

        /// <summary>
        ///     获取或设置执行动画失败时，返回的值。
        /// </summary>
        public static int ExecuteUnexistAnimation { get; set; } = -1;

        /// <summary>
        ///     搜索指定名称，指定类型的动画资源。
        ///     搜索时包含动画前缀。
        /// </summary>
        /// <typeparam name="T">指定的动画类型</typeparam>
        /// <param name="animation_name">动画的索引，会添加动画前缀。</param>
        /// <returns>所搜索的资源，或者 null。</returns>
        public static T SearchAnimationResource<T>(string animation_name) where T : AnimationTimeline
        {
            return ResourceHelp.SearchResource<T>(AnimationResourcePrefix + animation_name).Clone() as T;
        }

        /// <summary>
        ///     搜索指定名称的动画资源。
        ///     搜索时包含动画前缀。
        /// </summary>
        /// <param name="animation_name">动画的索引，会添加动画前缀。</param>
        /// <returns>所搜索资源，或者 null</returns>
        public static AnimationTimeline SearchAnimationResource(string animation_name)
        {
            return SearchAnimationResource<AnimationTimeline>(animation_name);
        }

        /// <summary>
        ///     搜索指定名称的故事板资源。
        ///     搜索时包含故事板前缀。
        /// </summary>
        /// <param name="storyboard_name">故事板的索引，会添加故事板前缀。</param>
        /// <returns>所搜索的资源，或者 null</returns>
        public static Storyboard SearchStoryboardResource(string storyboard_name)
        {
            return ResourceHelp.SearchResource<Storyboard>(StoryboardResourcePrefix + storyboard_name)?.Clone();
        }

        /// <summary>
        ///     在此控件上，在指定属性上执行指定名称的动画。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="property">要动画的属性</param>
        /// <param name="animation_name">动画的索引，会添加动画前缀。</param>
        /// <param name="after_animation">在此动画完成后，要执行的回调。</param>
        /// <returns>ExecuteAnimationSuccessed 或者 ExecuteUnexistAnimation 之一。</returns>
        public static int ExecuteAnimation(this FrameworkElement control, DependencyProperty property,
            string animation_name, EventHandler after_animation = null)
        {
            var animation = SearchAnimationResource(animation_name);
            if (animation == null)
                return ExecuteUnexistAnimation;
            if (after_animation != null)
                animation.Completed += after_animation;
            control.BeginAnimation(property, animation);
            return ExecuteAnimationSuccessed;
        }

        /// <summary>
        ///     在此控件上，执行指定名称的故事板。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="storyboard_name">故事板的索引，会添加故事板前缀。</param>
        /// <param name="after_storyboard">在此故事板完成后，要执行的回调。</param>
        /// <returns>ExecuteAnimationSuccessed 或者 ExecuteUnexistAnimation 之一。</returns>
        public static int ExecuteStoryboard(this FrameworkElement control, string storyboard_name,
            EventHandler after_storyboard = null)
        {
            var storyboard = SearchStoryboardResource(storyboard_name);
            if (storyboard == null)
                return ExecuteUnexistAnimation;
            if (after_storyboard != null)
                storyboard.Completed += after_storyboard;
            control.BeginStoryboard(storyboard);
            return ExecuteAnimationSuccessed;
        }

        /// <summary>
        ///     在此控件上，按照指定的名称顺序，执行第一个存在的故事板。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="storyboard_names">故事版的索引数组，会添加故事板前缀。</param>
        /// <param name="after_storyboard">在找到的故事板完成后，要执行的回调。</param>
        /// <returns>ExecuteAnimationSuccessed 或者 ExecuteUnexistAnimation 之一。</returns>
        public static int TryExecuteStoryboard(this FrameworkElement control, IEnumerable<string> storyboard_names,
            EventHandler after_storyboard = null)
        {
            foreach (var storyboard_name in storyboard_names)
                if (SearchStoryboardResource(storyboard_name) != null)
                    return ExecuteStoryboard(control, storyboard_name, after_storyboard);
            return ExecuteUnexistAnimation;
        }

        /// <summary>
        ///     交换两个控件的存在性。
        /// </summary>
        /// <param name="control_to_hide">要隐藏的控件。</param>
        /// <param name="control_to_appear">要显示的控件。</param>
        /// <param name="appear_animation_name">显示控件的故事板，会添加故事板前缀。</param>
        /// <param name="hide_animation_name">隐藏控件的故事板，会添加故事板前缀。</param>
        public static void SwitchControl(FrameworkElement control_to_hide, FrameworkElement control_to_appear,
            string hide_animation_name, string appear_animation_name)
        {
            if (control_to_hide.ExecuteStoryboard(hide_animation_name,
                    (o, e) => control_to_hide.Visibility = Visibility.Hidden) == ExecuteUnexistAnimation)
                control_to_hide.Visibility = Visibility.Hidden;
            control_to_appear.Visibility = Visibility.Visible;
            if (control_to_appear.ExecuteStoryboard(appear_animation_name) == ExecuteUnexistAnimation)
                control_to_appear.Visibility = Visibility.Visible;
        }
    }
}