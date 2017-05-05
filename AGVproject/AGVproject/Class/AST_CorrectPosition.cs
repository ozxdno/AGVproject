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
        /// 进行校准时必要的信息，X 方向：平行于 X 轴的方向，Y 同理
        /// </summary>
        public struct CORRECT
        {
            /// <summary>
            /// X 方向校准信息是否无效
            /// </summary>
            public bool xInvalid;
            /// <summary>
            /// Y 方向校准信息是否无效
            /// </summary>
            public bool yInvalid;

            /// <summary>
            /// X 方向直线的斜率
            /// </summary>
            public double xK;
            /// <summary>
            /// X 方向直线的角度
            /// </summary>
            public double xA;
            /// <summary>
            /// X 方向直线的截距
            /// </summary>
            public double xB;
            /// <summary>
            /// X 方向直线的中点对应于激光雷达扫描数据角度
            /// </summary>
            public double xC;
            /// <summary>
            /// X 方向直线的长度
            /// </summary>
            public double xD;
            /// <summary>
            /// X 方向左边直线通过了交换 X-Y 的变换
            /// </summary>
            public bool xL_exchanged;
            /// <summary>
            /// 该直线的左边一条直线的角度
            /// </summary>
            public double xL;
            /// <summary>
            /// X 方向右边直线通过了交换 X-Y 的变换
            /// </summary>
            public bool xR_exchanged;
            /// <summary>
            /// 该直线的右边一条直线的角度
            /// </summary>
            public double xR;

            public double yK;
            public double yA;
            public double yB;
            public double yC;
            public double yD;
            public bool yL_exchanged;
            public double yL;
            public bool yR_exchanged;
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

            public bool ApproachX;
            public bool ApproachY;
            public bool ApproachA;
        }
        private struct ERROR
        {
            public int index;

            public double xPos;
            public double xFit;
            public double xL;
            public double xR;
            public double xD;

            public double yPos;
            public double yFit;
            public double yL;
            public double yR;
            public double yD;
        }

        ////////////////////////////////////////////////// public method ///////////////////////////////////////////

        /// <summary>
        /// 开始校准
        /// </summary>
        /// <param name="correctTarget">校准信息</param>
        public static void Start(CORRECT correctTarget)
        {
            // 获取匹配直线
            if (correctTarget.xInvalid && correctTarget.yInvalid) { return; }
            getMatch(correctTarget);

            correctTarget.xInvalid |= config.xLine == -1;
            correctTarget.yInvalid |= config.yLine == -1;
            if (correctTarget.xInvalid) { config.xLine = -1; }
            if (correctTarget.yInvalid) { config.yLine = -1; }
            
            if (config.xLine == -1 && config.yLine == -1) { TH_AutoSearchTrack.control.Event = "Match Failed in X and Y"; }
            if (config.xLine == -1 && config.yLine != -1) { TH_AutoSearchTrack.control.Event = "Match Failed in X"; }
            if (config.xLine != -1 && config.yLine == -1) { TH_AutoSearchTrack.control.Event = "Match Failed in Y"; }
            if (config.xLine != -1 && config.yLine != -1) { TH_AutoSearchTrack.control.Event = "Match Successed"; }

            // 获取中心角度
            double xAverage = 0, yAverage = 0;
            if (config.xLine != -1) { xAverage = CoordinatePoint.AverageA(config.Lines[config.xLine]); }
            if (config.yLine != -1) { yAverage = CoordinatePoint.AverageA(config.Lines[config.yLine]); }
            CoordinatePoint.POINT refPos = TH_MeasurePosition.getPosition();

            // 控制初始化
            config.ApproachX = false;
            config.ApproachY = false;
            config.ApproachA = false;
            
            // 调整角度
            while (!config.ApproachA)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines = new List<List<CoordinatePoint.POINT>>();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch2(correctTarget);

                // 退出条件
                if (!Form_Start.corrpos) { TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0); return; }

                // 获取控制
                //int xSpeed = getSpeedX(correctTarget);
                //int ySpeed = getSpeedY(correctTarget);
                int aSpeed = getSpeedA(correctTarget);

                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }

            // 调整 Y
            while (!config.ApproachY)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines = new List<List<CoordinatePoint.POINT>>();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch2(correctTarget);

                // 退出条件
                if (!Form_Start.corrpos) { TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0); return; }

                // 获取控制
                //int xSpeed = getSpeedX(correctTarget);
                int ySpeed = getSpeedY(correctTarget);
                //int aSpeed = getSpeedA(correctTarget);

                TH_SendCommand.AGV_MoveControl_0x70(0, ySpeed, 0);
            }

            // 调整 X
            while (!config.ApproachX)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines = new List<List<CoordinatePoint.POINT>>();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch2(correctTarget);

                // 退出条件
                if (!Form_Start.corrpos) { TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0); return; }

                // 获取控制
                int xSpeed = getSpeedX(correctTarget);
                //int ySpeed = getSpeedY(correctTarget);
                //int aSpeed = getSpeedA(correctTarget);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, 0, 0);
            }

            // 调整完毕
            TH_SendCommand.AGV_MoveControl_0x70(0, 0, 0);
        }
        /// <summary>
        /// 获取当前校准信息
        /// </summary>
        /// <returns></returns>
        public static CORRECT getCorrect()
        {
            CORRECT correct = new CORRECT();
            config.Lines = new List<List<CoordinatePoint.POINT>>();
            config.LineError = 50; // 线段的点的浮动误差 
            config.PtReqNum = 30;
            config.NegeError = 200; // 线段至少10cm长

            // 切割成直线，获取对应直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            cutPointsToGroup(points);
            getLandMark();

            // 判断数据是否有效
            correct.xInvalid = config.xLine == -1;
            correct.yInvalid = config.yLine == -1;

            if (correct.xInvalid && correct.yInvalid) { TH_AutoSearchTrack.control.Event = "Invalid X and Y Lines"; }
            if (correct.xInvalid && !correct.yInvalid) { TH_AutoSearchTrack.control.Event = "Invalid X Line"; }
            if (!correct.xInvalid && correct.yInvalid) { TH_AutoSearchTrack.control.Event = "Invalid Y Line"; }
            if (!correct.xInvalid && !correct.yInvalid) { TH_AutoSearchTrack.control.Event = "LandMark Done"; }

            // 获取 X 方向下次校准的参数
            if (!correct.xInvalid)
            {
                CoordinatePoint.POINT ptBG = config.Lines[config.xLine][0];
                CoordinatePoint.POINT ptED = config.Lines[config.xLine][config.Lines[config.xLine].Count - 1];

                double[] xKAB = CoordinatePoint.Fit(config.Lines[config.xLine]);
                correct.xK = xKAB[0];
                correct.xA = xKAB[1];
                correct.xB = xKAB[2];
                correct.xC = CoordinatePoint.AverageA(config.Lines[config.xLine]);
                correct.xD = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
                correct.xL = 0;
                correct.xR = 0;

                if (0 < config.xLine) { double[] xL = CoordinatePoint.Fit(config.Lines[config.xLine - 1]); correct.xL = xL[1]; }
                if (config.xLine < config.Lines.Count - 1) { double[] xR = CoordinatePoint.Fit(config.Lines[config.xLine + 1]); correct.xR = xR[1]; }
            }

            // 获取 Y 方向下次校准的参数
            if (!correct.yInvalid)
            {
                double[] yKAB = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(config.Lines[config.yLine]));
                correct.yK = yKAB[0];
                correct.yA = yKAB[1];
                correct.yB = yKAB[2];
                correct.yC = CoordinatePoint.AverageA(config.Lines[config.yLine]);
                correct.yD = yKAB[2];
                correct.yL = 0;
                correct.yR = 0;

                if (0 < config.yLine) { double[] yL = CoordinatePoint.Fit(config.Lines[config.yLine - 1]); correct.yL = yL[1]; }
                if (config.yLine < config.Lines.Count - 1) { double[] yR = CoordinatePoint.Fit(config.Lines[config.yLine + 1]); correct.yR = yR[1]; }
            }
            
            // 返回结果
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
        private static void sortLines()
        {
            // 删除短线
            for (int i = config.Lines.Count - 1; i >= 0; i--)
            {
                // 点数量要求
                int N = config.Lines[i].Count;
                if (N < config.PtReqNum) { config.Lines.RemoveAt(i); continue; }

                // 跨度要求
                double xDis = Math.Abs(config.Lines[i][1].x - config.Lines[i][N - 1].x);
                double yDis = Math.Abs(config.Lines[i][1].y - config.Lines[i][N - 1].y);
                double dis = Math.Sqrt(xDis * xDis + yDis * yDis);
                if (dis <= config.NegeError) { config.Lines.RemoveAt(i); continue; }
            }
        }

        private static void getLandMark()
        {
            // 对获取直线进行预处理
            sortLines();
            
            // 若不存在任何直线
            if (config.Lines.Count == 0) { config.xLine = -1; config.yLine = -1; return; }

            // 对现存直线进行分类
            List<int> xLines = new List<int>();
            List<int> yLines = new List<int>();
            for (int i = 0; i < config.Lines.Count; i++)
            {
                CoordinatePoint.POINT ptBG = config.Lines[i][0];
                CoordinatePoint.POINT ptED = config.Lines[i][config.Lines[i].Count - 1];

                double K = double.MaxValue;
                if (ptBG.x != ptED.x) { K = Math.Abs((ptBG.y - ptED.y) / (ptBG.x - ptED.x)); }

                if (K > 1) { yLines.Add(i); } else { xLines.Add(i); }
            }

            // 挑选直线
            double Factor = double.MinValue;
            int select = -1;
            foreach (int i in xLines)
            {
                int N = config.Lines[i].Count;
                double xDis = Math.Abs(config.Lines[i][1].x - config.Lines[i][N - 1].x);
                double yDis = Math.Abs(config.Lines[i][1].y - config.Lines[i][N - 1].y);

                // 计算因素
                double iF = N;// -yDis / 1000 + N / 100.0;

                // 取得结果
                if (iF > Factor) { Factor = iF; select = i; }
            }
            config.xLine = select;

            Factor = double.MinValue; select = -1;

            foreach (int i in yLines)
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
                double iF = N;//-xDis / 1000 + N / 100.0;

                // 取得结果
                if (iF > Factor) { Factor = iF; select = i; }
            }
            config.yLine = select;
        }

        private static void getMatch(CORRECT correct)
        {
            // 初始化
            config.xLine = -1; config.yLine = -1;

            // 切割成直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            config.Lines = new List<List<CoordinatePoint.POINT>>();
            cutPointsToGroup(points);
            sortLines();
            
            // 存下索引号备用
            List<int> indexofLine = new List<int>();
            for (int i = 0; i < config.Lines.Count; i++) { indexofLine.Add(i); }
            
            // 寻找匹配直线
            bool xFound = false, yFound = false;
            while (indexofLine.Count != 0 && (!xFound || !yFound))
            {
                // 找到点数量最多的线
                int indexofMost = -1, Most = -1, Remove = -1;
                for (int i = 0; i < indexofLine.Count; i++)
                {
                    int N = config.Lines[indexofLine[i]].Count; if (N > Most) { Remove = i; Most = N; }
                }
                indexofMost = indexofLine[Remove];
                indexofLine.RemoveAt(Remove);

                // 获取这条线的 KAB
                double[] KAB = CoordinatePoint.Fit(config.Lines[indexofMost]);
                double A = KAB[1], L = 0, R = 0;
                if (indexofMost > 0) { KAB = CoordinatePoint.Fit(config.Lines[indexofMost - 1]); L = KAB[1]; }
                if (indexofMost < config.Lines.Count - 1) { KAB = CoordinatePoint.Fit(config.Lines[indexofMost + 1]); R = KAB[1]; }

                if (A < 0) { A += 180; }
                if (L < 0) { L += 180; }
                if (R < 0) { R += 180; }

                // 寻找 X 方向的匹配直线
                if (!xFound && !correct.xInvalid)
                {
                    if (correct.xL == 0 && correct.xR == 0) { xFound = true; config.xLine = indexofMost; }

                    double iAngleL = L-A;//Math.Abs(L - A); if (iAngleL > 180) { iAngleL -= 180; }
                    double sAngleL = correct.xL - correct.xA;//Math.Abs(correct.xA - correct.xL); if (sAngleL > 180) { sAngleL -= 180; }
                    double iAngleR = R - A;// Math.Abs(R - A); if (iAngleR > 180) { iAngleR -= 180; }
                    double sAngleR = correct.xR - correct.xA;//Math.Abs(correct.xA - correct.xR); if (sAngleR > 180) { sAngleR -= 180; }
                    
                    double ErrorL = Math.Abs(iAngleL - sAngleL);
                    double ErrorR = Math.Abs(iAngleR - sAngleR);
                    double ErrorC = Math.Abs(CoordinatePoint.AverageA(config.Lines[indexofMost]) - correct.xC);

                    bool SuitL = L == 0 || correct.xL == 0 || (L != 0 && correct.xL != 0 && ErrorL < 10);
                    bool SuitR = R == 0 || correct.xR == 0 || (R != 0 && correct.xR != 0 && ErrorR < 10);
                    bool SuitC = ErrorC < 10;
                    if (!xFound && (SuitL || SuitR) && SuitC) { xFound = true; config.xLine = indexofMost; continue; }
                }

                // 寻找 Y 方向的匹配直线
                if (!yFound && !correct.yInvalid)
                {
                    if (correct.yL == 0 && correct.yR == 0) { yFound = true; config.yLine = indexofMost; }

                    double iAngleL = L-A;//Math.Abs(L - A); if (iAngleL > 180) { iAngleL -= 180; }
                    double sAngleL = correct.yL - correct.yA;//Math.Abs(correct.yA - correct.yL); if (sAngleL > 180) { sAngleL -= 180; }
                    double iAngleR = R-A;//Math.Abs(R - A); if (iAngleR > 180) { iAngleR -= 180; }
                    double sAngleR = correct.yR - correct.yA;//Math.Abs(correct.yA - correct.yR); if (sAngleR > 180) { sAngleR -= 180; }

                    double ErrorL = Math.Abs(iAngleL - sAngleL);
                    double ErrorR = Math.Abs(iAngleR - sAngleR);
                    double ErrorC = Math.Abs(CoordinatePoint.AverageA(config.Lines[indexofMost]) - correct.yC);

                    bool SuitL = L == 0 || correct.yL == 0 || (L != 0 && correct.yL != 0 && ErrorL < 10);
                    bool SuitR = R == 0 || correct.yR == 0 || (R != 0 && correct.yR != 0 && ErrorR < 10);
                    bool SuitC = ErrorC < 10;

                    if (!yFound && (SuitL || SuitR) && SuitC) { yFound = true; config.yLine = indexofMost; }
                }
            }
        }
        private static void getMatch2(CORRECT correct)
        {
            // 初始化
            config.xLine = -1; config.yLine = -1;

            // 切割成直线
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
            config.Lines = new List<List<CoordinatePoint.POINT>>();
            cutPointsToGroup(points);
            sortLines();

            // 获取各条直线的 KAB 值。xKAB, yKAB
            List<double[]> xKAB = new List<double[]>();
            List<double[]> yKAB = new List<double[]>();
            for (int i = 0; i < config.Lines.Count; i++)
            {
                double[] x = CoordinatePoint.Fit(config.Lines[i]);
                double[] y = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(config.Lines[i]));

                xKAB.Add(x); yKAB.Add(y);
            }

            // 获取各种误差参数
            List<ERROR> Error = new List<ERROR>();
            CoordinatePoint.POINT ptBG, ptED;
            for (int i = 0; i < config.Lines.Count; i++)
            {
                ERROR err = new ERROR(); err.index = i;
                int N = config.Lines[i].Count;

                ptBG = config.Lines[i][0];
                ptED = config.Lines[i][N - 1];

                err.xPos = Math.Min(Math.Abs(ptBG.a - correct.xC), Math.Abs(ptED.a - correct.xC));
                if (ptBG.a < correct.xC && correct.xC < ptED.a) { err.xPos = 0; }
                err.xFit = Math.Abs(xKAB[i][1] - correct.xA);

                if (i > 0 && correct.xL_exchanged)
                { err.xL = Math.Abs((xKAB[i][1] - yKAB[i - 1][1]) - (correct.xA - correct.xL)); }
                if (i > 0 && !correct.xL_exchanged)
                { err.xL = Math.Abs((xKAB[i][1] - xKAB[i - 1][1]) - (correct.xA - correct.xL)); }
                if (i < N - 1 && correct.xR_exchanged)
                { err.xR = Math.Abs((xKAB[i][1] - yKAB[i + 1][1]) - (correct.xA - correct.xR)); }
                if (i < N - 1 && !correct.xR_exchanged)
                { err.xR = Math.Abs((xKAB[i][1] - xKAB[i + 1][1]) - (correct.xA - correct.xR)); }

                err.xD = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
                err.xD = Math.Abs(err.xD - correct.xD);

                err.yPos = Math.Min(Math.Abs(ptBG.a - correct.yC), Math.Abs(ptED.a - correct.yC));
                if (ptBG.a < correct.yC && correct.yC < ptED.a) { err.yPos = 0; }
                err.yFit = Math.Abs(yKAB[i][1] - correct.yA);

                if (i > 0 && correct.yL_exchanged)
                { err.yL = Math.Abs((yKAB[i][1] - yKAB[i - 1][1]) - (correct.yA - correct.yL)); }
                if (i > 0 && !correct.yL_exchanged)
                { err.yL = Math.Abs((yKAB[i][1] - xKAB[i - 1][1]) - (correct.yA - correct.yL)); }
                if (i < N - 1 && correct.yR_exchanged)
                { err.yR = Math.Abs((yKAB[i][1] - yKAB[i + 1][1]) - (correct.yA - correct.yR)); }
                if (i < N - 1 && !correct.yR_exchanged)
                { err.yR = Math.Abs((yKAB[i][1] - xKAB[i + 1][1]) - (correct.yA - correct.yR)); }

                err.yD = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
                err.yD = Math.Abs(err.yD - correct.yD);

                Error.Add(err);
            }

            // 寻找最优的匹配直线
            double xFactor = double.MaxValue, yFactor = double.MaxValue;
            int xBest = -1, yBest = -1;

            foreach (ERROR error in Error)
            {
                double xe = error.xPos + error.xFit + error.xL + error.xR + 0.1 * error.xD;
                double ye = error.yPos + error.yFit + error.yL + error.yR + 0.1 * error.yD;

                if (xe < xFactor) { xFactor = xe; xBest = error.index; }
                if (ye < yFactor) { yFactor = ye; yBest = error.index; }
            }

            config.xLine = xBest; config.yLine = yBest;
        }

        private static int getSpeedX(CORRECT correctTarget)
        {
            // 直线无效
            if (correctTarget.yInvalid) { config.ApproachX = true; }
            if (config.yLine == -1) { return AST_GuideBySpeed.getSpeedX(0); }

            // 获取数据
            double[] KAB = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(config.Lines[config.yLine]));

            // 获取控制
            double current = KAB[2];
            double target = correctTarget.yD;
            double Kp = 0.6;

            double adjust = Kp * (current - target);

            // 调整终点
            config.ApproachX = Math.Abs(current - target) < 10;

            // 避撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        private static int getSpeedY(CORRECT correctTarget)
        {
            // 直线无效
            if (correctTarget.xInvalid) { config.ApproachY = true; }
            if (config.xLine == -1) { return AST_GuideBySpeed.getSpeedY(0); }

            // 获取数据
            double[] KAB = CoordinatePoint.Fit(config.Lines[config.xLine]);

            // 获取控制
            double current = KAB[2];
            double target = correctTarget.xD;
            double Kp = 0.6;

            double adjust = Kp * (current - target);

            // 调整终点
            config.ApproachY = Math.Abs(current - target) < 10;

            // 避撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        private static int getSpeedA(CORRECT correctTarget)
        {
            // 直线无效
            if (correctTarget.xInvalid && correctTarget.yInvalid) { config.ApproachA = true; }
            if (config.xLine == -1 && config.yLine == -1) { return AST_GuideBySpeed.getSpeedA(0); }

            // 获取数据
            int index = config.xLine != -1 ? config.xLine : config.yLine;
            double[] KAB = CoordinatePoint.Fit(config.Lines[index]);

            // 获取控制
            double current = KAB[1];
            double target = config.xLine != -1 ? correctTarget.xA : correctTarget.yA;
            double Kp = 30;

            double adjust = Kp * (current - target);

            // 调整终点
            config.ApproachA = Math.Abs(current - target) < 1;

            // 避撞
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
    }
}
