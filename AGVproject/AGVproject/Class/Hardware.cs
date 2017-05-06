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

        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        public static void Load()
        {
            Length = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.Length");
            Width = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.Width");
            WheelRadius = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.WheelRadius");
            WheelSpanX = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.WheelSpanX");
            WheelSpanY = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.WheelSpanY");

            AxisSideU = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.AxisSideU");
            AxisSideD = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.AxisSideD");
            AxisSideL = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.AxisSideL");
            AxisSideR = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.AxisSideR");

            ForeSightBG = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.ForeSightBG");
            ForeSightED = Configuration.getFieldValue1_DOUBLE("Hardware_PlatForm.ForeSightED");
        }
        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        public static void Save()
        {
            Configuration.setFieldValue("Hardware_PlatForm.Length", Length);
            Configuration.setFieldValue("Hardware_PlatForm.Width", Width);
            Configuration.setFieldValue("Hardware_PlatForm.WheelRadius", WheelRadius);
            Configuration.setFieldValue("Hardware_PlatForm.WheelSpanX", WheelSpanX);
            Configuration.setFieldValue("Hardware_PlatForm.WheelSpanY", WheelSpanY);

            Configuration.setFieldValue("Hardware_PlatForm.AxisSideU", AxisSideU);
            Configuration.setFieldValue("Hardware_PlatForm.AxisSideD", AxisSideD);
            Configuration.setFieldValue("Hardware_PlatForm.AxisSideL", AxisSideL);
            Configuration.setFieldValue("Hardware_PlatForm.AxisSideR", AxisSideR);

            Configuration.setFieldValue("Hardware_PlatForm.ForeSightBG", ForeSightBG);
            Configuration.setFieldValue("Hardware_PlatForm.ForeSightED", ForeSightED);
        }
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

        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        public static void Load()
        {
            max = Configuration.getFieldValue1_DOUBLE("Hardware_URG.max");
            min = Configuration.getFieldValue1_DOUBLE("Hardware_URG.min");
            AngleStart = Configuration.getFieldValue1_DOUBLE("Hardware_URG.AngleStart");
            AnglePace = Configuration.getFieldValue1_DOUBLE("Hardware_URG.AnglePace");
            ReceiveBG = Configuration.getFieldValue1_INT("Hardware_URG.ReceiveBG");
            ReceiveED = Configuration.getFieldValue1_INT("Hardware_URG.ReceiveED");
            CutBG = Configuration.getFieldValue1_INT("Hardware_URG.CutBG");
            CutED = Configuration.getFieldValue1_INT("Hardware_URG.CutED");
        }
        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        public static void Save()
        {
            Configuration.setFieldValue("Hardware_URG.max", max);
            Configuration.setFieldValue("Hardware_URG.min", min);
            Configuration.setFieldValue("Hardware_URG.AngleStart", AngleStart);
            Configuration.setFieldValue("Hardware_URG.AnglePace", AnglePace);
            Configuration.setFieldValue("Hardware_URG.ReceiveBG", ReceiveBG);
            Configuration.setFieldValue("Hardware_URG.ReceiveED", ReceiveED);
            Configuration.setFieldValue("Hardware_URG.CutBG", CutBG);
            Configuration.setFieldValue("Hardware_URG.CutED", CutED);
        }
    }

    /// <summary>
    /// 超声波传感器参数
    /// </summary>
    class Hardware_UltraSonic
    {
        /// <summary>
        /// 头部，左边，X 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Head_L_X;
        /// <summary>
        /// 头部，左边，Y 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Head_L_Y;
        /// <summary>
        /// 头部，右边，X 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Head_R_X;
        /// <summary>
        /// 头部，右边，Y 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Head_R_Y;
        /// <summary>
        /// 尾部，左边，X 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Tail_L_X;
        /// <summary>
        /// 尾部，左边，Y 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Tail_L_Y;
        /// <summary>
        /// 尾部，右边，X 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Tail_R_X;
        /// <summary>
        /// 尾部，右边，Y 方向的超声波传感器
        /// </summary>
        public static ULTRASONIC Tail_R_Y;

        /// <summary>
        /// 超声波传感器参数
        /// </summary>
        public struct ULTRASONIC
        {
            /// <summary>
            /// 小车坐标系下的 X 轴坐标 单位：mm
            /// </summary>
            public double x;
            /// <summary>
            /// 小车坐标系下的 Y 轴坐标 单位：mm
            /// </summary>
            public double y;
            /// <summary>
            /// 小车坐标系下的 Z 轴坐标 单位：mm
            /// </summary>
            public double z;

            /// <summary>
            /// 最大有效距离 单位：mm
            /// </summary>
            public double max;
            /// <summary>
            /// 最小有效距离 单位：mm
            /// </summary>
            public double min;
        }

        /// <summary>
        /// 左右两边（X 方向）的超声波传感器间距 单位：mm
        /// </summary>
        public static double xSpan { get { return Math.Abs(Head_L_X.x - Head_R_X.x); } }
        /// <summary>
        /// 前后（Y 方向）的超声波传感器间距 单位：mm
        /// </summary>
        public static double ySpan { get { return Math.Abs(Head_L_Y.y - Tail_L_Y.y); } }

        /// <summary>
        /// 整体超声波传感器最大有效距离 单位：mm
        /// </summary>
        public static double Max
        {
            get { return Math.Min(Head_L_X.max, Math.Min(Head_L_Y.max, Math.Min(Head_R_X.max, Math.Min(Head_R_Y.max, Math.Min(Tail_L_X.max, Math.Min(Tail_L_Y.max, Math.Min(Tail_R_X.max, Tail_R_Y.max))))))); }
        }
        /// <summary>
        /// 整体超声波传感器最小有效距离 单位：mm
        /// </summary>
        public static double Min
        {
            get { return Math.Max(Head_L_X.min, Math.Max(Head_L_Y.min, Math.Max(Head_R_X.min, Math.Max(Head_R_Y.min, Math.Max(Tail_L_X.min, Math.Max(Tail_L_Y.min, Math.Max(Tail_R_X.min, Tail_R_Y.min))))))); }
        }

        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        public static void Load()
        {
            Head_L_X = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Head_L_X"));
            Head_L_Y = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Head_L_Y"));
            Head_R_X = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Head_R_X"));
            Head_R_Y = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Head_R_Y"));
            Tail_L_X = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Tail_L_X"));
            Tail_L_Y = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Tail_L_Y"));
            Tail_R_X = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Tail_R_X"));
            Tail_R_Y = Double2UltraSonic(Configuration.getFieldValue2_DOUBLE("Hardware_UltraSonic.Tail_R_Y"));
        }
        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        public static void Save()
        {
            Configuration.setFieldValue("Hardware_UltraSonic.Head_L_X", UltraSonic2Double(Head_L_X));
            Configuration.setFieldValue("Hardware_UltraSonic.Head_L_Y", UltraSonic2Double(Head_L_Y));
            Configuration.setFieldValue("Hardware_UltraSonic.Head_R_X", UltraSonic2Double(Head_R_X));
            Configuration.setFieldValue("Hardware_UltraSonic.Head_R_Y", UltraSonic2Double(Head_R_Y));
            Configuration.setFieldValue("Hardware_UltraSonic.Tail_L_X", UltraSonic2Double(Tail_L_X));
            Configuration.setFieldValue("Hardware_UltraSonic.Tail_L_Y", UltraSonic2Double(Tail_L_Y));
            Configuration.setFieldValue("Hardware_UltraSonic.Tail_R_X", UltraSonic2Double(Tail_R_X));
            Configuration.setFieldValue("Hardware_UltraSonic.Tail_R_Y", UltraSonic2Double(Tail_R_Y));
        }

        private static List<double> UltraSonic2Double(ULTRASONIC ultraSonic)
        {
            List<double> data = new List<double>();

            data.Add(ultraSonic.x);
            data.Add(ultraSonic.y);
            data.Add(ultraSonic.z);

            data.Add(ultraSonic.max);
            data.Add(ultraSonic.min);

            return data;
        }
        private static ULTRASONIC Double2UltraSonic(List<double> data)
        {
            ULTRASONIC ultraSonic = new ULTRASONIC();

            ultraSonic.x = data[0];
            ultraSonic.y = data[0];
            ultraSonic.z = data[0];
            ultraSonic.max = data[0];
            ultraSonic.min = data[0];

            return ultraSonic;
        }
    }
}
