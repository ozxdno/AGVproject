using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_CorrectPosition
    {

        ////////////////////////////////////////////////// public attribute /////////////////////////////////////////

        /// <summary>
        /// 进行校准时必要的信息
        /// </summary>
        public struct CORRECT
        {
            public bool Invalid;

            public double xK;
            public double xA;
            public double xB;
            public double xL;
            public double xR;

            public double yK;
            public double yA;
            public double yB;
            public double yL;
            public double yR;
        }

        ////////////////////////////////////////////////// private attribute /////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public List<List<CoordinatePoint.POINT>> Lines;
            public double LineError;
            public double NegeError;
            public int PtReqNum;

            public int xLine;
            public int yLine;
        }

        ////////////////////////////////////////////////// public method ///////////////////////////////////////////
        
        public static void Start()
        {
            
        }
        public static CORRECT getCorrect()
        {
            CORRECT correct = new CORRECT();
            config.Lines = new List<List<CoordinatePoint.POINT>>();
            config.LineError = 50; 
            config.PtReqNum = 20;
            config.NegeError = 100; // 线段至少10cm长

            // 切割成直线，获取对应直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);
            SortLines();

            // 
            if (config.xLine == -1 || config.yLine == -1)
            {
                correct.Invalid = true; return correct;
            }
            
            // 获取下次校准的参数

            double[] xKAB = CoordinatePoint.Fit(config.Lines[config.xLine]);
            correct.xK = xKAB[0];
            correct.xA = xKAB[1];
            correct.xB = xKAB[2];
            correct.xL = 0;
            correct.xR = 0;

            if (0 < config.xLine)
            {
                double[] xL = CoordinatePoint.Fit(config.Lines[config.xLine - 1]);
                correct.xL = xL[1];
            }
            if (config.xLine < config.Lines.Count - 1)
            {
                double[] xR = CoordinatePoint.Fit(config.Lines[config.xLine + 1]);
                correct.xR = xR[1];
            }

            double[] yKAB = CoordinatePoint.Fit(config.Lines[config.yLine]);
            correct.yK = yKAB[0];
            correct.yA = yKAB[1];
            correct.yB = yKAB[2];
            correct.yL = 0;
            correct.yR = 0;

            if (0 < config.yLine)
            {
                double[] yL = CoordinatePoint.Fit(config.Lines[config.yLine - 1]);
                correct.yL = yL[1];
            }
            if (config.yLine < config.Lines.Count - 1)
            {
                double[] yR = CoordinatePoint.Fit(config.Lines[config.yLine + 1]);
                correct.yR = yR[1];
            }

            return correct;
        }

        ////////////////////////////////////////////////// private method /////////////////////////////////////////

        private static void cutPointsToGroup(List<CoordinatePoint.POINT> points)
        {
            // 点的数量不够
            if (points.Count == 0) { return; }
            if (points.Count == 1 || points.Count == 2) { config.Lines.Add(points); return; }

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
            if (MaxDis <= config.LineError) { config.Lines.Add(points); return; }

            List<CoordinatePoint.POINT> newLine = new List<CoordinatePoint.POINT>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);

            newLine = new List<CoordinatePoint.POINT>();
            for (int i = indexofmax; i < points.Count; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);
        }
        private static void SortLines()
        {
            //
            for (int i = config.Lines.Count - 1; i >= 0; i--)
            {
                if (config.Lines[i].Count < config.PtReqNum) { config.Lines.RemoveAt(i); }
            }


                // 若不存在任何直线
                if (config.Lines.Count == 0) { config.xLine = -1; config.yLine = -1; return; }

            // 分成两个方向
            double xFactor = 0, yFactor = 0;
            int xIndex = -1, yIndex = -1;

            for (int i = 0; i < config.Lines.Count; i++)
            {
                // 点数量要求
                int N = config.Lines[i].Count;
                if (N < config.PtReqNum) { continue; }

                // 跨度要求
                double xDis = Math.Abs(config.Lines[i][1].x - config.Lines[i][N - 1].x);
                double yDis = Math.Abs(config.Lines[i][1].y - config.Lines[i][N - 1].y);
                double dis = Math.Sqrt(xDis * xDis + yDis * yDis);
                if (dis <= config.NegeError) { continue; }

                // 计算因素
                double xF = xDis * 2  + N;
                double yF = yDis * 2  + N;

                // 比较大小
                if (xF > xFactor) { xFactor = xF; xIndex = i; }
                if (yF > yFactor) { yFactor = yF; yIndex = i; }
            }

            // 返回直线
            config.xLine = xIndex; config.yLine = yIndex;
        }
    }
}
