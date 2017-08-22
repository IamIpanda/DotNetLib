using System.Collections.Generic;
using System.Text;

namespace IamI.Lib.Others._1024
{
    public class Core
    {
        public int GameSize { get; }
        public int Width => 2 * GameSize + 1;
        public int Height => GameSize;
        public long[, ] GameData;
        public long Score { get; set; }
        public long ToAddScore { get; set; } = 0;


        public const long None = 0;
        public const long BackGround = -1;
        public const long OutOfRange = 1;
        public const long WillDelete = -7;

        public Core(int size)
        {
            GameSize = size;
            GameData = new long[Width, Height];
            Score = 0;
        }

        public Core(Core Mirror)
        {
            GameSize = Mirror.GameSize;
            GameData = new long[Width, Height];
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                    GameData[i, j] = Mirror.GameData[i, j];
            Score = Mirror.Score;
        }

        public long this[int x, int y]
        {
            get
            {
                if (y < 0 || x < 0 || x >= Width || y >= Height) return OutOfRange;
                return GameData[x, y];
            }
            set
            {
                if (y < 0 || x < 0 || x >= Width || y >= Height) return;
                GameData[x, y] = value;
            }
        }

        public List<long> this[int y]
        {
            set
            {
                for (var i = 0; i < Width && i < value.Count; i++)
                    GameData[i, y] = value[i];
            }
        }

        public void Execute(Action action)
        {
            if (action == null) return;
            switch (action.Type)
            {
                case ActionType.Appear:
                    this[action.Position.X, action.Position.Y] = action.Number;
                    break;
                case ActionType.Disappear:
                    this[action.Position.X, action.Position.Y] = None;
                    break;
                case ActionType.Move:
                    if (action.To.X == action.From.X && action.To.Y == action.From.Y) break;
                    this[action.To.X, action.To.Y] = this[action.From.X, action.From.Y];
                    this[action.From.X, action.From.Y] = None;
                    break;
            }
        }

        public void Execute(Movement movement)
        {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var actions in movement.Actions)
                foreach (var action in actions)
                    Execute(action);
        }

        public Core AddFrom(Core source)
        {
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                    if (source[i, j] == WillDelete)
                        this[i, j] = None;
                    else if (source[i, j] != None)
                        this[i, j] = source[i, j];
            
            return this;
        }

        public Point LatestBlock(int from_x, int from_y, int move_x, int move_y)
        {
            var answer = None;
            var x = from_x;
            var y = from_y;
            while (answer == None)
            {
                x += move_x;
                y += move_y;
                answer = this[x, y];
            }
            return new Point(x, y);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Size = {0}", GameSize);
            sb.AppendLine();
            for (var j = 0; j < Height; j++)
            {
                for (var i = 0; i < Width; i++)
                    sb.AppendFormat("{0,5} ", GameData[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}