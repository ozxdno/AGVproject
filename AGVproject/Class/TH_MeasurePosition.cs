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

        public bool IsOpen { get { return locateport != null && locateport.IsOpen; } }
        public bool IsClose { get { return locateport == null || !locateport.IsOpen; } }

        public static TH_DATA TH_data;
        
        public struct TH_DATA
        {
            public string PortName;
            public int BaudRate;

            public bool IsSetting;
            public bool IsGetting;

            public double X;
            public double Y;
            public double A;
        }

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static SerialPort locateport;
        private static List<byte> receData;
        private static int receLength;
        private static List<KeyPoint> Track;
        private static PORT_CONFIG portConfig;

        private struct PORT_CONFIG
        {
            public bool IsReading;
            public bool IsClosing;
            public bool IsFilling;

            public bool MarkTrack;
            public bool Stopping;

            public SPEED CurrSpeed;
            public SPEED PrevSpeed;

            public List<byte> Frame;
        }
        private struct SPEED
        {
            public int HeadL;
            public int HeadR;
            public int TailL;
            public int TailR;
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        public bool Open(bool CreatePort = false)
        {
            // 建立串口
            if (IsOpen) { return true; }
            if (CreatePort || locateport == null) { locateport = new SerialPort(TH_data.PortName, TH_data.BaudRate); }

            try
            {
                // 初始化线程
                Initial_TH_MeasurePosition();

                // 打开串口
                locateport.ReceivedBytesThreshold = 1;
                locateport.DataReceived -= portDataReceived;
                locateport.DataReceived += portDataReceived;
                locateport.Open();
                
                return true;
            }
            catch { return false; }
        }
        public bool Close()
        {
            if (!locateport.IsOpen) { return true; }
            portConfig.IsClosing = true;

            // 等待读取完毕
            while (portConfig.IsReading) { System.Windows.Forms.Application.DoEvents(); }

            // 关闭
            try
            {
                locateport.Close();
                portConfig.IsClosing = false;
                return true;
            }
            catch
            {
                portConfig.IsClosing = false;
                return false;
            }
        }
        
        public void StartMarkTrack()
        {
            Track = new List<KeyPoint>();
            portConfig.MarkTrack = true;
        }
        public void OutputTrack()
        {
            portConfig.MarkTrack = false;

            // 整理输出数据

            // 输出文件
        }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private void Initial_TH_MeasurePosition()
        {
            TH_data.IsSetting = false;
            TH_data.IsGetting = false;

            receData = new List<byte>();
            receLength = 14;

            portConfig.IsReading = false;
            portConfig.IsClosing = false;
            portConfig.IsFilling = false;

            portConfig.MarkTrack = false;
            portConfig.Stopping = false;

            portConfig.CurrSpeed = new SPEED();
            portConfig.PrevSpeed = new SPEED();

            portConfig.Frame = new List<byte>();
        }

        private void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭
            if (portConfig.IsClosing) { return; }
            
            // 读取数据
            portConfig.IsReading = true;
            try
            {
                int receLength = locateport.BytesToRead;
                byte[] temp_receData = new byte[receLength];
                locateport.Read(temp_receData, 0, receLength);

                // 接在原数据末尾
                for (int i = 0; i < receLength; i++) { receData.Add(temp_receData[i]); }
            }
            catch { portConfig.IsReading = false; return; }
            portConfig.IsReading = false;

            // 寻找完整的帧，并对应做出处理
            int indexBG = -1;
            for (int i = 0; i < receData.Count; i++)
            {
                // 找到完整的一帧
                if (receData[i] != 0xAA) { continue; }
                indexBG = i;
                if (i + receLength > receData.Count) { break; }
                if (receData[indexBG + receLength] != 0xBB) { continue; }
                portConfig.Frame.Clear();
                for (int j = 0; j < receLength; j++) { portConfig.Frame.Add(receData[indexBG + j]); }

                // 对这一帧进行处理
                getCurrentSpeed();
                getCurrentPosition();
                if (portConfig.MarkTrack) { autoOutputTrack(); }
            }

            // 丢弃已经处理的数据
            if (indexBG == -1) { receData.Clear(); return; }
            receData.RemoveRange(0, indexBG);
        }

        private void getCurrentSpeed()
        {
            int H, L;

            H = portConfig.Frame[2];
            L = portConfig.Frame[3];
            portConfig.CurrSpeed.HeadL = (H << 8) | L;

            H = portConfig.Frame[5];
            L = portConfig.Frame[6];
            portConfig.CurrSpeed.TailL = (H << 8) | L;

            H = portConfig.Frame[8];
            L = portConfig.Frame[9];
            portConfig.CurrSpeed.TailR = (H << 8) | L;

            H = portConfig.Frame[11];
            L = portConfig.Frame[12];
            portConfig.CurrSpeed.HeadR = (H << 8) | L;

            if (portConfig.Frame[1] == 1) { portConfig.CurrSpeed.HeadL = -portConfig.CurrSpeed.HeadL; }
            if (portConfig.Frame[4] == 1) { portConfig.CurrSpeed.HeadL = -portConfig.CurrSpeed.HeadL; }
            if (portConfig.Frame[7] == 0) { portConfig.CurrSpeed.HeadL = -portConfig.CurrSpeed.HeadL; }
            if (portConfig.Frame[10] == 0) { portConfig.CurrSpeed.HeadL = -portConfig.CurrSpeed.HeadL; }
        }
        private void getCurrentPosition()
        {
            double currSpeed_HL = portConfig.CurrSpeed.HeadL;
            double currSpeed_HR = portConfig.CurrSpeed.HeadR;
            double currSpeed_TL = portConfig.CurrSpeed.TailL;
            double currSpeed_TR = portConfig.CurrSpeed.TailR;

            double AverageSpeed = (currSpeed_HL + currSpeed_HR + currSpeed_TL + currSpeed_TR) / 4.0;
            currSpeed_HL = AverageSpeed;
            currSpeed_HR = AverageSpeed;
            currSpeed_TL = AverageSpeed;
            currSpeed_TL = AverageSpeed;

            const double R = 0.1015, PI = Math.PI, Lx = 0.1825, Ly = 0.2950;
            
            double X = (currSpeed_HL + currSpeed_HR - currSpeed_TL - currSpeed_TR) * 2 * PI * R / 4;
            double Y = (currSpeed_HL + currSpeed_TL + currSpeed_TR + currSpeed_HR) * 2 * PI * R / 4;
            double A = (currSpeed_TL + currSpeed_HR - currSpeed_HL - currSpeed_TR) * 2 * PI * R / (4 * (Lx + Ly));

            X = X / 4.077;
            Y = Y / 4.077;
            A = A / 4.077;

            TH_data.IsSetting = true;
            while (TH_data.IsGetting) ;

            TH_data.X += X * Math.Cos(TH_data.A) - Y * Math.Sin(TH_data.A);
            TH_data.X += X * Math.Sin(TH_data.A) + Y * Math.Cos(TH_data.A);
            TH_data.A += A;

            TH_data.IsSetting = false;
        }
        private void autoOutputTrack()
        {
            KeyPoint CurrentP = new KeyPoint();
            CurrentP.X = TH_data.X;
            CurrentP.Y = TH_data.Y;
            CurrentP.A = TH_data.A;

            // 刚开始
            if (Track.Count == 0) { Track.Add(CurrentP); portConfig.Stopping = true; }

            // 停止时自动添加关键点
            int Error_Stop = 0;
            if (Math.Abs(portConfig.CurrSpeed.HeadL) < Error_Stop &&
                Math.Abs(portConfig.CurrSpeed.HeadL) < Error_Stop &&
                Math.Abs(portConfig.CurrSpeed.HeadL) < Error_Stop &&
                Math.Abs(portConfig.CurrSpeed.HeadL) < Error_Stop &&
                Math.Abs(portConfig.PrevSpeed.HeadL) > Error_Stop &&
                Math.Abs(portConfig.PrevSpeed.HeadR) > Error_Stop &&
                Math.Abs(portConfig.PrevSpeed.TailL) > Error_Stop &&
                Math.Abs(portConfig.PrevSpeed.TailR) > Error_Stop)
            { if (!portConfig.Stopping) { portConfig.Stopping = true; Track.Add(CurrentP); } return; }
            portConfig.Stopping = false;

            // 速度变化量
            double Acc_HeadL = portConfig.CurrSpeed.HeadL - portConfig.PrevSpeed.HeadL;
            double Acc_HeadR = portConfig.CurrSpeed.HeadR - portConfig.PrevSpeed.HeadR;
            double Acc_TailL = portConfig.CurrSpeed.TailL - portConfig.PrevSpeed.TailL;
            double Acc_TailR = portConfig.CurrSpeed.TailR - portConfig.PrevSpeed.TailR;

            // 匀速则不更新路径
            int Error_KeepSpeed = 0;
            if (Math.Abs(Acc_HeadL) < Error_KeepSpeed &&
                Math.Abs(Acc_HeadR) < Error_KeepSpeed &&
                Math.Abs(Acc_TailL) < Error_KeepSpeed &&
                Math.Abs(Acc_TailR) < Error_KeepSpeed) { return; }

            // 匀变速也不更新路径
            int Error_AccSpeed = 0;
            if (Math.Abs(Acc_HeadR - Acc_HeadL) < Error_AccSpeed &&
                Math.Abs(Acc_TailL - Acc_HeadL) < Error_AccSpeed &&
                Math.Abs(Acc_TailR - Acc_HeadL) < Error_AccSpeed) { return; }

            // 添加新的关键点
            Track.Add(CurrentP);
        }
    }
}
