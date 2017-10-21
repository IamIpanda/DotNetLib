using System;
using System.Collections.Generic;
using System.Drawing;

namespace IamI.Lib.Others._1024
{
    /// <summary>
    ///     对玩家动作的抽象，封装了多数游戏的实际核心运算。
    /// </summary>
    public class Behave
    {
        private readonly Random _random = new Random();

        /// <summary>
        ///     创建一个 Behave 对象。
        /// </summary>
        /// <param name="target_movement">要绑定的 Movement 对象，指示操作哪个移动队列。</param>
        /// <param name="target_core">要绑定的 Core 对象，指示操作哪个数据核心。</param>
        public Behave(Movement target_movement, Core target_core)
        {
            TargetMovement = target_movement;
            TargetCore = target_core;
        }

        /// <summary>
        ///     与此 Behave 绑定的移动队列。
        /// </summary>
        public Movement TargetMovement { get; protected set; }

        /// <summary>
        ///     与此 Behave 绑定的数据核心。
        /// </summary>
        public Core TargetCore { get; protected set; }

        /// <summary>
        ///     响应移动请求。
        /// </summary>
        /// <param name="x_move">指示在x轴上向哪个方向移动。</param>
        /// <param name="y_move">指示在y轴上向哪个方向移动。</param>
        /// <param name="x_start">指示从哪个X坐标开始检查。</param>
        /// <param name="y_start">指示从哪个Y坐标开始检查。</param>
        /// <param name="step_x_move">X坐标位移步长</param>
        /// <param name="step_y_move">Y坐标位移步长</param>
        /// <returns>一个Core，展示了移动完成后的数据情况。</returns>
        protected Core Move(int x_move, int y_move, int x_start, int y_start, int step_x_move, int step_y_move)
        {
            // 初始化
            var answer = new Core(TargetCore);
            var record = new Core(TargetCore.GameSize);
            if (x_start < 0) x_start += answer.Width;
            if (y_start < 0) y_start += answer.Height;
            var during_step_x_move = -x_move;
            var during_step_y_move = -y_move;
            var x = x_start;
            var y = y_start;
            // 逐行/逐列处理
            while (answer[x_start, y_start] != Core.OUT_OF_RANGE)
            {
                x = x_start;
                y = y_start;
                while (answer[x, y] != Core.OUT_OF_RANGE)
                {
                    if (answer[x, y] != Core.NONE && answer[x, y] != Core.BACK_GROUND)
                    {
                        var now_number = answer[x, y];
                        // 取目标单元移动位置
                        var to = answer.LatestBlock(x, y, x_move, y_move);
                        var to_number = answer[to.X, to.Y];
                        if (to_number == now_number)
                        {
                            // 合并
                            answer.Execute(TargetMovement.Move(x, y, to.X, to.Y));
                            answer.Execute(TargetMovement.Appear(to.X, to.Y, 2 * to_number, 1));
                            TargetCore.ToAddScore += Math.Abs(to_number * 2);
                            record[to.X, to.Y] = answer[to.X, to.Y]; // 记录
                            answer[to.X, to.Y] = Core.BACK_GROUND; // 此位置已行动
                        }
                        else if (to_number + now_number == 0)
                        {
                            // 湮灭
                            answer.Execute(TargetMovement.Move(x, y, to.X, to.Y));
                            answer.Execute(TargetMovement.Disappear(to.X, to.Y, 1));
                            TargetCore.ToAddScore += Math.Abs(to_number * 4);
                            record[to.X, to.Y] = Core.WILL_DELETE; // 记录
                            answer[to.X, to.Y] = Core.BACK_GROUND; // 此位置已行动
                        }
                        else
                        {
                            // 反方向检查
                            var predict = answer.LatestBlock(x, y, -x_move, -y_move);
                            var predict_number = answer[predict.X, predict.Y];
                            // 如果反方向上已经存在了一个负值，那么停止
                            if (predict_number + now_number == 0) goto while_out;
                            // 移动
                            to.Offset(-x_move, -y_move);
                            answer.Execute(TargetMovement.Move(x, y, to.X, to.Y));
                        }
                    }
                    while_out:
                    x += during_step_x_move;
                    y += during_step_y_move;
                }
                x_start += step_x_move;
                y_start += step_y_move;
            }
            return answer.AddFrom(record);
        }

        /// <summary>
        ///     响应向左移动。
        /// </summary>
        /// <returns>一个 Core 对象，指示移动结束后的结果。</returns>
        public Core MoveLeft()
        {
            return Move(-1, 0, 1, 0, 0, 1);
        }

        /// <summary>
        ///     响应向右移动。
        /// </summary>
        /// <returns>一个 Core 对象，指示移动结束后的结果。</returns>
        public Core MoveRight()
        {
            return Move(1, 0, -2, 0, 0, 1);
        }

        /// <summary>
        ///     响应向上移动。
        /// </summary>
        /// <returns>一个 Core 对象，指示移动结束后的结果。</returns>
        public Core MoveUp()
        {
            return Move(0, -1, 0, 1, 1, 0);
        }

        /// <summary>
        ///     响应向下移动。
        /// </summary>
        /// <returns>一个 Core 对象，指示移动结束后的结果。</returns>
        public Core MoveDown()
        {
            return Move(0, 1, 0, -2, 1, 0);
        }

        /// <summary>
        ///     为目标区域内生成新块。
        /// </summary>
        /// <param name="type">一个整形数组，指示可以生成哪些数字的块。</param>
        /// <param name="area">指示可以生成的目标区域</param>
        /// <param name="moment">指示何时产生新块。</param>
        public void Generate(List<long> type, Rectangle area, int moment = 1)
        {
            var target = type[_random.Next(0, type.Count)];
            var points = new List<Point>();
            for (var i = area.Left; i <= area.Right; i++)
            for (var j = area.Top; j <= area.Bottom; j++)
                if (TargetCore[i, j] == Core.NONE)
                    points.Add(new Point(i, j));
            if (points.Count == 0) return;
            var point = points[_random.Next(0, points.Count)];
            TargetMovement.Appear(point.X, point.Y, target, moment);
        }

        /// <summary>
        ///     强制清空全区。
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < TargetCore.Width; i++)
            for (var j = 0; j < TargetCore.Height; j++)
                TargetCore[i, j] = 0;
        }

        /// <summary>
        ///     动画清空全区
        /// </summary>
        /// <param name="moment">指示何时生效。</param>
        public void Clear(int moment)
        {
            for (var i = 0; i < TargetCore.Width; i++)
            for (var j = 0; j < TargetCore.Height; j++)
                if (TargetCore[i, j] != Core.NONE && TargetCore[i, j] != Core.BACK_GROUND)
                    TargetMovement.Disappear(i, j, moment);
        }

        /// <summary>
        ///     初始化游戏。
        /// </summary>
        /// <param name="inits">指示在哪些区域生成哪些数字块。</param>
        /// <param name="moment">指示何时生效。</param>
        public void NewGame(Dictionary<List<long>, Rectangle> inits, int moment = 0)
        {
            foreach (var pair in inits)
            {
                var i = _random.Next(pair.Value.Left, pair.Value.Right);
                var j = _random.Next(pair.Value.Top, pair.Value.Bottom);
                var value = pair.Key[_random.Next(0, pair.Key.Count)];
                TargetMovement.Appear(i, j, value, moment);
            }
        }
    }
}