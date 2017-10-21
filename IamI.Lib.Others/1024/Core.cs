using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IamI.Lib.Others._1024
{
    /// <summary>
    ///     游戏的数据核心。
    /// </summary>
    public class Core
    {
        /// <summary>
        ///     表示此格上没有方块。
        /// </summary>
        public const long NONE = 0;

        /// <summary>
        ///     表示此格上是背景图像（也不可通过）。
        /// </summary>
        public const long BACK_GROUND = -1;

        /// <summary>
        ///     表示此格已超出游戏范围。
        /// </summary>
        public const long OUT_OF_RANGE = 1;

        /// <summary>
        ///     指示游戏在下一次刷新时移除这格上的值。
        /// </summary>
        public const long WILL_DELETE = -7;

        /// <summary>
        ///     游戏数据。
        /// </summary>
        public long[,] GameData;

        /// <summary>
        ///     根据给定的尺寸创建一个 Core。
        /// </summary>
        /// <param name="size"></param>
        public Core(int size)
        {
            GameSize = size;
            GameData = new long[Width, Height];
            Score = 0;
        }

        /// <summary>
        ///     复制给定的 Core。
        /// </summary>
        /// <param name="mirror">克隆源</param>
        public Core(Core mirror)
        {
            GameSize = mirror.GameSize;
            GameData = new long[Width, Height];
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                GameData[i, j] = mirror.GameData[i, j];
            Score = mirror.Score;
        }

        /// <summary>
        ///     指示游戏棋盘的大小。
        ///     通过影响 Width 和 Height 来实现。
        /// </summary>
        public int GameSize { get; }

        /// <summary>
        ///     指示游戏棋盘的宽度。
        /// </summary>
        public int Width => 2 * GameSize + 1;

        /// <summary>
        ///     指示游戏棋盘的高度。
        /// </summary>
        public int Height => GameSize;

        /// <summary>
        ///     游戏得分。
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        ///     指出要为玩家加多少分。
        /// </summary>
        public long ToAddScore { get; set; } = 0;

        /// <summary>
        ///     获取给定坐标上的值。
        /// </summary>
        /// <param name="x">坐标 X</param>
        /// <param name="y">坐标 Y</param>
        /// <returns>GameData[x, y] | OutOfRange</returns>
        public long this[int x, int y]
        {
            get
            {
                if (y < 0 || x < 0 || x >= Width || y >= Height) return OUT_OF_RANGE;
                return GameData[x, y];
            }
            set
            {
                if (y < 0 || x < 0 || x >= Width || y >= Height) return;
                GameData[x, y] = value;
            }
        }

        /// <summary>
        ///     设置此游戏状态的某一列。
        /// </summary>
        /// <param name="y">要设置的列</param>
        public List<long> this[int y]
        {
            set
            {
                for (var i = 0; i < Width && i < value.Count; i++)
                    GameData[i, y] = value[i];
            }
        }

        /// <summary>
        ///     在此 Core 上执行给定的 Action。
        /// </summary>
        /// <param name="action">要执行的 Action。</param>
        public void Execute(Action action)
        {
            if (action == null) return;
            switch (action.Type)
            {
                case ActionType.Appear:
                    this[action.Position.X, action.Position.Y] = action.Number;
                    break;
                case ActionType.Disappear:
                    this[action.Position.X, action.Position.Y] = NONE;
                    break;
                case ActionType.Move:
                    if (action.To.X == action.From.X && action.To.Y == action.From.Y) break;
                    this[action.To.X, action.To.Y] = this[action.From.X, action.From.Y];
                    this[action.From.X, action.From.Y] = NONE;
                    break;
                case ActionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     在此 Core 上执行给定的 Movement。
        /// </summary>
        /// <param name="movement">要执行的 Movement。</param>
        public void Execute(Movement movement)
        {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var actions in movement.Actions)
            foreach (var action in actions)
                Execute(action);
        }

        /// <summary>
        ///     在此 Core 上执行给定的 Core。
        /// </summary>
        /// <param name="source">要执行的 Core。</param>
        /// <returns>处理后的此 Core 自身。</returns>
        public Core AddFrom(Core source)
        {
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                if (source[i, j] == WILL_DELETE)
                    this[i, j] = NONE;
                else if (source[i, j] != NONE)
                    this[i, j] = source[i, j];
            return this;
        }

        /// <summary>
        ///     搜寻离某个位置按照指定方向上最近的方块。
        /// </summary>
        /// <param name="from_x">移动的开始位置的X坐标。</param>
        /// <param name="from_y">移动的开始位置的Y坐标。</param>
        /// <param name="move_x">X方向上的移动步长</param>
        /// <param name="move_y">Y方向上的移动步长</param>
        /// <returns></returns>
        public Point LatestBlock(int from_x, int from_y, int move_x, int move_y)
        {
            var answer = NONE;
            var x = from_x;
            var y = from_y;
            while (answer == NONE)
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