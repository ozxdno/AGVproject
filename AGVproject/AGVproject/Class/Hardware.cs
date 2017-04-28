using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 移动平台参数（小车参数）
    /// </summary>
    class Hardware_PlatForm
    {
        /// <summary>
        /// 移动平台长度 单位：mm
        /// </summary>
        public static double Length;
        /// <summary>
        /// 移动平台宽度 单位：mm
        /// </summary>
        public static double Width;

        /// <summary>
        /// 移动平台车轮半径 单位：mm
        /// </summary>
        public static double WheelRadius;
        /// <summary>
        /// 移动平台左右车轮中心之间的跨度 单位：mm
        /// </summary>
        public static double WheelSpanX;
        /// <summary>
        /// 移动平台前后车轮中心之间的跨度 单位：mm
        /// </summary>
        public static double WheelSpanY;

        /// <summary>
        /// 对激光雷达位置建系后小车左边边界的 X 轴坐标
        /// </summary>
        public static double AxisSideL;
        /// <summary>
        /// 对激光雷达位置建系后小车左边边界的 X 轴坐标
        /// </summary>
        public static double AxisSideR;
        /// <summary>
        /// 对激光雷达位置建系后小车左边边界的 Y 轴坐标
        /// </summary>
        public static double AxisSideU;
        /// <summary>
        /// 对激光雷达位置建系后小车左边边界的 Y 轴坐标
        /// </summary>
        public static double AxisSideD;
        
        /// <summary>
        /// 小车前瞻（最近的有效距离） 单位：mm
        /// </summary>
        public static double ForeSightBG;
        /// <summary>
        /// 小车前瞻（最远的有效距离） 单位：mm
        /// </summary>
        public static double ForeSightED;
    }

    /// <summary>
    /// 激光雷达参数
    /// </summary>
    class Hardware_URG
    {
        /// <summary>
        /// 最长有效距离
        /// </summary>
        public static double max;
        /// <summary>
        /// 最短有效距离
        /// </summary>
        public static double min;

        /// <summary>
        /// 激光雷达每帧的第一个数据所对应的角度
        /// </summary>
        public static double AngleStart;
        /// <summary>
        /// 激光雷达收到相邻两个数据的角度差
        /// </summary>
        public static double AnglePace;

        /// <summary>
        /// 起始位（从该位开始收取数据）
        /// </summary>
        public static int ReceiveBG;
        /// <summary>
        /// 结束位（不接收该位以后的数据）
        /// </summary>
        public static int ReceiveED;

        /// <summary>
        /// 去除该位以前的所有数据
        /// </summary>
        public static int CutBG;
        /// <summary>
        /// 去除该位以后的
        /// </summary>
        public static int CutED;
    }

    /// <summary>
    /// 超声波传感器参数
    /// </summary>
    class Hardware_UltraSonic
    {
        public static ULTRASONIC Head_L_X;
        public static ULTRASONIC Head_L_Y;
        public static ULTRASONIC Head_R_X;
        public static ULTRASONIC Head_R_Y;
        public static ULTRASONIC Tail_L_X;
        public static ULTRASONIC Tail_L_Y;
        public static ULTRASONIC Tail_R_X;
        public static ULTRASONIC Tail_R_Y;

        public struct ULTRASONIC
        {
            public double x;
            public double y;
            public double z;

            public double max;
            public double min;
        }

        public static double xSpan { get { return Math.Abs(Head_L_X.x - Head_R_X.x); } }
        public static double ySpan { get { return Math.Abs(Head_L_Y.y - Tail_L_Y.y); } }

        public static double Max
        {
            get { return Math.Max(Head_L_X.max, Math.Max(Head_L_Y.max, Math.Max(Head_R_X.max, Math.Max(Head_R_Y.max, Math.Max(Tail_L_X.max, Math.Max(Tail_L_Y.max, Math.Max(Tail_R_X.max, Tail_R_Y.max))))))); }
        }
        public static double Min
        {
            get { return Math.Min(Head_L_X.min, Math.Min(Head_L_Y.min, Math.Min(Head_R_X.min, Math.Min(Head_R_Y.min, Math.Min(Tail_L_X.min, Math.Min(Tail_L_Y.min, Math.Min(Tail_R_X.min, Tail_R_Y.min))))))); }
        }
    }
}
