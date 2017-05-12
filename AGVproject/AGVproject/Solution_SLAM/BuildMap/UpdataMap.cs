using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 更新地图信息并显示在界面中
    /// </summary>
    class UpdataMap
    {
        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////



        ////////////////////////////////////////////// private attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public Graphics g;
        }

        ////////////////////////////////////////////// public method ///////////////////////////////////////////

        /// <summary>
        /// 地图初始化（完全未知，纯黑色填充）
        /// </summary>
        public static void InitBuildMap()
        {
            // 不允许刷新线程操作该图片
            HouseMap.NoOperate = true;

            // 创建图片
            if (HouseMap.Map != null) { HouseMap.Map.Dispose(); }
            HouseMap.Map = new Bitmap(HouseMap.MapWidth, HouseMap.MapHeight);

            // 创建画笔，纯色填充
            config.g = Graphics.FromImage(HouseMap.Map);
            config.g.FillRectangle(Brushes.Black, new Rectangle(0, 0, HouseMap.MapWidth, HouseMap.MapHeight));

            // 释放控制权
            HouseMap.NoOperate = false;
        }
        /// <summary>
        /// 在地图上绘出激光雷达当前扫描到的数据
        /// </summary>
        /// <param name="points">数据点</param>
        public static void DrawCurrentSurrounding(List<CoordinatePoint.POINT> points)
        {

        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////
    }
}
