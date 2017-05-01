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
            /// <summary>
            /// 此次校准信息是否有效
            /// </summary>
            public bool Invalid;

            /// <summary>
            /// X 方向直线的斜率
            /// </summary>
            public double xK;
            public double xA;
            public double xB;
            public double xD;
            public double xL;
            public double xR;

            public double yK;
            public double yA;
            public double yB;
            public double yD;
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
        
        /// <summary>
        /// 开始校准
        /// </summary>
        /// <param name="correct">校准信息，若无效则不校准</param>
        public static void Start(ref CORRECT correct)
        {
            SearchMatchLine(ref correct); if (correct.Invalid) { return; }

            double xAverage = CoordinatePoint.AverageA(config.Lines[config.xLine]);
            double yAverage = CoordinatePoint.AverageA(config.Lines[config.yLine]);
            double[] xKAB, yKAB;

            while (true)
            {
                xKAB = getKAB_X(ref xAverage); yKAB = getKAB_Y(ref yAverage);

                double xAdjust = 0.8 * (yKAB[2] - correct.yD);
                double yAdjust = 0.6 * (xKAB[2] - correct.xD);
                double aAdjust = 40 * (xKAB[1] - correct.xA);

                int xSpeed = AST_GuideBySpeed.getSpeedX(xAdjust);
                int ySpeed = AST_GuideBySpeed.getSpeedX(yAdjust);
                int aSpeed = AST_GuideBySpeed.getSpeedX(aAdjust);
                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        /// <summary>
        /// 获取当前校准信息
        /// </summary>
        /// <returns></returns>
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
            correct.xD = xKAB[2];
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
            double[] yKABr = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(config.Lines[config.yLine]));
            correct.yK = yKAB[0];
            correct.yA = yKAB[1];
            correct.yB = yKAB[2];
            correct.yD = yKABr[2];
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
        private static void getMatchLine(ref CORRECT correct)
        {
            // 初始化
            config.xLine = -1; config.yLine = -1;

            // 切割成直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);

            // 删除短线
            for (int i = config.Lines.Count - 1; i >= 0; i--)
            {
                if (config.Lines[i].Count < config.PtReqNum) { config.Lines.RemoveAt(i); }
            }

            // 直线不够
            if (config.Lines.Count == 0) { correct.Invalid = false; return; }
            if (config.Lines.Count == 1) { config.xLine = 0; config.yLine = 0; return; }
            
            // 按点数量从多到少进行排序
            List<int> indexofLine = new List<int>();
            for (int i = 0; i < config.Lines.Count; i++) { indexofLine.Add(i); }

            bool xFound = false, yFound = false;
            while (indexofLine.Count != 0 || (xFound && yFound))
            {
                int indexofMost = -1, Most = -1, Remove = -1;
                for (int i = 0; i < indexofLine.Count; i++)
                {
                    int N = config.Lines[indexofLine[i]].Count; if (N > Most) { Remove = i; Most = N; }
                }
                indexofMost = indexofLine[Remove];
                indexofLine.RemoveAt(Remove);

                double[] KAB = CoordinatePoint.Fit(config.Lines[indexofMost]);
                double A = KAB[1], L = 0, R = 0;
                if (indexofMost > 0) { KAB = CoordinatePoint.Fit(config.Lines[indexofMost - 1]); L = KAB[1]; }
                if (indexofMost < config.Lines.Count - 1) { KAB = CoordinatePoint.Fit(config.Lines[indexofMost + 1]); R = KAB[1]; }

                if (!xFound)
                {
                    if (correct.xL == 0 && correct.xR == 0) { xFound = true; config.xLine = indexofMost; }

                    double ErrorL = Math.Abs(Math.Abs(L - A) - Math.Abs(correct.xA - correct.xL));
                    double ErrorR = Math.Abs(Math.Abs(R - A) - Math.Abs(correct.xA - correct.xR));

                    if (!xFound && L != 0 && correct.xL != 0 && ErrorL < 3) { xFound = true; config.xLine = indexofMost; }
                    if (!xFound && R != 0 && correct.xR != 0 && ErrorR < 3) { xFound = true; config.xLine = indexofMost; }
                }
                if (!yFound)
                {
                    if (correct.yL == 0 && correct.yR == 0) { yFound = true; config.yLine = indexofMost; }

                    double ErrorL = Math.Abs(Math.Abs(L - A) - Math.Abs(correct.yA - correct.yL));
                    double ErrorR = Math.Abs(Math.Abs(R - A) - Math.Abs(correct.yA - correct.yR));

                    if (!yFound && L != 0 && correct.yL != 0 && ErrorL < 3) { yFound = true; config.yLine = indexofMost; }
                    if (!yFound && R != 0 && correct.yR != 0 && ErrorR < 3) { yFound = true; config.yLine = indexofMost; }
                }
            }
        }
        private static void SearchMatchLine(ref CORRECT correct)
        {
            getMatchLine(ref correct);
            if (correct.Invalid) { return; }
            if (config.xLine == -1 && config.yLine == -1) { correct.Invalid = true; return; }

            if (config.xLine != -1 && config.yLine != -1) { return; }

            int indexofMatch = config.xLine != -1 ? config.xLine : config.yLine;
            double[] KAB = CoordinatePoint.Fit(config.Lines[indexofMatch]);
            double aLast = config.xLine != -1 ? correct.xA : correct.yA;
            double aNext = KAB[1];

            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.ApproachA = false;
            while (!AST_GuideByPosition.ApproachA)
            {
                int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                int aSpeed = AST_GuideByPosition.getSpeedA(aLast - aNext);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            getMatchLine(ref correct);
        }
        private static double[] getKAB_X(ref double centreAngle)
        {
            // 切割成直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);

            // 获取直线
            int indexofMatch = getMatchLine(centreAngle);
            if (indexofMatch == -1) { return new double[3] { 0, 0, 0 }; }

            // 更新中心点
            centreAngle = CoordinatePoint.AverageA(config.Lines[indexofMatch]);

            // 拟合直线
            return CoordinatePoint.Fit(config.Lines[indexofMatch]);
        }
        private static double[] getKAB_Y(ref double centreAngle)
        {
            // 切割成直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);

            // 获取直线
            int indexofMatch = getMatchLine(centreAngle);
            if (indexofMatch == -1) { return new double[3] { 0, 0, 0 }; }

            // 更新中心点
            centreAngle = CoordinatePoint.AverageA(config.Lines[indexofMatch]);

            // 拟合直线
            return CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(config.Lines[indexofMatch]));
        }
        private static int getMatchLine(double centreAngle)
        {
            for (int i = 0; i < config.Lines.Count; i++)
            {
                double angleBG = config.Lines[i][0].a;
                double angleED = config.Lines[i][config.Lines[i].Count - 1].a;

                if (angleBG <= centreAngle && centreAngle <= angleED) { return i; }
            }
            return -1;
        }
    }
}
