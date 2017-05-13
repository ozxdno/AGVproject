using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Solution_SLAM.Feature
{
    /// <summary>
    /// 特征信息
    /// </summary>
    struct Feature
    {
        /// <summary>
        /// 特征类型
        /// </summary>
        public TYPE Type;
        /// <summary>
        /// 该特征包含点的个数
        /// </summary>
        public int N;
        /// <summary>
        /// 长度 单位：mm
        /// </summary>
        public double Length;
        /// <summary>
        /// 该直线与激光雷达的相对位置 单位：度
        /// </summary>
        public double Direction;
        /// <summary>
        /// 该直线与激光雷达的最近距离 单位：mm
        /// </summary>
        public double Distance;
        /// <summary>
        /// 与上一条直线的夹角 单位：度
        /// </summary>
        public double AngleP;
        /// <summary>
        /// 与下一条直线的夹角 单位：度
        /// </summary>
        public double AngleN;

        /// <summary>
        /// 斜率（Y / X）
        /// </summary>
        public double xK;
        /// <summary>
        /// 偏角 单位：度
        /// </summary>
        public double xA;
        /// <summary>
        /// 截距 单位：mm
        /// </summary>
        public double xB;

        /// <summary>
        /// 斜率（Y / X）
        /// </summary>
        public double yK;
        /// <summary>
        /// 偏角 单位：度
        /// </summary>
        public double yA;
        /// <summary>
        /// 截距 单位：mm
        /// </summary>
        public double yB;
    }

    /// <summary>
    /// 特征类型集合
    /// </summary>
    enum TYPE
    {
        /// <summary>
        /// 空
        /// </summary>
        Empty,
        /// <summary>
        /// 线
        /// </summary>
        Line,
        /// <summary>
        /// 点
        /// </summary>
        Dot,
        /// <summary>
        /// 无效
        /// </summary>
        Invaild
    }
}
