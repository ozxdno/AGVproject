using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AGVproject
{
    public partial class Form_Start : Form
    {
        public Form_Start()
        {
            InitializeComponent();

            this.controlPortName.Text = "COM10";
            this.controlBaudRate.Text = "115200";

            this.xSpeed.Text = "0";
            this.ySpeed.Text = "0";
            this.aSpeed.Text = "0";

            this.urgPortName.Text = "COM3";
            this.urgBaudRate.Text = "115200";
        }
        
        private static Class.TH_SendCommand TH_command = new Class.TH_SendCommand();
        private static Class.TH_RefreshUrgData TH_urg = new Class.TH_RefreshUrgData();
        private static Class.TH_MeasurePosition TH_locate = new Class.TH_MeasurePosition();
        private static Class.CorrectPosition P_correct = new Class.CorrectPosition();
        private static Class.Adjust_inAisle A_adjust = new Class.Adjust_inAisle();

        private static bool ReceiveSonic = true;
        private static System.Timers.Timer TIMER = new System.Timers.Timer(100);
        private static int testRefresh = 0;
        private static int MoveTime = -1;
        
        private void controlOpen_Click(object sender, EventArgs e)
        {
            Class.TH_SendCommand.TH_data.PortName = this.controlPortName.Text;

            try { Class.TH_SendCommand.TH_data.BaudRate = int.Parse(this.controlBaudRate.Text); } catch { MessageBox.Show("BaudRate Error"); }

            if (!TH_command.Open(true)) { MessageBox.Show("Open CON Error"); return; }

            ReceiveSonic = true;
            this.ReceiveSonicData.Text = "Stop Sonic Data";
            TH_command.StopSendCommand_Sonic_0x86();
        }

        private void Refresh_FormStart(object source, System.Timers.ElapsedEventArgs e)
        {
            testRefresh++;

            this.BeginInvoke((EventHandler)delegate
            {
                this.Text = testRefresh.ToString();

                string[] MSG;

                // 控制口数据
                Class.TH_SendCommand.TH_data.IsGetting = true;
                while (Class.TH_SendCommand.TH_data.IsSetting) ;

                MSG = new string[10];
                MSG[0] = "";
                MSG[1] = "";
                MSG[2] = "Head_L_X: " + Class.TH_SendCommand.TH_data.Head_L_X.ToString();
                MSG[3] = "Head_L_Y: " + Class.TH_SendCommand.TH_data.Head_L_Y.ToString();
                MSG[4] = "Head_R_X: " + Class.TH_SendCommand.TH_data.Head_R_X.ToString();
                MSG[5] = "Head_R_Y: " + Class.TH_SendCommand.TH_data.Head_R_Y.ToString();
                MSG[6] = "Tail_L_X: " + Class.TH_SendCommand.TH_data.Tail_L_X.ToString();
                MSG[7] = "Tail_L_Y: " + Class.TH_SendCommand.TH_data.Tail_L_Y.ToString();
                MSG[8] = "Tail_R_X: " + Class.TH_SendCommand.TH_data.Tail_R_X.ToString();
                MSG[9] = "Tail_R_Y: " + Class.TH_SendCommand.TH_data.Tail_R_Y.ToString();

                Class.TH_SendCommand.TH_data.IsGetting = false;
                if (TH_command.IsClose) { MSG[0] = "CON port not Open"; }
                this.controlportMSG.Lines = MSG;

                // 激光雷达数据

                MSG = new string[6];
                MSG[0] = "";
                MSG[1] = "";

                // 编码器数据

                // 运行时间
                if (MoveTime >= 0) { MoveTime--; }
                if (MoveTime == 0) { TH_command.AGV_MoveControl_0x70(0, 0, 0); }
            });
        }

        private void Form_Start_FormClosed(object sender, FormClosedEventArgs e)
        {
            Class.TH_SendCommand.TH_data.TH_cmd_abort = true;
        }

        private void Form_Start_Load(object sender, EventArgs e)
        {
            TIMER.Elapsed += new System.Timers.ElapsedEventHandler(Refresh_FormStart);
            TIMER.AutoReset = true;
            TIMER.Enabled = true;
        }

        private void ReceiveSonicData_Click(object sender, EventArgs e)
        {
            ReceiveSonic = !ReceiveSonic;
            if (ReceiveSonic) { this.ReceiveSonicData.Text = "Receive Sonic Data"; TH_command.MeasureUltraSonic_0x86(); return; }
            this.ReceiveSonicData.Text = "Stop Sonic Data";
            TH_command.StopSendCommand_Sonic_0x86();
        }

        private void xSpeed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue != 13) { return; }

            int xSpeed = 0, ySpeed = 0, aSpeed = 0;
            try { xSpeed = int.Parse(this.xSpeed.Text); } catch { xSpeed = 0; }
            try { ySpeed = int.Parse(this.ySpeed.Text); } catch { ySpeed = 0; }
            try { aSpeed = int.Parse(this.aSpeed.Text); } catch { aSpeed = 0; }

            TH_command.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            MoveTime = 100;
        }
    }
}
