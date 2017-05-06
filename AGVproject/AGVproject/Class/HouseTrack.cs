using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AGVproject.Solution_SLAM.Feature;

namespace AGVproject.Class
{
    /// <summary>
    /// 对路径信息进行操作
    /// </summary>
    class HouseTrack
    {
        //////////////////////////////////////////////////// public attribute ///////////////////////////////////////
        
        /// <summary>
        /// 路径总数量
        /// </summary>
        public static int TotalTrack
        {
            get { lock (config.TrackLock) { return Track.Count; } }
        }

        /// <summary>
        /// 路径信息
        /// </summary>
        public struct TRACK
        {
            /// <summary>
            /// 目标点的仓库坐标
            /// </summary>
            public CoordinatePoint.POINT TargetPosition;

            /// <summary>
            /// 是否为左边的垛区
            /// </summary>
            public bool IsLeft;
            /// <summary>
            /// 垛区编号
            /// </summary>
            public int No;
            /// <summary>
            /// 小车在垛区的哪一边
            /// </summary>
            public TH_AutoSearchTrack.Direction Direction;
            /// <summary>
            /// 距这一边左边端点的距离
            /// </summary>
            public double Distance;

            /// <summary>
            /// 辅助信息
            /// </summary>
            public object Auxiliary;
        }

        //////////////////////////////////////////////////// private attribute ///////////////////////////////////////
        
        private static List<TRACK> Track;
        private static CONFIG config;
        private struct CONFIG
        {
            public object TrackLock;
        }

        /////////////////////////////////////////////////// public method //////////////////////////////////////////

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initial()
        {
            Track = new List<TRACK>();
            config.TrackLock = new object();
        }
        /// <summary>
        /// 获取默认路径
        /// </summary>
        public static void getDefauteTrack()
        {
            TRACK oTrack = new TRACK();
            oTrack.IsLeft = HouseStack.getIsLeft(0);
            oTrack.No = 0;
            oTrack.Direction = TH_AutoSearchTrack.Direction.Down;
            oTrack.Distance = HouseStack.getLength(0) / 2;
            fillPosition(ref oTrack);
        }

        /// <summary>
        /// 清除当前路径信息
        /// </summary>
        public static void Clear()
        {
            lock (config.TrackLock) { Track.Clear(); }
        }
        /// <summary>
        /// 重新设置整个路径
        /// </summary>
        /// <param name="track">路径信息</param>
        public static void Set(List<TRACK> track)
        {
            lock (config.TrackLock) { Track = track; }
        }
        /// <summary>
        /// 获取整个路径的信息
        /// </summary>
        /// <returns></returns>
        public static List<TRACK> Get()
        {
            List<TRACK> track = new List<TRACK>();
            if (Track == null) { return track; }

            lock (config.TrackLock) { foreach (TRACK i in Track) { track.Add(i); } }
            return track;
        }
        /// <summary>
        /// 重新按堆垛信息获取整个路径的坐标
        /// </summary>
        public static void Fit()
        {
            for (int i = 0; i < Track.Count; i++) { fitTrack(i); }
        }
        
