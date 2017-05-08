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
        /// 保存额外信息的代理函数
        /// </summary>
        public delegate void SaveExtra(object Extra);
        /// <summary>
        /// 保存路径额外信息的方法
        /// </summary>
        public static SaveExtra SaveHandle;

        /// <summary>
        /// 加载额外信息的代理函数
        /// </summary>
        public delegate object LoadExtra();
        /// <summary>
        /// 加载额外信息的方法
        /// </summary>
        public static LoadExtra LoadHandle;

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
            public object Extra;
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

            SaveHandle = Solution_FollowTrack.TrackFile.Save;
            LoadHandle = Solution_FollowTrack.TrackFile.Load;
        }
        /// <summary>
        /// 获取默认路径
        /// </summary>
        public static void getDefauteTrack()
        {
            Clear();

            TRACK track0 = new TRACK();
            track0.IsLeft = HouseStack.getIsLeft(0);
            track0.No = 0;
            track0.Direction = TH_AutoSearchTrack.Direction.Down;
            track0.Distance = HouseStack.getLength(0) / 2;
            fillPosition(ref track0);
            addTrack(track0);

            TRACK track1 = new TRACK();
            track1.IsLeft = false;
            track1.No = 1;
            track1.Direction = TH_AutoSearchTrack.Direction.Left;
            track1.Distance = HouseStack.getWidth(1) + HouseStack.getKeepDistanceU(1);
            fillPosition(ref track1);
            addTrack(track1);

            TRACK track2 = new TRACK();
            track2.IsLeft = false;
            track2.No = 1;
            track2.Direction = TH_AutoSearchTrack.Direction.Up;
            track2.Distance = HouseStack.getLength(1) + HouseStack.getKeepDistanceR(1);
            fillPosition(ref track2);
            addTrack(track2);

            for (int i = 1; i <= HouseStack.TotalStacksR; i++)
            {
                TRACK ur = new TRACK();
                ur.IsLeft = false;
                ur.No = i;
                ur.Direction = TH_AutoSearchTrack.Direction.Up;
                ur.Distance = HouseStack.getLength(i) + HouseStack.getKeepDistanceR(i);
                fillPosition(ref ur);
                addTrack(ur);

                TRACK ul = new TRACK();
                ul.IsLeft = false;
                ul.No = i;
                ul.Direction = TH_AutoSearchTrack.Direction.Up;
                ul.Distance = -HouseStack.getKeepDistanceL(i);
                fillPosition(ref ul);
                addTrack(ul);

                TRACK dl = new TRACK();
                dl.IsLeft = false;
                dl.No = i;
                dl.Direction = TH_AutoSearchTrack.Direction.Down;
                dl.Distance = HouseStack.getLength(i) + HouseStack.getKeepDistanceL(i);
                fillPosition(ref dl);
                addTrack(dl);

                TRACK dr = new TRACK();
                dr.IsLeft = false;
                dr.No = i;
                dr.Direction = TH_AutoSearchTrack.Direction.Down;
                dr.Distance = -HouseStack.getKeepDistanceR(i);
                fillPosition(ref dr);
                addTrack(dr);
            }

            TRACK track3 = new TRACK();
            track3.IsLeft = false;
            track3.No = HouseStack.TotalStacksR;
            track3.Direction = TH_AutoSearchTrack.Direction.Down;
            track3.Distance = HouseStack.getLength(track3.No) + HouseStack.getKeepDistanceL(track3.No);
            fillPosition(ref track3);
            addTrack(track3);

            TRACK track4 = new TRACK();
            track4.IsLeft = true;
            track4.No = HouseStack.TotalStacksR + 1;
            track4.Direction = TH_AutoSearchTrack.Direction.Down;
            track4.Distance = -HouseStack.getKeepDistanceR(track4.No);
            fillPosition(ref track4);
            addTrack(track4);
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
        /// 删除最后的路径
        /// </summary>
        public static void delTrack()
        {
            lock (config.TrackLock)
            {
                if (Track == null || Track.Count == 0) { return; }
                Track.RemoveAt(Track.Count - 1);
            }
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
        /// 填充仓库坐标系下的坐标
        /// </summary>
        /// <param name="track">路径信息</param>
        /// <returns></returns>
        public static void fillPosition(ref TRACK track)
        {
            CoordinatePoint.POINT Base = HouseStack.getPosition(track.No);

            double keepU = HouseStack.getKeepDistanceU(track.No);
            double keepD = HouseStack.getKeepDistanceD(track.No);
            double keepL = HouseStack.getKeepDistanceL(track.No);
            double keepR = HouseStack.getKeepDistanceR(track.No);

            double L = HouseStack.getLength(track.No);
            double W = HouseStack.getWidth(track.No);

            if (track.Direction == TH_AutoSearchTrack.Direction.Up) { Base.x += track.Distance; Base.y -= keepU; }
            if (track.Direction == TH_AutoSearchTrack.Direction.Down) { Base.x += L - track.Distance; Base.y += W + keepD; }
            if (track.Direction == TH_AutoSearchTrack.Direction.Left) { Base.x -= keepL; Base.y += W - track.Distance; }
            if (track.Direction == TH_AutoSearchTrack.Direction.Right) { Base.x += L + keepR; Base.y += track.Distance; }

            track.TargetPosition = Base;
        }

        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        /// <returns></returns>
        public static bool Save()
        {
            return true;
        }
        /// <summary>
        /// 保存路径信息
        /// </summary>
        /// <param name="fullname">文件名</param>
        /// <returns></returns>
        public static bool Save(string fullname )
        {
            Configuration.Clear(); List<TRACK> track = Get();

            foreach (TRACK t in track)
            {
                Configuration.setFieldValue("TargetPosition", CoordinatePoint.Point2Double(t.TargetPosition));
                Configuration.setFieldValue("IsLeft", t.IsLeft);
                Configuration.setFieldValue("No", t.No);
                Configuration.setFieldValue("Direction", (int)t.Direction);
                Configuration.setFieldValue("Distance", t.Distance);

                SaveHandle(t.Extra);
            }

            return Configuration.Save(fullname);
        }
        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        /// <returns></returns>
        public static bool Load()
        {
            Initial(); return true;
        }
        /// <summary>
        /// 加载路径信息
        /// </summary>
        /// <param name="fullname">路径名称</param>
        /// <returns></returns>
        public static bool Load(string fullname)
        {
            Configuration.Clear();
            if (!Configuration.Load(fullname)) { return false; }

            List<TRACK> track = new List<TRACK>();
            while (!Configuration.IsEmpty())
            {
                TRACK t = new TRACK();
                
                t.TargetPosition = CoordinatePoint.Double2Point(Configuration.getFieldValue2_DOUBLE("TargetPosition"));
                t.IsLeft = Configuration.getFieldValue1_BOOL("IsLeft");
                t.No = Configuration.getFieldValue1_INT("No");
                t.Direction = (TH_AutoSearchTrack.Direction)Configuration.getFieldValue1_INT("Direction");
                t.Distance = Configuration.getFieldValue1_DOUBLE("Distance");
                t.Extra = LoadHandle();

                track.Add(t);
            }

            Set(track); return true;
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
        public static bool getIsLeft(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return false; }
            TRACK track = getTrack(No);

            return track.IsLeft;
        }
        public static int getStackNo(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return -1; }
            TRACK track = getTrack(No);

            return track.No;
        }
        public static TH_AutoSearchTrack.Direction getDirection(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return TH_AutoSearchTrack.Direction.Tuning; }
            TRACK track = getTrack(No);

            return track.Direction;
        }
        public static object getExtra(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return null; }
            TRACK track = getTrack(No);

            return track.Extra;
        }
    }
}
