using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGVproject.Class
{
    /// <summary>
    /// 控制串口发送命令线程，包含发送命令与接收返回的超声波数据。
    /// </summary>
    class TH_SendCommand
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        /// <summary>
        /// 串口当前状态
        /// </summary>
        public static bool IsOpen { get { return config.Port != null && config.Port.IsOpen && !config.IsClosing; } }
        /// <summary>
        /// 线程控制命令
        /// </summary>
        public static bool Abort;
        /// <summary>
        /// 发送命令的时间间隔，控制时间
        /// </summary>
        public static int TimeForControl;

        /// <summary>
        /// 期望控制速度
        /// </summary>
        public static int xSpeed = 0, ySpeed = 0, aSpeed = 0;
        
        
        /// <summary>
        /// 超声波传感器编号
        /// </summary>
        public enum Sonic { Head_L_X,Head_L_Y, Head_R_Y, Head_R_X,Tail_R_X, Tail_R_Y, Tail_L_Y, Tail_L_X } 

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////
        
        private static CONFIG config;
        private struct CONFIG
        {
            public SerialPort Port;
            public System.Threading.Thread Thread;
            
            public bool IsReading;
            public bool IsClosing;
            public bool IsSetting;
            public bool IsGetting;

            public bool IsSettingCommand;
            public bool IsGettingCommand;

            public byte[] Command;
            public List<byte> Receive;
            public byte[] Frame;

            public int[] SonicData;

            public SPEED TargetSpeed;
            public SPEED CurrentSpeed;

            public struct SPEED { public int xSpeed, ySpeed, aSpeed; }
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////
        
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns>串口是否成功打开</returns>
        public static bool Open()
        {
            // 如果已经打开
            while (config.IsClosing) ; if (IsOpen) { return true; }

            // 获取串口参数
            string portName = "";
            if (Form_Start.config.SelectedControlPortName != -1)
            { portName = Form_Start.config.ConPortName[Form_Start.config.SelectedControlPortName]; }

            int baudRate = -1;
            if (Form_Start.config.SelectedControlBaudRate != -1)
            { baudRate = Form_Start.config.ConBaudRate[Form_Start.config.SelectedControlBaudRate]; }

            // 尝试打开串口并开启线程
            try
            {
                // 初始化
                Initial_TH_SendCommand();

                // 打开串口
                config.Port = new SerialPort(portName, baudRate);
                config.Port.DataReceived -= portDataReceived;
                config.Port.DataReceived += portDataReceived;
                config.Port.Open();

                // 打开线程
                Abort = true;
                while (config.Thread != null && config.Thread.ThreadState == System.Threading.ThreadState.Running) ;
                Abort = false;

                config.Thread = new System.Threading.Thread(SendCommand);
                config.Thread.Start();
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>串口是否成功关闭</returns>
        public static bool Close()
        {
            // 串口已经关闭
            if (!IsOpen || config.IsClosing) { return true; }

            // 正在关闭，等待读取完毕
            config.IsClosing = true;
            while (config.IsReading) ;

            // 尝试关闭
            try { config.Port.Close(); config.IsClosing = false; return true; }
            catch { config.IsClosing = false; return false; }
        }
        
        /// <summary>
        /// 小车行进控制
        /// </summary>
        /// <param name="xSpeed">X 方向速度 单位：mm</param>
        /// <param name="ySpeed">Y 方向速度 单位：mm</param>
        /// <param name="aSpeed">A 方向速度 单位：0.01 度</param>
        /// <returns></returns>
        public static bool AGV_MoveControl_0x70(int xSpeed, int ySpeed, int aSpeed)
        {
            // 限幅
            if (xSpeed > 800) { xSpeed = 800; }
            if (xSpeed < -800) { xSpeed = -800; }
            if (ySpeed > 800) { ySpeed = 800; }
            if (ySpeed < -800) { ySpeed = -800; }
            if (aSpeed > 7200) { aSpeed = 7200; }
            if (aSpeed < -7200) { aSpeed = -7200; }

            // 限跳变
            //if (xSpeed - TH_SendCommand.xSpeed > 50) { xSpeed = TH_SendCommand.xSpeed + 50; }
            //if (TH_SendCommand.xSpeed - xSpeed > 50) { xSpeed = TH_SendCommand.xSpeed - 50; }
            //if (ySpeed - TH_SendCommand.ySpeed > 50) { ySpeed = TH_SendCommand.ySpeed + 50; }
            //if (TH_SendCommand.ySpeed - ySpeed > 50) { ySpeed = TH_SendCommand.ySpeed - 50; }
            //if (aSpeed - TH_SendCommand.aSpeed > 100) { aSpeed = TH_SendCommand.aSpeed + 100; }
            //if (TH_SendCommand.aSpeed - aSpeed > 100) { aSpeed = TH_SendCommand.aSpeed - 100; }

            // 记录给出速度
            TH_SendCommand.xSpeed = xSpeed;
            TH_SendCommand.ySpeed = ySpeed;
            TH_SendCommand.aSpeed = aSpeed;

            // 给速度
            int speed = 0, direction = 0, rotate = (int)Math.Round(aSpeed * 3.14159 / 180);
            if (rotate < 0) { rotate = 128 - rotate; }
            
            speed = (int)(Math.Sqrt(xSpeed * xSpeed + ySpeed * ySpeed));
            if (xSpeed == 0 && ySpeed > 0) { direction = 90; }
            if (xSpeed == 0 && ySpeed < 0) { direction = 270; }
            if (xSpeed > 0 && ySpeed == 0) { direction = 0; }
            if (xSpeed < 0 && ySpeed == 0) { direction = 180; }
            if (xSpeed != 0 && ySpeed != 0)
            {
                double angle = Math.Atan( (Math.Abs((double)ySpeed)) / Math.Abs((double)xSpeed));
                direction = (int)((angle) * 180 / Math.PI);
                if (xSpeed > 0 && ySpeed > 0) { }
                if (xSpeed > 0 && ySpeed < 0) { direction = 360 - direction; }
                if (xSpeed < 0 && ySpeed > 0) { direction = 180 - direction; }
                if (xSpeed < 0 && ySpeed < 0) { direction = 180 + direction; }
            }
            
            // 填充 0x70 命令
            byte[] ControlCommand = new byte[11];
            ControlCommand[0] = 0xf1;
            ControlCommand[1] = 0x70;
            ControlCommand[2] = (byte)(speed >> 8);
            ControlCommand[3] = (byte)(speed);
            ControlCommand[4] = (byte)(direction >> 8);
            ControlCommand[5] = (byte)(direction);
            ControlCommand[6] = (byte)(rotate);
            ControlCommand[7] = 0x00;
            Fill_CheckBytes(ref ControlCommand);

            // 写入
            config.IsSettingCommand = true;
            while (config.IsGettingCommand) ;
            
            config.Command = ControlCommand;

            config.IsSettingCommand = false;
            return true;
        }

        /// <summary>
        /// 获取所有超声波传感器所测得的距离信息，无效值返回 0， 单位：mm
        /// </summary>
        /// <returns></returns>
        public static int[] getUltraSonicData()
        {
            int[] SonicData;

            while (config.IsSetting) ;
            config.IsGetting = true;

            SonicData = config.SonicData;

            config.IsGetting = false;
            return SonicData;
        }
        /// <summary>
        /// 获取超声波传感器所测得的距离信息，无效值返回 0， 单位：mm
        /// </summary>
        /// <param name="No">超声波编号</param>
        /// <returns></returns>
        public static int getUltraSonicData(Sonic No)
        {
            // 取数据
            int SonicData = 0;

            while (config.IsSetting) ;
            config.IsGetting = true;

            SonicData = config.SonicData[(int)No];

            config.IsGetting = false;

            // 判断数据是否有效
            if (No == Sonic.Head_L_X)
            {
                if (SonicData < Hardware_UltraSonic.Head_L_X.min || SonicData > Hardware_UltraSonic.Head_L_X.max)
                { return 0; }
            }
            if (No == Sonic.Head_L_Y)
            {
                if (SonicData < Hardware_UltraSonic.Head_L_Y.min || SonicData > Hardware_UltraSonic.Head_L_Y.max)
                { return 0; }
            }
            if (No == Sonic.Head_R_X)
            {
                if (SonicData < Hardware_UltraSonic.Head_R_X.min || SonicData > Hardware_UltraSonic.Head_R_X.max)
                { return 0; }
            }
            if (No == Sonic.Head_R_Y)
            {
                if (SonicData < Hardware_UltraSonic.Head_R_Y.min || SonicData > Hardware_UltraSonic.Head_R_Y.max)
                { return 0; }
            }
            if (No == Sonic.Tail_L_X)
            {
                if (SonicData < Hardware_UltraSonic.Tail_L_X.min || SonicData > Hardware_UltraSonic.Tail_L_X.max)
                { return 0; }
            }
            if (No == Sonic.Tail_L_Y)
            {
                if (SonicData < Hardware_UltraSonic.Tail_L_Y.min || SonicData > Hardware_UltraSonic.Tail_L_Y.max)
                { return 0; }
            }
            if (No == Sonic.Tail_R_X)
            {
                if (SonicData < Hardware_UltraSonic.Tail_R_X.min || SonicData > Hardware_UltraSonic.Tail_R_X.max)
                { return 0; }
            }
            if (No == Sonic.Tail_R_Y)
            {
                if (SonicData < Hardware_UltraSonic.Tail_R_Y.min || SonicData > Hardware_UltraSonic.Tail_R_Y.max)
                { return 0; }
            }

            // 返回
            return SonicData;
        }
        /// <summary>
        /// 获取超声波传感器所测得的距离信息，无效值返回 0， 单位：mm
        /// </summary>
        /// <param name="No">超声波编号</param>
        /// <returns></returns>
        public static int getUltraSonicData(int No)
        {
            return getUltraSonicData((Sonic)No);
        }

        /// <summary>
        /// 获取超声波传感器所得到的点（无效点返回 NegPoint）
        /// </summary>
        /// <param name="No">超声波传感器编号</param>
        /// <returns></returns>
        public static CoordinatePoint.POINT getUltraSonicPoint(Sonic No)
        {
            CoordinatePoint.POINT point = CoordinatePoint.getNegPoint();

            while (config.IsSetting) ;
            config.IsGetting = true;
            int SonicData = config.SonicData[(int)No];
            config.IsGetting = false;

            if (No == Sonic.Head_L_X)
            {
                if (SonicData < Hardware_UltraSonic.Head_L_X.min || SonicData > Hardware_UltraSonic.Head_L_X.max)
                { return point; }

                point.x = Hardware_UltraSonic.Head_L_X.x - SonicData;
                point.y = Hardware_UltraSonic.Head_L_X.y;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Head_L_Y)
            {
                if (SonicData < Hardware_UltraSonic.Head_L_Y.min || SonicData > Hardware_UltraSonic.Head_L_Y.max)
                { return point; }

                point.x = Hardware_UltraSonic.Head_L_Y.x;
                point.y = Hardware_UltraSonic.Head_L_Y.y + SonicData;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Head_R_X)
            {
                if (SonicData < Hardware_UltraSonic.Head_R_X.min || SonicData > Hardware_UltraSonic.Head_R_X.max)
                { return point; }

                point.x = Hardware_UltraSonic.Head_R_X.x + SonicData;
                point.y = Hardware_UltraSonic.Head_R_X.y;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Head_R_Y)
            {
                if (SonicData < Hardware_UltraSonic.Head_R_Y.min || SonicData > Hardware_UltraSonic.Head_R_Y.max)
                { return point; }

                point.x = Hardware_UltraSonic.Head_R_Y.x;
                point.y = Hardware_UltraSonic.Head_R_Y.y + SonicData;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Tail_L_X)
            {
                if (SonicData < Hardware_UltraSonic.Tail_L_X.min || SonicData > Hardware_UltraSonic.Tail_L_X.max)
                { return point; }

                point.x = Hardware_UltraSonic.Tail_L_X.x - SonicData;
                point.y = Hardware_UltraSonic.Tail_L_X.y;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Tail_L_Y)
            {
                if (SonicData < Hardware_UltraSonic.Tail_L_Y.min || SonicData > Hardware_UltraSonic.Tail_L_Y.max)
                { return point; }

                point.x = Hardware_UltraSonic.Tail_L_Y.x;
                point.y = Hardware_UltraSonic.Tail_L_Y.y - SonicData;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Tail_R_X)
            {
                if (SonicData < Hardware_UltraSonic.Tail_R_X.min || SonicData > Hardware_UltraSonic.Tail_R_X.max)
                { return point; }

                point.x = Hardware_UltraSonic.Tail_R_X.x + SonicData;
                point.y = Hardware_UltraSonic.Tail_R_X.y;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }
            if (No == Sonic.Tail_R_Y)
            {
                if (SonicData < Hardware_UltraSonic.Tail_R_Y.min || SonicData > Hardware_UltraSonic.Tail_R_Y.max)
                { return point; }

                point.x = Hardware_UltraSonic.Tail_R_Y.x;
                point.y = Hardware_UltraSonic.Tail_R_Y.y - SonicData;
                point = CoordinatePoint.Create_XY(point.x, point.y);
            }

            return point;
        }
        /// <summary>
        /// 获取超声波传感器所得到的所有有效点（数量可能不足 8 个）
        /// </summary>
        /// <returns></returns>
        public static List<CoordinatePoint.POINT> getUltraSonicPoint()
        {
            List<CoordinatePoint.POINT> points = new List<CoordinatePoint.POINT>();

            for (int i = 0; i < 8; i++)
            {
                CoordinatePoint.POINT point = getUltraSonicPoint((Sonic)i);
                if (CoordinatePoint.IsNegPoint(point)) { continue; }

                points.Add(point);
            }

            return points;
        }
        
        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void SendCommand()
        {
            while (true)
            {
                // 串口已关闭
                if (!IsOpen) { config.Thread.Abort(); Abort = false; return; }

                // 外部要求关闭线程
                if (Abort) { config.Thread.Abort(); Abort = false; return; }
                
                // 写入失败，再次写入。
                try
                {
                    config.Port.DiscardOutBuffer();
                    


                    while (config.IsSettingCommand) ;
                    config.IsGettingCommand = true;
                    config.Port.Write(config.Command, 0, config.Command.Length);
                    config.IsGettingCommand = false;

                    System.Threading.Thread.Sleep(TimeForControl);
                }
                catch { config.IsGettingCommand = false; continue; }
            }
        }
        private static void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭或已经关闭
            if (config.IsClosing || !IsOpen) { return; }

            // 正在读取
            config.IsReading = true;

            // 尝试读入
            try
            {
                int receLength = config.Port.BytesToRead;
                byte[] TempReceive = new byte[receLength];
                config.Port.Read(TempReceive, 0, receLength);

                foreach (byte ibyte in TempReceive) { config.Receive.Add(ibyte); }
            }
            catch
            {
                config.IsReading = false;
                config.Receive = new List<byte>();
                return;
            }
            if (config.Receive.Count < 23) { config.IsReading = false; return; }

            // 寻找其中的完整帧
            int indexBG = -1;
            for (int i = 0; i < config.Receive.Count - 1; i++)
            {
                if (config.Receive[i] != 0xf2) { continue; }
                if (config.Receive[i + 1] != 0x70) { continue; }
                indexBG = i;

                // 长度足够
                if (indexBG + config.Frame.Length > config.Receive.Count) { break; }

                // 填充新的一帧
                for (int j = 0; j < config.Frame.Length; j++)
                { config.Frame[j] = config.Receive[j + indexBG]; }

                // 刷新起始位置
                indexBG += config.Frame.Length;

                // 校验帧尾
                uint sumReceived = 0;
                for (int j = 0; j < config.Frame.Length - 2; j++) { sumReceived += config.Frame[j]; }
                sumReceived = (sumReceived >> 16) + (sumReceived & 0x0000ffff);

                byte checkH = (byte)(sumReceived >> 8);
                byte checkL = (byte)(sumReceived & 0x00ff);
                bool check_OK = config.Frame[config.Frame.Length - 2] == checkH &&
                    config.Frame[config.Frame.Length - 1] == checkL;
                if (!check_OK) { break; }

                // 填充数据
                Fill_SonicData();
            }

            // 删除处理过的数据
            if (indexBG == -1) { indexBG = config.Receive.Count; }
            config.Receive.RemoveRange(0, indexBG);
            config.IsReading = false;
        }

        private static void Initial_TH_SendCommand()
        {
            TimeForControl = 100;

            config.IsReading = false;
            config.IsClosing = false;
            config.IsGetting = false;
            config.IsSetting = false;

            config.IsSettingCommand = false;
            config.IsGettingCommand = false;

            config.Command = new byte[0];
            config.Receive = new List<byte>();
            config.Frame = new byte[23];
            config.SonicData = new int[8];

            AGV_MoveControl_0x70(0, 0, 0);
        }
        
        private static void Fill_Command()
        {
            int xSpeed = config.CurrentSpeed.xSpeed;
            int ySpeed = config.CurrentSpeed.ySpeed;
            int aSpeed = config.CurrentSpeed.aSpeed;

            // 限跳变
            if (xSpeed - TH_SendCommand.xSpeed > 50) { xSpeed = TH_SendCommand.xSpeed + 50; }
            if (TH_SendCommand.xSpeed - xSpeed > 50) { xSpeed = TH_SendCommand.xSpeed - 50; }
            if (ySpeed - TH_SendCommand.ySpeed > 50) { ySpeed = TH_SendCommand.ySpeed + 50; }
            if (TH_SendCommand.ySpeed - ySpeed > 50) { ySpeed = TH_SendCommand.ySpeed - 50; }
            if (aSpeed - TH_SendCommand.aSpeed > 100) { aSpeed = TH_SendCommand.aSpeed + 100; }
            if (TH_SendCommand.aSpeed - aSpeed > 100) { aSpeed = TH_SendCommand.aSpeed - 100; }

            // 记录给出速度
            TH_SendCommand.xSpeed = xSpeed;
            TH_SendCommand.ySpeed = ySpeed;
            TH_SendCommand.aSpeed = aSpeed;

            // 给速度
            int speed = 0, direction = 0, rotate = (int)Math.Round(aSpeed * 3.14159 / 180);
            if (rotate < 0) { rotate = 128 - rotate; }

            speed = (int)(Math.Sqrt(xSpeed * xSpeed + ySpeed * ySpeed));
            if (xSpeed == 0 && ySpeed > 0) { direction = 90; }
            if (xSpeed == 0 && ySpeed < 0) { direction = 270; }
            if (xSpeed > 0 && ySpeed == 0) { direction = 0; }
            if (xSpeed < 0 && ySpeed == 0) { direction = 180; }
            if (xSpeed != 0 && ySpeed != 0)
            {
                double angle = Math.Atan((Math.Abs((double)ySpeed)) / Math.Abs((double)xSpeed));
                direction = (int)((angle) * 180 / Math.PI);
                if (xSpeed > 0 && ySpeed > 0) { }
                if (xSpeed > 0 && ySpeed < 0) { direction = 360 - direction; }
                if (xSpeed < 0 && ySpeed > 0) { direction = 180 - direction; }
                if (xSpeed < 0 && ySpeed < 0) { direction = 180 + direction; }
            }

            // 填充 0x70 命令
            byte[] ControlCommand = new byte[11];
            ControlCommand[0] = 0xf1;
            ControlCommand[1] = 0x70;
            ControlCommand[2] = (byte)(speed >> 8);
            ControlCommand[3] = (byte)(speed);
            ControlCommand[4] = (byte)(direction >> 8);
            ControlCommand[5] = (byte)(direction);
            ControlCommand[6] = (byte)(rotate);
            ControlCommand[7] = 0x00;
            Fill_CheckBytes(ref ControlCommand);

            // 写入
            config.IsSettingCommand = true;
            while (config.IsGettingCommand) ;

            config.Command = ControlCommand;

            config.IsSettingCommand = false;
        }
        private static void Fill_CheckBytes(ref byte[] command)
        {
            uint sumCommand = 0;
            for (int i = 0; i < command.Length - 2; i++) { sumCommand += command[i]; }

            sumCommand = (sumCommand >> 16) + (sumCommand & 0x0000ffff);

            command[command.Length - 2] = (byte)(sumCommand >> 8);
            command[command.Length - 1] = (byte)(sumCommand & 0x000000ff);
        }
        private static void Fill_SonicData()
        {
            config.IsSetting = true;
            while (config.IsGetting) ;

            int BG = 5;
            config.SonicData[0] = config.Frame[BG + 0] << 8 | config.Frame[BG + 1];
            config.SonicData[1] = config.Frame[BG + 2] << 8 | config.Frame[BG + 3];
            config.SonicData[2] = config.Frame[BG + 4] << 8 | config.Frame[BG + 5];
            config.SonicData[3] = config.Frame[BG + 6] << 8 | config.Frame[BG + 7];
            config.SonicData[4] = config.Frame[BG + 8] << 8 | config.Frame[BG + 9];
            config.SonicData[5] = config.Frame[BG + 10] << 8 | config.Frame[BG + 11];
            config.SonicData[6] = config.Frame[BG + 12] << 8 | config.Frame[BG + 13];
            config.SonicData[7] = config.Frame[BG + 14] << 8 | config.Frame[BG + 15];

            config.IsSetting = false;
        }
    }
}