        /// <summary>
        /// 添加路径
        /// </summary>
        /// <param name="track">路径信息</param>
        public static void addTrack(TRACK track)
        {
            lock (config.TrackLock) { Track.Add(track); }
        }
        /// <summary>
        /// 在指定位置插入路径
        /// </summary>
        /// <param name="No">插入位置</param>
        /// <param name="track">路径信息</param>
        public static void addTrack(int No, TRACK track)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            lock (config.TrackLock) { Track.Insert(No, track); }
        }
        /// <summary>
        /// 删除路径
        /// </summary>
        /// <param name="No">路径编号</param>
        public static void delTrack(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            lock (config.TrackLock) { Track.RemoveAt(No); }
        }
        /// <summary>
        /// 设置路径
        /// </summary>
        /// <param name="No">路径编号</param>
        /// <param name="track">路径信息</param>
        public static void setTrack(int No, TRACK track)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            lock (config.TrackLock) { Track[No] = track; }
        }
        /// <summary>
        /// 获取路径
        /// </summary>
        /// <param name="No">路径编号</param>
        /// <returns></returns>
        public static TRACK getTrack(int No)
        {
            TRACK track = new TRACK();
            if (No < 0 || No > Track.Count - 1) { return track; }

            lock (config.TrackLock) { track = Track[No]; }
            return track;
        }
        /// <summary>
        /// 重新按堆垛信息获取路径的坐标
        /// </summary>
        /// <param name="No">路径编号</param>
        public static void fitTrack(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            lock (config.TrackLock)
            {
                TRACK track = Track[No];
                fillPosition(ref track);
                Track[No] = track;
            }
        }

        /// <summary>
        /// 利用相对于堆垛的位置获取仓库坐标系下的坐标
        /// </summary>
        /// <param name="pos">相对位置信息</param>
        /// <returns></returns>
        public static void fillPosition(ref TRACK pos)
        {
            CoordinatePoint.POINT Base = HouseStack.getPosition(pos.No);

            double keepU = HouseStack.getKeepDistanceU(pos.No);
            double keepD = HouseStack.getKeepDistanceD(pos.No);
            double keepL = HouseStack.getKeepDistanceL(pos.No);
            double keepR = HouseStack.getKeepDistanceR(pos.No);

            double L = HouseStack.getLength(pos.No);
            double W = HouseStack.getWidth(pos.No);

            if (pos.Direction == TH_AutoSearchTrack.Direction.Up) { Base.x += pos.Distance; Base.y -= keepU; }
            if (pos.Direction == TH_AutoSearchTrack.Direction.Down) { Base.x += L - pos.Distance; Base.y += W + keepD; }
            if (pos.Direction == TH_AutoSearchTrack.Direction.Left) { Base.x -= keepL; Base.y += W - pos.Distance; }
            if (pos.Direction == TH_AutoSearchTrack.Direction.Right) { Base.x += L + keepR; Base.y += pos.Distance; }

            pos.TargetPosition = Base;
        }
        
        /// <summary>
        /// 保存当前路径
        /// </summary>
        /// <returns></returns>
        public static bool Save()
        {
            // 加载保存界面
            string RoutePath = Form_Start.config.SelectedRoute < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedRoute].Path;
            string RouteName = Form_Start.config.SelectedRoute < 0 ? "Auto" : Form_Start.config.Map[Form_Start.config.SelectedRoute].Name;
            string path = "", name = "";

            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Route File（*.route）|*.route|Picture File（*.jpg）|*.jpg";
            sf.RestoreDirectory = true;
            sf.FileName = RouteName;
            if (sf.ShowDialog() != DialogResult.OK) { return false; }

            int cut = sf.FileName.LastIndexOf("\\");
            path = sf.FileName.Substring(0, cut);
            name = sf.FileName.Substring(cut + 1);
            name = name.Substring(0, name.Length - 6);
            if (name == "Auto") { MessageBox.Show("This Name is reserved !"); return false; }

            // 保存文件
            

            return true;
        }
        /// <summary>
        /// 加载指定路径
        /// </summary>
        /// <returns></returns>
        public static bool Load()
        {
            return false;
        }

        public static CoordinatePoint.POINT getTargetPosition(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return new CoordinatePoint.POINT(); }
            TRACK track = getTrack(No);

            return track.TargetPosition;
        }
        public static double getTragetPositionX(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return -1; }
            TRACK track = getTrack(No);

            return track.TargetPosition.x;
        }
        public static double getTragetPositionY(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return -1; }
            TRACK track = getTrack(No);

            return track.TargetPosition.y;
        }
        public static double getTragetPositionA(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return 0; }
            TRACK track = getTrack(No);

            return track.TargetPosition.aCar;
        }
    }
}
