using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGVproject.Class
{
    class TH_MeasurePosition
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public static bool IsOpen { get { return config.Port != null && config.Port.IsOpen && !config.IsClosing; } }
        
        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////
        
        private static CONFIG config;
        private struct CONFIG
        {
            public SerialPort Port;

            public bool IsReading;
            public bool IsClosing;
            public bool IsGetting;
            public bool IsSetting;

            public List<byte> receData;
            public byte[] Frame;
            public SPEED Speed;
            public CoordinatePoint.POINT Current;

            public struct SPEED { public int HeadL, HeadR, TailL, TailR; }
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

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
        
        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void Initial_TH_MeasurePosition()
        {
            config.IsReading = false;
            config.IsClosing = false;
            config.IsSetting = false;
            config.IsGetting = false;

            config.receData = new List<byte>();
            config.Frame = new byte[14];

            config.Speed = new CONFIG.SPEED();
            config.Current = new CoordinatePoint.POINT();
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
            if (StopAmount < 2) { return; }

            config.Speed.HeadL = 0;
            config.Speed.HeadR = 0;
            config.Speed.TailL = 0;
            config.Speed.TailR = 0;
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
