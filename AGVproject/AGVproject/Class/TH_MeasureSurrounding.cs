using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using SCIP_library;

namespace AGVproject.Class
{
    class TH_MeasureSurrounding
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public static bool IsOpen { get { return config.Port != null && config.Port.IsOpen && !config.IsClosing; } }
        public static bool Abort { get; set; }
        
        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////
        
        private static CONFIG config;
        private struct CONFIG
        {
            public SerialPort Port;
            public System.Threading.Thread Thread;
            
            public bool IsClosing;
            public bool IsReading;
            public bool IsSetting;
            public bool IsGetting;

            public long TimeStamp;

            public CoordinatePoint.POINT LastCarPosition;
            public CoordinatePoint.POINT NextCarPosition;

            public List<CoordinatePoint.POINT> CurrentSurrounding;
            public List<CoordinatePoint.POINT> NextSurrounding;
        }

        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public static bool Open()
        {
            // 已经打开
            while (config.IsClosing) ; if (IsOpen) { return true; }
            
            // 打开线程
            if (config.Thread == null)
            {
                Abort = false;
                config.Thread = new System.Threading.Thread(MeasureSurrounding);
                config.Thread.Start();
            }

            // 读取串口配置
            string portName = "";
            if (Form_Start.config.SelectedUrgPortName != -1)
            { portName = Form_Start.config.UrgPortName[Form_Start.config.SelectedUrgPortName]; }

            int baudRate = -1;
            if (Form_Start.config.SelectedUrgBaudRate != -1)
            { baudRate = Form_Start.config.UrgBaudRate[Form_Start.config.SelectedUrgBaudRate]; }

            // 尝试打开串口并开启线程
            try
            {
                // 初始化线程
                Initial_TH_MeasureSurrounding();

                // 打开串口
                config.Port = new SerialPort(portName, baudRate);
                config.Port.NewLine = "\n\n";
                config.Port.Open();
                config.Port.Write(SCIP_Writer.SCIP2());
                config.Port.ReadLine();
                config.Port.Write(SCIP_Writer.MD(Hardware_URG.ReceiveBG, Hardware_URG.ReceiveED));
                config.Port.ReadLine();
                
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

            // 等待结束
            while (config.IsReading) ;
            
            // 关闭串口
            try
            {
                config.Port.Write(SCIP_Writer.QT());
                config.Port.ReadLine();
                config.Port.Close();
                config.IsClosing = false;
                return true;
            }
            catch { config.IsClosing = false; return false; }
        }

        public static List<CoordinatePoint.POINT> getSurrounding()
        {
            List<CoordinatePoint.POINT> Surrounding = new List<CoordinatePoint.POINT>();

            while (config.IsSetting) ;
            config.IsGetting = true;

            foreach (CoordinatePoint.POINT point in config.CurrentSurrounding)
            { Surrounding.Add(point); }

            config.IsGetting = false;
            return Surrounding;
        }
        public static void clearSurrounding()
        {
            while (config.IsSetting) ;
            while (config.IsGetting) ;
            config.IsSetting = true;
            config.IsGetting = true;

            config.CurrentSurrounding = new List<CoordinatePoint.POINT>();
            config.NextSurrounding = new List<CoordinatePoint.POINT>();

            config.IsSetting = false;
            config.IsGetting = false;
        }
        public static bool IsLimitPosition(CoordinatePoint.POINT point)
        {
            bool inX = Hardware_PlatForm.AxisSideL < point.x && point.x < Hardware_PlatForm.AxisSideR;
            bool inY = Hardware_PlatForm.AxisSideD < point.y && point.y < Hardware_PlatForm.AxisSideU;

            return inX && inY;
        }
        
        public static List<CoordinatePoint.POINT> getSurroundingX(double BG, double ED)
        {
            while (config.IsSetting) ;
            config.IsGetting = true;

            List<CoordinatePoint.POINT> points = CoordinatePoint.SelectX(BG, ED, config.CurrentSurrounding);

            config.IsGetting = false;
            return points;
        }
        public static List<CoordinatePoint.POINT> getSurroundingY(double BG, double ED)
        {
            while (config.IsSetting) ;
            config.IsGetting = true;

            List<CoordinatePoint.POINT> points = CoordinatePoint.SelectY(BG, ED, config.CurrentSurrounding);

            config.IsGetting = false;
            return points;
        }
        public static List<CoordinatePoint.POINT> getSurroundingA(double BG, double ED)
        {
            while (config.IsSetting) ;
            config.IsGetting = true;

            List<CoordinatePoint.POINT> points = CoordinatePoint.SelectA(BG, ED, config.CurrentSurrounding);

            config.IsGetting = false;
            return points;
        }
        public static List<CoordinatePoint.POINT> getSurroundingD(double BG, double ED)
        {
            while (config.IsSetting) ;
            config.IsGetting = true;

            List<CoordinatePoint.POINT> points = CoordinatePoint.SelectD(BG, ED, config.CurrentSurrounding);

            config.IsGetting = false;
            return points;
        }
        public static List<CoordinatePoint.POINT> getSurroundingR(double BG, double ED)
        {
            while (config.IsSetting) ;
            config.IsGetting = true;

            List<CoordinatePoint.POINT> points = CoordinatePoint.SelectR(BG, ED, config.CurrentSurrounding);

            config.IsGetting = false;
            return points;
        }

        public static bool IsEmptyX(double disEmpty, List<CoordinatePoint.POINT> points)
        {
            if (points == null) { return true; }

            foreach (CoordinatePoint.POINT point in points)
            {
                double x = Math.Abs(point.x);
                if (x < disEmpty) { return false; }
            }

            return true;
        }
        public static bool IsEmptyY(double disEmpty, List<CoordinatePoint.POINT> points)
        {
            if (points == null) { return true; }

            foreach (CoordinatePoint.POINT point in points)
            {
                double y = Math.Abs(point.y);
                if (y < disEmpty) { return false; }
            }

            return true;
        }
        public static bool IsEmptyA(double disEmpty, List<CoordinatePoint.POINT> points)
        {
            if (points == null) { return true; }

            foreach (CoordinatePoint.POINT point in points)
            {
                double a = Math.Abs(point.a);
                if (a < disEmpty) { return false; }
            }

            return true;
        }
        public static bool IsEmptyD(double disEmpty, List<CoordinatePoint.POINT> points)
        {
            if (points == null) { return true; }

            foreach (CoordinatePoint.POINT point in points)
            {
                double d = Math.Abs(point.d);
                if (d < disEmpty) { return false; }
            }

            return true;
        }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void MeasureSurrounding()
        {
            while (true)
            {
                // 外部要求关闭线程
                if (Abort) { config.Thread.Abort(); Abort = false; return; }
                
                // 延时
                System.Threading.Thread.Sleep(100);
                
                // 加入历史数据、当前数据、超声波数据
                config.NextSurrounding = new List<CoordinatePoint.POINT>();
                
                getPreviousSurrounding();
                getCurrentSurrounding();
                getUltraSonicSurrounding();

                // 更新当前环境
                config.IsSetting = true;
                while (config.IsGetting) ;
                config.CurrentSurrounding = config.NextSurrounding;
                config.IsSetting = false;
            }
        }

        private static void Initial_TH_MeasureSurrounding()
        {
            config.IsClosing = false;
            config.IsReading = false;
            config.IsSetting = false;
            config.IsGetting = false;
            
            config.CurrentSurrounding = new List<CoordinatePoint.POINT>();
            config.NextSurrounding = new List<CoordinatePoint.POINT>();
        }

        private static List<long> portDataReceived()
        {
            // 串口已经关闭或正在关闭
            List<long> distance = new List<long>();
            if (config.IsClosing || !IsOpen) { return distance; }

            // 正在读取
            config.IsReading = true;

            // 读取数据
            config.Port.DiscardInBuffer();
            string receiveData = config.Port.ReadLine();

            // 读取失败则返回
            if (!SCIP_Reader.MD(receiveData, ref config.TimeStamp, ref distance)) { return new List<long>(); }
            if (distance.Count < Hardware_URG.ReceiveED) { return new List<long>(); }
            
            // 数据裁剪
            distance.RemoveRange(Hardware_URG.CutED, distance.Count - Hardware_URG.CutED);
            distance.RemoveRange(0, Hardware_URG.CutBG);

            config.IsReading = false;
            return distance;
        }
        private static List<long> Filter(List<long> distance)
        {
            // 滤除过近的点
            for (int i = 0; i < distance.Count; i++) { if (distance[i] < 100) { distance[i] = 0; } }
            //return distance;

            // 滤除跳变
            int N_nege = 20, N = distance.Count;
            double floatError = 100;

            List<long> diff = new List<long>();
            for (int i = 1; i < N; i++) { diff.Add(Math.Abs(distance[i] - distance[i - 1])); }

            List<int> P = new List<int>();
            for (int i = 0; i < N - 1; i++) { if (diff[i] > floatError) { P.Add(i); } }

            for (int i = 0; i < P.Count - 1; i++)
            {
                if (P[i + 1] - P[i] > N_nege) { continue; }
                for (int j = P[i] + 1; j <= P[i + 1]; j++) { distance[j] = 0; }
            }

            // 返回
            return distance;
        }

        private static void getPreviousSurrounding()
        {
            // 不存在历史信息
            if (config.CurrentSurrounding == null) { return; }

            // 坐标偏移信息
            config.NextCarPosition = TH_MeasurePosition.getPosition();
            double xMove = config.NextCarPosition.x - config.LastCarPosition.x;
            double yMove = config.NextCarPosition.y - config.LastCarPosition.y;
            double rMove = config.NextCarPosition.rCar - config.LastCarPosition.rCar;
            config.LastCarPosition = config.NextCarPosition;

            // 复制历史信息
            foreach (CoordinatePoint.POINT point in config.CurrentSurrounding) { config.NextSurrounding.Add(point); }
            
            // 挑选历史数据
            for (int i = config.NextSurrounding.Count - 1; i >= 0; i--)
            {
                config.NextSurrounding[i] = CoordinatePoint.TransformCoordinate(config.NextSurrounding[i], xMove, yMove, rMove);
                if (config.NextSurrounding[i].a >= 0 && config.NextSurrounding[i].a <= 180)
                { config.NextSurrounding.RemoveAt(i); continue; }
                if (IsLimitPosition(config.NextSurrounding[i])) { config.NextSurrounding.RemoveAt(i); }
            }

            // 数据稀释
            config.NextSurrounding = CoordinatePoint.SortA(config.NextSurrounding);
            int N = config.NextSurrounding.Count - 2;
            for (int i = N; i >= 0; i--)
            {
                double angle = Math.Abs(config.NextSurrounding[i].a - config.NextSurrounding[i + 1].a);
                if (angle > Hardware_URG.AnglePace) { continue; }

                if (config.NextSurrounding[i].d > config.NextSurrounding[i + 1].d) { config.NextSurrounding.RemoveAt(i);continue; }
                config.NextSurrounding.RemoveAt(i + 1);
            }
        }
        private static void getCurrentSurrounding()
        {
            // 如果串口关闭，返回
            if (!IsOpen || config.IsClosing) { return; }

            // 获取
            List<long> distance = portDataReceived();
            if (distance.Count == 0) { return; }

            // 滤波
            distance = Filter(distance);

            // 添加数据
            for (int i = 0; i < distance.Count; i++)
            {
                if (distance[i] == 0) { continue; }

                double angle = Hardware_URG.AngleStart + i * Hardware_URG.AnglePace;
                config.NextSurrounding.Add(CoordinatePoint.Create_DA(distance[i], angle));
            }
        }
        private static void getUltraSonicSurrounding()
        {
            if (!TH_SendCommand.IsOpen) { return; }

            // 车头超声波数据
            CoordinatePoint.POINT Head_L_X = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Head_L_X);
            CoordinatePoint.POINT Head_L_Y = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Head_L_Y);
            CoordinatePoint.POINT Head_R_X = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Head_R_X);
            CoordinatePoint.POINT Head_R_Y = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Head_R_Y);

            if (!CoordinatePoint.IsNegPoint(Head_L_X)) { config.NextSurrounding.Add(Head_L_X); }
            if (!CoordinatePoint.IsNegPoint(Head_L_Y)) { config.NextSurrounding.Add(Head_L_Y); }
            if (!CoordinatePoint.IsNegPoint(Head_R_X)) { config.NextSurrounding.Add(Head_R_X); }
            if (!CoordinatePoint.IsNegPoint(Head_R_Y)) { config.NextSurrounding.Add(Head_R_Y); }

            // 车尾超声波数据
            CoordinatePoint.POINT Tail_L_X = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Tail_L_X);
            CoordinatePoint.POINT Tail_L_Y = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Tail_L_Y);
            CoordinatePoint.POINT Tail_R_X = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Tail_R_X);
            CoordinatePoint.POINT Tail_R_Y = TH_SendCommand.getUltraSonicPoint(TH_SendCommand.Sonic.Tail_R_Y);

            if (!CoordinatePoint.IsNegPoint(Tail_L_X)) { config.NextSurrounding.Add(Tail_L_X); }
            if (!CoordinatePoint.IsNegPoint(Tail_L_Y)) { config.NextSurrounding.Add(Tail_L_Y); }
            if (!CoordinatePoint.IsNegPoint(Tail_R_X)) { config.NextSurrounding.Add(Tail_R_X); }
            if (!CoordinatePoint.IsNegPoint(Tail_R_Y)) { config.NextSurrounding.Add(Tail_R_Y); }
        }
    }
}
