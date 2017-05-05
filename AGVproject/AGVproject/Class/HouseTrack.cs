using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 对路径信息进行操作
    /// </summary>
    class HouseTrack
    {
        //////////////////////////////////////////////////// public attribute ///////////////////////////////////////
        
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
            /// 目标点和堆垛的相对位置
            /// </summary>
            public STACK_POS StackPos;
            
            /// <summary>
            /// X 方向行进模式
            /// </summary>
            public MODE_X xMode;
            /// <summary>
            /// Y 方向行进模式
            /// </summary>
            public MODE_Y yMode;
            /// <summary>
            /// A 方向行进模式
            /// </summary>
            public MODE_A aMode;

            /// <summary>
            /// 堆垛位置信息
            /// </summary>
            public struct STACK_POS
            {
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
            }

            /// <summary>
            /// X 方向行进模式集合
            /// </summary>
            public enum MODE_X { KeepL, KeepR, BySpeed, ByPosition }
            /// <summary>
            /// Y 方向行进模式集合
            /// </summary>
            public enum MODE_Y { KeepU, KeepD, BySpeed, ByPosition }
            /// <summary>
            /// A 方向行进模式集合
            /// </summary>
            public enum MODE_A { BySurrounding, BySpeed, ByPosition }
        }

        //////////////////////////////////////////////////// private attribute ///////////////////////////////////////
        
        /// <summary>
        /// 路径信息，读取权大于写入权
        /// </summary>
        private static List<TRACK> Track = new List<TRACK>();
        private static CONFIG config;
        private struct CONFIG
        {
            public bool IsSetting;
            public bool IsGetting;
        }

        /////////////////////////////////////////////////// public method //////////////////////////////////////////

        /// <summary>
        /// 清除当前路径信息
        /// </summary>
        public static void Clear()
        {
            while (config.IsGetting) ;
            config.IsSetting = true;
            Track.Clear();
            config.IsSetting = false;
        }
        /// <summary>
        /// 重新设置整个路径
        /// </summary>
        /// <param name="track">路径信息</param>
        public static void Set(List<TRACK> track)
        {
            while (config.IsGetting) ;
            config.IsSetting = true;
            Track = track;
            config.IsSetting = false;
        }
        /// <summary>
        /// 获取整个路径的信息
        /// </summary>
        /// <returns></returns>
        public static List<TRACK> Get()
        {
            config.IsGetting = true;
            while (config.IsSetting) ;
            
            List<TRACK> track = new List<TRACK>();
            if (Track == null) { return track; }

            foreach (TRACK i in Track) { track.Add(i); }
            config.IsGetting = false;

            return track;
        }
        /// <summary>
        /// 重新按堆垛信息获取整个路径的坐标
        /// </summary>
        public static void Fit()
        {
            while (config.IsGetting) ;
            config.IsSetting = true;

            for (int i = 0; i < Track.Count; i++) { Fit(i); }

            config.IsSetting = false;
        }
        /// <summary>
        /// 重新按堆垛信息获取路径的坐标
        /// </summary>
        /// <param name="No">路径编号</param>
        public static void Fit(int No)
        {
            while (config.IsGetting) ;
            config.IsSetting = true;
            
            // 

            config.IsSetting = false;
        }

        /// <summary>
        /// 添加路径
        /// </summary>
        /// <param name="track">路径信息</param>
        public static void AddTrack(TRACK track)
        {
            while (config.IsGetting) ;
            config.IsSetting = true;
            Track.Add(track);
            config.IsSetting = false;
        }
        /// <summary>
        /// 删除路径
        /// </summary>
        /// <param name="No">路径编号</param>
        public static void DelTrack(int No)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            while (config.IsGetting) ;
            config.IsSetting = true;
            Track.RemoveAt(No);
            config.IsSetting = false;
        }
        /// <summary>
        /// 设置路径
        /// </summary>
        /// <param name="No">路径编号</param>
        /// <param name="track">路径信息</param>
        public static void SetTrack(int No, TRACK track)
        {
            if (No < 0 || No > Track.Count - 1) { return; }
            while (config.IsGetting) ;
            config.IsSetting = true;
            Track[No] = track;
            config.IsSetting = false;
        }
        
        /// <summary>
        /// 保存当前路径为文本格式
        /// </summary>
        /// <returns></returns>
        public static bool Save_TXT()
        {
            return false;
        }
        /// <summary>
        /// 保存当前路径为图片格式
        /// </summary>
        /// <returns></returns>
        public static bool Save_PIC()
        {
            return false;
        }
        /// <summary>
        /// 加载指定的文本格式路径
        /// </summary>
        /// <returns></returns>
        public static bool Load_TXT()
        {
            return false;
        }
        /// <summary>
        /// 加载指定的图片格式路径
        /// </summary>
        /// <returns></returns>
        public static bool Load_PIC()
        {
            return false;
        }
    }
}
