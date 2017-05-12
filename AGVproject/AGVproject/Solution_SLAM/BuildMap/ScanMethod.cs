using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 扫描过程中小车的移动策略
    /// </summary>
    class ScanMethod
    {
        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        /// <summary>
        /// X 方向正常行进速度
        /// </summary>
        public static double xSpeed;
        /// <summary>
        /// Y 方向正常行进速度
        /// </summary>
        public static double ySpeed;
        /// <summary>
        /// A 方向正常行进速度
        /// </summary>
        public static double aSpeed;

        /// <summary>
        /// 本次扫描是否已经结束
        /// </summary>
        public static bool Over;

        ////////////////////////////////////////////// private attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public double xSpeed, ySpeed, aSpeed;

            public CoordinatePoint.POINT CurrentPosition;

            public bool EndMark;
            public CoordinatePoint.POINT End_T;
            public CoordinatePoint.POINT End_F;

            public int N_ArriveSamePosition;
        }

        ////////////////////////////////////////////// public method ///////////////////////////////////////////

        /// <summary>
        /// 让小车行进一段距离
        /// </summary>
        public static void Start()
        {
            if (Over) { TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0); return; }
            config.CurrentPosition = TH_MeasurePosition.getPosition();

            if (!AST_GuideBySpeed.ApproachX && !AST_GuideBySpeed.ApproachY)
            { config.xSpeed = 0; config.ySpeed = ScanMethod.ySpeed; config.aSpeed = 0; }
            if (AST_GuideBySpeed.ApproachX)
            { config.xSpeed = 0; config.ySpeed = ScanMethod.ySpeed; config.aSpeed = 0; }
            if (AST_GuideBySpeed.ApproachY)
            {
                ScanMethod.ySpeed = -ScanMethod.ySpeed;
                config.xSpeed = ScanMethod.xSpeed; config.ySpeed = 0; config.aSpeed = 0;

                config.EndMark = !config.EndMark;
                if (config.EndMark == true)
                {
                    double dis = CoordinatePoint.getDistance(config.CurrentPosition, config.End_T);
                    if (dis < 100) { config.N_ArriveSamePosition++; } else { config.N_ArriveSamePosition = 0; }
                }
                if (config.EndMark == false)
                {
                    double dis = CoordinatePoint.getDistance(config.CurrentPosition, config.End_F);
                    if (dis < 100) { config.N_ArriveSamePosition++; } else { config.N_ArriveSamePosition = 0; }
                }

                if (config.N_ArriveSamePosition > 4)
                { Over = true; TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0); return;  }
            }

            int xSpeed = AST_GuideBySpeed.getSpeedX(config.xSpeed);
            int ySpeed = AST_GuideBySpeed.getSpeedY(config.ySpeed);
            int aSpeed = 0;

            TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            System.Threading.Thread.Sleep(100);
        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////
        
    }
}
