using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.Feature
{
    /// <summary>
    /// 从激光雷达数据中剥离出特征信息
    /// </summary>
    class ExtractFeatures
    {
        ///////////////////////////////////////////// public attribute ////////////////////////////////////////

        /// <summary>
        /// 本次扫描中的特征集合
        /// </summary>
        public static List<Feature> Features;

        /// <summary>
        /// 对应于特征所分割出的线段
        /// </summary>
        public static List<List<CoordinatePoint.POINT>> ptGroups;

        ///////////////////////////////////////////// private attribute ////////////////////////////////////////

        private static AbruptFilter Filter;

        ///////////////////////////////////////////// public method ////////////////////////////////////////

        /// <summary>
        /// 获取本次扫描的特征集合
        /// </summary>
        public static void Start()
        {
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            points = Filter.FilterD(points, 20, 50);
            ptGroups = new List<List<CoordinatePoint.POINT>>();
            cutPointsToGroup(points);

            Features = new List<Feature>();
            foreach (List<CoordinatePoint.POINT> group in ptGroups) { getFeature(group); }
        }

        /// <summary>
        /// 把当前获取的特征信息拷贝到目标地址
        /// </summary>
        /// <param name="dest">目标地址</param>
        public static void CopyFeatures(ref List<Feature> dest)
        {
            dest = new List<Feature>();
            if (Features == null) { return; }
            foreach (Feature f in Features) { dest.Add(f); }
        }
        /// <summary>
        /// 把一系列特征从源地址拷贝到目的地址
        /// </summary>
        /// <param name="sour">源地址</param>
        /// <param name="dest">目的地址</param>
        public static void CopyFeatures(List<Feature> sour, ref List<Feature> dest)
        {
            dest = new List<Feature>();
            if (sour == null) { return; }
            foreach (Feature f in sour) { dest.Add(f); }
        }

        ///////////////////////////////////////////// private method ////////////////////////////////////////

        private static void cutPointsToGroup(List<CoordinatePoint.POINT> points)
        {
            // 点的数量不够
            if (points.Count == 0) { return; }
            if (points.Count == 1 || points.Count == 2) { ptGroups.Add(points); return; }

            // 基本参数
            double MaxDis = 0.0;
            int indexofmax = 0;

            // 直线参数
            double x1 = points[0].x, y1 = points[0].y;
            double x2 = points[points.Count - 1].x, y2 = points[points.Count - 1].y;

            double A = y2 - y1, B = -(x2 - x1), C = (x2 - x1) * y1 - (y2 - y1) * x1;

            // 寻找最大距离
            for (int i = 0; i < points.Count; i++)
            {
                double iDis = Math.Abs(A * points[i].x + B * points[i].y + C) / Math.Sqrt(A * A + B * B);
                if (MaxDis > iDis) { continue; }

                indexofmax = i; MaxDis = iDis;
            }

            // 分割直线
            if (MaxDis <= 50) { ptGroups.Add(points); return; }

            List<CoordinatePoint.POINT> newLine = new List<CoordinatePoint.POINT>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);

            newLine = new List<CoordinatePoint.POINT>();
            for (int i = indexofmax; i < points.Count; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);
        }
        private static void getFeature(List<CoordinatePoint.POINT> points)
        {
            if (points.Count < 10) { return; }

            CoordinatePoint.POINT ptBG = points[0];
            CoordinatePoint.POINT ptED = points[points.Count - 1];

            double Length = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
            if (Length < 100) { return; }

            double[] xKAB = CoordinatePoint.Fit(points);
            double[] yKAB = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(CoordinatePoint.Copy(points)));

            Feature feature = new Feature();

            feature.Type = TYPE.Line;
            feature.N = points.Count;
            feature.Length = Length;
            feature.Direction = CoordinatePoint.AverageA(points);
            feature.Distance = CoordinatePoint.MinD(points);
            feature.AngleP = 0;
            feature.AngleN = 0;
            feature.xK = xKAB[0];
            feature.xA = xKAB[1];
            feature.xB = xKAB[2];
            feature.yK = yKAB[0];
            feature.yA = yKAB[1];
            feature.yB = yKAB[2];

            Features.Add(feature);
        }
    }
}
