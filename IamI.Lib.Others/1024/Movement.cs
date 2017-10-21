using System;
using System.Collections.Generic;
using System.Text;

namespace IamI.Lib.Others._1024
{
    /// <summary>
    ///     对一组操作的抽象。
    /// </summary>
    public class Movement
    {
        /// <summary>
        ///     依时间排序，记录的一组操作。
        /// </summary>
        public List<List<Action>> Actions = new List<List<Action>>();

        /// <summary>
        ///     获取 moment =
        /// </summary>
        /// <param name="index"></param>
        public List<Action> this[int index]
        {
            get
            {
                while (Actions.Count <= index)
                    Actions.Add(new List<Action>());
                return Actions[index];
            }
        }

        /// <summary>
        ///     向队列中添加一个 Action, Type = ActionType::Appear
        /// </summary>
        /// <param name="position_x">指示方块出现的X坐标</param>
        /// <param name="position_y">指示方块出现的Y坐标</param>
        /// <param name="number">指示方块上的数字</param>
        /// <param name="time">指示何时出现</param>
        /// <returns>生成的 Action，已加入队列中。</returns>
        public Action Appear(int position_x, int position_y, long number, int time = 0)
        {
            var action = Action.Appear(position_x, position_y, number);
            if (action != null)
                this[time].Add(action);
            return action;
        }

        /// <summary>
        ///     向队列中添加一个 Action，Type = ActionType::Disappear
        /// </summary>
        /// <param name="position_x">指示方块消失的X坐标</param>
        /// <param name="position_y">指示方块消失的Y坐标</param>
        /// <param name="time">指示何时消失</param>
        /// <returns>生成的 Action，已加入队列中</returns>
        public Action Disappear(int position_x, int position_y, int time = 0)
        {
            var action = Action.Disappear(position_x, position_y);
            if (action != null)
                this[time].Add(action);
            return action;
        }

        public Action Move(int from_x, int from_y, int to_x, int to_y, int time = 0)
        {
            var action = Action.Move(from_x, from_y, to_x, to_y);
            if (action != null)
                this[time].Add(action);
            return action;
        }

        /// <summary>
        ///     从队列中弹出一组操作进行执行。
        /// </summary>
        /// <returns>队列顶的 Actions。</returns>
        public List<Action> Pop()
        {
            if (Actions.Count == 0) return null;
            var answer = Actions[0];
            Actions.RemoveAt(0);
            return answer;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[Movement]");
            sb.AppendLine();
            sb.AppendLine();
            for (var i = 0; i < Actions.Count; i++)
            {
                sb.AppendFormat("Step [{0}]", i + 1);
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, Actions[i].ConvertAll(input => input.ToString())));
                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}