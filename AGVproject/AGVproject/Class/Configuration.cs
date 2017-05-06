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

        /// <summary>
        /// 清空 CFG 中缓存信息
        /// </summary>
        public static void Clear()
        {
            if (CFG == null) { CFG = new List<CFG_FILE>(); } CFG.Clear();
        }
        /// <summary>
        /// 判断 CFG 是否为空
        /// </summary>
        /// <returns></returns>
        public static bool IsEmpty()
        {
            return CFG == null || CFG.Count == 0;
        }
        /// <summary>
        /// 加载配置文件信息
        /// </summary>
        public static bool Load()
        {
            // 读取文件内容
            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            CFG = new List<CFG_FILE>();

            string FullPath = exe_path + "\\cqu_agv.cfg";
            if (!Load(FullPath)) { return false; }

            // 分发变量
            Hardware_PlatForm.Load();
            Hardware_URG.Load();
            Hardware_UltraSonic.Load();

            HouseMap.Load();
            HouseStack.Load();
            HouseTrack.Load();

            Form_Start.load();
            TH_AutoSearchTrack.Load();

            return true;
        }
        /// <summary>
        /// 加载指定位置的文件
        /// </summary>
        /// <param name="fullname">文件路径 + 文件名 + 后缀</param>
        public static bool Load(string fullname)
        {
            StreamReader sr = new StreamReader(fullname);

            try
            {
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
            }
            catch
            {
                sr.Close(); return false;
            }
            
            sr.Close(); return true;
        }
        /// <summary>
        /// 获取指定文件的文件路径、文件名、文件后缀。
        /// </summary>
        /// <param name="extensions">所支持的后缀</param>
        /// <param name="path">路径</param>
        /// <param name="name">名称</param>
        /// <param name="extension">后缀</param>
        /// <param name="load">是否加载该文件</param>
        public static bool Load(string extensions, ref string path, ref string name, ref string extension, bool load = true)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = extensions;
            if (fileDialog.ShowDialog() != DialogResult.OK) { return false; }

            int cut = fileDialog.FileName.LastIndexOf("\\");
            path = fileDialog.FileName.Substring(0, cut);
            name = fileDialog.FileName.Substring(cut + 1);

            cut = name.LastIndexOf('.');
            extension = name.Substring(cut + 1);
            name = name.Substring(0, cut);
            
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return false; }

            if (!load) { return true; }
            return Load(fileDialog.FileName);
        }
        /// <summary>
        /// 保存配置文件信息
        /// </summary>
        public static bool Save()
        {
            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            CFG = new List<CFG_FILE>();

            Hardware_PlatForm.Save();
            Hardware_URG.Save();
            Hardware_UltraSonic.Save();

            HouseMap.Save();
            HouseStack.Save();
            HouseTrack.Save();

            Form_Start.save();
            TH_AutoSearchTrack.Save();

            string FullPath = exe_path + "\\cqu_agv.cfg";
            return Save(FullPath);
        }
        /// <summary>
        /// 保存为指定名称的文件
        /// </summary>
        /// <param name="fullname">文件名</param>
        public static bool Save(string fullname)
        {
            StreamWriter sw = new StreamWriter(fullname, false);

            try
            {
                foreach (CFG_FILE cfg in CFG)
                {
                    string line = cfg.Field + ":";
                    if (cfg.Value.Length != 0) { line += cfg.Value[0]; }
                    for (int i = 1; i < cfg.Value.Length; i++) { line += "|" + cfg.Value[i]; }

                    sw.WriteLine(line);
                }
            }
            catch
            {
                sw.Close(); return false;
            }

            sw.Close(); return true;
        }
        /// <summary>
        /// 获取指定文件的文件路径、文件名、文件后缀。
        /// </summary>
        /// <param name="extensions">所支持的后缀</param>
        /// <param name="path">路径</param>
        /// <param name="name">名称</param>
        /// <param name="extension">后缀</param>
        /// <param name="save">是否保存该文件</param>
        public static bool Save(string extensions, ref string path, ref string name, ref string extension, bool save = true)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = extensions;
            sf.RestoreDirectory = true;
            sf.FileName = name;
            if (sf.ShowDialog() != DialogResult.OK) { return false; }

            int cut = sf.FileName.LastIndexOf("\\");
            path = sf.FileName.Substring(0, cut);
            name = sf.FileName.Substring(cut + 1);

            cut = name.LastIndexOf('.');
            extension = name.Substring(cut + 1);
            name = name.Substring(0, cut);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return false; }

            if (!save) { return true; }
            return Save(sf.FileName);
        }
        /// <summary>
        /// 切割完整的文件名，得到路径、名称、后缀信息
        /// </summary>
        /// <param name="fullname">完整文件名</param>
        /// <param name="path">路径</param>
        /// <param name="name">名称</param>
        /// <param name="extension">后缀</param>
        public static void cutFullName(string fullname, ref string path, ref string name, ref string extension)
        {
            int cut = fullname.LastIndexOf('\\');
            path = fullname.Substring(0, cut);
            name = fullname.Substring(cut + 1);

            cut = name.LastIndexOf('.');
            extension = name.Substring(cut + 1);
            name = name.Substring(0, cut);
        }

        public static string getFieldValue1_STRING(string Field)
        {
            if (CFG == null) { return ""; }
            
            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return item.Value[0]; } }

            return "";
        }
        public static int getFieldValue1_INT(string Field)
        {
            if (CFG == null) { return -1; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return int.Parse(item.Value[0]); } }

            return -1;
        }
        public static bool getFieldValue1_BOOL(string Field)
        {
            if (CFG == null) { return false; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return int.Parse(item.Value[0]) == 1; } }

            return false;
        }
        public static double getFieldValue1_DOUBLE(string Field)
        {
            if (CFG == null) { return 0; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return double.Parse(item.Value[0]); } }

            return 0;
        }
        public static string[] getFieldValue2(string Field)
        {
            if (CFG == null) { return new string[0]; }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return item.Value; } }

            return new string[0];
        }
        public static List<string> getFieldValue2_STRING(string Field)
        {
            if (CFG == null) { return new List<string>(); }

            foreach (CFG_FILE item in CFG)
            { if (item.Field.Equals(Field)) { CFG.Remove(item); return item.Value.ToList(); } }

            return new List<string>();
        }
        public static List<int> getFieldValue2_INT(string Field)
        {
            List<int> intValue = new List<int>();
            if (CFG == null) { return intValue; }

            foreach (CFG_FILE item in CFG)
            {
                if (item.Field.Equals(Field))
                {
                    CFG.Remove(item);
                    foreach (string istr in item.Value) { try { intValue.Add(int.Parse(istr)); } catch { } }
                    break;
                }
            }
            
            return intValue;
        }
        public static List<double> getFieldValue2_DOUBLE(string Field)
        {
            List<double> doubleValue = new List<double>();
            if (CFG == null) { return doubleValue; }

            foreach (CFG_FILE item in CFG)
            {
                if (item.Field.Equals(Field))
                {
                    CFG.Remove(item);
                    foreach (string istr in item.Value) { try { doubleValue.Add(double.Parse(istr)); } catch { } }
                    break;
                }
            }
            
            return doubleValue;
        }

        public static void setFieldValue(string Field, string Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value };
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, bool Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value ? new string[1] { "1" } : new string[1] { "0" };
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, int Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value.ToString() };
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, double Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = new string[1] { Value.ToString() };
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, string[] Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value;
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, List<string> Value)
        {
            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = Value.ToArray();
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, List<int> Value)
        {
            string[] strValue = new string[Value.Count];
            for (int i = 0; i < Value.Count; i++) { strValue[i] = Value[i].ToString(); }


            CFG_FILE cfg = new CFG_FILE();
            cfg.Field = Field;
            cfg.Value = strValue;
            CFG.Add(cfg);
        }
        public static void setFieldValue(string Field, List<double> Value)
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
