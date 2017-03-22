using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGVproject.Class
{
    /// <summary>
    /// 控制串口控制类，包含对控制串口的所有操作。
    /// </summary>
    class ControlPort
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        /// <summary>
        /// 串口当前状态
        /// </summary>
        public bool IsOpen { get { return controlport != null && controlport.IsOpen; } }
        /// <summary>
        /// 串口当前状态
        /// </summary>
        public bool IsClose { get { return controlport == null || !controlport.IsOpen; } }
        /// <summary>
        /// 是否正在接收串口返回数据，此位为 false 后接收数据可用。
        /// </summary>
        public bool Receiving { get { return portState.IsFilling; } }

        /// <summary>
        /// 超声波测得数据，Receving = false 后此数据可用。
        /// </summary>
        public ULTRASONIC UltraSonic = new ULTRASONIC();
        /// <summary>
        /// 测得里程数据，Receving = false 后此数据可用。
        /// </summary>
        public AGV_POSITION AGV_position = new AGV_POSITION();
        
        public struct ULTRASONIC
        {
            public int Head_L_X;
            public int Head_L_Y;
            public int Head_R_X;
            public int Head_R_Y;
            public int Tail_L_X;
            public int Tail_L_Y;
            public int Tail_R_X;
            public int Tail_R_Y;
        }
        public struct AGV_POSITION
        {
            public double X;
            public double Y;
            public double A;
        }

        //////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static SerialPort controlport;
        
        private static byte[] receData = new byte[40];
        private static byte[] sentData;
        private static int receLength;

        private static PORT_STATE portState;
        private struct PORT_STATE
        {
            public bool sent_0x86;
            public bool sent_0x84;
            public bool sent_0x70;

            public bool received;

            public bool IsReading;
            public bool IsClosing;
            public bool IsFilling;
        }

        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        /// <summary>
        /// 打开串口，返回串口是否被正常打开。
        /// </summary>
        /// <param name="Name">串口名称</param>
        /// <param name="Baudrate">串口波特率</param>
        /// <returns>串口是否被正常打开</returns>
        public bool Open(string Name, string Baudrate)
        {
            if (IsOpen) { return true; }

            try
            {
                Initial_PortState();
                controlport = new SerialPort(Name, int.Parse(Baudrate));
                controlport.DataReceived -= portDataReceived;
                controlport.DataReceived += portDataReceived;
                controlport.Open();
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 对没有释放的本串口再次打开，返回串口是否被正常打开。
        /// </summary>
        /// <returns>串口是否被正常打开</returns>
        public bool Open()
        {
            if (IsOpen) { return true; }
            if (controlport == null) { return false; }

            try
            {
                Initial_PortState();
                controlport.Open();
                controlport.DataReceived -= portDataReceived;
                controlport.DataReceived += portDataReceived;
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 关闭串口，返回串口是否被正常关闭。
        /// </summary>
        /// <returns>串口是否被正常关闭</returns>
        public bool Close()
        {
            if (!controlport.IsOpen) { return true; }
            portState.IsClosing = true;

            // 等待读取完毕
            while (portState.IsReading) { System.Windows.Forms.Application.DoEvents(); }

            // 关闭
            try
            {
                controlport.Close();
                portState.IsClosing = false;
                return true;
            }
            catch
            {
                portState.IsClosing = false;
                return false;
            }
        }

        /// <summary>
        /// 获取超声波测得数据，返回命令是否被正常发送。Receiving = false 后在 UltraSonic 数据段取数据。
        /// </summary>
        /// <returns>命令是否正常执行</returns>
        public bool GetUltraSonicData_0x86()
        {
            // 串口没开
            if (IsClose) { return false; }

            // 开启 已接收 位，暂停当前的接收处理。
            portState.sent_0x86 = true;
            portState.sent_0x84 = false;
            portState.sent_0x70 = false;
            portState.received = true;
            portState.IsFilling = true;

            // 填充命令
            sentData = new byte[4] { 0xf1, 0x86, 0x00, 0x00 };
            Fill_CheckBytes();

            // 清空输出
            // UltraSonic = new ULTRASONIC();

            // 发送命令
            receLength = 12;
            return portDataSend();
        }
        /// <summary>
        /// 获取编码器所测量的当前里程，返回命令是否被正常发送。Receiving = false 后在 AGV_position 中取数据。
        /// </summary>
        /// <returns>命令是否被正常发送</returns>
        public bool MessureMoveDistance_0x84()
        {
            // 串口没开
            if (IsClose) { return false; }

            // 初始化状态
            portState.sent_0x86 = false;
            portState.sent_0x84 = true;
            portState.sent_0x70 = false;
            portState.received = true;
            portState.IsFilling = true;

            // 填充命令
            sentData = new byte[4]{ 0xf1, 0x84, 0x00, 0x00 };
            Fill_CheckBytes();

            // 清空输出
            // AGV_position = new AGV_POSITION();

            // 发送命令
            receLength = 12;
            return portDataSend();
        }
        /// <summary>
        /// 控制小车行进，返回命令是否已经被正常发送。Receiving = true 表示命令已被接收。
        /// </summary>
        /// <param name="xSpeed">单位：mm。MAX: 800，MIN: -800</param>
        /// <param name="ySpeed">单位：mm。MAX: 800，MIN: -800</param>
        /// <param name="aSpeed">单位：0.01度。MAX:7277，MIN:-7334 </param>
        /// <returns>命令是否被正常发送</returns>
        public bool MoveControl_0x70(int xSpeed, int ySpeed, int aSpeed)
        {
            // 串口没开
            if (IsClose) { return false; }

            // 限幅
            aSpeed = (int)Math.Round(aSpeed * 3.14159 / 180);
            if (xSpeed >  800) { xSpeed =  800; }
            if (xSpeed < -800) { xSpeed = -800; }
            if (ySpeed >  800) { ySpeed =  800; }
            if (ySpeed < -800) { ySpeed = -800; }
            if (aSpeed >  127) { aSpeed =  127; }
            if (aSpeed < -128) { aSpeed = -128; }

            // 把三轴速度转换为可输出命令
            if (xSpeed != 0 && (ySpeed != 0 || aSpeed != 0)) { return false; }
            if (ySpeed != 0 && (xSpeed != 0 || aSpeed != 0)) { return false; }
            if (aSpeed != 0 && (xSpeed != 0 || ySpeed != 0)) { return false; }

            int speed = 0, direction = 0, rotate = aSpeed;

            if (xSpeed > 0) { speed = xSpeed; direction = 90; rotate = 0; }
            if (xSpeed < 0) { speed = -xSpeed; direction = 270; rotate = 0; }
            if (ySpeed > 0) { speed = ySpeed; direction = 0; rotate = 0; }
            if (ySpeed < 0) { speed = -ySpeed; direction = 180; rotate = 0; }

            // 初始化状态
            portState.sent_0x86 = false;
            portState.sent_0x84 = false;
            portState.sent_0x70 = true;
            portState.received = true;
            portState.IsFilling = true;

            // 建立命令
            sentData = new byte[11];
            sentData[0] = 0xf1;
            sentData[1] = 0x70;
            sentData[2] = (byte)(speed >> 8);
            sentData[3] = (byte)(speed);
            sentData[4] = (byte)(direction >> 8);
            sentData[5] = (byte)(direction);
            sentData[6] = (byte)(rotate);
            sentData[7] = 0x00;
            Fill_CheckBytes();

            // 发送命令
            receLength = 7;
            return portDataSend();
        }

        /// <summary>
        /// 复位 AGV 小车，返回命令是否被正常发送。
        /// </summary>
        /// <returns>命令是否被正常发送</returns>
        public bool AGV_reset()
        {
            // 串口没开
            if (IsClose) { return false; }

            // 初始化状态
            portState.sent_0x86 = false;
            portState.sent_0x84 = false;
            portState.sent_0x70 = true;
            portState.received = true;
            portState.IsFilling = true;

            // 填充命令
            sentData = new byte[11];
            sentData[0] = 0xf1;
            sentData[1] = 0x70;
            sentData[7] = 0x02;
            Fill_CheckBytes();

            // 发送命令
            receLength = 7;
            return portDataSend();
        }
        /// <summary>
        /// 急停 AGV 小车，返回命令是否被正常发送。
        /// </summary>
        /// <returns>命令是否被正常发送</returns>
        public bool AGV_stop()
        {
            // 串口没开
            if (IsClose) { return false; }

            // 初始化状态
            portState.sent_0x86 = false;
            portState.sent_0x84 = false;
            portState.sent_0x70 = true;
            portState.received = true;
            portState.IsFilling = true;

            // 填充命令
            sentData = new byte[11];
            sentData[0] = 0xf1;
            sentData[1] = 0x70;
            sentData[7] = 0x01;
            Fill_CheckBytes();

            // 发送命令
            receLength = 7;
            return portDataSend();
        }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private void Initial_PortState()
        {
            portState.sent_0x70 = false;
            portState.sent_0x84 = false;
            portState.sent_0x86 = false;

            portState.received = true;

            portState.IsReading = false;
            portState.IsClosing = false;
            portState.IsFilling = false;
        }

        private bool portDataSend()
        {
            try
            {
                controlport.ReceivedBytesThreshold = receLength;
                controlport.DiscardOutBuffer();
                portState.received = false;
                controlport.Write(sentData, 0, sentData.Length);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void portDataReceived(object sender, EventArgs e)
        {
            // 正在关闭或已经读取完毕
            if (portState.IsClosing || portState.received) { return; }

            // 正在读取
            portState.IsReading = true;
            try
            {
                controlport.Read(receData, 0, Math.Min(receLength, controlport.ReceivedBytesThreshold));
            }
            catch
            {
                portState.IsReading = false; return;
            }
            portState.IsReading = false;

            #region 预处理行进控制返回
            if (portState.sent_0x70)
            {
                if (!True_ReceiveData()) { return; }
                portState.IsFilling = false;
                return;
            }
            #endregion

            #region 预处理超声波数据
            if (portState.sent_0x86)
            {
                if (!True_ReceiveData()) { return; }

                // 填充数据
                UltraSonic.Head_L_Y = receData[2];
                UltraSonic.Head_L_X = receData[3];
                UltraSonic.Head_R_X = receData[4];
                UltraSonic.Head_R_Y = receData[5];
                UltraSonic.Tail_R_Y = receData[6];
                UltraSonic.Tail_R_X = receData[7];
                UltraSonic.Tail_L_X = receData[8];
                UltraSonic.Tail_L_Y = receData[9];
                
                portState.IsFilling = false;
                return;
            }
            #endregion

            #region 预处理里程数据
            if (portState.sent_0x84)
            {
                if (!True_ReceiveData()) { return; }
                
                uint byte1 = receData[2];
                uint byte2 = receData[3];
                uint byte3 = receData[4];
                uint byte4 = receData[5];
                AGV_position.Y = byte1 << 24 | byte2 << 16 | byte3 << 8 | byte4;

                byte1 = receData[6];
                byte2 = receData[7];
                byte3 = receData[8];
                byte4 = receData[9];
                AGV_position.X = byte1 << 24 | byte2 << 16 | byte3 << 8 | byte4;
                
                portState.IsFilling = false;
                return;
            }
            #endregion
        }

        private void Fill_CheckBytes()
        {
            uint sumCommand = 0;
            for (int i = 0; i < sentData.Length - 2; i++) { sumCommand += sentData[i]; }

            sumCommand = (sumCommand >> 16) + (sumCommand & 0x0000ffff);

            sentData[sentData.Length - 2] = (byte)(sumCommand >> 8);
            sentData[sentData.Length - 1] = (byte)(sumCommand & 0x000000ff);
        }
        private bool True_ReceiveData()
        {
            if (receData.Length < 4) { return false; }
            if (sentData.Length < 4) { return false; }
            
            if (receData[0] != sentData[0]) { return false; }
            if (receData[1] != sentData[1]) { return false; }

            uint sumReceived = 0;
            for (int i = 0; i < receLength - 2; i++) { sumReceived += receData[i]; }
            sumReceived = (sumReceived >> 16) + (sumReceived & 0x0000ffff);

            byte checkH = (byte)(sumReceived >> 8);
            byte checkL = (byte)(sumReceived & 0x00ff);

            if (receData[receLength - 2] != checkH) { return false; }
            if (receData[receLength - 1] != checkL) { return false; }

            return true;
        }
    }
}
