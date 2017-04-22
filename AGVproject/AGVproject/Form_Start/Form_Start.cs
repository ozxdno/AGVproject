using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using AGVproject.Class;
using AGVproject.Form_Input;

namespace AGVproject
{
    public partial class Form_Start : Form
    {
        public Form_Start()
        {
            InitializeComponent();
        }

        public static CONFIG config;
        public struct CONFIG
        {
            public System.Timers.Timer Timer;
            public DateTime Time;
            public List<int> KeyValue;

            public List<FILE> Map;
            public List<FILE> Route;

            public int SelectedMap;
            public int SelectedRoute;

            public bool CheckMap;
            public bool CheckRoute;

            public List<string> ConPortName;
            public List<string> UrgPortName;
            public List<string> LocPortName;

            public List<int> ConBaudRate;
            public List<int> UrgBaudRate;
            public List<int> LocBaudRate;

            public int SelectedControlPortName;
            public int SelectedControlBaudRate;
            public int SelectedUrgPortName;
            public int SelectedUrgBaudRate;
            public int SelectedLocatePortName;
            public int SelectedLocateBaudRate;

            public bool CheckControlPort;
            public bool CheckUrgPort;
            public bool CheckLocatePort;

            public double urgRange;
            public double PixLength;
            public struct FILE { public string Full, Path, Name; public string[] Text; }
        }
        
        private void Refresh_FormStart(object source, System.Timers.ElapsedEventArgs e)
        {
            this.BeginInvoke((EventHandler)delegate
            {
                // 刷新配置
                config.CheckMap = this.CheckMap.Checked;
                config.CheckRoute = this.CheckRoute.Checked;
                config.CheckControlPort = this.checkConPort.Checked;
                config.CheckUrgPort = this.checkUrgPort.Checked;
                config.CheckLocatePort = this.checkLocPort.Checked;
                
                // 刷新时间
                config.Time = DateTime.Now;

                string TimeString = config.Time.Year.ToString() + "-";
                if (config.Time.Month < 10) { TimeString += "0" + config.Time.Month.ToString() + "-"; }
                if (config.Time.Month >= 10) { TimeString += config.Time.Month.ToString() + "-"; }
                if (config.Time.Day < 10) { TimeString += "0" + config.Time.Day.ToString() + " "; }
                if (config.Time.Day >= 10) { TimeString += config.Time.Day.ToString() + " "; }
                if (config.Time.Hour < 10) { TimeString += "0" + config.Time.Hour.ToString() + ":"; }
                if (config.Time.Hour >= 10) { TimeString += config.Time.Hour.ToString() + ":"; }
                if (config.Time.Minute < 10) { TimeString += "0" + config.Time.Minute.ToString() + ":"; }
                if (config.Time.Minute >= 10) { TimeString += config.Time.Minute.ToString() + ":"; }
                if (config.Time.Second < 10) { TimeString += "0" + config.Time.Second.ToString(); }
                if (config.Time.Second >= 10) { TimeString += config.Time.Second.ToString(); }
                
                this.TimeLabel.Text = TimeString;

                // 刷新事件
                if (TH_AutoSearchTrack.control.Event == null) { this.EventLabel.Text = "Waitting"; }
                if (TH_AutoSearchTrack.control.Event != null) { this.EventLabel.Text = TH_AutoSearchTrack.control.Event; }

                // 刷新图片
                TH_UpdataPictureBox.BoxLength = this.pictureBox.Height;
                TH_UpdataPictureBox.BoxWidth = this.pictureBox.Width;
                TH_UpdataPictureBox.Updata();

                this.pictureBox.Height = TH_UpdataPictureBox.BaseMapPicture.Height;
                this.pictureBox.Width = TH_UpdataPictureBox.BaseMapPicture.Width;
                this.pictureBox.Image = TH_UpdataPictureBox.BaseMapPicture;
                if (TH_UpdataPictureBox.CurrsorInMap) { this.Cursor = TH_UpdataPictureBox.Cursor; }
            });
        }
        private void Form_Start_FormClosed(object sender, FormClosedEventArgs e)
        {
            config.Timer.Close();

            TH_AutoSearchTrack.control.Abort = true;
            TH_SendCommand.Abort = true;
            TH_MeasureSurrounding.Abort = true;

            TH_SendCommand.Close();
            TH_MeasureSurrounding.Close();
            TH_MeasurePosition.Close();

            Configuration.Save();
        }
        private void Form_Start_Load(object sender, EventArgs e)
        {
            // 加载配置信息
            Configuration.Load();

            // Map
            foreach (CONFIG.FILE file in config.Map)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedMap;

                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem SelectedMap = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems
                [config.SelectedMap + 5];
            setSelectedMap(SelectedMap, e);

            TH_UpdataPictureBox.Initial();

            // Route
            foreach (CONFIG.FILE file in config.Route)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedRoute;

                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem SelectedRoute = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems
                [config.SelectedRoute + 6];
            setSelectedRoute(SelectedRoute, e);

            // ControlPort
            foreach (string COM in config.ConPortName)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(COM);
                NewMenu.Click += getCON_PortName;

                int N = this.CON_PortNameToolStripMenuItem.DropDownItems.Count;
                this.CON_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }
            foreach (int baudRate in config.ConBaudRate)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(baudRate.ToString());
                NewMenu.Click += getCON_BaudRate;

