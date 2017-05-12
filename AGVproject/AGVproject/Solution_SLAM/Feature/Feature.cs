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
        /// 长度 单位：mm
        /// </summary>
        public double Length;
        /// <summary>
        /// 激光雷达扫到该直线起始点的角度 单位：度
        /// </summary>
        public double DirectionBG;
        /// <summary>
        /// 激光雷达扫到该直线终止点的角度 单位：度
        /// </summary>
        public double DirectionED;
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
        public double K;
        /// <summary>
        /// 偏角 单位：度
        /// </summary>
        public double A;
        /// <summary>
        /// 截距 单位：mm
        /// </summary>
        public double B;
        /// <summary>
        /// 与原点最近距离 单位：mm
        /// </summary>
        public double D;
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
        Dot
    }
}
