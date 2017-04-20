using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 速度导航，超声波避障
    /// </summary>
    class AST_GuideBySpeed
    {
        /// <summary>
        /// 已到达 X 方向调整极限
        /// </summary>
        public static bool ApproachX;
        /// <summary>
        /// 已到达 Y 方向调整极限
        /// </summary>
        public static bool ApproachY;
        /// <summary>
        /// 已到达 A 方向调整极限
        /// </summary>
        public static bool ApproachA;

        /// <summary>
        /// 维持 X 方向速度，当遇到障碍物时，自动放缓速度（可以做超声波避障）
        /// </summary>
        /// <param name="keepSpeed">X 方向维持速度</param>
        /// <returns>X 方向速度</returns>
        public static int getSpeedX(double keepSpeed)
        {
            if (keepSpeed > 0) { return getSpeedX_Right(keepSpeed); }
            if (keepSpeed < 0) { return -getSpeedX_Left(-keepSpeed); }

            int SpeedL = -getSpeedX_Left(0);
            int SpeedR = getSpeedX_Right(0);
            return SpeedL + SpeedR;
        }
        /// <summary>
        /// 维持 Y 方向速度，当遇到障碍物时，自动放缓速度（可以做超声波避障）
        /// </summary>
        /// <param name="keepSpeed">Y 方向速度</param>
        /// <returns></returns>
        public static int getSpeedY(double keepSpeed)
        {
            if (keepSpeed > 0) { return getSpeedY_Forward(keepSpeed); }
            if (keepSpeed < 0) { return -getSpeedY_Forward(-keepSpeed); }

            int SpeedF = getSpeedY_Forward(0);
            int SpeedB = -getSpeedY_Backward(0);
            return SpeedF + SpeedB;
        }
        /// <summary>
        /// 维持 A 方向速度，当遇到障碍物时，自动放缓速度（可以做超声波避障）
        /// </summary>
        /// <param name="keepSpeed">A 方向维持速度</param>
        /// <returns></returns>
        public static int getSpeedA(double keepSpeed)
        {
            if (keepSpeed > 0) { return getSpeedA_RotateL(keepSpeed); }
            if (keepSpeed < 0) { return -getSpeedA_RotateR(-keepSpeed); }

            int SpeedL = getSpeedA_RotateL(0);
            int SpeedR = -getSpeedA_RotateR(0);
            return SpeedL + SpeedR;
        }

        private static int getSpeedX_Left(double keepSpeed)
        {
            // 获取数据
            int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);

            if (Head_L_X == 0) { Head_L_X = (int)Hardware_UltraSonic.Head_L_X.max; }
            if (Tail_L_X == 0) { Tail_L_X = (int)Hardware_UltraSonic.Head_L_Y.max; }

            double minL = Math.Min(Head_L_X, Tail_L_X);

            // 获取控制
            double current = minL;
            double target = TH_AutoSearchTrack.control.MinDistance_L;
            double Kp = 0.8;
            double adjust = Kp * (current - target);

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_X) { adjust = TH_AutoSearchTrack.control.MaxSpeed_X; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_X) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_X; }
            
            // 调整极限
            ApproachX = Math.Abs(current - target) < 10;

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
        private static int getSpeedX_Right(double keepSpeed)
        {
            // 获取数据
            int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);

            if (Head_R_X == 0) { Head_R_X = (int)Hardware_UltraSonic.Head_R_X.max; }
            if (Tail_R_X == 0) { Tail_R_X = (int)Hardware_UltraSonic.Tail_R_X.max; }

            double minR = Math.Min(Head_R_X, Tail_R_X);

            // 获取控制
            double current = minR;
            double target = TH_AutoSearchTrack.control.MinDistance_R;
            double Kp = 0.8;
            double adjust = Kp * (current - target);

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_X) { adjust = TH_AutoSearchTrack.control.MaxSpeed_X; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_X) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_X; }

            // 调整极限
            ApproachX = Math.Abs(current - target) < 10;

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
        private static int getSpeedY_Forward(double keepSpeed)
        {
            // 获取数据
            int Head_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_Y);
            int Head_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_Y);

            if (Head_L_Y == 0) { Head_L_Y = (int)Hardware_UltraSonic.Head_L_Y.max; }
            if (Head_R_Y == 0) { Head_R_Y = (int)Hardware_UltraSonic.Head_R_Y.max; }

            double minH = Math.Min(Head_L_Y, Head_R_Y);

            // 获取控制
            double current = minH;
            double target = TH_AutoSearchTrack.control.MinDistance_H;
            double Kp = 1;
            double adjust = Kp * (current - target);

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_Y) { adjust = TH_AutoSearchTrack.control.MaxSpeed_Y; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_Y) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_Y; }

            // 调整极限
            ApproachY = Math.Abs(current - target) < 10;

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
        private static int getSpeedY_Backward(double keepSpeed)
        {
            // 获取数据
            int Tail_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);
            int Tail_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_Y);

            if (Tail_L_Y == 0) { Tail_L_Y = (int)Hardware_UltraSonic.Tail_L_Y.max; }
            if (Tail_R_Y == 0) { Tail_R_Y = (int)Hardware_UltraSonic.Tail_R_Y.max; }

            double minT = Math.Min(Tail_L_Y, Tail_R_Y);
            
            // 获取控制
            double current = minT;
            double target = TH_AutoSearchTrack.control.MinDistance_T;
            double Kp = 1;
            double adjust = Kp * (current - target);

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_Y) { adjust = TH_AutoSearchTrack.control.MaxSpeed_Y; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_Y) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_Y; }

            // 调整极限
            ApproachY = Math.Abs(current - target) < 10;

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
        private static int getSpeedA_RotateL(double keepSpeed)
        {
            // 获取数据
            int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            int Head_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_Y);
            int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);
            int Tail_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_Y);

            if (Head_L_X == 0) { Head_L_X = (int)Hardware_UltraSonic.Head_L_X.max; }
            if (Head_L_Y == 0) { Head_L_Y = (int)Hardware_UltraSonic.Head_L_Y.max; }
            if (Tail_R_X == 0) { Tail_R_X = (int)Hardware_UltraSonic.Tail_R_X.max; }
            if (Tail_R_Y == 0) { Tail_R_Y = (int)Hardware_UltraSonic.Tail_R_Y.max; }

            double minR = Math.Min(Head_L_X, Math.Min(Head_L_Y, Math.Min(Tail_R_X, Tail_R_Y)));
            
            // 获取控制
            double current = minR;
            double target = Math.Min(TH_AutoSearchTrack.control.MinDistance_L, TH_AutoSearchTrack.control.MinDistance_R);
            double Kp = 5;
            double adjust = Kp * (current - target);

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_A) { adjust = TH_AutoSearchTrack.control.MaxSpeed_A; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_A) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_A; }

            // 调整极限
            ApproachA = Math.Abs(current - target) < 10;

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
        private static int getSpeedA_RotateR(double keepSpeed)
        {
            // 获取数据
            int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            int Head_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_Y);
            int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);
            int Tail_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);

            if (Head_R_X == 0) { Head_R_X = (int)Hardware_UltraSonic.Head_R_X.max; }
            if (Head_R_Y == 0) { Head_R_Y = (int)Hardware_UltraSonic.Head_R_Y.max; }
            if (Tail_L_X == 0) { Tail_L_X = (int)Hardware_UltraSonic.Tail_L_X.max; }
            if (Tail_L_Y == 0) { Tail_L_Y = (int)Hardware_UltraSonic.Tail_L_Y.max; }

            double minR = Math.Min(Head_R_X, Math.Min(Head_R_Y, Math.Min(Tail_L_X, Tail_L_Y)));
            
            // 获取控制
            double current = minR;
            double target = Math.Min(TH_AutoSearchTrack.control.MinDistance_L, TH_AutoSearchTrack.control.MinDistance_R);
            double Kp = 5;
            double adjust = Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 10;

            // 限幅
            if (adjust > TH_AutoSearchTrack.control.MaxSpeed_A) { adjust = TH_AutoSearchTrack.control.MaxSpeed_A; }
            if (adjust < -TH_AutoSearchTrack.control.MaxSpeed_A) { adjust = -TH_AutoSearchTrack.control.MaxSpeed_A; }

            // 限速
            if (keepSpeed > adjust || adjust < 0) { keepSpeed = adjust; }
            return (int)keepSpeed;
        }
    }
}
