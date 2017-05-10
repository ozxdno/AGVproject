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
            public long Tick;

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

            public string WarningMessage;
            public long WarningTick;
            

            public struct FILE { public string Full, Path, Name; public string[] Text; }
        }
        public static void load()
        {
            config.Map = new List<CONFIG.FILE>();
            List<string> Map = Configuration.getFieldValue2_STRING("Form_Start.Map");
            for (int i = 0; i < Map.Count; i++)
            {
                CONFIG.FILE map = new CONFIG.FILE();
                string path = "", name = "", extension = "";
                Configuration.cutFullName(Map[i], ref path, ref name, ref extension);

                map.Full = Map[i];
                map.Path = path;
                map.Name = name;
                map.Text = Configuration.getFieldValue2("Map" + i.ToString());
                config.Map.Add(map);
            }

            config.Route = new List<CONFIG.FILE>();
            List<string> Route = Configuration.getFieldValue2_STRING("Form_Start.Route");
            for (int i = 0; i < Route.Count; i++)
            {
                CONFIG.FILE route = new CONFIG.FILE();
                string path = "", name = "", extension = "";
                Configuration.cutFullName(Route[i], ref path, ref name, ref extension);

                route.Full = Route[i];
                route.Path = path;
                route.Name = name;
                route.Text = Configuration.getFieldValue2("Route" + i.ToString());
                config.Route.Add(route);
            }

            config.SelectedMap = Configuration.getFieldValue1_INT("Form_Start.SelectedMap");
            config.SelectedRoute = Configuration.getFieldValue1_INT("Form_Start.SelectedRoute");
            config.CheckMap = Configuration.getFieldValue1_BOOL("Form_Start.CheckMap");
            config.CheckRoute = Configuration.getFieldValue1_BOOL("Form_Start.CheckRoute");

            config.ConPortName = Configuration.getFieldValue2_STRING("Form_Start.ConPortName");
            config.ConBaudRate = Configuration.getFieldValue2_INT("Form_Start.ConBaudRate");
            config.UrgPortName = Configuration.getFieldValue2_STRING("Form_Start.UrgPortName");
            config.UrgBaudRate = Configuration.getFieldValue2_INT("Form_Start.UrgBaudRate");
            config.LocPortName = Configuration.getFieldValue2_STRING("Form_Start.LocPortName");
            config.LocBaudRate = Configuration.getFieldValue2_INT("Form_Start.LocBaudRate");

            config.SelectedControlPortName = Configuration.getFieldValue1_INT("Form_Start.SelectedControlPortName");
            config.SelectedControlBaudRate = Configuration.getFieldValue1_INT("Form_Start.SelectedControlBaudRate");
            config.SelectedUrgPortName = Configuration.getFieldValue1_INT("Form_Start.SelectedUrgPortName");
            config.SelectedUrgBaudRate = Configuration.getFieldValue1_INT("Form_Start.SelectedUrgBaudRate");
            config.SelectedLocatePortName = Configuration.getFieldValue1_INT("Form_Start.SelectedLocatePortName");
            config.SelectedLocateBaudRate = Configuration.getFieldValue1_INT("Form_Start.SelectedLocateBaudRate");

            config.CheckControlPort = Configuration.getFieldValue1_BOOL("Form_Start.CheckControlPort");
            config.CheckUrgPort = Configuration.getFieldValue1_BOOL("Form_Start.CheckUrgPort");
            config.CheckLocatePort = Configuration.getFieldValue1_BOOL("Form_Start.CheckLocatePort");

            config.urgRange = Configuration.getFieldValue1_DOUBLE("Form_Start.urgRange");
        }
        public static void save()
        {
            List<string> Map = new List<string>();
            foreach (CONFIG.FILE map in config.Map) { Map.Add(map.Full); }
            Configuration.setFieldValue("Form_Start.Map", Map);
            for (int i = 0; i < config.Map.Count; i++)
            { Configuration.setFieldValue("Map" + i.ToString(), config.Map[i].Text); }

            List<string> Route = new List<string>();
            foreach (CONFIG.FILE route in config.Route) { Route.Add(route.Full); }
            Configuration.setFieldValue("Form_Start.Route", Route);
            for (int i = 0; i < config.Route.Count; i++)
            { Configuration.setFieldValue("Route" + i.ToString(), config.Route[i].Text); }

            Configuration.setFieldValue("Form_Start.SelectedMap", config.SelectedMap);
            Configuration.setFieldValue("Form_Start.SelectedRoute", config.SelectedRoute);
            Configuration.setFieldValue("Form_Start.CheckMap", config.CheckMap);
            Configuration.setFieldValue("Form_Start.CheckRoute", config.CheckRoute);

            Configuration.setFieldValue("Form_Start.ConPortName", config.ConPortName);
            Configuration.setFieldValue("Form_Start.ConBaudRate", config.ConBaudRate);
            Configuration.setFieldValue("Form_Start.UrgPortName", config.UrgPortName);
            Configuration.setFieldValue("Form_Start.UrgBaudRate", config.UrgBaudRate);
            Configuration.setFieldValue("Form_Start.LocPortName", config.LocPortName);
            Configuration.setFieldValue("Form_Start.LocBaudRate", config.LocBaudRate);

            Configuration.setFieldValue("Form_Start.SelectedControlPortName", config.SelectedControlPortName);
            Configuration.setFieldValue("Form_Start.SelectedControlBaudRate", config.SelectedControlBaudRate);
            Configuration.setFieldValue("Form_Start.SelectedUrgPortName", config.SelectedUrgPortName);
            Configuration.setFieldValue("Form_Start.SelectedUrgBaudRate", config.SelectedUrgBaudRate);
            Configuration.setFieldValue("Form_Start.SelectedLocatePortName", config.SelectedLocatePortName);
            Configuration.setFieldValue("Form_Start.SelectedLocateBaudRate", config.SelectedLocateBaudRate);

            Configuration.setFieldValue("Form_Start.CheckControlPort", config.CheckControlPort);
            Configuration.setFieldValue("Form_Start.CheckUrgPort", config.CheckUrgPort);
            Configuration.setFieldValue("Form_Start.CheckLocatePort", config.CheckLocatePort);

            Configuration.setFieldValue("Form_Start.urgRange", config.urgRange);
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
                config.Tick++;

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
                HouseMap.xScroll = (int)(this.panel1.Width * ((double)this.panel1.HorizontalScroll.Value / this.panel1.HorizontalScroll.Maximum));
                HouseMap.yScroll = (int)(this.panel1.Height * ((double)this.panel1.VerticalScroll.Value / this.panel1.VerticalScroll.Maximum));
                HouseMap.FormHeight = this.Height;
                HouseMap.FormWidth = this.Width;
                HouseMap.ShowPermitTrack = this.keepToolStripMenuItem.Checked;

                HouseMap.DrawMap();

                this.pictureBox.Height = HouseMap.MapHeight;
                this.pictureBox.Width = HouseMap.MapWidth;
                this.pictureBox.Image = HouseMap.Map;
                if (HouseMap.CursorInMap) { this.Cursor = HouseMap.Cursor; }

                // 刷新开始按钮位置
                int posX = this.panel1.Width - 210;
                int posY = this.panel1.Height - 100;
                this.button.Location = new Point(posX, posY);

                int mouX = MousePosition.X;
                int mouY = MousePosition.Y;
                int reqX = this.Location.X + this.Width - 230;
                int reqY = this.Location.Y + this.Height - 130;
                //this.button.Visible = HouseMap.DrawOver && reqX < mouX && mouX < reqX + 180 && reqY < mouY && mouY < reqY + 80;
                this.button.Visible = this.showToolStripMenuItem.Checked;

                // 刷新说明面板位置
                int yBG = this.Location.Y + 70;
                int yED = reqY + 80;
                this.textBox1.Location = new Point(posX, 0);
                //this.textBox1.Visible = HouseMap.DrawOver && reqX < mouX && mouX < reqX + 180 && yBG < mouY && mouY < yED;
                this.textBox1.Visible = this.showToolStripMenuItem.Checked;

                // 刷新警告信息位置
                if (config.WarningMessage == null) { config.WarningMessage = ""; }
                this.labelWarning.Text = config.WarningMessage;

                posX = (this.panel1.Width - this.labelWarning.Width) / 2;
                posY = (this.panel1.Height - this.labelWarning.Height) / 2;
                this.labelWarning.Location = new Point(posX, posY);
                this.labelWarning.Visible = config.WarningMessage.Length != 0 && config.Tick < config.WarningTick;
            });
        }
        private void Form_Start_FormClosed(object sender, FormClosedEventArgs e)
        {
            config.Timer.Close();

            TH_AutoSearchTrack.control.Abort = true;
            TH_AutoSearchTrack.control.EMA = true;
            TH_SendCommand.Abort = true;
            TH_MeasureSurrounding.Abort = true;

            TH_SendCommand.Close();
            TH_MeasureSurrounding.Close();
            TH_MeasurePosition.Close();
        }
        private void Form_Start_Load(object sender, EventArgs e)
        {
            // Map
            for (int i = config.Map.Count-1; i >= 0; i--)
            { if (!File.Exists(config.Map[i].Full)) { config.Map.RemoveAt(i); } }

            if (config.SelectedMap > config.Map.Count - 1) { config.SelectedMap = -1; }
            config.MapNameIndexBG = this.mapToolStripMenuItem.DropDownItems.Count;

            foreach (CONFIG.FILE file in config.Map)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedMap;
                NewMenu.ToolTipText = file.Full;
                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem map = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
            setSelectedMap(map, e);
            this.CheckMap.Checked = config.CheckMap;

            // Route
            for (int i = config.Route.Count - 1; i >= 0; i--)
            { if (!File.Exists(config.Route[i].Full)) { config.Route.RemoveAt(i); } }

            if (config.SelectedRoute > config.Route.Count - 1) { config.SelectedRoute = -1; }
            config.RouteNameIndexBG = this.routeToolStripMenuItem.DropDownItems.Count;

            foreach (CONFIG.FILE file in config.Route)
            {
                ToolStripMenuItem NewMenu = new ToolStripMenuItem(file.Name);
                NewMenu.Click += setSelectedRoute;
                NewMenu.ToolTipText = file.Full;
                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem route = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
            setSelectedRoute(route, e);
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

            // right menu
            this.pixLengthValue.Text = HouseMap.PixLength.ToString();
            this.urgRangeValue.Text = config.urgRange.ToString();
            
            // finish
            this.finishToolStripMenuItem.Checked = true;

            // permit route
            this.keepToolStripMenuItem.Checked = HouseMap.ShowPermitTrack;
            this.kEEPToolStripMenuItem1.Checked = HouseMap.ShowPermitTrack;

            // config
            config.Timer = new System.Timers.Timer(100);
            config.Timer.Elapsed += new System.Timers.ElapsedEventHandler(Refresh_FormStart);
            config.Timer.AutoReset = true;
            config.Timer.Start();
            config.WarningMessage = "Welcom";
            config.WarningTick = config.Tick + 12;

            Form_SizeChanged(sender, e);
            this.showToolStripMenuItem.Checked = true;
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 面板
            this.panel1.Height = this.Height - 100;
            this.panel1.Width = this.Width - 30;

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 刷新开始按钮位置
            int posX = this.panel1.Width - 210;
            int posY = this.panel1.Height - 100;
            this.button.Location = new Point(posX, posY);
            this.textBox1.Height = this.Height - this.button.Height - 140;

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 刷新说明面板位置
            int yBG = this.Location.Y + 70;
            int yED = this.Location.Y + this.Height - 50;
            this.textBox1.Location = new Point(posX, 0);

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 刷新警告信息位置
            if (config.WarningMessage == null) { config.WarningMessage = ""; }
            this.labelWarning.Text = config.WarningMessage;

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 事件标签
            this.EventLabel.Width = this.Width - 185;
            this.EventLabel.Location = new Point(this.Width - this.EventLabel.Width - 20, this.Height - 60);

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;

            // 图的位置
            Point picBoxPos = new Point(0, 0);
            if (HouseMap.MapHeight < this.panel1.Height)
            { picBoxPos.Y = (this.panel1.Height - HouseMap.MapHeight) / 2; }
            if (HouseMap.MapWidth < this.panel1.Width)
            { picBoxPos.X = (this.panel1.Width - HouseMap.MapWidth) / 2; }
            this.pictureBox.Location = picBoxPos;
            this.TimeLabel.Location = new Point(12, this.Height - 60);

            // 滑动量归零
            this.panel1.HorizontalScroll.Value = 0;
            this.panel1.VerticalScroll.Value = 0;
        }

        private void Menu_RouteKeep(object sender, EventArgs e)
        {
            this.keepToolStripMenuItem.Checked = !this.keepToolStripMenuItem.Checked;
            this.kEEPToolStripMenuItem1.Checked = this.keepToolStripMenuItem.Checked;
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
        private void MouseRightClicked_Draw(object sender, EventArgs e)
        {
            this.drawToolStripMenuItem.Checked = !this.drawToolStripMenuItem.Checked;
            HouseMap.DrawOver = !this.drawToolStripMenuItem.Checked;
            this.finishToolStripMenuItem.Checked = HouseMap.DrawOver;
        }
        private void MouseRightClicked_Keep(object sender, EventArgs e)
        {
            this.keepToolStripMenuItem.Checked = !this.keepToolStripMenuItem.Checked;
            this.kEEPToolStripMenuItem1.Checked = this.keepToolStripMenuItem.Checked;
        }
        private void MouseRightClicked_Undo(object sender, EventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }
            
            HouseTrack.delTrack();
        }
        private void MouseRightClicked_Finish(object sender, EventArgs e)
        {
            this.finishToolStripMenuItem.Checked = !this.finishToolStripMenuItem.Checked;
            HouseMap.DrawOver = this.finishToolStripMenuItem.Checked;
            this.drawToolStripMenuItem.Checked = !HouseMap.DrawOver;
        }
        private void MouseRightClicked_Clear(object sender, EventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }
            HouseTrack.Clear();
        }
        private void MouseRightClicked_Fit(object sender, EventArgs e)
        {
            if (!config.CheckMap && !config.CheckRoute) { return; }

            HouseStack.Fit();
            HouseTrack.Fit();
        }
        private void MouseRightClicked_Save(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Map File(*.map)|*.map|Route File(*.route)|*.route|Picture File(*.png)|*.png";
            
            bool OK = Configuration.Save(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string fullname = path + "\\" + name + "." + extension;
            if (extension == "png") { Bitmap MapPic = HouseMap.getMap(); MapPic.Save(fullname); MapPic.Dispose(); return; }
            
            if (extension == "map")
            {
                HouseStack.Save(fullname); config.SelectedMap = config.Map.Count;
                for (int i = 0; i < config.Map.Count; i++)
                { if (config.Map[i].Full == fullname) { config.SelectedMap = i; break; } }

                if (config.SelectedMap == config.Map.Count)
                {
                    CONFIG.FILE map = new CONFIG.FILE();
                    map.Full = fullname;
                    map.Path = path;
                    map.Name = name;
                    map.Text = new string[0];
                    config.Map.Add(map);

                    ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                    NewMenu.Click += setSelectedMap;
                    NewMenu.ToolTipText = fullname;
                    this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
                }

                ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
                setSelectedMap(menu, e);
            }

            if (extension == "route")
            {
                HouseTrack.Save(fullname); config.SelectedRoute = config.Route.Count;
                for (int i = 0; i < config.Route.Count; i++)
                { if (config.Route[i].Full == fullname) { config.SelectedRoute = i; break; } }

                if (config.SelectedRoute == config.Route.Count)
                {
                    CONFIG.FILE route = new CONFIG.FILE();
                    route.Full = fullname;
                    route.Path = path;
                    route.Name = name;
                    route.Text = new string[0];
                    config.Route.Add(route);

                    ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                    NewMenu.Click += setSelectedRoute;
                    NewMenu.ToolTipText = fullname;
                    this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
                }

                ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
                setSelectedRoute(menu, e);
            }
        }
        private void MouseRightClicked_Input(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Map File(*.map)|*.map|Route File(*.route)|*.route";
            bool OK = Configuration.Load(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string full = path + "\\" + name + "." + extension;

            if (extension == "map")
            {
                config.SelectedMap = config.Map.Count;
                for (int i = 0; i < config.Map.Count; i++)
                { if (config.Map[i].Full == full) { config.SelectedMap = i; break; } }

                if (config.SelectedMap == config.Map.Count)
                {
                    CONFIG.FILE map = new CONFIG.FILE();
                    map.Full = full;
                    map.Path = path;
                    map.Name = name;
                    map.Text = new string[0];
                    config.Map.Add(map);

                    ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                    NewMenu.Click += setSelectedMap;
                    NewMenu.ToolTipText = full;
                    this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
                }

                ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
                setSelectedMap(menu, e);
            }
            if (extension == "route")
            {
                config.SelectedRoute = config.Route.Count;
                for (int i = 0; i < config.Route.Count; i++)
                { if (config.Route[i].Full == full) { config.SelectedRoute = i; break; } }

                if (config.SelectedRoute == config.Route.Count)
                {
                    CONFIG.FILE route = new CONFIG.FILE();
                    route.Full = full;
                    route.Path = path;
                    route.Name = name;
                    route.Text = new string[0];
                    config.Route.Add(route);

                    ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                    NewMenu.Click += setSelectedRoute;
                    NewMenu.ToolTipText = full;
                    this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
                }

                ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
                setSelectedRoute(menu, e);
            }
        }
        private void MouseRightClicked_Show(object sender, EventArgs e)
        {
            this.showToolStripMenuItem.Checked = !this.showToolStripMenuItem.Checked;
        }
        private void MouseRightClicked_PixLengthValue(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Input = HouseMap.PixLength.ToString();
            input.ShowDialog();

            try { HouseMap.PixLength = double.Parse(input.Input); } catch
            { config.WarningMessage = "Input Error!"; config.WarningTick = config.Tick + 10; return; }

            this.pixLengthValue.Text = HouseMap.PixLength.ToString();
            Form_SizeChanged(sender, e);
        }
        private void MouseRightClicked_UrgRangeValue(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Input = config.urgRange.ToString();
            input.ShowDialog();

            try { config.urgRange = double.Parse(input.Input); }
            catch { config.WarningMessage = "Input Error!"; config.WarningTick = config.Tick + 10; return; }

            this.urgRangeValue.Text = config.urgRange.ToString();
        }
        private void MouseRightClicked_PositionValue(object sender, EventArgs e)
        {
            Form_Input.Form_Input input = new Form_Input.Form_Input();
            input.Input = TH_MeasurePosition.ToString();
            input.ShowDialog();

            CoordinatePoint.POINT pos = new CoordinatePoint.POINT();
            bool OK = TH_MeasurePosition.ToPosition(input.Input, ref pos);
            if (!OK) { config.WarningMessage = "Input Error!"; config.WarningTick = config.Tick + 10; return; }

            TH_MeasurePosition.setPosition(pos);
        }
        private void MouseRightClicked_SelectPosition(object sender, EventArgs e)
        {
            HouseMap.MOUSE mouse = HouseMap.getMousePosition();
            CoordinatePoint.POINT pos = TH_MeasurePosition.getPosition();

            double X = HouseMap.Length_Map2Real(mouse.X);
            double Y = HouseMap.Length_Map2Real(mouse.Y);
            TH_MeasurePosition.setPosition(X, Y, pos.aCar);
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
                Solution_FollowTrack.BuildTrack.Over = true;
                this.button.Text = "Continue"; return;
            }

            if (this.button.Text == "Continue")
            {
                TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Continue;
                TH_AutoSearchTrack.control.EMA = false;
                
                //if (!TH_MeasurePosition.IsOpen) { return; }
                //if (!TH_MeasureSurrounding.IsOpen) { return; }
                //if (!TH_SendCommand.IsOpen) { return; }
                
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

        static Solution_FollowTrack.CorrectPosition.CORRECT correct = new Solution_FollowTrack.CorrectPosition.CORRECT();
        private void formKeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyValue == 82)
            {
                correct = Solution_FollowTrack.CorrectPosition.getCorrect();
            }
            if (e.KeyValue == 83)
            {
                Solution_FollowTrack.CorrectPosition.Start(correct);
            }
            if (e.KeyValue == 66)
            {
                TH_AutoSearchTrack.ProcessHandle = Solution_FollowTrack.BuildTrack.Start;
            }
            if (e.KeyValue == 70)
            {
                TH_AutoSearchTrack.ProcessHandle = Solution_FollowTrack.FollowTrack.Start;
            }
        }

        private void setSelectedMap(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.mapToolStripMenuItem.DropDownItems.Count;

            for (int i = config.MapNameIndexBG - 1; i < N; i++)
            { ToolStripMenuItem iMenu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[i]; iMenu.Checked = false; }

            config.SelectedMap = -1;
            for (int i = config.MapNameIndexBG - 1; i < N; i++, config.SelectedMap++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[i];
                if (iMenu.ToolTipText == menu.ToolTipText) { break; }
            }

            if (config.SelectedMap != -1) { HouseStack.Load(menu.ToolTipText); }
            if (config.SelectedMap == -1) { HouseStack.getDefaultStacks(); }
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
            ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.MapNameIndexBG - 1];
            menu.Checked = true;
            showMapRoute();
        }
        private void inputMap(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Map File(*.map)|*.map";
            bool OK = Configuration.Load(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string full = path + "\\" + name + "." + extension;

            config.SelectedMap = config.Map.Count;
            for (int i = 0; i < config.Map.Count; i++)
            { if (config.Map[i].Full == full) { config.SelectedMap = i; break; } }

            if (config.SelectedMap == config.Map.Count)
            {
                CONFIG.FILE map = new CONFIG.FILE();
                map.Full = full;
                map.Path = path;
                map.Name = name;
                map.Text = new string[0];
                config.Map.Add(map);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedMap;
                NewMenu.ToolTipText = full;
                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
            setSelectedMap(menu, e);
        }
        private void saveMap(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Map File(*.map)|*.map|Picture File(*.png)|*.png";
            name = config.SelectedMap == -1 ? "Auto" : config.Map[config.SelectedMap].Name;

            bool OK = Configuration.Save(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string fullname = path + "\\" + name + "." + extension;
            if (extension == "png") { Bitmap MapPic = HouseMap.getMap(); MapPic.Save(fullname); MapPic.Dispose(); return; }
            
            HouseStack.Save(fullname); config.SelectedMap = config.Map.Count;
            for (int i = 0; i < config.Map.Count; i++)
            { if (config.Map[i].Full == fullname) { config.SelectedMap = i; break; } }

            if (config.SelectedMap == config.Map.Count)
            {
                CONFIG.FILE map = new CONFIG.FILE();
                map.Full = fullname;
                map.Path = path;
                map.Name = name;
                map.Text = new string[0];
                config.Map.Add(map);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedMap;
                NewMenu.ToolTipText = fullname;
                this.mapToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.mapToolStripMenuItem.DropDownItems[config.SelectedMap + config.MapNameIndexBG];
            setSelectedMap(menu, e);
        }

        private void setSelectedRoute(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            int N = this.routeToolStripMenuItem.DropDownItems.Count;

            for (int i = config.RouteNameIndexBG - 1; i < N; i++)
            { ToolStripMenuItem iMenu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[i]; iMenu.Checked = false; }

            config.SelectedRoute = -1;
            for (int i = config.RouteNameIndexBG - 1; i < N; i++, config.SelectedRoute++)
            {
                ToolStripMenuItem iMenu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[i];
                if (iMenu.ToolTipText == menu.ToolTipText) { break; }
            }

            if (config.SelectedRoute != -1) { HouseTrack.Load(menu.ToolTipText); }
            if (config.SelectedRoute == -1) { HouseTrack.getDefauteTrack(); }
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
            ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.RouteNameIndexBG - 1];
            setSelectedRoute(menu, e);
            showMapRoute();
        }
        private void inputRoute(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Route File(*.route)|*.route";
            bool OK = Configuration.Load(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string full = path + "\\" + name + "." + extension;

            config.SelectedRoute = config.Route.Count;
            for (int i = 0; i < config.Route.Count; i++)
            { if (config.Route[i].Full == full) { config.SelectedRoute = i; break; } }

            if (config.SelectedRoute == config.Route.Count)
            {
                CONFIG.FILE route = new CONFIG.FILE();
                route.Full = full;
                route.Path = path;
                route.Name = name;
                route.Text = new string[0];
                config.Route.Add(route);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedRoute;
                NewMenu.ToolTipText = full;
                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
            setSelectedRoute(menu, e);
        }
        private void editRoute(object sender, EventArgs e)
        {
            HouseStack.Fit(); HouseTrack.Fit();
        }
        private void saveRoute(object sender, EventArgs e)
        {
            string path = "", name = "", extension = "";
            string extensions = "Route File(*.route)|*.route|Picture File(*.png)|*.png";
            name = config.SelectedRoute == -1 ? "Auto" : config.Route[config.SelectedRoute].Name;

            bool OK = Configuration.Save(extensions, ref path, ref name, ref extension, false);
            if (!OK) { return; }
            string fullname = path + "\\" + name + "." + extension;
            if (extension == "png") { Bitmap MapPic = HouseMap.getMap(); MapPic.Save(fullname); MapPic.Dispose(); return; }
            
            HouseTrack.Save(fullname); config.SelectedRoute = config.Route.Count;
            for (int i = 0; i < config.Route.Count; i++)
            { if (config.Route[i].Full == fullname) { config.SelectedRoute = i; break; } }

            if (config.SelectedRoute == config.Route.Count)
            {
                CONFIG.FILE route = new CONFIG.FILE();
                route.Full = fullname;
                route.Path = path;
                route.Name = name;
                route.Text = new string[0];
                config.Route.Add(route);

                ToolStripMenuItem NewMenu = new ToolStripMenuItem(name);
                NewMenu.Click += setSelectedRoute;
                NewMenu.ToolTipText = fullname;
                this.routeToolStripMenuItem.DropDownItems.Add(NewMenu);
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)this.routeToolStripMenuItem.DropDownItems[config.SelectedRoute + config.RouteNameIndexBG];
            setSelectedRoute(menu, e);
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
