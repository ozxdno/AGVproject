using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using SCIP_library;

namespace AGVproject.Class
{
    class UrgPort
    {
        //////////////////////////////////////// public  attribute ////////////////////////////////////////////////

        public bool IsOpen { get { return urgport != null && urgport.IsOpen; } }
        public bool IsClose { get { return urgport == null || !urgport.IsOpen;  } }
        public bool Receiving { get { return portState.IsFilling; } }
        public URG_DATA urgData;
        
        public struct URG_DATA
        {
            public int start;
            public int end;

            public List<long> distance;
            public long TimeStamp;

            public List<double> x;
            public List<double> y;

            public double StartAngle;
            public double AnglePace;
        }

        //////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static SerialPort urgport;
        private static PORT_STATE portState;
        private static byte[] receData = new byte[40];
        private static byte[] sentData;

        private struct PORT_STATE
        {
            public bool IsClosing;
            public bool IsReading;
            public bool IsFilling;
        }
        

        //////////////////////////////////////// public  method    ////////////////////////////////////////////////

        public bool Open(string portName, string baudRate)
        {
            if (IsOpen) { return true; }

            try
            {
                Initial_PortState();
                urgport = new SerialPort(portName, int.Parse(baudRate));
                urgport.NewLine = "\n\n";
                urgport.Open();
                urgport.Write(SCIP_Writer.SCIP2());
                urgport.ReadLine();
                urgport.Write(SCIP_Writer.MD(urgData.start, urgData.end));
                urgport.ReadLine();
                return true;
            }
            catch { return false; }
        }
        public bool Open()
        {
            if (urgport == null) { return false; }
            try
            {
                Initial_PortState();
                urgport.Open();
                return true;
            }
            catch { return false; }
        }
        public bool Close()
        {
            if (!urgport.IsOpen) { return true; }
            portState.IsClosing = true;

            // 等待读取完毕
            // while (portState.IsReading) { System.Windows.Forms.Application.DoEvents(); }

            // 关闭
            try
            {
                urgport.Write(SCIP_Writer.QT());
                urgport.ReadLine();
                urgport.Close();
                portState.IsClosing = false;
                return true;
            }
            catch
            {
                portState.IsClosing = false;
                return false;
            }
        }
        
        public bool GetUrgData()
        {
            if (urgport == null || !urgport.IsOpen) { return false; }

            portState.IsFilling = true;
            if (!portDataReceived()) { return false; }

            portState.IsFilling = false;
            return true;
        }
        public void MidFilter()
        {
            for (int i = 1; i < urgData.distance.Count - 1; i++)
            {
                long i_dis = urgData.distance[i];
                long l_dis = urgData.distance[i - 1];
                long n_dis = urgData.distance[i + 1];

                if (l_dis > i_dis && n_dis > i_dis) { urgData.distance[i] = l_dis > n_dis ? n_dis : l_dis; }
                if (l_dis < i_dis && n_dis < i_dis) { urgData.distance[i] = l_dis > n_dis ? l_dis : n_dis; }
            }
        }
        public void Pole2Rectangular()
        {
            urgData.x = new List<double>();
            urgData.y = new List<double>();

            for (int i = 0; i < urgData.distance.Count; i++)
            {
                double angle = urgData.StartAngle + i * urgData.AnglePace;
                double dis = urgData.distance[i];
                //if (dis == 0) { continue; }

                urgData.x.Add(dis * Math.Cos(angle * Math.PI / 180));
                urgData.y.Add(dis * Math.Sin(angle * Math.PI / 180));
            }
        }

        //////////////////////////////////////// private method    ////////////////////////////////////////////////

        private void Initial_PortState()
        {
            portState.IsClosing = false;
            portState.IsFilling = false;
            portState.IsReading = false;

            urgData.start = 0;
            urgData.end = 760;

            urgData.distance = new List<long>();
            urgData.x = new List<double>();
            urgData.y = new List<double>();
            urgData.StartAngle = -30.0;
            urgData.AnglePace = 360.0 / 1024.0;
        }
        
        private bool portDataReceived()
        {
            urgport.DiscardInBuffer();
            string receiveData = urgport.ReadLine();

            if (!SCIP_Reader.MD(receiveData, ref urgData.TimeStamp, ref urgData.distance))
            {
                Console.WriteLine(receiveData);
                return false;
            }
            if (urgData.distance.Count == 0)
            {
                Console.WriteLine(receiveData);
                return false;
            }

            urgData.distance.RemoveRange(0, 44);
            urgData.distance.RemoveRange(673, urgData.distance.Count - 673);
            return true;
        }
    }
}
