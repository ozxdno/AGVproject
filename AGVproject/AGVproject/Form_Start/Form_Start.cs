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

            public List<FILE> Map;
            public List<FILE> Route;

            public int MapNameIndexBG;
            public int RouteNameIndexBG;
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
                if (TH_AutoSearchTrack.control.Event == null) { TH_AutoSearchTrack.control.Event = "Normal: Waitting  "; }
                this.EventLabel.Text = TH_AutoSearchTrack.control.Event;
                if (TH_AutoSearchTrack.control.EventColor.IsEmpty == true) { TH_AutoSearchTrack.control.EventColor = Color.Black; }
                this.EventLabel.ForeColor = TH_AutoSearchTrack.control.EventColor;

                // 刷新图片
                HouseMap.PictureBoxHeight = this.pictureBox.Height;
                HouseMap.PictureBoxWidth = this.pictureBox.Width;

                HouseMap.DrawMap();

                this.pictureBox.Height = HouseMap.MapHeight;
                this.pictureBox.Width = HouseMap.MapWidth;
                this.pictureBox.Image = HouseMap.Map;
                //if (TH_UpdataPictureBox.CurrsorInMap) { this.Cursor = TH_UpdataPictureBox.Cursor; }

                // 刷新开始按钮位置
                int xScroll = this.panel1.HorizontalScroll.Value / this.panel1.HorizontalScroll.Maximum;
                int yScroll = this.panel1.VerticalScroll.Value / this.panel1.VerticalScroll.Maximum;

                int posX = xScroll * this.panel1.Width + this.panel1.Width - 210;
                int posY = yScroll * this.panel1.Height + this.panel1.Height - 100;
                this.button.Location = new Point(posX, posY);

                int mouX = MousePosition.X;
                int mouY = MousePosition.Y;
                int reqX = this.Location.X + this.Width - 230;
                int reqY = this.Location.Y + this.Height - 130;
                this.button.Visible = reqX < mouX && mouX < reqX + 180 && reqY < mouY && mouY < reqY + 80;
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
            for (int i = config.Map.Count-1; i >= 0; i--)
            {
                if (!File.Exists(config.Map[i].Full)) { config.Map.RemoveAt(i); }
            }

            if (config.SelectedMap > config.Map.Count - 1) { config.SelectedMap = -1; }
            config.MapNameIndexBG = this.mapToolStripMenuItem.DropDownItems.Count;

            foreach (CONFIG.FILE file in config.Map)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedMap;

                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem SelectedMap = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems
                [config.SelectedMap + config.MapNameIndexBG];
            setSelectedMap(SelectedMap, e);

            // Route
            for (int i = config.Route.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(config.Route[i].Full)) { config.Route.RemoveAt(i); }
            }

            if (config.SelectedRoute > config.Route.Count - 1) { config.SelectedRoute = -1; }
            config.RouteNameIndexBG = this.routeToolStripMenuItem.DropDownItems.Count;

            foreach (CONFIG.FILE file in config.Route)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedRoute;

                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem SelectedRoute = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems
                [config.SelectedRoute + config.RouteNameIndexBG];
            setSelectedRoute(SelectedRoute, e);
            this.CheckRoute.Checked = config.CheckRoute;

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
            config.Timer = new System.Timers.Timer(100);
            config.Timer.Elapsed += new System.Timers.ElapsedEventHandler(Refresh_FormStart);
            config.Timer.AutoReset = true;
            config.Timer.Start();

            Form_SizeChanged(sender, e);
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            int posX = this.Width - 240;
            int posY = this.Height - 200;
            this.button.Location = new Point(posX, posY);

            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            this.panel1.Height = this.Height - 100;
            this.panel1.Width = this.Width - 30;
            
            Point picBoxPos = new Point(0, 0);
            if (HouseMap.MapHeight < this.panel1.Height)
            {
                picBoxPos.Y = (this.panel1.Height - HouseMap.MapHeight) / 2;
            }
            if (HouseMap.MapWidth < this.panel1.Width)
            {
                picBoxPos.X = (this.panel1.Width - HouseMap.MapWidth) / 2;
            }
            this.pictureBox.Location = picBoxPos;

            this.TimeLabel.Location = new Point(12, this.Height - 60);

            this.EventLabel.Width = this.Width - 185;
            this.EventLabel.Location = new Point(this.Width - this.EventLabel.Width - 20, this.Height - 60);
        }
        
        private void MouseEnterMap(object sender, EventArgs e)
        {
            HouseMap.MouseEnter();
        }
        private void MouseLeaveMap(object sender, EventArgs e)
        {
            HouseMap.MouseLeave();
            this.Cursor = Cursors.Default;
        }
        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            HouseMap.MouseMove(e.X, e.Y);
        }
        private void MapMouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) { return; }
            HouseMap.MouseLeftClicked();
        }
        private void MapMouseDoubleClicked(object sender, MouseEventArgs e)
        {
            HouseMap.MouseDoubleClicked();
        }
        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }
            string item = e.ClickedItem.Text;
            contextMenuStrip.Close();

            if (item == "Finish")
            {
            }
            if (item == "Save Map" && config.CheckMap)
            {
                saveMap(sender, e);
            }
            if (item == "Save Route" && config.CheckRoute)
            {
                saveRoute(sender, e);
            }
            if (item == "Clear")
            {
                HouseTrack.Clear();
                HouseMap.DrawOver = false;
            }
            if (item == "Undo")
            {
                HouseTrack.delTrack(HouseTrack.TotalTrack - 1);
                HouseMap.DrawOver = false;
            }
        }

        private void Start(object sender, EventArgs e)
        {
            if (!TH_MeasurePosition.IsOpen) { openLocPort(this.openLocatePortToolStripMenuItem, e); }
            if (!TH_MeasureSurrounding.IsOpen) { openUrgPort(this.OpenUrgPortToolStripMenuItem, e); }
            if (!TH_SendCommand.IsOpen) { openControlPort(this.OpenControlPortToolStripMenuItem, e); }
            
            if (this.button.Text == "Stop")
            {
                TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Stop;
                TH_AutoSearchTrack.control.EMA = true;
                Solution_FollowTrack.BuildRoute.Stop();
                this.button.Text = "Continue"; return;
            }

            if (this.button.Text == "Continue")
            {
                TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Continue;
                TH_AutoSearchTrack.control.EMA = false;
                
                if (!TH_MeasurePosition.IsOpen) { return; }
                if (!TH_MeasureSurrounding.IsOpen) { return; }
                if (!TH_SendCommand.IsOpen) { return; }
                
                this.button.Text = "Stop"; return;
            }


            TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Normal;
            TH_AutoSearchTrack.Start();
            
            //if (!TH_MeasurePosition.IsOpen) { return; }
            //if (!TH_MeasureSurrounding.IsOpen) { return; }
            //if (!TH_SendCommand.IsOpen) { return; }

            this.button.Text = "Stop";

        }
        private void Restart(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to Restart ?", "Attention", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.Cancel) { return; }

            openLocPort(this.openLocatePortToolStripMenuItem, e);
            openUrgPort(this.OpenUrgPortToolStripMenuItem, e);
            openControlPort(this.OpenControlPortToolStripMenuItem, e);
            TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Normal;
            TH_AutoSearchTrack.Restart();

            if (!TH_MeasurePosition.IsOpen) { return; }
            if (!TH_MeasureSurrounding.IsOpen) { return; }
            if (!TH_SendCommand.IsOpen) { return; }

            this.button.Text = "Stop";
        }
        private void formKeyDown(object sender, KeyEventArgs e)
        {
            
            
        }

        private void setSelectedMap(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.mapToolStripMenuItem.DropDownItems.Count;

            for (int i = config.MapNameIndexBG; i < N; i++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[i];
                iMenu.Checked = false;
            }
            
            config.SelectedMap = -1;
            for (int i = 0; i < config.Map.Count; i++)
            {
                if (config.Map[i].Name == menu.Text) { config.SelectedMap = i; break; }
            }

            HouseStack.getDefaultStacks();

            //bool existFile = Configuration.Load_Map(config.SelectedMap);
            //if (existFile)
            //{
            //    //List<TH_UpdataPictureBox.STACK> Stacks = new List<TH_UpdataPictureBox.STACK>();
            //    //for (int i = 0; i <= HouseStack.TotalStacks; i++)
            //    //{ Stacks.Add(TH_UpdataPictureBox.RealStack2MapStack(i)); }

            //    //TH_UpdataPictureBox.setStack(Stacks);
            //}
            //else
            //{
            //    //DialogResult dr = MessageBox.Show("Not Exist Map: " + menu.Text + ". Do you want to Delete it ?", "Tip", MessageBoxButtons.YesNo);
            //    //if ( dr == DialogResult.Yes) { delSelectedMap(sender, e); }
            //}

            menu.Checked = true;
            showMapRoute();
        }
        private void delSelectedMap(object sender, EventArgs e)
        {
            if (config.SelectedMap == -1) { return; }

            ToolStripMenuItem delMenu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
            delMenu.Dispose();

            //this.mapToolStripMenuItem.DropDownItems.RemoveAt(config.SelectedMap);
            config.Map.RemoveAt(config.SelectedMap);

            config.SelectedMap = -1;
            ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.MapNameIndexBG];
            menu.Checked = true;
            showMapRoute();
        }
        private void inputMap(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Select Map";
            fileDialog.Filter = "Map File(*.map)|*.map";
            if (fileDialog.ShowDialog() != DialogResult.OK) { return; }

            int cut = fileDialog.FileName.LastIndexOf("\\");
            string path = fileDialog.FileName.Substring(0, cut);
            string name = fileDialog.FileName.Substring(cut + 1);
            name = name.Substring(0, name.Length - 4);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return; }

            int index = -1;
            for (int i = 0; i < config.Map.Count; i++)
            {
                if (config.Map[i].Path != path || config.Map[i].Name != name) { continue; }
                index = i;
                DialogResult dr = MessageBox.Show("Exist Map: " + name + ". Do you want to Cover it ?", "Tip", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No) { return; }
            }

            if (index == -1)
            {
                index = config.Map.Count;

                CONFIG.FILE map = new CONFIG.FILE();
                map.Full = path + "\\" + name + ".map";
                map.Path = path;
                map.Name = name;
                map.Text = new string[0];
                config.Map.Add(map);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedMap;
                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[index + config.MapNameIndexBG];
            setSelectedMap(menu, e);
        }
        private void saveMap(object sender, EventArgs e)
        {
            int index = -1;
            bool needAdd = Configuration.Save_Map(ref index);
            if (index == -1) { return; }
            if (!needAdd)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[index + config.MapNameIndexBG];
                setSelectedMap(menu, e); return;
            }

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(config.Map[index].Name);
            NewMenu.Click += setSelectedMap;
            this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            setSelectedMap(NewMenu, e);
        }
        private void changePixLen(object sender, EventArgs e)
        {
            //Form_Input.Form_Input input = new Form_Input.Form_Input();
            //input.Location = new Point(MousePosition.X, MousePosition.Y);
            //input.ShowDialog();

            //double pixLen = 0;
            //try { pixLen = double.Parse(input.Input); } catch { MessageBox.Show("Input Error !"); return; }

            //this.pixLen.Text = input.Input;
            //config.PixLength = pixLen;

            //List<TH_UpdataPictureBox.STACK> Stacks = new List<TH_UpdataPictureBox.STACK>();
            //for (int i = 0; i <= HouseStack.TotalStacks; i++) { Stacks.Add(TH_UpdataPictureBox.RealStack2MapStack(i)); }

            //TH_UpdataPictureBox.setStack(Stacks);
        }

        private void setSelectedRoute(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.routeToolStripMenuItem.DropDownItems.Count;

            for (int i = config.RouteNameIndexBG; i < N; i++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[i];
                iMenu.Checked = false;
            }

            config.SelectedRoute = -1;
            for (int i = 0; i < config.Route.Count; i++)
            {
                if (config.Route[i].Name == menu.Text) { config.SelectedRoute = i; break; }
            }

            //bool existFile = Configuration.Load_Route(config.SelectedRoute);
            //if (existFile)
            //{
            //    //TH_UpdataPictureBox.DrawOver = TH_UpdataPictureBox.Route.Count != 0;
            //}
            //else
            //{
            //    //DialogResult dr = MessageBox.Show("Not Exist Route: " + menu.Text + ". Do you want to Delete it ?", "Tip", MessageBoxButtons.YesNo);
            //    //if (dr == DialogResult.Yes) { delSelectedRoute(sender, e); }
            //}

            menu.Checked = true;
            showMapRoute();
        }
        private void delSelectedRoute(object sender, EventArgs e)
        {
            if (config.SelectedRoute == -1) { return; }

            ToolStripMenuItem delMenu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
            delMenu.Dispose();

            //this.routeToolStripMenuItem.DropDownItems.RemoveAt(config.SelectedRoute);
            config.Route.RemoveAt(config.SelectedRoute);

            config.SelectedRoute = -1;
            ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.RouteNameIndexBG];
            setSelectedRoute(menu, e);
            showMapRoute();
        }
        private void inputRoute(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Select Route";
            fileDialog.Filter = "Route File(*.map)|*.route";
            if (fileDialog.ShowDialog() != DialogResult.OK) { return; }

            int cut = fileDialog.FileName.LastIndexOf("\\");
            string path = fileDialog.FileName.Substring(0, cut);
            string name = fileDialog.FileName.Substring(cut + 1);
            name = name.Substring(0, name.Length - 6);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return; }

            int index = -1;
            for (int i = 0; i < config.Route.Count; i++)
            {
                if (config.Route[i].Path != path || config.Route[i].Name != name) { continue; }
                index = i;
                DialogResult dr = MessageBox.Show("Exist Route: " + name + ". Do you want to Cover it ?", "Tip", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No) { return; }
            }

            if (index == -1)
            {
                index = config.Route.Count;

                CONFIG.FILE route = new CONFIG.FILE();
                route.Full = path + "\\" + name + ".route";
                route.Path = path;
                route.Name = name;
                route.Text = new string[0];
                config.Route.Add(route);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedRoute;
                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[index + config.RouteNameIndexBG];
            setSelectedRoute(menu, e);
        }
        private void editRoute(object sender, EventArgs e)
        {
            //TH_UpdataPictureBox.DrawOver = false;
        }
        private void saveRoute(object sender, EventArgs e)
        {
            HouseTrack.Save(); return;

            int index = -1;
            bool needAdd = Configuration.Save_Route(ref index);
            if (index == -1) { return; }
            if (!needAdd)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[index + config.RouteNameIndexBG];
                setSelectedRoute(menu, e); return;
            }

            ToolStripMenuItem NewMenu = new ToolStripMenuItem(config.Route[index].Name);
            NewMenu.Click += setSelectedRoute;
            this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            setSelectedRoute(NewMenu, e);
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
        private void clearUrg(object sender, EventArgs e)
        {
            TH_MeasureSurrounding.clearSurrounding();
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
        private void resetCurrPos(object sender, EventArgs e)
        {
            TH_MeasurePosition.setPosition(0, 0, 0);
        }
    }
}
