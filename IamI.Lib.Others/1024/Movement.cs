using System;
using System.Collections.Generic;
using System.Text;

namespace IamI.Lib.Others._1024
{
    /// <summary>
    /// 
    /// </summary>
    public class Movement
    {
        public List<List<Action>> Actions = new List<List<Action>>();

        public List<Action> this[int i]
        {
            get
            {
                while(Actions.Count <= i)
                    Actions.Add(new List<Action>());
                return Actions[i];
            }
        }

        public Action Appear(int position_x, int position_y, long number, int time = 0)
        {
            var action = Action.Appear(position_x, position_y, number);
            if (action != null)
                this[time].Add(action);
            return action;
        }

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