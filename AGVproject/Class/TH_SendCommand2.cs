using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGVproject.Class
{
    class TH_SendCommand2
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public bool IsOpen { get { return controlport != null && controlport.IsOpen; } }
        public bool IsClose { get { return controlport == null || !controlport.IsOpen; } }
        
        public static TH_DATA TH_data;
        public struct TH_DATA
        {
            public string PortName;
            public int BaudRate;

            public bool IsGetting;
            public bool IsSetting;

            public int Head_L_X;
            public int Head_L_Y;
            public int Head_R_X;
            public int Head_R_Y;
            public int Tail_L_X;
            public int Tail_L_Y;
            public int Tail_R_X;
            public int Tail_R_Y;

            public uint X;
            public uint Y;

            public int TimeForDistance;
            public int TimeForSonic;
            public int TimeForControl;

            public int ReceivedBytesThreshold;
        }

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static System.Timers.Timer TIMER;

        private static SerialPort controlport;
        
        private static PORT_CONFIG portConfig;
        private struct PORT_CONFIG
        {
            public bool Sent0x86;
            public bool Sent0x84;
            public bool Sent0x70;

            public bool Receiving_0x86;
            public bool Receiving_0x84;
            public bool Receiving_0x70;

            public int ReceiveLength_0x86;
            public int ReceiveLength_0x84;
            public int ReceiveLength_0x70;

            public bool IsReading;
            public bool IsClosing;

            public bool IsSettingCommand;
            public bool IsGettingCommand;

            public byte[] ControlCommand;
            public byte[] SonicCommand;
            public byte[] DistanceCommand;

            public List<byte> Receive;
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        public bool Open(bool CreatePort = false)
        {
            if (IsOpen) { return true; }
            if (CreatePort) { controlport = new SerialPort(TH_data.PortName, TH_data.BaudRate); }

            try
            {
                // 初始化线程
                Initial_TH_SendCommand();

                // 打开串口
                controlport.DataReceived -= portDataReceived;
                controlport.DataReceived += portDataReceived;
                controlport.Open();

                // 打开定时器
                TIMER.Enabled = true;
                return true;
            }
            catch { return false; }
        }
        public bool Close()
        {
            if (!controlport.IsOpen) { return true; }
            portConfig.IsClosing = true;

            // 等待读取完毕
            while (portConfig.IsReading) { System.Windows.Forms.Application.DoEvents(); }

            // 关闭
            try
            {
                controlport.Close();
                portConfig.IsClosing = false;
                return true;
            }
            catch
            {
                portConfig.IsClosing = false;
                return false;
            }
        }

        public bool AGV_MoveControl_0x70(int xSpeed, int ySpeed, int aSpeed)
        {
            // 限幅
            aSpeed = (int)Math.Round(aSpeed * 3.14159 / 180);
            if (xSpeed > 800) { xSpeed = 800; }
            if (xSpeed < -800) { xSpeed = -800; }
            if (ySpeed > 800) { ySpeed = 800; }
            if (ySpeed < -800) { ySpeed = -800; }
            if (aSpeed < 0) { aSpeed = 128 - aSpeed; }
            if (aSpeed > 255) { aSpeed = 255; }
            if (aSpeed < 0) { aSpeed = 0; }
            
            int speed = 0, direction = 0, rotate = aSpeed;

            if (xSpeed > 0) { speed = xSpeed; direction = 90; rotate = 0; }
            if (xSpeed < 0) { speed = -xSpeed; direction = 270; rotate = 0; }
            if (ySpeed > 0) { speed = ySpeed; direction = 0; rotate = 0; }
            if (ySpeed < 0) { speed = -ySpeed; direction = 180; rotate = 0; }

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
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;
            
            portConfig.Sent0x70 = true;
            portConfig.ControlCommand = ControlCommand;

            portConfig.IsSettingCommand = false;
            return true;
        }
        public bool MeasureDistance_0x84(int waitTime = 0)
        {
            // 串口没开，填充无效
            if (IsClose) { return false; }

            // 填充 0x84 命令
            byte[] DistanceCommand = new byte[4] { 0xf1, 0x84, 0x00, 0x00 };
            Fill_CheckBytes(ref DistanceCommand);

            // 写入
            portConfig.Receiving_0x84 = true;
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;

            portConfig.Sent0x84 = true;
            portConfig.DistanceCommand = DistanceCommand;

            portConfig.IsSettingCommand = false;
            
            // 等待结果
            if (waitTime <= 0) { while (portConfig.Receiving_0x84) ; }
            if (waitTime > 0) { System.Threading.Thread.Sleep(waitTime); }
            return !portConfig.Receiving_0x84;
        }
        public bool MeasureUltraSonic_0x86(int waitTime = 0)
        {
            // 串口没开，填充无效
            if (IsClose) { return false; }

            // 填充命令
            byte[] SonicCommand = new byte[4] { 0xf1, 0x86, 0x00, 0x00 };
            Fill_CheckBytes(ref SonicCommand);

            // 写入
            portConfig.Receiving_0x86 = true;
            portConfig.IsSettingCommand = true;
            while (portConfig.IsGettingCommand) ;

            portConfig.Sent0x86 = true;
            portConfig.SonicCommand = SonicCommand;

            portConfig.IsSettingCommand = false;

            // 等待数据有效
            if (waitTime <= 0) { while (portConfig.Receiving_0x86) ; return true; }
            System.Threading.Thread.Sleep(waitTime);
            return !portConfig.Receiving_0x86;
        }

        public void StopSendCommand_Control_0x70() { portConfig.Sent0x70 = false; }
        public void StopSendCommand_Distance_0x84() { portConfig.Sent0x84 = false; }
        public void StopSendCommand_Sonic_0x86() { portConfig.Sent0x86 = false; }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void SendCommand(object source, System.Timers.ElapsedEventArgs e)
        {
            while (true)
            {
                // 串口已关闭
                if (controlport == null && !controlport.IsOpen) { TIMER.Close(); return; }
                
                // 写入失败，再次写入。
                try
                {
                    controlport.ReceivedBytesThreshold = 1;
                    controlport.DiscardOutBuffer();

                    portConfig.IsGettingCommand = true;
                    while (portConfig.IsSettingCommand) ;

                    if (portConfig.Sent0x86)
                    {
                        controlport.Write(portConfig.SonicCommand, 0, portConfig.SonicCommand.Length);
                        System.Threading.Thread.Sleep(TH_data.TimeForSonic);
                    }
                    if (portConfig.Sent0x84)
                    {
                        controlport.Write(portConfig.DistanceCommand, 0, portConfig.DistanceCommand.Length);
                        System.Threading.Thread.Sleep(TH_data.TimeForDistance);
                    }
                    if (portConfig.Sent0x70)
                    {
                        controlport.Write(portConfig.ControlCommand, 0, portConfig.ControlCommand.Length);
                        //System.Threading.Thread.Sleep(TH_data.TimeForSonic);
                    }
                    
                    portConfig.IsGettingCommand = false;
                }
                catch { portConfig.IsGettingCommand = false; continue; }
            }
        }
        private void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭
            if (portConfig.IsClosing) { return; }

            // 正在读取
            portConfig.IsReading = true;
            int receLength = 0;
            try
            {
                receLength = controlport.BytesToRead;
                byte[] TempReceive = new byte[receLength];
                controlport.Read(TempReceive, 0, receLength);

                foreach (byte ibyte in TempReceive) { portConfig.Receive.Add(ibyte); }
            }
            catch
            {
                portConfig.IsReading = false;
                portConfig.Receive = new List<byte>();
                return;
            }
            portConfig.IsReading = false;

            // 满足长度要求
            if (portConfig.Receive.Count < TH_data.ReceivedBytesThreshold) { return; }
            
            // 寻找最后的 0xf1（最新的）
            int indexBG = -1;
            for (int i = 0; i < receLength; i++)
            {
                if (portConfig.Receive[i] != 0xf1) { continue; }
                if (indexBG + portConfig.ReceiveLength_0x86 > portConfig.Receive.Count) { break; }
                indexBG = i;
            }

            // 校验帧头
            if (indexBG == -1) { portConfig.Receive = new List<byte>(); return; }
            if (portConfig.Receive[indexBG + 1] != 0x86) { portConfig.Receive = new List<byte>(); return; }

            // 校验帧尾
            uint sumReceived = 0;
            for (int i = 0; i < portConfig.ReceiveLength_0x86 - 2; i++) { sumReceived += portConfig.Receive[indexBG + i]; }
            sumReceived = (sumReceived >> 16) + (sumReceived & 0x0000ffff);

            byte checkH = (byte)(sumReceived >> 8);
            byte checkL = (byte)(sumReceived & 0x00ff);

            if (portConfig.Receive[indexBG + portConfig.ReceiveLength_0x86 - 2] != checkH) { portConfig.Receive.Clear(); return; }
            if (portConfig.Receive[indexBG + portConfig.ReceiveLength_0x86 - 1] != checkL) { portConfig.Receive.Clear(); return; }

            // 填充数据
            TH_data.IsSetting = true;
            while (TH_data.IsGetting) ;

            //TH_data.Head_L_Y = (portConfig.Receive[indexBG + 2]) << 8 | portConfig.Receive[indexBG + 3];
            //TH_data.Head_L_X = (portConfig.Receive[indexBG + 4]) << 8 | portConfig.Receive[indexBG + 5];
            //TH_data.Head_R_X = (portConfig.Receive[indexBG + 6]) << 8 | portConfig.Receive[indexBG + 7];
            //TH_data.Head_R_Y = (portConfig.Receive[indexBG + 8]) << 8 | portConfig.Receive[indexBG + 9];
            //TH_data.Tail_R_Y = (portConfig.Receive[indexBG + 10]) << 8 | portConfig.Receive[indexBG + 11];
            //TH_data.Tail_R_X = (portConfig.Receive[indexBG + 12]) << 8 | portConfig.Receive[indexBG + 13];
            //TH_data.Tail_L_X = (portConfig.Receive[indexBG + 14]) << 8 | portConfig.Receive[indexBG + 15];
            //TH_data.Tail_L_Y = (portConfig.Receive[indexBG + 16]) << 8 | portConfig.Receive[indexBG + 17];

            TH_data.Head_L_Y = portConfig.Receive[indexBG + 3];
            TH_data.Head_L_X = portConfig.Receive[indexBG + 5];
            TH_data.Head_R_X = portConfig.Receive[indexBG + 7];
            TH_data.Head_R_Y = portConfig.Receive[indexBG + 9];
            TH_data.Tail_R_Y = portConfig.Receive[indexBG + 11];
            TH_data.Tail_R_X = portConfig.Receive[indexBG + 13];
            TH_data.Tail_L_X = portConfig.Receive[indexBG + 15];
            TH_data.Tail_L_Y = portConfig.Receive[indexBG + 17];

            TH_data.IsSetting = false;
            portConfig.Receiving_0x86 = false;
            portConfig.Receive.Clear();
        }

        private void Initial_TH_SendCommand()
        {
            // 初始化 portConfig
            portConfig.Sent0x70 = false;
            portConfig.Sent0x84 = false;
            portConfig.Sent0x86 = false;

            portConfig.Receiving_0x70 = false;
            portConfig.Receiving_0x84 = false;
            portConfig.Receiving_0x86 = false;

            portConfig.ReceiveLength_0x86 = 20;
            portConfig.ReceiveLength_0x84 = 12;
            portConfig.ReceiveLength_0x70 = 7;
            
            portConfig.IsReading = false;
            portConfig.IsClosing = false;

            portConfig.IsSettingCommand = false;
            portConfig.IsGettingCommand = false;

            portConfig.Receive = new List<byte>();
            
            // 公共数据段初始化
            TH_data.IsSetting = false;
            TH_data.IsGetting = false;
            
            if (TH_data.ReceivedBytesThreshold == 0) { TH_data.ReceivedBytesThreshold = 60; }

            if (TH_data.TimeForControl == 0) { TH_data.TimeForControl = 100; }
            if (TH_data.TimeForDistance == 0) { TH_data.TimeForDistance = 10; }
            if (TH_data.TimeForSonic == 0) { TH_data.TimeForSonic = 100; }

            // 定时器
            TIMER = new System.Timers.Timer(TH_data.TimeForControl + TH_data.TimeForDistance + TH_data.TimeForSonic);
            TIMER.Elapsed += new System.Timers.ElapsedEventHandler(SendCommand);
            TIMER.AutoReset = true;
        }
        
        private void Fill_CheckBytes(ref byte[] command)
        {
            uint sumCommand = 0;
            for (int i = 0; i < command.Length - 2; i++) { sumCommand += command[i]; }

            sumCommand = (sumCommand >> 16) + (sumCommand & 0x0000ffff);

            command[command.Length - 2] = (byte)(sumCommand >> 8);
            command[command.Length - 1] = (byte)(sumCommand & 0x000000ff);
        }
    }
}
