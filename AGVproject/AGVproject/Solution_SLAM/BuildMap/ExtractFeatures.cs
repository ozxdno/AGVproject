﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    class ExtractFeatures
    {
        /// <summary>
        /// 本次扫描中的特征集合
        /// </summary>
        public static List<Feature.Feature> Features;
        /// <summary>
        /// 获取本次扫描的特征集合
        /// </summary>
        public static void Start()
        {
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);

            Features = new List<Feature.Feature>();

            foreach (List<CoordinatePoint.POINT> group in ptGroups)
            {
                getDotFeature(points); getLineFeature(points);
            }
        }

        public static List<List<CoordinatePoint.POINT>> ptGroups;
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
        private static void getDotFeature(List<CoordinatePoint.POINT> points)
        {
            CoordinatePoint.POINT ptBG = points[0];
            CoordinatePoint.POINT ptED = points[points.Count - 1];

            double Length = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
            if (Length > 50) { return; }
            
            Feature.Feature dot = new Feature.Feature();

            dot.Type = Feature.TYPE.Dot;
            dot.Length = Length;
            dot.DirectionBG = ptBG.a;
            dot.DirectionED = ptED.a;

            dot.D = CoordinatePoint.MinD(points);

            Features.Add(dot);
        }
        private static void getLineFeature(List<CoordinatePoint.POINT> points)
        {
            CoordinatePoint.POINT ptBG = points[0];
            CoordinatePoint.POINT ptED = points[points.Count - 1];

            double Length = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
            if (Length <= 50) { return; }

            Feature.Feature line = new Feature.Feature();

            line.Type = Feature.TYPE.Line;
            line.Length = Length;
            line.DirectionBG = ptBG.a;
            line.DirectionED = ptED.a;

            double[] KAB = CoordinatePoint.Fit(points);
            line.K = KAB[0];
            line.A = KAB[1];
            line.B = KAB[2];
            line.D = CoordinatePoint.MinD(points);

            Features.Add(line);
        }
    }
}