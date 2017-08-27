using System;
using System.Text;
using Point = System.Drawing.Point;

namespace IamI.Lib.Others._1024
{
    /// <summary>
    /// 方块要进行的操作
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 什么都不做
        /// </summary>
        None,
        /// <summary>
        /// 从 From 向 To 移动。
        /// </summary>
        Move,
        /// <summary>
        /// 在 Position 处出现。
        /// </summary>
        Appear,
        /// <summary>
        /// 从 Position 处消失。
        /// </summary>
        Disappear
    }

    /// <summary>
    /// 对游戏状态的每一个方块而言，进行的操作的抽象。
    /// </summary>
    public class Action
    {
        /// <summary>
        /// 要进行的操作类别。
        /// </summary>
        public ActionType Type { get; set; }
        protected Point Argument1 { get; set; }
        protected Point Argument2 { get; set; }
        /// <summary>
        /// 当 Type = ActionType::Move 时，要从哪里移动。
        /// </summary>
        public Point From => Argument1;
        /// <summary>
        /// 当 Type = ActionType::Move 时，要移动向哪里。
        /// </summary>
        public Point To => Argument2;
        /// <summary>
        /// 当 Type = ActionType::Appear | ActionType::Disappear 时，要在哪里进行操作。
        /// </summary>
        public Point Position => Argument1;
        /// <summary>
        /// 方块上显示的数字。
        /// </summary>
        public long Number { get; set; }

        protected Action()
        {
            Type = ActionType.None;
            Argument1 = new Point(0, 0);
            Argument2 = new Point(0, 0);
            Number = 0;
        }

        /// <summary>
        /// 生成一个 Action，Type = ActionType::Appear
        /// </summary>
        /// <param name="position_x">指定方块要出现的 X 坐标。</param>
        /// <param name="position_y">指定方块要出现的 Y 坐标。</param>
        /// <param name="number">指定方块上的数字。</param>
        /// <returns>生成的 Action。</returns>
        public static Action Appear(int position_x, int position_y, long number)
        {
            return new Action
            {
                Type = ActionType.Appear,
                Argument1 = new Point(position_x, position_y),
                Number = number
            };
        }

        /// <summary>
        /// 生成一个 Action，Type = ActionType::Disappear
        /// </summary>
        /// <param name="position_x">指定方块要消失的 X 坐标。</param>
        /// <param name="position_y">指定方块要消失的 Y 坐标。</param>
        /// <returns>生成的 Action。</returns>
        public static Action Disappear(int position_x, int position_y)
        {
            return new Action
            {
                Type = ActionType.Disappear,
                Argument1 = new Point(position_x, position_y),
                Number = 0
            };
        }

        /// <summary>
        /// 生成一个 Action，Type = ActionType::Move
        /// </summary>
        /// <param name="from_x">方块移动起点的 X 坐标</param>
        /// <param name="from_y">方块移动起点的 Y 坐标</param>
        /// <param name="to_x">方块移动终点的 X 坐标</param>
        /// <param name="to_y">方块移动终点的 Y 坐标</param>
        /// <returns>生成的 Action。</returns>
        public static Action Move(int from_x, int from_y, int to_x, int to_y)
        {
            if (from_x == to_x && from_y == to_y)
                return null;
            return new Action()
            {
                Type = ActionType.Move,
                Argument1 = new Point(from_x, from_y),
                Argument2 = new Point(to_x, to_y)
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Action ");
            switch (Type)
            {
                case ActionType.None:
                    sb.Append("none");
                    break;
                case ActionType.Appear:
                    sb.AppendFormat("appear    [{0}, {1}] = {2}", Position.X, Position.Y, Number);
                    break;
                case ActionType.Disappear:
                    sb.AppendFormat("disappear [{0}, {1}] = {2}", Position.X, Position.Y, Core.None);
                    break;
                case ActionType.Move:
                    sb.AppendFormat("move      [{0}, {1}] = [{2}, {3}]", To.X, To.Y, From.X, From.Y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sb.ToString();
        }
    } 
}