using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace AGVproject.Class
{
    class Configuration
    {
        private static List<CFG_FILE> CFG;
        private struct CFG_FILE { public string Field; public string[] Value; }
        
        public static void Load()
        {
            // 读取文件内容
            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            CFG = new List<CFG_FILE>();

            string FullPath = exe_path + "\\cqu_agv.cfg";
            if (!File.Exists(FullPath)) { return; }

            StreamReader sr = new StreamReader(FullPath);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                int cut = line.IndexOf(':'); if (cut == -1) { continue; }

                string[] part = new string[2];
                part[0] = line.Substring(0, cut);
                part[1] = line.Substring(cut + 1);

                CFG_FILE item = new CFG_FILE();
                char[] split = new char[1] { '|' };
                item.Field = part[0];
                item.Value = part[1].Split(split, StringSplitOptions.RemoveEmptyEntries);
                CFG.Add(item);
            }
            sr.Close();

            Load_FormStart();
            Load_HouseMap();
            Load_Hardware();
            Load_AST();
        }
        public static void Save()
        {
            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            CFG = new List<CFG_FILE>();

            Save_FormStart();
            Save_HouseMap();
            Save_Hardware();
            Save_AST();

            string FullPath = exe_path + "\\cqu_agv.cfg";
            StreamWriter sw = new StreamWriter(FullPath, false);

            foreach (CFG_FILE cfg in CFG)
            {
                string line = cfg.Field + ":";
                if (cfg.Value.Length != 0) { line += cfg.Value[0]; }
                for (int i = 1; i < cfg.Value.Length; i++) { line += "|" + cfg.Value[i]; }

                sw.WriteLine(line);
            }

            sw.Close();
        }
        
        private static void Load_FormStart()
        {
            List<string> Path = getFieldValue2_STRING("FormStart.MapPath");
            List<string> Name = getFieldValue2_STRING("FormStart.MapName");
            Form_Start.config.Map = new List<Form_Start.CONFIG.FILE>();
            for (int i = 0; i < Path.Count; i++)
            {
                Form_Start.CONFIG.FILE file = new Form_Start.CONFIG.FILE();
                file.Path = Path[i];
                file.Name = Name[i];
                file.Full = file.Path + "\\" + file.Name + ".map";
                file.Text = getFieldValue2(file.Full);

                Form_Start.config.Map.Add(file);
            }

            Path = getFieldValue2_STRING("FormStart.RoutePath");
            Name = getFieldValue2_STRING("FormStart.RouteName");
            Form_Start.config.Route = new List<Form_Start.CONFIG.FILE>();
            for (int i = 0; i < Path.Count; i++)
            {
                Form_Start.CONFIG.FILE file = new Form_Start.CONFIG.FILE();
                file.Path = Path[i];
                file.Name = Name[i];
                file.Full = file.Path + "\\" + file.Name + ".route";
                file.Text = getFieldValue2(file.Full);

                Form_Start.config.Route.Add(file);
            }

            Form_Start.config.SelectedMap = getFieldValue1_INT("FormStart.SelectedMap");
            Form_Start.config.SelectedRoute = getFieldValue1_INT("FormStart.SelectedRoute");

            Form_Start.config.ConPortName = getFieldValue2_STRING("FormStart.ConPortName");
            Form_Start.config.UrgPortName = getFieldValue2_STRING("FormStart.UrgPortName");
            Form_Start.config.LocPortName = getFieldValue2_STRING("FormStart.LocPortName");

            Form_Start.config.ConBaudRate = getFieldValue2_INT("FormStart.ConBaudRate");
            Form_Start.config.UrgBaudRate = getFieldValue2_INT("FormStart.UrgBaudRate");
            Form_Start.config.LocBaudRate = getFieldValue2_INT("FormStart.LocBaudRate");

            Form_Start.config.SelectedControlPortName = getFieldValue1_INT("FormStart.SelectedControlPortName");
            Form_Start.config.SelectedUrgPortName = getFieldValue1_INT("FormStart.SelectedUrgPortName");
            Form_Start.config.SelectedLocatePortName = getFieldValue1_INT("FormStart.SelectedLocatePortName");

            Form_Start.config.SelectedControlBaudRate = getFieldValue1_INT("FormStart.SelectedControlBaudRate");
            Form_Start.config.SelectedUrgBaudRate = getFieldValue1_INT("FormStart.SelectedUrgBaudRate");
            Form_Start.config.SelectedLocateBaudRate = getFieldValue1_INT("FormStart.SelectedLocateBaudRate");

            Form_Start.config.CheckMap = getFieldValue1_INT("FormStart.CheckMap") == 1;
            Form_Start.config.CheckRoute = getFieldValue1_INT("FormStart.CheckRoute") == 1;
            Form_Start.config.CheckControlPort = getFieldValue1_INT("FormStart.CheckControlPort") == 1;
            Form_Start.config.CheckUrgPort = getFieldValue1_INT("FormStart.CheckUrgPort") == 1;
            Form_Start.config.CheckLocatePort = getFieldValue1_INT("FormStart.CheckLocatePort") == 1;

            Form_Start.config.urgRange = getFieldValue1_INT("FormStart.urgRange");
            Form_Start.config.PixLength = getFieldValue1_DOUBLE("FormStart.PixLength");
        }
        private static void Save_FormStart()
        {
            int N = Form_Start.config.Map.Count;
            string[] Value = new string[N];
            for (int i = 0; i < N; i++) { Value[i] = Form_Start.config.Map[i].Path; }
            setFieldValue("FormStart.MapPath", Value);
            Value = new string[N];
            for (int i = 0; i < N; i++) { Value[i] = Form_Start.config.Map[i].Name; }
            setFieldValue("FormStart.MapName", Value);
            for (int i = 0; i < N; i++)
            { setFieldValue(Form_Start.config.Map[i].Full, Form_Start.config.Map[i].Text); }
            setFieldValue("FormStart.SelectedMap", Form_Start.config.SelectedMap);

            N = Form_Start.config.Route.Count;
            Value = new string[N];
            for (int i = 0; i < N; i++) { Value[i] = Form_Start.config.Route[i].Path; }
            setFieldValue("FormStart.RoutePath", Value);
            Value = new string[N];
            for (int i = 0; i < N; i++) { Value[i] = Form_Start.config.Route[i].Name; }
            setFieldValue("FormStart.RouteName", Value);
            for (int i = 0; i < N; i++)
            { setFieldValue(Form_Start.config.Route[i].Full, Form_Start.config.Route[i].Text); }
            setFieldValue("FormStart.SelectedRoute", Form_Start.config.SelectedRoute);

            setFieldValue("FormStart.ConPortName", Form_Start.config.ConPortName);
            setFieldValue("FormStart.ConBaudRate", Form_Start.config.ConBaudRate);
            setFieldValue("FormStart.UrgPortName", Form_Start.config.UrgPortName);
            setFieldValue("FormStart.UrgBaudRate", Form_Start.config.UrgBaudRate);
            setFieldValue("FormStart.LocPortName", Form_Start.config.LocPortName);
            setFieldValue("FormStart.LocBaudRate", Form_Start.config.LocBaudRate);

            setFieldValue("FormStart.SelectedControlPortName", Form_Start.config.SelectedControlPortName);
            setFieldValue("FormStart.SelectedControlBaudRate", Form_Start.config.SelectedControlBaudRate);
            setFieldValue("FormStart.SelectedUrgPortName", Form_Start.config.SelectedUrgPortName);
            setFieldValue("FormStart.SelectedUrgBaudRate", Form_Start.config.SelectedUrgBaudRate);
            setFieldValue("FormStart.SelectedLocatePortName", Form_Start.config.SelectedLocatePortName);
            setFieldValue("FormStart.SelectedLocateBaudRate", Form_Start.config.SelectedLocateBaudRate);

            setFieldValue("FormStart.CheckMap", Form_Start.config.CheckMap);
            setFieldValue("FormStart.CheckRoute", Form_Start.config.CheckRoute);
            setFieldValue("FormStart.CheckControlPort", Form_Start.config.CheckControlPort);
            setFieldValue("FormStart.CheckUrgPort", Form_Start.config.CheckUrgPort);
            setFieldValue("FormStart.CheckLocatePort", Form_Start.config.CheckLocatePort);

            setFieldValue("FormStart.urgRange", Form_Start.config.urgRange);
            setFieldValue("FormStart.PixLength", Form_Start.config.PixLength);
        }
        private static void Load_HouseMap()
        {
            HouseMap.HouseLength = getFieldValue1_DOUBLE("HouseMap.HouseLength");
            HouseMap.HouseWidth = getFieldValue1_DOUBLE("HouseMap.HouseWidth");
            HouseMap.TotalStacksL = getFieldValue1_INT("HouseMap.TotalStacksL");
            HouseMap.TotalStacksR = getFieldValue1_INT("HouseMap.TotalStacksR");

            HouseMap.DefaultCentreRoadWidth = getFieldValue1_DOUBLE("HouseMap.DefaultCentreRoadWidth");
            HouseMap.DefaultAisleWidth = getFieldValue1_DOUBLE("HouseMap.DefaultAisleWidth");
            HouseMap.DefaultStackLength = getFieldValue1_DOUBLE("HouseMap.DefaultStackLength");
            HouseMap.DefaultStackWidth = getFieldValue1_DOUBLE("HouseMap.DefaultStackWidth");
        }
        private static void Save_HouseMap()
        {
            setFieldValue("HouseMap.HouseLength", HouseMap.HouseLength);
            setFieldValue("HouseMap.HouseWidth", HouseMap.HouseWidth);
            setFieldValue("HouseMap.TotalStacksL", HouseMap.TotalStacksL);
            setFieldValue("HouseMap.TotalStacksR", HouseMap.TotalStacksR);

            setFieldValue("HouseMap.DefaultCentreRoadWidth", HouseMap.DefaultCentreRoadWidth);
            setFieldValue("HouseMap.DefaultAisleWidth", HouseMap.DefaultAisleWidth);
            setFieldValue("HouseMap.DefaultStackLength", HouseMap.DefaultStackLength);
            setFieldValue("HouseMap.DefaultStackWidth", HouseMap.DefaultStackWidth);
        }
        private static void Load_Hardware()
        {
            Hardware_PlatForm.Length = getFieldValue1_DOUBLE("PlatForm.Length");
            Hardware_PlatForm.Width = getFieldValue1_DOUBLE("PlatForm.Width");
            Hardware_PlatForm.AxisSideL = getFieldValue1_DOUBLE("PlatForm.AxisSideL");
            Hardware_PlatForm.AxisSideR = getFieldValue1_DOUBLE("PlatForm.AxisSideR");
            Hardware_PlatForm.AxisSideU = getFieldValue1_DOUBLE("PlatForm.AxisSideU");
            Hardware_PlatForm.AxisSideD = getFieldValue1_DOUBLE("PlatForm.AxisSideD");
            Hardware_PlatForm.ForeSightBG = getFieldValue1_DOUBLE("PlatForm.ForeSightBG");
            Hardware_PlatForm.ForeSightED = getFieldValue1_DOUBLE("PlatForm.ForeSightED");

            Hardware_URG.max = getFieldValue1_DOUBLE("URG.max");
            Hardware_URG.min = getFieldValue1_DOUBLE("URG.min");
            Hardware_URG.AngleStart = getFieldValue1_DOUBLE("URG.AngleStart");
            Hardware_URG.AnglePace = getFieldValue1_DOUBLE("URG.AnglePace");
            Hardware_URG.ReceiveBG = getFieldValue1_INT("URG.ReceiveBG");
            Hardware_URG.ReceiveED = getFieldValue1_INT("URG.ReceiveED");
            Hardware_URG.CutBG = getFieldValue1_INT("URG.CutBG");
            Hardware_URG.CutED = getFieldValue1_INT("URG.CutED");

            List<double> Value = getFieldValue2_DOUBLE("UltraSonic.Head_L_X");
            Hardware_UltraSonic.Head_L_X.x = Value[0];
            Hardware_UltraSonic.Head_L_X.y = Value[1];
            Hardware_UltraSonic.Head_L_X.z = Value[2];
            Hardware_UltraSonic.Head_L_X.max = Value[3];
            Hardware_UltraSonic.Head_L_X.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Head_L_Y");
            Hardware_UltraSonic.Head_L_Y.x = Value[0];
            Hardware_UltraSonic.Head_L_Y.y = Value[1];
            Hardware_UltraSonic.Head_L_Y.z = Value[2];
            Hardware_UltraSonic.Head_L_Y.max = Value[3];
            Hardware_UltraSonic.Head_L_Y.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Head_R_X");
            Hardware_UltraSonic.Head_R_X.x = Value[0];
            Hardware_UltraSonic.Head_R_X.y = Value[1];
            Hardware_UltraSonic.Head_R_X.z = Value[2];
            Hardware_UltraSonic.Head_R_X.max = Value[3];
            Hardware_UltraSonic.Head_R_X.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Head_R_Y");
            Hardware_UltraSonic.Head_R_Y.x = Value[0];
            Hardware_UltraSonic.Head_R_Y.y = Value[1];
            Hardware_UltraSonic.Head_R_Y.z = Value[2];
            Hardware_UltraSonic.Head_R_Y.max = Value[3];
            Hardware_UltraSonic.Head_R_Y.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Tail_L_X");
            Hardware_UltraSonic.Tail_L_X.x = Value[0];
            Hardware_UltraSonic.Tail_L_X.y = Value[1];
            Hardware_UltraSonic.Tail_L_X.z = Value[2];
            Hardware_UltraSonic.Tail_L_X.max = Value[3];
            Hardware_UltraSonic.Tail_L_X.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Tail_L_Y");
            Hardware_UltraSonic.Tail_L_Y.x = Value[0];
            Hardware_UltraSonic.Tail_L_Y.y = Value[1];
            Hardware_UltraSonic.Tail_L_Y.z = Value[2];
            Hardware_UltraSonic.Tail_L_Y.max = Value[3];
            Hardware_UltraSonic.Tail_L_Y.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Tail_R_X");
            Hardware_UltraSonic.Tail_R_X.x = Value[0];
            Hardware_UltraSonic.Tail_R_X.y = Value[1];
            Hardware_UltraSonic.Tail_R_X.z = Value[2];
            Hardware_UltraSonic.Tail_R_X.max = Value[3];
            Hardware_UltraSonic.Tail_R_X.min = Value[4];

            Value = getFieldValue2_DOUBLE("UltraSonic.Tail_R_Y");
            Hardware_UltraSonic.Tail_R_Y.x = Value[0];
            Hardware_UltraSonic.Tail_R_Y.y = Value[1];
            Hardware_UltraSonic.Tail_R_Y.z = Value[2];
            Hardware_UltraSonic.Tail_R_Y.max = Value[3];
            Hardware_UltraSonic.Tail_R_Y.min = Value[4];
        }
        private static void Save_Hardware()
        {
            setFieldValue("PlatForm.Length", Hardware_PlatForm.Length);
            setFieldValue("PlatForm.Width", Hardware_PlatForm.Width);
            setFieldValue("PlatForm.AxisSideL", Hardware_PlatForm.AxisSideL);
            setFieldValue("PlatForm.AxisSideR", Hardware_PlatForm.AxisSideR);
            setFieldValue("PlatForm.AxisSideU", Hardware_PlatForm.AxisSideU);
            setFieldValue("PlatForm.AxisSideD", Hardware_PlatForm.AxisSideD);
            setFieldValue("PlatForm.ForeSightBG", Hardware_PlatForm.ForeSightBG);
            setFieldValue("PlatForm.ForeSightED", Hardware_PlatForm.ForeSightED);

            setFieldValue("URG.max", Hardware_URG.max);
            setFieldValue("URG.min", Hardware_URG.min);
            setFieldValue("URG.AngleStart", Hardware_URG.AngleStart);
            setFieldValue("URG.AnglePace", Hardware_URG.AnglePace);
            setFieldValue("URG.ReceiveBG", Hardware_URG.ReceiveBG);
            setFieldValue("URG.ReceiveED", Hardware_URG.ReceiveED);
            setFieldValue("URG.CutBG", Hardware_URG.CutBG);
            setFieldValue("URG.CutED", Hardware_URG.CutED);

            List<double> Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Head_L_X.x);
            Value.Add(Hardware_UltraSonic.Head_L_X.y);
            Value.Add(Hardware_UltraSonic.Head_L_X.z);
            Value.Add(Hardware_UltraSonic.Head_L_X.max);
            Value.Add(Hardware_UltraSonic.Head_L_X.min);
            setFieldValue("UltraSonic.Head_L_X", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Head_L_Y.x);
            Value.Add(Hardware_UltraSonic.Head_L_Y.y);
            Value.Add(Hardware_UltraSonic.Head_L_Y.z);
            Value.Add(Hardware_UltraSonic.Head_L_Y.max);
            Value.Add(Hardware_UltraSonic.Head_L_Y.min);
            setFieldValue("UltraSonic.Head_L_Y", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Head_R_X.x);
            Value.Add(Hardware_UltraSonic.Head_R_X.y);
            Value.Add(Hardware_UltraSonic.Head_R_X.z);
            Value.Add(Hardware_UltraSonic.Head_R_X.max);
            Value.Add(Hardware_UltraSonic.Head_R_X.min);
            setFieldValue("UltraSonic.Head_R_X", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Head_R_Y.x);
            Value.Add(Hardware_UltraSonic.Head_R_Y.y);
            Value.Add(Hardware_UltraSonic.Head_R_Y.z);
            Value.Add(Hardware_UltraSonic.Head_R_Y.max);
            Value.Add(Hardware_UltraSonic.Head_R_Y.min);
            setFieldValue("UltraSonic.Head_R_Y", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Tail_L_X.x);
            Value.Add(Hardware_UltraSonic.Tail_L_X.y);
            Value.Add(Hardware_UltraSonic.Tail_L_X.z);
            Value.Add(Hardware_UltraSonic.Tail_L_X.max);
            Value.Add(Hardware_UltraSonic.Tail_L_X.min);
            setFieldValue("UltraSonic.Tail_L_X", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Tail_L_Y.x);
            Value.Add(Hardware_UltraSonic.Tail_L_Y.y);
            Value.Add(Hardware_UltraSonic.Tail_L_Y.z);
            Value.Add(Hardware_UltraSonic.Tail_L_Y.max);
            Value.Add(Hardware_UltraSonic.Tail_L_Y.min);
            setFieldValue("UltraSonic.Tail_L_Y", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Tail_R_X.x);
            Value.Add(Hardware_UltraSonic.Tail_R_X.y);
            Value.Add(Hardware_UltraSonic.Tail_R_X.z);
            Value.Add(Hardware_UltraSonic.Tail_R_X.max);
            Value.Add(Hardware_UltraSonic.Tail_R_X.min);
            setFieldValue("UltraSonic.Tail_R_X", Value);

            Value = new List<double>();
            Value.Add(Hardware_UltraSonic.Tail_R_Y.x);
            Value.Add(Hardware_UltraSonic.Tail_R_Y.y);
            Value.Add(Hardware_UltraSonic.Tail_R_Y.z);
            Value.Add(Hardware_UltraSonic.Tail_R_Y.max);
            Value.Add(Hardware_UltraSonic.Tail_R_Y.min);
            setFieldValue("UltraSonic.Tail_R_Y", Value);
        }
        private static void Load_AST()
        {
            TH_AutoSearchTrack.control.MinDistance_H = getFieldValue1_DOUBLE("AST.MinDistance_H");
            TH_AutoSearchTrack.control.MinDistance_T = getFieldValue1_DOUBLE("AST.MinDistance_T");
            TH_AutoSearchTrack.control.MinDistance_L = getFieldValue1_DOUBLE("AST.MinDistance_L");
            TH_AutoSearchTrack.control.MinDistance_R = getFieldValue1_DOUBLE("AST.MinDistance_R");

            TH_AutoSearchTrack.control.MaxSpeed_X = getFieldValue1_INT("AST.MaxSpeed_X");
            TH_AutoSearchTrack.control.MaxSpeed_Y = getFieldValue1_INT("AST.MaxSpeed_Y");
            TH_AutoSearchTrack.control.MaxSpeed_A = getFieldValue1_INT("AST.MaxSpeed_A");
            
            TH_AutoSearchTrack.control.EMA = getFieldValue1_BOOL("AST.EMA");
            TH_AutoSearchTrack.control.NearStack = getFieldValue1_INT("AST.NearStack");
        }
        private static void Save_AST()
        {
            setFieldValue("AST.MinDistance_H", TH_AutoSearchTrack.control.MinDistance_H);
            setFieldValue("AST.MinDistance_T", TH_AutoSearchTrack.control.MinDistance_T);
            setFieldValue("AST.MinDistance_L", TH_AutoSearchTrack.control.MinDistance_L);
            setFieldValue("AST.MinDistance_R", TH_AutoSearchTrack.control.MinDistance_R);
            
            setFieldValue("AST.MaxSpeed_X", TH_AutoSearchTrack.control.MaxSpeed_X);
            setFieldValue("AST.MaxSpeed_Y", TH_AutoSearchTrack.control.MaxSpeed_Y);
            setFieldValue("AST.MaxSpeed_A", TH_AutoSearchTrack.control.MaxSpeed_A);
            
            setFieldValue("AST.EMA", TH_AutoSearchTrack.control.EMA);
            setFieldValue("AST.NearStack", TH_AutoSearchTrack.control.NearStack);
        }
        public static bool Save_Map(ref int index)
        {
            string MapPath = Form_Start.config.SelectedMap < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedMap].Path;
            string MapName = Form_Start.config.SelectedMap < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedMap].Name;
            string path = "", name = "";

            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Map File（*.map）|*.map";
            sf.RestoreDirectory = true;
            sf.FileName = MapName;
            if (sf.ShowDialog() != DialogResult.OK) { return false; }
            
            int cut = sf.FileName.LastIndexOf("\\");
            path = sf.FileName.Substring(0, cut);
            name = sf.FileName.Substring(cut + 1);
            name = name.Substring(0, name.Length - 4);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return false; }

            StreamWriter sw = new StreamWriter(sf.FileName);

            foreach (HouseMap.STACK stack in HouseMap.Stacks)
            {
                sw.WriteLine("No = " + stack.No.ToString());
                sw.WriteLine("IsLeft = " + stack.IsLeft.ToString());
                sw.WriteLine("Length = " + stack.Length.ToString());
                sw.WriteLine("Width = " + stack.Width.ToString());

                sw.WriteLine("AisleWidth_U = " + stack.AisleWidth_U.ToString());
                sw.WriteLine("AisleWidth_D = " + stack.AisleWidth_D.ToString());
                sw.WriteLine("AisleWidth_L = " + stack.AisleWidth_L.ToString());
                sw.WriteLine("AisleWidth_R = " + stack.AisleWidth_R.ToString());

                sw.WriteLine("KeepDistanceU = " + stack.KeepDistanceU.ToString());
                sw.WriteLine("KeepDistanceD = " + stack.KeepDistanceU.ToString());
                sw.WriteLine("KeepDistanceL = " + stack.KeepDistanceU.ToString());
                sw.WriteLine("KeepDistanceR = " + stack.KeepDistanceU.ToString());

                sw.WriteLine("CarPosition = " + ((int)stack.CarPosition).ToString());
                sw.WriteLine("Distance = " + stack.Distance.ToString());

                sw.WriteLine("");
            }

            sw.Close();
            
            foreach (Form_Start.CONFIG.FILE map in Form_Start.config.Map)
            {
                index++;
                if (map.Path != path || map.Name != name) { continue; }
                return false;
            }

            Form_Start.CONFIG.FILE newMap = new Form_Start.CONFIG.FILE();
            newMap.Full = path + "\\" + name + ".map";
            newMap.Path = path;
            newMap.Name = name;
            newMap.Text = new string[0];
            Form_Start.config.Map.Add(newMap);
            index = Form_Start.config.Map.Count - 1;
            Form_Start.config.SelectedMap = index;
            return true;
        }
        public static bool Load_Map(int index)
        {
            if (index == -1) { HouseMap.getDefaultStacks(); return true; }

            string filepath = Form_Start.config.Map[index].Full;
            if (!File.Exists(filepath)) { HouseMap.getDefaultStacks(); return false; }

            List<HouseMap.STACK> Stacks = new List<HouseMap.STACK>();
            int TotalL = 0, TotalR = 0;

            StreamReader sr = new StreamReader(filepath);
            while (!sr.EndOfStream)
            {
                HouseMap.STACK stack = new HouseMap.STACK();

                string line = sr.ReadLine(); stack.No = Convert.ToInt32(line.Substring(5));
                line = sr.ReadLine(); stack.IsLeft = Convert.ToBoolean(line.Substring(9));
                line = sr.ReadLine(); stack.Length = Convert.ToDouble(line.Substring(9));
                line = sr.ReadLine(); stack.Width = Convert.ToDouble(line.Substring(8));

                line = sr.ReadLine(); stack.AisleWidth_U = Convert.ToDouble(line.Substring(15));
                line = sr.ReadLine(); stack.AisleWidth_D = Convert.ToDouble(line.Substring(15));
                line = sr.ReadLine(); stack.AisleWidth_L = Convert.ToDouble(line.Substring(15));
                line = sr.ReadLine(); stack.AisleWidth_R = Convert.ToDouble(line.Substring(15));

                line = sr.ReadLine(); stack.KeepDistanceU = Convert.ToDouble(line.Substring(16));
                line = sr.ReadLine(); stack.KeepDistanceD = Convert.ToDouble(line.Substring(16));
                line = sr.ReadLine(); stack.KeepDistanceL = Convert.ToDouble(line.Substring(16));
                line = sr.ReadLine(); stack.KeepDistanceR = Convert.ToDouble(line.Substring(16));

                line = sr.ReadLine(); stack.CarPosition = (TH_AutoSearchTrack.Direction)Convert.ToInt32(line.Substring(14));
                line = sr.ReadLine(); stack.Distance = Convert.ToDouble(line.Substring(11));
                line = sr.ReadLine();

                Stacks.Add(stack);
                if (stack.No == 0) { continue; }
                if (stack.IsLeft) { TotalL++; } else { TotalR++; }
            }

            sr.Close();
            HouseMap.Stacks = Stacks;
            HouseMap.TotalStacksL = TotalL;
            HouseMap.TotalStacksR = TotalR;
            return true;
        }
        public static bool Save_Route(ref int index)
        {
            string RoutePath = Form_Start.config.SelectedRoute < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedRoute].Path;
            string RouteName = Form_Start.config.SelectedRoute < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedRoute].Name;
            string path = "", name = "";

            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Route File（*.route）|*.route";
            sf.RestoreDirectory = true;
            sf.FileName = RouteName;
            if (sf.ShowDialog() != DialogResult.OK) { return false; }

            int cut = sf.FileName.LastIndexOf("\\");
            path = sf.FileName.Substring(0, cut);
            name = sf.FileName.Substring(cut + 1);
            name = name.Substring(0, name.Length - 6);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return false; }

            StreamWriter sw = new StreamWriter(sf.FileName);

            foreach (TH_UpdataPictureBox.ROUTE route in TH_UpdataPictureBox.Route)
            {
                sw.WriteLine("No = " + route.No.ToString());
                sw.WriteLine("IsLeft = " + route.IsLeft.ToString());
                sw.WriteLine("Direction = " + ((int)route.Direction).ToString());
                sw.WriteLine("Direction = " + (route.Distance * Form_Start.config.PixLength).ToString());
                
                sw.WriteLine("");
            }

            sw.Close();

            foreach (Form_Start.CONFIG.FILE route in Form_Start.config.Route)
            {
                index++;
                if (route.Path != path || route.Name != name) { continue; }
                return false;
            }

            Form_Start.CONFIG.FILE newRoute = new Form_Start.CONFIG.FILE();
            newRoute.Full = path + "\\" + name + ".route";
            newRoute.Path = path;
            newRoute.Name = name;
            newRoute.Text = new string[0];
            Form_Start.config.Route.Add(newRoute);
            index = Form_Start.config.Route.Count - 1;
            Form_Start.config.SelectedRoute = index;
            return true;
        }
        public static bool Load_Route(int index)
        {
            if (index == -1)
            {
                while (TH_UpdataPictureBox.IsGettingRoute) ;
                TH_UpdataPictureBox.IsSettingRoute = true;
                TH_UpdataPictureBox.getAutoRoute();
                TH_UpdataPictureBox.IsSettingRoute = false;
                return true;
            }

            string filepath = Form_Start.config.Route[index].Full;
            if (!File.Exists(filepath)) { return false; }

            List<TH_UpdataPictureBox.ROUTE> Route = new List<TH_UpdataPictureBox.ROUTE>();

            StreamReader sr = new StreamReader(filepath);
            while (!sr.EndOfStream)
            {
                TH_UpdataPictureBox.ROUTE route = new TH_UpdataPictureBox.ROUTE();

                string line = sr.ReadLine(); route.No = Convert.ToInt32(line.Substring(5));
                line = sr.ReadLine(); route.IsLeft = Convert.ToBoolean(line.Substring(9));
                line = sr.ReadLine(); route.Direction = (TH_AutoSearchTrack.Direction)Convert.ToInt32(line.Substring(12));
                line = sr.ReadLine(); route.Distance = (int)(Convert.ToDouble(line.Substring(11)) / Form_Start.config.PixLength);
                
                line = sr.ReadLine();

                route.MapPoint = TH_UpdataPictureBox.getRouteMapPoint(route);
                Route.Add(route);
            }

            sr.Close();
            
            while (TH_UpdataPictureBox.IsGettingRoute) ;
            TH_UpdataPictureBox.IsSettingRoute = true;
            TH_UpdataPictureBox.Route = Route;
            TH_UpdataPictureBox.IsSettingRoute = false;
            return true;
        }

        private static string getFieldValue1_STRING(string Field)
        {
            if (CFG == null) { return ""; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return item.Value[0]; } }

            return "";
        }
        private static int getFieldValue1_INT(string Field)
        {
            if (CFG == null) { return -1; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return int.Parse(item.Value[0]); } }

            return -1;
        }
        private static bool getFieldValue1_BOOL(string Field)
        {
            if (CFG == null) { return false; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return int.Parse(item.Value[0]) == 1; } }

            return false;
        }
        private static double getFieldValue1_DOUBLE(string Field)
        {
            if (CFG == null) { return 0; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return double.Parse(item.Value[0]); } }

            return 0;
        }
        private static string[] getFieldValue2(string Field)
        {
            if (CFG == null) { return new string[0]; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return item.Value; } }

            return new string[0];
        }
        private static List<string> getFieldValue2_STRING(string Field)
        {
            if (CFG == null) { return new List<string>(); }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { return item.Value.ToList(); } }

            return new List<string>();
        }
        private static List<int> getFieldValue2_INT(string Field)
        {
            List<int> intValue = new List<int>();
            if (CFG == null) { return intValue; }

            foreach (CFG_FILE item in CFG)
            {
                if (item.Field.Equals(Field))
                { foreach (string istr in item.Value) { try { intValue.Add(int.Parse(istr)); } catch { } } }
            }

            return intValue;
        }
        private static List<double> getFieldValue2_DOUBLE(string Field)
        {
            List<double> doubleValue = new List<double>();
            if (CFG == null) { return doubleValue; }

            foreach (CFG_FILE item in CFG)
            {
                if (item.Field.Equals(Field))
                { foreach (string istr in item.Value) { try { doubleValue.Add(double.Parse(istr)); } catch { } } }
            }

            return doubleValue;
        }

        private static void setFieldValue(string Field, string Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value };
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, bool Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value ? new string[1] { "1" } : new string[1] { "0" };
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, int Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value.ToString() };
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, double Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value.ToString() };
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, string[] Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value;
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, List<string> Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value.ToArray();
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, List<int> Value)
        {
            string[] strValue = new string[Value.Count];
            for (int i = 0; i < Value.Count; i++) { strValue[i] = Value[i].ToString(); }


            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = strValue;
            CFG.Add(cfg);
        }
        private static void setFieldValue(string Field, List<double> Value)
        {
            string[] strValue = new string[Value.Count];
            for (int i = 0; i < Value.Count; i++) { strValue[i] = Value[i].ToString(); }


            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = strValue;
            CFG.Add(cfg);
        }
    }
}
