using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    class CorrectPosition
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
            /// X 方向直线的长度
            /// </summary>
            public double xL;
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
            /// X 方向直线的最小距离（与截距一个性质）
            /// </summary>
            public double xD;
            /// <summary>
            /// 右边直线斜率
            /// </summary>
            public double x1;
            /// <summary>
            /// 左边直线斜率
            /// </summary>
            public double x2;

            public double yK;
            public double yL;
            public double yA;
            public double yB;
            public double yC;
            public double yD;
            public double y1;
            public double y2;
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

            public double xAdjustError;
            public double yAdjustError;
            public double aAdjustError;
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

            Initial();
            
            // 粗调
            config.ApproachX = false;
            config.ApproachY = false;
            config.ApproachA = false;
            config.aAdjustError = 3;
            config.xAdjustError = 50;
            config.yAdjustError = 50;

            AdjustA(correctTarget);
            AdjustY(correctTarget);
            AdjustX(correctTarget);

            // 精调
            config.ApproachX = false;
            config.ApproachY = false;
            config.ApproachA = false;
            config.aAdjustError = 1;
            config.xAdjustError = 10;
            config.yAdjustError = 10;

            AdjustA(correctTarget);
            AdjustY(correctTarget);
            AdjustX(correctTarget);

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

            // 获取 X 方向下次校准的参数
            if (!correct.xInvalid)
            {
                CoordinatePoint.POINT ptBG = config.Lines[config.xLine][0];
                CoordinatePoint.POINT ptED = config.Lines[config.xLine][config.Lines[config.xLine].Count - 1];

                double[] xKAB = CoordinatePoint.Fit(config.Lines[config.xLine]);
                correct.xK = xKAB[0];
                correct.xL = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
                correct.xA = xKAB[1];
                correct.xB = xKAB[2];
                correct.xC = CoordinatePoint.AverageA(config.Lines[config.xLine]);
                correct.xD = CoordinatePoint.MinD(config.Lines[config.xLine]);
            }

            // 获取 Y 方向下次校准的参数
            if (!correct.yInvalid)
            {
                CoordinatePoint.POINT ptBG = config.Lines[config.yLine][0];
                CoordinatePoint.POINT ptED = config.Lines[config.yLine][config.Lines[config.yLine].Count - 1];

                List<CoordinatePoint.POINT> exLine = CoordinatePoint.ExChangeXY(CoordinatePoint.Copy(config.Lines[config.yLine]));

                double[] yKAB = CoordinatePoint.Fit(exLine);
                correct.yK = yKAB[0];
                correct.yL = Math.Sqrt((ptBG.x - ptED.x) * (ptBG.x - ptED.x) + (ptBG.y - ptED.y) * (ptBG.y - ptED.y));
                correct.yA = yKAB[1];
                correct.yB = yKAB[2];
                correct.yC = CoordinatePoint.AverageA(config.Lines[config.yLine]);
                correct.yD = CoordinatePoint.MinD(exLine);
            }
            
            // 返回结果
            return correct;
        }
        
        ////////////////////////////////////////////////// private method /////////////////////////////////////////

        private static void Initial()
        {
            config.Lines = new List<List<CoordinatePoint.POINT>>();
            config.LineError = 50; // 线段的点的浮动误差 
            config.PtReqNum = 30;
            config.NegeError = 200; // 线段至少10cm长
        }

        private static void cutPointsToGroup(List<CoordinatePoint.POINT> points)
        {
            // 点的数量不够
            if (points.Count == 0) { return; }
            if (points.Count == 1 || points.Count == 2) { return; }

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
            if (MaxDis <= config.LineError)
            {
                if (points.Count < config.PtReqNum) { return; }
                config.Lines.Add(points); return;
            }

            List<CoordinatePoint.POINT> newLine = new List<CoordinatePoint.POINT>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);

            newLine = new List<CoordinatePoint.POINT>();
            for (int i = indexofmax + 1; i < points.Count; i++) { newLine.Add(points[i]); }
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

            // 获取各条直线的各种误差参数
            List<CORRECT> Error = new List<CORRECT>();
            CoordinatePoint.POINT ptBG, ptED;

            for (int i = 0; i < config.Lines.Count; i++)
            {
                // 获取直线
                List<CoordinatePoint.POINT> readLine = config.Lines[i];
                List<CoordinatePoint.POINT> copyLine = CoordinatePoint.Copy(readLine);
                copyLine = CoordinatePoint.ExChangeXY(copyLine);

                // 误差集合
                CORRECT err = new CORRECT();

                // 获取 KAB 误差
                double[] xKAB = CoordinatePoint.Fit(readLine);
                double[] yKAB = CoordinatePoint.Fit(copyLine);

                err.xK = Math.Abs(correct.xK - xKAB[0]);
                err.xA = Math.Abs(correct.xA - xKAB[1]);
                err.xB = Math.Abs(correct.xB - xKAB[2]);
                err.yK = Math.Abs(correct.yK - yKAB[0]);
                err.yA = Math.Abs(correct.yA - yKAB[1]);
                err.yB = Math.Abs(correct.yB - yKAB[2]);

                // 获取 L 误差
                ptBG = readLine[0]; ptED = readLine[readLine.Count - 1];
                double length = CoordinatePoint.getDistance(ptBG, ptED);
                err.xL = Math.Abs(correct.xL - length);
                err.yL = Math.Abs(correct.yL - length);

                // 获取 C 误差
                double centre = CoordinatePoint.AverageA(readLine);
                err.xC = Math.Abs(correct.xC - centre);
                err.yC = Math.Abs(correct.yC - centre);

                // 获取 D 误差
                double distance = CoordinatePoint.MinD(readLine);
                err.xD = Math.Abs(correct.xD - distance);
                err.yD = Math.Abs(correct.yD - distance);

                // 添加到误差列表
                Error.Add(err);
            }
            
            // 寻找最优的匹配直线
            double xFactor = double.MaxValue, yFactor = double.MaxValue;
            int xBest = -1, yBest = -1;

            for (int i = 0; i < Error.Count; i++)
            {
                CORRECT error = Error[i];

                double xe = 0.1 * error.xL + error.xA + error.xC + 0.1 * error.xD;
                double ye = 0.1 * error.yL + error.yA + error.yC + 0.1 * error.yD;

                if (xe < xFactor) { xFactor = xe; xBest = i; }
                if (ye < yFactor) { yFactor = ye; yBest = i; }
            }

            if (!correct.xInvalid && xFactor < 40) { config.xLine = xBest; }
            if (!correct.yInvalid && yFactor < 40) { config.yLine = yBest; }
        }
        
        private static void AdjustX(CORRECT correctTarget)
        {
            if (correctTarget.yInvalid) { return; }

            while (!config.ApproachX)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines.Clear();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch(correctTarget);

                // 获取控制
                int xSpeed = getSpeedX(correctTarget);
                int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void AdjustY(CORRECT correctTarget)
        {
            if (correctTarget.xInvalid) { return; }

            while (!config.ApproachY)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines.Clear();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch(correctTarget);

                // 获取控制
                int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                int ySpeed = getSpeedY(correctTarget);
                int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void AdjustA(CORRECT correctTarget)
        {
            if (correctTarget.xInvalid && correctTarget.yInvalid) { return; }

            while (!config.ApproachA)
            {
                // 切割成直线
                List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 180);
                config.Lines.Clear();
                cutPointsToGroup(points);
                sortLines();

                // 获取匹配直线索引号
                getMatch(correctTarget);

                // 获取控制
                int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                int aSpeed = getSpeedA(correctTarget);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }

        private static int getSpeedX(CORRECT correctTarget)
        {
            // 直线无效
            if (correctTarget.yInvalid) { config.ApproachX = true; }
            if (config.yLine == -1) { return AST_GuideBySpeed.getSpeedX(0); }

            // 获取数据
            List<CoordinatePoint.POINT> copyLine = CoordinatePoint.Copy(config.Lines[config.yLine]);
            double[] KAB = CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(copyLine));

            // 获取控制
            double current = KAB[2];
            double target = correctTarget.yB;
            double Kp = 0.6;

            double adjust = Kp * (current - target);

            // 调整终点
            config.ApproachX = Math.Abs(current - target) < config.xAdjustError;

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
            double target = correctTarget.xB;
            double Kp = 0.6;

            double adjust = Kp * (current - target);

            // 调整终点
            config.ApproachY = Math.Abs(current - target) < config.yAdjustError;

            // 避撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        private static int getSpeedA(CORRECT correctTarget)
        {
            // 直线无效
            if (correctTarget.xInvalid && correctTarget.yInvalid) { config.ApproachA = true; }
            if (config.xLine == -1 && config.yLine == -1) { return AST_GuideBySpeed.getSpeedA(0); }

            // 获取数据
            List<CoordinatePoint.POINT> copyLine = new List<CoordinatePoint.POINT>();
            if (config.yLine != -1) { copyLine = CoordinatePoint.Copy(config.Lines[config.yLine]); }

            double[] KAB = config.xLine == -1 ?
                CoordinatePoint.Fit(CoordinatePoint.ExChangeXY(copyLine)) :
                CoordinatePoint.Fit(config.Lines[config.xLine]);

            // 获取控制
            double current = KAB[1];
            double target = config.xLine != -1 ? correctTarget.xA : correctTarget.yA;
            double Kp = 30;

            double adjust = Kp * (current - target);
            if (config.xLine == -1) { adjust = -adjust; }

            // 调整终点
            config.ApproachA = Math.Abs(current - target) < config.aAdjustError;

            // 避撞
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
    }
}
