using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 用环境特征来估计小车的移动量
    /// </summary>
    class MeasureMove
    {
        ///////////////////////////////////////////// public attribute ////////////////////////////////////////

        /// <summary>
        /// X 方向移动量估计值 单位：mm
        /// </summary>
        public double xMove;
        /// <summary>
        /// Y 方向移动量估计值 单位：mm
        /// </summary>
        public double yMove;
        /// <summary>
        /// A 方向移动量估计值 单位：度
        /// </summary>
        public double aMove;

        ///////////////////////////////////////////// private attribute ////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public List<CoordinatePoint.POINT> LastS;
            public List<CoordinatePoint.POINT> NextS;
        }

        ///////////////////////////////////////////// public method ////////////////////////////////////////

        /// <summary>
        /// 利用前后两次的激光雷达数据来估计小车的位移量，返回估计是否成功
        /// </summary>
        /// <returns></returns>
        public static bool Start()
        {
            if (config.LastS == null || config.NextS == null) { return false; }
            if (config.LastS.Count < 10 || config.NextS.Count < 10) { return false; }
            if (config.LastS.Count != config.NextS.Count) { return false; }



            return true;
        }

        ///////////////////////////////////////////// private method ////////////////////////////////////////
    }
}
