using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace AGVproject
{
    static class StartProject
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_Start());
        }
    }

    public struct KeyPoint
    {
        public int Type;
        public int No;

        public double X;
        public double Y;
        public double A;

        public int UltraSonicL;
        public int UltraSonicR;

        public double UrgL;
        public double UrgR;
        public double UrgK;
        public double UrgB;
        public double UrgExtraK;
        public double UrgExtraB;
    }
}
