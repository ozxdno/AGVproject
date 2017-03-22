using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using SCIP_library;

namespace AGVproject.Class
{
    class TH_RefreshUrgData
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public bool IsOpen { get { return urgport != null && urgport.IsOpen; } }
        public bool IsClose { get { return urgport == null || !urgport.IsOpen; } }
        public static TH_DATA TH_data;

        public struct TH_DATA
        {
            public string PortName;
            public int BaudRate;

            public bool IsSetting;
            public bool IsGetting;

            public bool TH_cmd_abort;
            public bool TH_running { get { return TH_urg.ThreadState == System.Threading.ThreadState.Running; } }
            public bool TH_hanging;

            public List<long> distance;
            public long TimeStamp;

            public List<double> x;
            public List<double> y;
        }

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static System.Threading.Thread TH_urg = new System.Threading.Thread(Refresh_UrgReceiveData);

        private static SerialPort urgport;
        private static List<long> receData;
        private static PORT_CONFIG portConfig;

        private struct PORT_CONFIG
        {
            public int ReceiveBG;
            public int ReceiveED;
            public int CutBG;
            public int CutED;

            public double AngleStart;
            public double AnglePace;
        }

        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public bool Open(bool CreatePort = false)
        {
            // 建立串口
            if (IsOpen) { return true; }
            if (CreatePort) { urgport = new SerialPort(TH_data.PortName, TH_data.BaudRate); }

            try
            {
                // 初始化线程
                Initial_TH_urg();

                // 打开串口
                urgport.NewLine = "\n\n";
                urgport.Open();
                urgport.Write(SCIP_Writer.SCIP2());
                urgport.ReadLine();
                urgport.Write(SCIP_Writer.MD(portConfig.ReceiveBG, portConfig.ReceiveED));
                urgport.ReadLine();

                // 打开线程
                TH_data.TH_cmd_abort = true;
                while (TH_urg != null && TH_urg.ThreadState == System.Threading.ThreadState.Running) ;
                TH_data.TH_cmd_abort = false;
                TH_urg.Start();

                return true;
            }
            catch { System.Windows.Forms.MessageBox.Show("URG Open Error"); return false; }
        }
        public bool Close()
        {
            if (!urgport.IsOpen) { return true; }
            
            try
            {
                urgport.Write(SCIP_Writer.QT());
                urgport.ReadLine();
                urgport.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void Refresh_UrgReceiveData()
        {
            while (true)
            {
                // 如果串口关闭，则线程结束
                if (urgport == null || !urgport.IsOpen) { TH_urg.Abort(); TH_data.TH_cmd_abort = false; return; }

                // 外部要求关闭线程
                if (TH_data.TH_cmd_abort) { TH_urg.Abort(); TH_data.TH_cmd_abort = false; return; }

                // 外部要求线程挂起
                if (TH_data.TH_hanging) { continue; }

                // 延时
                System.Threading.Thread.Sleep(100);

                // 取数据
                receData = new List<long>();
                if (!portDataReceived()) { continue; }

                // 中值滤波
                MidFilter();

                // 转换为直角坐标
                List<double> TempX = new List<double>();
                List<double> TempY = new List<double>();

                for (int i = 0; i < receData.Count; i++)
                {
                    double angle = portConfig.AngleStart + i * portConfig.AnglePace;

                    TempX.Add(receData[i] * Math.Cos(angle * Math.PI / 180));
                    TempY.Add(receData[i] * Math.Sin(angle * Math.PI / 180));
                }

                // 等待读取完毕
                while (TH_data.IsGetting) ;

                // 传值
                TH_data.IsSetting = true;
                TH_data.distance = receData;
                TH_data.x = TempX;
                TH_data.y = TempY;
                TH_data.IsSetting = false;
            }
        }

        private void Initial_TH_urg()
        {
            portConfig.ReceiveBG = 0;
            portConfig.ReceiveED = 760;

            portConfig.CutBG = 44;
            portConfig.CutED = 726;

            portConfig.AngleStart = -30.0;
            portConfig.AnglePace = 360.0 / 1024.0;
            
            TH_data.IsSetting = false;
            TH_data.IsGetting = false;
            TH_data.TH_cmd_abort = false;
            TH_data.TH_hanging = false;
        }

        private static bool portDataReceived()
        {
            urgport.DiscardInBuffer();
            string receiveData = urgport.ReadLine();
            receData = new List<long>();

            if (!SCIP_Reader.MD(receiveData, ref TH_data.TimeStamp, ref receData))
            {
                Console.WriteLine(receiveData);
                return false;
            }
            if (receData.Count == 0)
            {
                Console.WriteLine(receiveData);
                return false;
            }
            
            receData.RemoveRange(portConfig.CutED, receData.Count - portConfig.CutED);
            receData.RemoveRange(0, portConfig.CutBG);
            return true;
        }
        private static void MidFilter()
        {
            for (int i = 1; i < receData.Count - 1; i++)
            {
                long i_dis = receData[i];
                long l_dis = receData[i - 1];
                long n_dis = receData[i + 1];

                if (l_dis > i_dis && n_dis > i_dis) { receData[i] = l_dis > n_dis ? n_dis : l_dis; }
                if (l_dis < i_dis && n_dis < i_dis) { receData[i] = l_dis > n_dis ? l_dis : n_dis; }
            }
        }
    }
}
