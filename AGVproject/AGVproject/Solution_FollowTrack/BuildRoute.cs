using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    class BuildRoute
    {
        ////////////////////////////////////////////////// public attribute /////////////////////////////////////////

        /// <summary>
        /// 路径信息
        /// </summary>
        public static List<ROUTE> Route = new List<ROUTE>();
        
        /// <summary>
        /// 路径信息
        /// </summary>
        public struct ROUTE
        {
            /// <summary>
            /// 小车初始位置
            /// </summary>
            public CoordinatePoint.POINT StartPosition;
            /// <summary>
            /// 小车目标位置
            /// </summary>
            public CoordinatePoint.POINT TargetPosition;

            /// <summary>
            /// 目标点校准信息
            /// </summary>
            public AST_CorrectPosition.CORRECT TargetCorrect;
        }

        ////////////////////////////////////////////////// private attribute /////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public bool Over;

            public ROUTE CurrentRoute;

            public CoordinatePoint.POINT LastPosition;
            public CoordinatePoint.POINT NextPosition;

            public double xMove;
            public double yMove;
            public double aMove;

            public bool IsStop;
            public int LastDirection; // 0 停止 1 X 2 Y 3 A
            public int NextDirection;
        }

        ////////////////////////////////////////////////// public method /////////////////////////////////////////

        public static void Start()
        {
            Route = new List<ROUTE>();
            config.Over = false;

            while (!config.Over)
            {
                config.LastPosition = TH_MeasurePosition.getPosition();
                System.Threading.Thread.Sleep(200);
                config.NextPosition = TH_MeasurePosition.getPosition();

                getMove();

                if (config.IsStop)
                {
                    // 记录历史数据
                    config.CurrentRoute.TargetPosition = TH_MeasurePosition.getPosition();
                    config.CurrentRoute.TargetCorrect = AST_CorrectPosition.getCorrect();
                    Route.Add(config.CurrentRoute);

                    config.CurrentRoute = new ROUTE();
                    config.CurrentRoute.StartPosition = Route[Route.Count - 1].TargetPosition;

                    // 等待再次启动
                    while (config.IsStop && !config.Over)
                    { System.Threading.Thread.Sleep(200); config.NextPosition = TH_MeasurePosition.getPosition(); getMove(); }
                    continue;
                }

                if (config.NextDirection != config.LastDirection)
                {
                    config.CurrentRoute.TargetPosition = TH_MeasurePosition.getPosition();
                    config.CurrentRoute.TargetCorrect = AST_CorrectPosition.getCorrect();
                    Route.Add(config.CurrentRoute);

                    config.CurrentRoute = new ROUTE();
                    config.CurrentRoute.StartPosition = Route[Route.Count - 1].TargetPosition;
                }
            }
        }
        public static void Stop()
        {
            config.Over = true;
        }

        ////////////////////////////////////////////////// private method /////////////////////////////////////////
        
        private static void getMove()
        {
            config.LastDirection = config.NextDirection;

            config.xMove = config.NextPosition.x - config.LastPosition.x;
            config.yMove = config.NextPosition.y - config.LastPosition.y;
            config.aMove = config.NextPosition.aCar - config.LastPosition.aCar;

            config.IsStop = config.xMove == 0 && config.yMove == 0 && config.aMove == 0;
            config.NextDirection = config.LastDirection;

            if (config.IsStop) { config.NextDirection = 0; }
            if (config.xMove > 10) { config.NextDirection = 1; }
            if (config.yMove > 10) { config.NextDirection = 2; }
            if (config.aMove > 10) { config.NextDirection = 3; }
        }
    }
}
