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
    class BuildTrack
    {
        ////////////////////////////////////////////////// public attribute /////////////////////////////////////////

        /// <summary>
        /// 结束建立路径
        /// </summary>
        public static bool Over;
        
        ////////////////////////////////////////////////// private attribute /////////////////////////////////////////
        

        ////////////////////////////////////////////////// public method /////////////////////////////////////////

        /// <summary>
        /// 开始记录路径
        /// </summary>
        public static void Start()
        {
            HouseTrack.Clear(); Over = false;

            while (!Over && !TH_AutoSearchTrack.control.EMA)
            {
                // 等待停止事件
                if (!IsStop()) { continue; }

                // 记录路径
                HouseTrack.TRACK Track = new HouseTrack.TRACK();

                Track.TargetPosition = TH_MeasurePosition.getPosition();
                Track.Extra = CorrectPosition.getCorrect();
                HouseTrack.addTrack(Track);

                // 提示
                //TH_AutoSearchTrack.control.Event = "Saved This Mark!";
                Form_Start.config.WarningMessage = "Saved This Mark!";
                Form_Start.config.WarningTick = Form_Start.config.Tick + 10;

                // 等待再次启动
                while (!Over && !TH_AutoSearchTrack.control.EMA && IsStop()) ;
            }
        }

        ////////////////////////////////////////////////// private method /////////////////////////////////////////
        
        private static void getMove()
        {
            //double xMove = config.TargetPosition.x - config.StartPosition.x;
            //double yMove = config.TargetPosition.y - config.StartPosition.y;

            //double aMove = config.TargetPosition.aCar - config.StartPosition.aCar;
            //double rMove = config.TargetPosition.rCar - config.StartPosition.rCar;

            //CoordinatePoint.POINT Move = CoordinatePoint.Create_XY(xMove, yMove);
            //Move = CoordinatePoint.TransformCoordinate(Move, 0, 0, rMove);

            //config.xMove = Move.x;
            //config.yMove = Move.y;
            //config.aMove = aMove;

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
            CoordinatePoint.POINT LastPosition = TH_MeasurePosition.getPosition();
            System.Threading.Thread.Sleep(100);
            CoordinatePoint.POINT NextPosition = TH_MeasurePosition.getPosition();

            // 判断是否停止
            if (LastPosition.x != NextPosition.x) { return false; }
            if (LastPosition.y != NextPosition.y) { return false; }
            if (LastPosition.aCar != NextPosition.aCar) { return false; }

            return true;
        }
    }
}
