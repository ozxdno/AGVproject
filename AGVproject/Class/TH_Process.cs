using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGVproject.Class
{
    class TH_Process
    {
        ////////////////////////////////////////// public attribute ////////////////////////////////////////////////

        public static TH_DATA TH_data;

        public struct TH_DATA
        {
            public bool TH_cmd_abort;
            public bool TH_runing { get { return TH_process.ThreadState == System.Threading.ThreadState.Running; } }

            public string control_PortName;
            public string control_BaudRate;

            public string urg_PortName;
            public string urg_BaudRate;
        }

        public static TH_RefreshUrgData TH_urg = new TH_RefreshUrgData();
        public static TH_SendCommand TH_command = new TH_SendCommand();
        public static CorrectPosition correctPos = new CorrectPosition();

        ////////////////////////////////////////// private attribute ////////////////////////////////////////////////

        private static System.Threading.Thread TH_process = new System.Threading.Thread(ControlProcess);
        
        ////////////////////////////////////////// public method ////////////////////////////////////////////////

        public void Start()
        {
            // 初始化其他线程
            TH_urg.Open(TH_data.urg_PortName, TH_data.urg_BaudRate);
            if (TH_urg.IsClose) { MessageBox.Show("URG Port Error !"); }

            TH_command.Open(TH_data.control_PortName, TH_data.control_BaudRate);
            if (TH_command.IsClose) { MessageBox.Show("Control Port Error !"); }

            // 打开线程
            TH_data.TH_cmd_abort = true;
            while (TH_process != null && TH_process.ThreadState == System.Threading.ThreadState.Running) ;
            TH_data.TH_cmd_abort = false;

            TH_process.Start();
        }

        ////////////////////////////////////////// private method ////////////////////////////////////////////////

        private static void ControlProcess()
        {
            while (true)
            {
                // 外部要求关闭线程，则关闭所有线程
                if (TH_data.TH_cmd_abort)
                {
                    TH_SendCommand.TH_data.TH_cmd_abort = true;
                    //while (TH_SendCommand.TH_data.TH_runing) ;

                    TH_RefreshUrgData.TH_data.TH_cmd_abort = true;
                    while (TH_RefreshUrgData.TH_data.TH_runing) ;

                    TH_process.Abort();
                    TH_data.TH_cmd_abort = false;
                    return;
                }

                // 
            }
        }
    }
}
