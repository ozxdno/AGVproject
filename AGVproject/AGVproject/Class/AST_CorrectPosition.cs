using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_CorrectPosition
    {
        private static List<List<CoordinatePoint.POINT>> ptGroups;

        public static void Start()
        {
            while (true)
            {
                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 得到测量数据
                double K = 0, A = 0, B = 0;
                while (!getKAB(ref K, ref A, ref B)) ;

                // 获取控制
                if (Math.Abs(A) < 0.5) { return; }
                double Kp = 50;
                int aSpeed = (int)(Kp * A);

                // 允许旋转
                List<CoordinatePoint.POINT> pointsLR = TH_MeasureSurrounding.getSurroundingY
                (Hardware_PlatForm.AxisSideD, Hardware_PlatForm.AxisSideU);
                double minLR = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsLR));

                if (minLR < TH_AutoSearchTrack.control.MinDistance_L) { continue; }
                if (minLR < TH_AutoSearchTrack.control.MinDistance_R) { continue; }

                // 控制
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }
        }
        public static bool getKAB(ref double K, ref double A, ref double B)
        {
            // 拟合墙壁信息
            List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(75, 105);
            ptGroups = new List<List<CoordinatePoint.POINT>>();
            cutPointsToGroup(pointsH);
            pointsH = selectPoints();
            double[] KAB = CoordinatePoint.Fit(pointsH);

            K = KAB[0];
            A = KAB[1];
            B = KAB[2];

            return pointsH.Count > 10;
        }

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
            if (MaxDis <= 30) { ptGroups.Add(points); return; }

            List<CoordinatePoint.POINT> newLine = new List<CoordinatePoint.POINT>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);

            newLine = new List<CoordinatePoint.POINT>();
            for (int i = indexofmax; i < points.Count; i++) { newLine.Add(points[i]); }
            cutPointsToGroup(newLine);
        }
        private static List<CoordinatePoint.POINT> selectPoints()
        {
            if (ptGroups.Count == 0) { return new List<CoordinatePoint.POINT>(); }

            // 挑选直线
            for (int i = 0; i < ptGroups.Count; i++)
            {
                double bgX = ptGroups[i][0].x;
                double edX = ptGroups[i][ptGroups[i].Count - 1].x;

                if (bgX > 0 && edX < 0) { return ptGroups[i]; }
                if (bgX < 0 && edX > 0) { return ptGroups[i]; }

                if (bgX == 0)
                {
                    if (i == 0) { return ptGroups[i]; }
                    if (ptGroups[i].Count >= ptGroups[i - 1].Count) { return ptGroups[i]; }
                    return ptGroups[i - 1];
                }
                if (edX == 0)
                {
                    if (i == ptGroups.Count) { return ptGroups[i]; }
                    if (ptGroups[i].Count >= ptGroups[i + 1].Count) { return ptGroups[i]; }
                    return ptGroups[i + 1];
                }
            }
            if (ptGroups.Count != 0) { return ptGroups[0]; }
            return new List<CoordinatePoint.POINT>();
        }
    }
}