                int N = this.CON_BaudRateToolStripMenuItem.DropDownItems.Count;
                this.CON_BaudRateToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }

            if (config.SelectedControlPortName != -1)
            {
                ToolStripMenuItem Selected_ConPortName = (ToolStripMenuItem)
                    this.CON_PortNameToolStripMenuItem.DropDownItems[config.SelectedControlPortName];

                getCON_PortName(Selected_ConPortName, e);
            }
            if (config.SelectedControlBaudRate != -1)
            {
                ToolStripMenuItem Selected_ConBaudRate = (ToolStripMenuItem)
                    this.CON_BaudRateToolStripMenuItem.DropDownItems[config.SelectedControlBaudRate];

                getCON_BaudRate(Selected_ConBaudRate, e);
            }

            this.checkConPort.Checked = config.CheckControlPort;

            // UrgPort
            foreach (string COM in config.UrgPortName)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(COM);
                NewMenu.Click += getURG_PortName;

                int N = this.URG_PortNameToolStripMenuItem.DropDownItems.Count;
                this.URG_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }
            foreach (int baudRate in config.UrgBaudRate)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(baudRate.ToString());
                NewMenu.Click += getURG_BaudRate;

                int N = this.URG_BaudRateToolStripMenuItem.DropDownItems.Count;
                this.URG_BaudRateToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }

            if (config.SelectedUrgPortName != -1)
            {
                ToolStripMenuItem Selected_UrgPortName = (ToolStripMenuItem)
                    this.URG_PortNameToolStripMenuItem.DropDownItems[config.SelectedUrgPortName];

                getURG_PortName(Selected_UrgPortName, e);
            }
            if (config.SelectedUrgBaudRate != -1)
            {
                ToolStripMenuItem Selected_UrgBaudRate = (ToolStripMenuItem)
                    this.URG_BaudRateToolStripMenuItem.DropDownItems[config.SelectedUrgBaudRate];

                getURG_BaudRate(Selected_UrgBaudRate, e);
            }

            this.checkUrgPort.Checked = config.CheckUrgPort;
            this.urgRangeToolStripMenuItem.Text = config.urgRange.ToString();

            // LocatePort
            foreach (string COM in config.LocPortName)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(COM);
                NewMenu.Click += getLOC_PortName;

                int N = this.LOC_PortNameToolStripMenuItem.DropDownItems.Count;
                this.LOC_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }
            foreach (int baudRate in config.LocBaudRate)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(baudRate.ToString());
                NewMenu.Click += getLOC_BaudRate;

                int N = this.LOC_BaudRateToolStripMenuItem.DropDownItems.Count;
                this.LOC_BaudRateToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);
            }

            if (config.SelectedLocatePortName != -1)
            {
                ToolStripMenuItem Selected_LocPortName = (ToolStripMenuItem)
                    this.LOC_PortNameToolStripMenuItem.DropDownItems[config.SelectedLocatePortName];

                getLOC_PortName(Selected_LocPortName, e);
            }
            if (config.SelectedLocateBaudRate != -1)
            {
                ToolStripMenuItem Selected_LocBaudRate = (ToolStripMenuItem)
                    this.LOC_BaudRateToolStripMenuItem.DropDownItems[config.SelectedLocateBaudRate];

                getLOC_BaudRate(Selected_LocBaudRate, e);
            }

            this.checkLocPort.Checked = config.CheckLocatePort;

            // config
            config.KeyValue = new List<int>();
            config.Timer = new System.Timers.Timer(100);
            config.Timer.Elapsed += new System.Timers.ElapsedEventHandler(Refresh_FormStart);
            config.Timer.AutoReset = true;
            config.Timer.Start();

            Form_SizeChanged(sender, e);
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            // 刷新位置
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            this.panel1.Height = this.Height - 100;
            this.panel1.Width = this.Width - 30;
            
            Point picBoxPos = new Point(0, 0);
            if (TH_UpdataPictureBox.MapLength < this.panel1.Height)
            {
                picBoxPos.Y = (this.panel1.Height - TH_UpdataPictureBox.MapLength) / 2;
            }
            if (TH_UpdataPictureBox.MapWidth < this.panel1.Width)
            {
                picBoxPos.X = (this.panel1.Width - TH_UpdataPictureBox.MapWidth) / 2;
            }
            this.pictureBox.Location = picBoxPos;

            this.TimeLabel.Location = new Point(12, this.Height - 60);

            this.EventLabel.Width = this.Width - 185;
            this.EventLabel.Location = new Point(this.Width - this.EventLabel.Width - 20, this.Height - 60);
        }


        private void MouseEnterMap(object sender, EventArgs e)
        {
            TH_UpdataPictureBox.CurrsorInMap = true;
        }
        private void MouseLeaveMap(object sender, EventArgs e)
        {
            TH_UpdataPictureBox.CurrsorInMap = false;
            this.Cursor = Cursors.Default;
        }
        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            TH_UpdataPictureBox.setMousePosition(e.X, e.Y);
        }
        private void MapMouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) { return; }
            TH_UpdataPictureBox.MouseLeftClicked();
        }
        private void MapMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }

            TH_UpdataPictureBox.MOUSE mousePos = TH_UpdataPictureBox.getMousePosition();
            if (mousePos.No == -1) { return; }
            if (mousePos.Direction != TH_AutoSearchTrack.Direction.Tuning) { return; }
            if (mousePos.No == 0) { TH_UpdataPictureBox.Route.RemoveAt(TH_UpdataPictureBox.Route.Count - 1); }

            HouseMap.STACK stack = HouseMap.Stacks[mousePos.No];

            Form_Stack.Form_Stack.StackNo = stack.No;
            Form_Stack.Form_Stack.Direction = (int)stack.CarPosition;
            //Form_Stack.Form_Stack.Distance = stack.;

            Form_Stack.Form_Stack.Length = stack.Length;
            Form_Stack.Form_Stack.Width = stack.Width;

            Form_Stack.Form_Stack.AisleWidth_U = stack.AisleWidth_U;
            Form_Stack.Form_Stack.AisleWidth_D = stack.AisleWidth_D;
            Form_Stack.Form_Stack.AisleWidth_L = stack.AisleWidth_L;
            Form_Stack.Form_Stack.AisleWidth_R = stack.AisleWidth_R;

            Form_Stack.Form_Stack.SetKeepU = stack.KeepDistanceU;
            Form_Stack.Form_Stack.SetKeepD = stack.KeepDistanceD;
            Form_Stack.Form_Stack.SetKeepL = stack.KeepDistanceL;
            Form_Stack.Form_Stack.SetKeepR = stack.KeepDistanceR;

            Form_Stack.Form_Stack formStack = new Form_Stack.Form_Stack();
            //formStack.Location = MousePosition;
            formStack.ShowDialog();

            stack.No = Form_Stack.Form_Stack.StackNo;
            stack.CarPosition = (TH_AutoSearchTrack.Direction)Form_Stack.Form_Stack.Direction;
            //stack. = Form_Stack.Form_Stack.StackNo;

            stack.Length = Form_Stack.Form_Stack.Length;
            stack.Width = Form_Stack.Form_Stack.Width;

            stack.AisleWidth_U = Form_Stack.Form_Stack.AisleWidth_U;
            stack.AisleWidth_D = Form_Stack.Form_Stack.AisleWidth_D;
            stack.AisleWidth_L = Form_Stack.Form_Stack.AisleWidth_L;
            stack.AisleWidth_R = Form_Stack.Form_Stack.AisleWidth_R;

            stack.KeepDistanceU = Form_Stack.Form_Stack.SetKeepU;
            stack.KeepDistanceD = Form_Stack.Form_Stack.SetKeepD;
            stack.KeepDistanceL = Form_Stack.Form_Stack.SetKeepL;
            stack.KeepDistanceR = Form_Stack.Form_Stack.SetKeepR;

            HouseMap.Stacks[mousePos.No] = stack;
            if (stack.No == 0) { TH_UpdataPictureBox.Stacks[0] = TH_UpdataPictureBox.RealStack2MapStack(0); return; }

            if (mousePos.No != 1 && mousePos.No != HouseMap.TotalStacks)
            {
                int No = mousePos.No - 1;
                if (mousePos.IsLeft) { No = mousePos.No + 1; }

                HouseMap.STACK ustack = HouseMap.Stacks[No];
                ustack.AisleWidth_D = stack.AisleWidth_U;
                HouseMap.Stacks[No] = ustack;
            }
            if (mousePos.No != HouseMap.TotalStacksL && mousePos.No != HouseMap.TotalStacksL + 1)
            {
                int No = mousePos.No + 1;
                if (mousePos.IsLeft) { No = mousePos.No - 1; }

                HouseMap.STACK dstack = HouseMap.Stacks[No];
                dstack.AisleWidth_U = stack.AisleWidth_D;
                HouseMap.Stacks[No] = dstack;
            }
            if (!mousePos.IsLeft)
            {
                HouseMap.STACK lstack = HouseMap.Stacks[HouseMap.TotalStacks - mousePos.No + 1];
                lstack.AisleWidth_R = stack.AisleWidth_L;
                HouseMap.Stacks[HouseMap.TotalStacks - mousePos.No + 1] = lstack;
            }
            if (mousePos.IsLeft)
            {
                HouseMap.STACK rstack = HouseMap.Stacks[HouseMap.TotalStacks - mousePos.No + 1];
                rstack.AisleWidth_L = stack.AisleWidth_R;
                HouseMap.Stacks[HouseMap.TotalStacks - mousePos.No + 1] = rstack;
            }

            for (int i = 1; i <= HouseMap.TotalStacks; i++)
            { TH_UpdataPictureBox.Stacks[i] = TH_UpdataPictureBox.RealStack2MapStack(i); }
        }
        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }
            string item = e.ClickedItem.Text;
            contextMenuStrip.Close();

            if (item == "Finish")
            {
                if (TH_UpdataPictureBox.Route.Count != 0) { TH_UpdataPictureBox.DrawOver = true; }
            }
            if (item == "Save Map" && config.CheckMap)
            {
                if (TH_UpdataPictureBox.Route.Count != 0) { TH_UpdataPictureBox.DrawOver = true; }

                int index = -1;
                Configuration.Save_Map(ref index);
                if (index == -1) { return; }

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(config.Map[config.Map.Count - 1].Name);
                NewMenu.Click += setSelectedMap;
                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
                setSelectedMap(NewMenu, e);
            }
            if (item == "Save Route" && config.CheckRoute)
            {
                if (TH_UpdataPictureBox.Route.Count != 0) { TH_UpdataPictureBox.DrawOver = true; }

                int index = -1;
                Configuration.Save_Route(ref index);
                if (index == -1) { return; }

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(config.Route[config.Route.Count - 1].Name);
                NewMenu.Click += setSelectedMap;
                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
                setSelectedRoute(NewMenu, e);
            }
            if (item == "Clear") { TH_UpdataPictureBox.DrawOver = false; TH_UpdataPictureBox.ClieckedClear(); }
            if (item == "Undo") { TH_UpdataPictureBox.DrawOver = false; TH_UpdataPictureBox.ClieckedUndo(); }
        }

        private void Start(object sender, EventArgs e)
        {
            TH_MeasurePosition.Open();
            TH_MeasureSurrounding.Open();
            TH_SendCommand.Open();

            if (this.btnStart.Text == "Stop")
            {
                TH_AutoSearchTrack.control.ActionList.Insert(0, TH_AutoSearchTrack.Action.Stop);
                TH_AutoSearchTrack.control.EMA = true;
                this.btnStart.Text = "Continue"; return;
            }

            if (this.btnStart.Text == "Continue")
            {
                if (TH_AutoSearchTrack.control.ActionList != null &&
                    TH_AutoSearchTrack.control.ActionList.Count > 0 &&
                    TH_AutoSearchTrack.control.ActionList[0] == TH_AutoSearchTrack.Action.Stop)
                { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

                TH_AutoSearchTrack.control.EMA = false;
                this.btnStart.Text = "Stop"; return;
            }

            this.btnStart.Text = "Stop";
            TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.AlignF);
            TH_AutoSearchTrack.Start();
        }
        private void Restart(object sender, EventArgs e)
        {
            TH_MeasurePosition.Open();
            TH_MeasureSurrounding.Open();
            TH_SendCommand.Open();

            DialogResult dr = MessageBox.Show("Do you want to Restart ?", "Attention", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) { return; }

            this.btnStart.Text = "Stop";
            TH_AutoSearchTrack.Restart();
        }
        private void formKeyDown(object sender, KeyEventArgs e)
        {
            // 1-49 a-65
            if (49 <= e.KeyValue && e.KeyValue <= 58)
            { config.KeyValue.Clear(); config.KeyValue.Add(e.KeyValue); return; }

            // a
            if (e.KeyValue == 65)
            {
                //TH_AutoSearchTrack.control.ActionList.Clear();
                //TH_AutoSearchTrack.control.EMA = true;

                //TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Wait);
            }
        }

        private void setSelectedMap(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.mapToolStripMenuItem.DropDownItems.Count;

            for (int i = 4; i < N; i++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[i];
                iMenu.Checked = false;
            }
            
            config.SelectedMap = -1;
            for (int i = 0; i < config.Map.Count; i++)
            {
                if (config.Map[i].Name == menu.Text) { config.SelectedMap = i; break; }
            }

            menu.Checked = true;
            showMapRoute();
        }
        private void setSelectedRoute(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.routeToolStripMenuItem.DropDownItems.Count;

            for (int i = 5; i < N; i++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[i];
                iMenu.Checked = false;
            }

            config.SelectedRoute = -1;
            for (int i = 0; i < config.Route.Count; i++)
            {
                if (config.Route[i].Name == menu.Text) { config.SelectedRoute = i; break; }
            }

            menu.Checked = true;
            showMapRoute();
        }
        private void showMapRoute()
        {
            string Map = "Auto", Route = "Auto";

            if (config.SelectedMap != -1) { Map = config.Map[config.SelectedMap].Name; }
            if (config.SelectedRoute != -1) { Route = config.Route[config.SelectedRoute].Name; }

            this.Text = "Map: " + Map + " / Route: " + Route;
        }

        private void openConfig(object sender, EventArgs e)
        {
            // 读取文件内容
            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));

            string FullPath = exe_path + "\\cqu_agv.cfg";
            if (!File.Exists(FullPath)) { return; }

            // 打开文件
            System.Diagnostics.Process.Start("notepad.exe", FullPath);
        }

        private void getCON_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.CON_PortNameToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.ConPortName.Count; i++)
            { if (config.ConPortName[i] == menu.Text) { config.SelectedControlPortName = i; break; } }

            menu.Checked = true;
            this.CON_PortNameToolStripMenuItem.Text = menu.Text;
        }
        private void getCON_BaudRate(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.CON_BaudRateToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.ConBaudRate.Count; i++)
            { if (config.ConBaudRate[i].ToString() == menu.Text) { config.SelectedControlBaudRate = i; break; } }

            menu.Checked = true;
            this.CON_BaudRateToolStripMenuItem.Text = menu.Text;
        }
        private void openControlPort(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            string portName = this.CON_PortNameToolStripMenuItem.Text;

            string text = menu.Text;
            menu.Text = "Open";
            this.controlPortToolStripMenuItem.Text = "Con Closed";

            if (text.Equals("Open"))
            {
                if (!TH_SendCommand.Open()) { return; }

                menu.Text = "Close";
                this.controlPortToolStripMenuItem.Text = "Con Opened"; return;
            }
            if (text.Equals("Close"))
            {
                if (!TH_SendCommand.Close()) { return; }

                menu.Text = "Open";
                this.controlPortToolStripMenuItem.Text = "Con Closed"; return;
            }
        }
        private void addCON_PortName(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Location = new Point(MousePosition.X, MousePosition.Y);
            input.ShowDialog();

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(input.Input);
            NewMenu.Click += getCON_PortName;

            int N = this.CON_PortNameToolStripMenuItem.DropDownItems.Count;
            this.CON_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);

            config.ConPortName.Add(input.Input);
            getCON_PortName(NewMenu, e);
        }
        private void delCON_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (this.CON_PortNameToolStripMenuItem.DropDownItems.Count == 2) { return; }

            int removeIndex = -1;
            foreach (ToolStripMenuItem iMenu in this.CON_PortNameToolStripMenuItem.DropDownItems)
            { removeIndex++; if (iMenu.Checked) { break; } }
            
            this.CON_PortNameToolStripMenuItem.DropDownItems.RemoveAt(removeIndex);
            config.ConPortName.RemoveAt(removeIndex);

            config.SelectedControlPortName = -1;
            this.CON_PortNameToolStripMenuItem.Text = "PortName";

            int showNext = removeIndex;
            int N = config.ConPortName.Count;
            if (showNext >= N) { showNext--; }
            if (showNext == -1) { return; }

            getCON_PortName(this.CON_PortNameToolStripMenuItem.DropDownItems[showNext], e);
        }
        private void addCON_BaudRate(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Location = new Point(MousePosition.X, MousePosition.Y);
            input.ShowDialog();

            int inputBaudRate = 0;
            try { inputBaudRate = int.Parse(input.Input); } catch { MessageBox.Show("Input Error!"); return; }

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(input.Input);
            NewMenu.Click += getCON_BaudRate;

            int N = this.CON_BaudRateToolStripMenuItem.DropDownItems.Count;
            this.CON_BaudRateToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);

            config.ConBaudRate.Add(inputBaudRate);
            getCON_BaudRate(NewMenu, e);
        }
        private void delCON_BaudRate(object sender, EventArgs e)
        {

        }

        private void getURG_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.URG_PortNameToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.UrgPortName.Count; i++)
            { if (config.UrgPortName[i] == menu.Text) { config.SelectedUrgPortName = i; break; } }

            menu.Checked = true;
            this.URG_PortNameToolStripMenuItem.Text = menu.Text;
        }
        private void getURG_BaudRate(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.URG_BaudRateToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.UrgBaudRate.Count; i++)
            { if (config.UrgBaudRate[i].ToString() == menu.Text) { config.SelectedUrgBaudRate = i; break; } }

            menu.Checked = true;
            this.URG_BaudRateToolStripMenuItem.Text = menu.Text;
        }
        private void openUrgPort(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            
            string text = menu.Text;
            menu.Text = "Open";
            this.UrgPortToolStripMenuItem.Text = "Urg Closed";

            if (text.Equals("Open"))
            {
                if (!TH_MeasureSurrounding.Open()) { return; }

                menu.Text = "Close";
                this.UrgPortToolStripMenuItem.Text = "Urg Opened"; return;
            }
            if (text.Equals("Close"))
            {
                if (!TH_MeasureSurrounding.Close()) { return; }

                menu.Text = "Open";
                this.UrgPortToolStripMenuItem.Text = "Urg Closed"; return;
            }
        }
        private void addURG_PortName(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Location = new Point(MousePosition.X, MousePosition.Y);
            input.ShowDialog();

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(input.Input);
            NewMenu.Click += getURG_PortName;

            int N = this.URG_PortNameToolStripMenuItem.DropDownItems.Count;
            this.URG_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);

            config.UrgPortName.Add(input.Input);
            getURG_PortName(NewMenu, e);
        }
        private void delURG_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (this.URG_PortNameToolStripMenuItem.DropDownItems.Count == 2) { return; }

            int removeIndex = -1;
            foreach (ToolStripMenuItem iMenu in this.URG_PortNameToolStripMenuItem.DropDownItems)
            { removeIndex++; if (iMenu.Checked) { break; } }

            this.URG_PortNameToolStripMenuItem.DropDownItems.RemoveAt(removeIndex);
            config.UrgPortName.RemoveAt(removeIndex);

            config.SelectedUrgPortName = -1;
            this.URG_PortNameToolStripMenuItem.Text = "PortName";

            int showNext = removeIndex;
            int N = config.UrgPortName.Count;
            if (showNext >= N) { showNext--; }
            if (showNext == -1) { return; }

            getURG_PortName(this.URG_PortNameToolStripMenuItem.DropDownItems[showNext], e);
        }
        private void setUrgRange(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Location = new Point(MousePosition.X, MousePosition.Y);
            input.ShowDialog();

            double urgRange = 0;
            try { urgRange = double.Parse(input.Input); } catch { MessageBox.Show("Input Error !"); return; }

            this.urgRangeToolStripMenuItem.Text = input.Input;
            config.urgRange = urgRange;
        }

        private void getLOC_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.LOC_PortNameToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.LocPortName.Count; i++)
            { if (config.LocPortName[i] == menu.Text) { config.SelectedLocatePortName = i; break; } }

            menu.Checked = true;
            this.LOC_PortNameToolStripMenuItem.Text = menu.Text;
        }
        private void getLOC_BaudRate(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem iMenu in this.LOC_BaudRateToolStripMenuItem.DropDownItems)
            {
                iMenu.Checked = false;
            }

            for (int i = 0; i < config.LocBaudRate.Count; i++)
            { if (config.LocBaudRate[i].ToString() == menu.Text) { config.SelectedLocateBaudRate = i; break; } }

            menu.Checked = true;
            this.LOC_BaudRateToolStripMenuItem.Text = menu.Text;
        }
        private void openLocPort(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            
            string text = menu.Text;
            menu.Text = "Open";
            this.LocatePortToolStripMenuItem.Text = "Loc Closed";

            if (text.Equals("Open"))
            {
                if (!TH_MeasurePosition.Open()) { return; }

                menu.Text = "Close";
                this.LocatePortToolStripMenuItem.Text = "Loc Opened"; return;
            }
            if (text.Equals("Close"))
            {
                if (!TH_MeasurePosition.Close()) { return; }

                menu.Text = "Open";
                this.LocatePortToolStripMenuItem.Text = "Loc Closed"; return;
            }
        }
        private void addLOC_PortName(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Location = new Point(MousePosition.X, MousePosition.Y);
            input.ShowDialog();

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(input.Input);
            NewMenu.Click += getLOC_PortName;

            int N = this.LOC_PortNameToolStripMenuItem.DropDownItems.Count;
            this.LOC_PortNameToolStripMenuItem.DropDownItems.Insert(N - 2, NewMenu);

            config.LocPortName.Add(input.Input);
            getLOC_PortName(NewMenu, e);
        }
        private void delLOC_PortName(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (this.LOC_PortNameToolStripMenuItem.DropDownItems.Count == 2) { return; }

            int removeIndex = -1;
            foreach (ToolStripMenuItem iMenu in this.LOC_PortNameToolStripMenuItem.DropDownItems)
            { removeIndex++; if (iMenu.Checked) { break; } }

            this.LOC_PortNameToolStripMenuItem.DropDownItems.RemoveAt(removeIndex);
            config.LocPortName.RemoveAt(removeIndex);

            config.SelectedLocatePortName = -1;
            this.LOC_PortNameToolStripMenuItem.Text = "PortName";

            int showNext = removeIndex;
            int N = config.LocPortName.Count;
            if (showNext >= N) { showNext--; }
            if (showNext == -1) { return; }

            getLOC_PortName(this.LOC_PortNameToolStripMenuItem.DropDownItems[showNext], e);
        }
    }
}
