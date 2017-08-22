using System.Text;
using Point = System.Drawing.Point;

namespace IamI.Lib.Others._1024
{
    public enum ActionType
    {
        None,
        Move,
        Appear,
        Disappear
    }

    public class Action
    {
        public ActionType Type { get; set; }
        public Point Argument1 { get; set; }
        public Point Argument2 { get; set; }
        public Point From => Argument1;
        public Point To => Argument2;
        public Point Position => Argument1;
        public long Number { get; set; }

        protected Action()
        {
            Type = ActionType.None;
            Argument1 = new Point(0, 0);
            Argument2 = new Point(0, 0);
            Number = 0;
        }

        public static Action Appear(int position_x, int position_y, long number)
        {
            return new Action()
            {
                Type = ActionType.Appear,
                Argument1 = new Point(position_x, position_y),
                Number = number
            };
        }

        public static Action Disappear(int position_x, int position_y)
        {
            return new Action
            {
                Type = ActionType.Disappear,
                Argument1 = new Point(position_x, position_y),
                Number = 0
            };
        }

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
            }
            return sb.ToString();
        }
    } 
}