using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 对多种传感器所测得的小车移动信息进行融合
    /// </summary>
    class FusionMove
    {
        /////////////////////////////////////////////// public attribute /////////////////////////////////////////
    
        /// <summary>
        /// 编码器所测得的移动量
        /// </summary>
        public static MOVE DisCoder;
        /// <summary>
        /// 激光雷达所测得的移动量
        /// </summary>
        public static MOVE Urg;

        /// <summary>
        /// 移动信息整合
        /// </summary>
        public struct MOVE
        {
            /// <summary>
            /// 小车 X 方向移动量，向右移动为正 单位：mm
            /// </summary>
            public double x;
            /// <summary>
            /// 小车 Y 方向移动量，向前移动为正 单位：mm
            /// </summary>
            public double y;
            /// <summary>
            /// 小车 A 方向移动量，逆时针旋转为正 单位：度
            /// </summary>
            public double a;

            /// <summary>
            /// X 方向的估计量是否无效
            /// </summary>
            public bool xInvalid;
            /// <summary>
            /// Y 方向的估计量是否无效
            /// </summary>
            public bool yInvalid;
            /// <summary>
            /// A 方向的估计量是否无效
            /// </summary>
            public bool aInvalid;
        }

        /////////////////////////////////////////////// private attribute /////////////////////////////////////////

        /////////////////////////////////////////////// public method /////////////////////////////////////////

        /////////////////////////////////////////////// private method /////////////////////////////////////////
    }
}
