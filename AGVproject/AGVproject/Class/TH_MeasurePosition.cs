using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGVproject.Class
{
    /// <summary>
    /// 获取小车仓库坐标系下的坐标
    /// </summary>
    class TH_MeasurePosition
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        /// <summary>
        /// 编码器串口是否打开
        /// </summary>
        public static bool IsOpen { get { return config.Port != null && config.Port.IsOpen && !config.IsClosing; } }
        /// <summary>
        /// 描述小车轮子速度
        /// </summary>
        public struct SPEED
        {
            /// <summary>
            /// 前左轮转速 单位：转/100ms
            /// </summary>
            public int HeadL;
            /// <summary>
            /// 前右轮转速 单位：转/100ms
            /// </summary>
            public int HeadR;
            /// <summary>
            /// 后左轮转速 单位：转/100ms
            /// </summary>
            public int TailL;
            /// <summary>
            /// 后右轮转速 单位：转/100ms
            /// </summary>
            public int TailR;

            /// <summary>
            /// X 方向的移动距离 单位：mm
            /// </summary>
            public double xMove;
            /// <summary>
            /// Y 方向的移动距离 单位：mm
            /// </summary>
            public double yMove;
            /// <summary>
            /// A 方向的旋转角度 单位：度
            /// </summary>
            public double aMove;
        }

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static KalmanFilter FilterX = new KalmanFilter();
        private static KalmanFilter FilterY = new KalmanFilter();
        private static KalmanFilter FilterA = new KalmanFilter();

        private static CONFIG config;
        private struct CONFIG
        {
            public SerialPort Port;

            public bool IsReading;
            public bool IsClosing;
            public bool IsGetting;
            public bool IsSetting;

            public bool IsSettingSpeed;
            public bool IsGettingSpeed;

            public List<byte> receData;
            public byte[] Frame;
            public SPEED Speed;
            public CoordinatePoint.POINT Current;
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        /// <summary>
        /// 打开编码器串口
        /// </summary>
        /// <returns>编码器串口是否成功打开</returns>
        public static bool Open()
        {
            // 串口已经打开
            while (config.IsClosing) ; if (IsOpen) { return true; }

            // 获取串口配置
            string portName = "";
            if (Form_Start.config.SelectedLocatePortName != -1)
            { portName = Form_Start.config.LocPortName[Form_Start.config.SelectedLocatePortName]; }

            int baudRate = -1;
            if (Form_Start.config.SelectedLocateBaudRate != -1)
            { baudRate = Form_Start.config.LocBaudRate[Form_Start.config.SelectedLocateBaudRate]; }

            // 尝试打开串口
            try
            {
                // 初始化线程
                Initial_TH_MeasurePosition();

                // 打开串口
                config.Port = new SerialPort(portName, baudRate);
                config.Port.DataReceived -= portDataReceived;
                config.Port.DataReceived += portDataReceived;
                config.Port.Open();
                
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 关闭编码器串口
        /// </summary>
        /// <returns></returns>
        public static bool Close()
        {
            // 已经关闭或正在关闭
            if (!IsOpen || config.IsClosing) { return true; }

            // 正在关闭
            config.IsClosing = true;

            // 等待读取完毕
            while (config.IsReading) { }

            // 关闭
            try { config.Port.Close(); config.IsClosing = false; return true; }
            catch { config.IsClosing = false; return false; }
        }

        /// <summary>
        /// 获取小车当前位置仓库坐标系下坐标
        /// </summary>
        /// <returns></returns>
        public static CoordinatePoint.POINT getPosition()
        {
            CoordinatePoint.POINT point = new CoordinatePoint.POINT();

            while (config.IsSetting) ;
            config.IsGetting = true;

            point = CoordinatePoint.Create_XY(1000 * config.Current.x, 1000 * config.Current.y);
            point.aCar = config.Current.aCar;
            point.rCar = config.Current.rCar;

            config.IsGetting = false; return point;
        }
        /// <summary>
        /// 设置小车当前位置仓库坐标系下坐标
        /// </summary>
        /// <param name="point">设定坐标</param>
        public static void setPosition(CoordinatePoint.POINT point)
        {
            while (config.IsSetting) ;
            while (config.IsGetting) ;
            config.IsSetting = true;
            config.IsGetting = true;

            config.Current.x = point.x / 1000;
            config.Current.y = point.y / 1000;
            config.Current.aCar = point.aCar;
            config.Current.rCar = point.rCar;

            if (point.aCar == 0) { config.Current.aCar = point.rCar * 180 / Math.PI; }
            if (point.rCar == 0) { config.Current.rCar = point.aCar * Math.PI / 180; }

            config.IsSetting = false;
            config.IsGetting = false;
        }
        /// <summary>
        /// 设置小车当前位置仓库坐标系下坐标
        /// </summary>
        /// <param name="x">X 轴坐标 单位：mm</param>
        /// <param name="y">Y 轴坐标 单位：mm</param>
        /// <param name="a">当前方向 单位：度</param>
        public static void setPosition(double x, double y, double a)
        {
            while (config.IsSetting) ;
            while (config.IsGetting) ;
            config.IsSetting = true;
            config.IsGetting = true;

            config.Current.x = x / 1000;
            config.Current.y = y / 1000;
            config.Current.aCar = a;
            config.Current.rCar = a * Math.PI / 180;
            
            config.IsSetting = false;
            config.IsGetting = false;
        }
        /// <summary>
        /// 获取当前小车轮子转速
        /// </summary>
        /// <returns></returns>
        public static SPEED getSpeed()
        {
            SPEED speed = new SPEED();

            while (config.IsSettingSpeed) ;
            config.IsGettingSpeed = true;
            
            speed.HeadL = config.Speed.HeadL;
            speed.HeadR = config.Speed.HeadR;
            speed.TailL = config.Speed.TailL;
            speed.TailR = config.Speed.TailR;

            config.IsGettingSpeed = false;

            int currSpeed_HL = -speed.HeadL;
            int currSpeed_HR = speed.HeadR;
            int currSpeed_TL = -speed.TailL;
            int currSpeed_TR = speed.TailR;

            const double R = 0.1015, PI = 3.1416, Lx = 0.1825, Ly = 0.2950;

            double X = (double)(currSpeed_TL + currSpeed_HR - currSpeed_HL - currSpeed_TR) * 2 * PI * R / 1000 / 4;
            double Y = (double)(currSpeed_TL + currSpeed_TR + currSpeed_HL + currSpeed_HR) * 2 * PI * R / 1000 / 4;
            double A = (double)(currSpeed_TR + currSpeed_HR - currSpeed_TL - currSpeed_HL) * 2 * PI * R / 1000 / (4 * (Lx + Ly));

            speed.xMove = X / 4.077 * 1000;
            speed.yMove = Y / 4.077 * 1000;
            speed.aMove = A / 4.077 * 180 / PI;
            
            return speed;
        }
        /// <summary>
        /// 把当前位置转成字符串
        /// </summary>
        /// <returns></returns>
        public static string ToString()
        {
            CoordinatePoint.POINT pos = getPosition();
            return ((int)pos.x).ToString() + ", " + ((int)pos.y).ToString() + ", " + ((int)pos.aCar).ToString();
        }
        /// <summary>
        /// 把当前位置转成字符串
        /// </summary>
        /// <returns></returns>
        public static string ToString(CoordinatePoint.POINT pos)
        {
            return ((int)pos.x).ToString() + ", " + ((int)pos.y).ToString() + ", " + ((int)pos.aCar).ToString();
        }
        /// <summary>
        /// 把字符串坐标转换成 POINT 结构体坐标
        /// </summary>
        /// <param name="pos">字符串坐标</param>
        /// <param name="point">POINT 结构体坐标</param>
        /// <returns></returns>
        public static bool ToPosition(string pos,ref CoordinatePoint.POINT point)
        {
            string[] pt = pos.Split(',');
            if (pt.Length != 3) { return false; }

            try { point.x = double.Parse(pt[0]); point.y = double.Parse(pt[1]); point.aCar = double.Parse(pt[2]); return true; }
            catch { return false; }
        }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void Initial_TH_MeasurePosition()
        {
            config.IsReading = false;
            config.IsClosing = false;
            config.IsSetting = false;
            config.IsGetting = false;

            config.receData = new List<byte>();
            config.Frame = new byte[14];

            config.IsGettingSpeed = false;
            config.IsSettingSpeed = false;

            config.Speed = new SPEED();
            config.Current = new CoordinatePoint.POINT();

            FilterX.P = 10; FilterX.Q = 10; FilterX.R = 10; FilterX.Last = 0;
            FilterY.P = 10; FilterY.Q = 10; FilterY.R = 10; FilterY.Last = 0;
            FilterA.P = 01; FilterA.Q = 01; FilterA.R = 01; FilterA.Last = 0;
        }

        private static void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭或已经关闭
            if (config.IsClosing || !IsOpen) { return; }
            
            // 读取数据
            config.IsReading = true;

            // 尝试读取数据
            try
            {
                int receLength = config.Port.BytesToRead;
                byte[] temp_receData = new byte[receLength];
                config.Port.Read(temp_receData, 0, receLength);

                // 接在原数据末尾
                for (int i = 0; i < receLength; i++) { config.receData.Add(temp_receData[i]); }
            }
            catch { config.IsReading = false; return; }

            // 寻找完整的帧，并对应做出处理
            int indexBG = -1, indexED = config.receData.Count;
            for (int i = 0; i < config.receData.Count; i++)
            {
                // 找到完整的一帧
                if (config.receData[i] != 0xAA) { continue; }
                indexBG = i;
                if (i + config.Frame.Length > config.receData.Count) { break; }
                if (config.receData[indexBG + config.Frame.Length - 1] != 0xBB) { continue; }

                // 取出完整的帧
                for (int j = 0; j < config.Frame.Length; j++) { config.Frame[j] = config.receData[indexBG + j]; }
                indexED = indexBG + config.Frame.Length;

                // 对这一帧进行处理
                getCurrentSpeed();
                getCurrentPosition();
            }

            // 丢弃已经处理的数据
            config.receData.RemoveRange(0, indexED);
            config.IsReading = false;
        }

        private static void getCurrentSpeed()
        {
            // 申明正在设置数据
            config.IsSettingSpeed = true;
            while (config.IsGettingSpeed) ;

            // 接收数据
            int H, L;

            H = config.Frame[2];
            L = config.Frame[3];
            config.Speed.HeadL = (H << 8) | L;

            H = config.Frame[5];
            L = config.Frame[6];
            config.Speed.TailL = (H << 8) | L;

            H = config.Frame[8];
            L = config.Frame[9];
            config.Speed.TailR = (H << 8) | L;

            H = config.Frame[11];
            L = config.Frame[12];
            config.Speed.HeadR = (H << 8) | L;

            if (config.Frame[1] == 0) { config.Speed.HeadL = -config.Speed.HeadL; }
            if (config.Frame[4] == 0) { config.Speed.TailL = -config.Speed.TailL; }
            if (config.Frame[7] == 0) { config.Speed.TailR = -config.Speed.TailR; }
            if (config.Frame[10] == 0) { config.Speed.HeadR = -config.Speed.HeadR; }
            
            int StopAmount = 0;
            if (config.Speed.HeadL == 0) { StopAmount++; }
            if (config.Speed.HeadR == 0) { StopAmount++; }
            if (config.Speed.TailL == 0) { StopAmount++; }
            if (config.Speed.TailR == 0) { StopAmount++; }
            if (StopAmount >= 2) { config.Speed.HeadL = 0; config.Speed.HeadR = 0; config.Speed.TailL = 0; config.Speed.TailR = 0; }

            // 设置完毕
            config.IsSettingSpeed = false;
        }
        private static void getCurrentPosition()
        {
            int currSpeed_HL = -config.Speed.HeadL;
            int currSpeed_HR = config.Speed.HeadR;
            int currSpeed_TL = -config.Speed.TailL;
            int currSpeed_TR = config.Speed.TailR;
            
            const double R = 0.1015, PI = 3.1416, Lx = 0.1825, Ly = 0.2950;
            
            double X = (double)(currSpeed_TL + currSpeed_HR - currSpeed_HL - currSpeed_TR) * 2 * PI * R / 1000 / 4;
            double Y = (double)(currSpeed_TL + currSpeed_TR + currSpeed_HL + currSpeed_HR) * 2 * PI * R / 1000 / 4;
            double A = (double)(currSpeed_TR + currSpeed_HR - currSpeed_TL - currSpeed_HL) * 2 * PI * R / 1000 / (4 * (Lx + Ly));

            X = X / 4.077;
            Y = Y / 4.077;
            A = A / 4.077;

            //X = FilterX.Start(X);
            //Y = FilterY.Start(Y);
            //A = FilterA.Start(A);

            config.IsSetting = true;
            while (config.IsGetting) ;
            
            config.Current.x += X * Math.Cos(config.Current.rCar) - Y * Math.Sin(config.Current.rCar);
            config.Current.y += X * Math.Sin(config.Current.rCar) + Y * Math.Cos(config.Current.rCar);
            config.Current.rCar += A;
            config.Current.aCar = config.Current.rCar * 180 / Math.PI;

            config.IsSetting = false;
        }
    }
}
