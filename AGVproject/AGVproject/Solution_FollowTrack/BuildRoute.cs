using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    /// <summary>
    /// 建立路径
    /// </summary>
    class BuildRoute
    {
        ////////////////////////////////////////////////// public attribute /////////////////////////////////////////

        /// <summary>
        /// 路径信息
        /// </summary>
        public static List<ROUTE> Route = new List<ROUTE>();
        
        /// <summary>
        /// 路径辅助信息
        /// </summary>
        public struct ROUTE
        {
            /// <summary>
            /// X 方向移动距离
            /// </summary>
            public double xMove;
            /// <summary>
            /// Y 方向移动距离
            /// </summary>
            public double yMove;
            /// <summary>
            /// A 方向移动距离
            /// </summary>
            public double aMove;

            /// <summary>
            /// 校准信息
            /// </summary>
            public AST_CorrectPosition.CORRECT Correct;
        }

        ////////////////////////////////////////////////// private attribute /////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public bool Over;

            public CoordinatePoint.POINT StartPosition;
            public CoordinatePoint.POINT TargetPosition;

            public CoordinatePoint.POINT LastPosition;
            public CoordinatePoint.POINT NextPosition;

            public double xMove;
            public double yMove;
            public double aMove;
        }

        ////////////////////////////////////////////////// public method /////////////////////////////////////////

        /// <summary>
        /// 开始记录路径
        /// </summary>
        public static void Start()
        {
            Route = new List<ROUTE>();
            config.Over = false;
            config.StartPosition = TH_MeasurePosition.getPosition();

            while (!config.Over)
            {
                // 等待停止事件
                if (!IsStop()) { continue; }

                // 记录路径
                config.TargetPosition = TH_MeasurePosition.getPosition();
                getMove();
                ROUTE route = new ROUTE(); route.Correct = AST_CorrectPosition.getCorrect();
                route.xMove = config.xMove;
                route.yMove = config.yMove;
                route.aMove = config.aMove;
                Route.Add(route);

                TH_AutoSearchTrack.control.Event = "Saved This Mark!";

                // 更新初始位置
                config.StartPosition = TH_MeasurePosition.getPosition();

                // 等待再次启动
                while (!config.Over && IsStop()) ;
                TH_AutoSearchTrack.control.Event = "Finding Mark...";
            }
        }
        /// <summary>
        /// 停止记录路径
        /// </summary>
        public static void Stop() { config.Over = true; }

        ////////////////////////////////////////////////// private method /////////////////////////////////////////
        
        private static void getMove()
        {
            double xMove = config.TargetPosition.x - config.StartPosition.x;
            double yMove = config.TargetPosition.y - config.StartPosition.y;

            double aMove = config.TargetPosition.aCar - config.StartPosition.aCar;
            double rMove = config.TargetPosition.rCar - config.StartPosition.rCar;

            CoordinatePoint.POINT Move = CoordinatePoint.Create_XY(xMove, yMove);
            Move = CoordinatePoint.TransformCoordinate(Move, 0, 0, rMove);

            config.xMove = Move.x;
            config.yMove = Move.y;
            config.aMove = aMove;

            //double dir = config.TargetPosition.aCar + 90;
            //while (dir < -90) { dir += 360; }
            //while (dir > 270) { dir -= 360; }

            //dir = dir - Move.a; config.aMove = aMove; config.xMove = 0; config.yMove = 0;

            //if (-30 < dir && dir < 30) { config.xMove = 0; config.yMove = Move.d; }
            //if (-120 < dir && dir < -60) { config.xMove = -Move.d; config.yMove = 0; }
            //if (60 < dir && dir < 120) { config.xMove = Move.d; config.yMove = 0; }
            //if (150 < dir && dir < 210) { config.xMove = 0; config.yMove = -Move.d; }
            //if (-210 < dir && dir < -150) { config.xMove = 0; config.yMove = -Move.d; }
        }
        private static bool IsStop()
        {
            // 间隔 100ms 检测一次
            config.LastPosition = TH_MeasurePosition.getPosition();
            System.Threading.Thread.Sleep(100);
            config.NextPosition = TH_MeasurePosition.getPosition();

            // 判断是否停止
            if (config.LastPosition.x != config.NextPosition.x) { return false; }
            if (config.LastPosition.y != config.NextPosition.y) { return false; }
            if (config.LastPosition.aCar != config.NextPosition.aCar) { return false; }

            return true;
        }
    }
}
