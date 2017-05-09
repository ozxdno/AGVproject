using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 点类，包含点的一系列操作
    /// </summary>
    class CoordinatePoint
    {
        /// <summary>
        /// 点结构体
        /// </summary>
        public struct POINT
        {
            /// <summary>
            /// 相对坐标系原点的X轴距离 单位：mm
            /// </summary>
            public double x;
            /// <summary>
            /// 相对坐标系原点的Y轴距离 单位：mm
            /// </summary>
            public double y;

            /// <summary>
            /// 该点与X轴正方向的夹角，-90° 到 +270° 单位：度
            /// </summary>
            public double a;
            /// <summary>
            /// 该点与X轴正方向的夹角，-PI/2 到 +3PI/2 单位：弧度
            /// </summary>
            public double r;
            /// <summary>
            /// 该点与原点的距离，单位：mm
            /// </summary>
            public double d;

            /// <summary>
            /// 该点在仓库坐标系下的车头方向 单位：度
            /// </summary>
            public double aCar;
            /// <summary>
            /// 该点在仓库坐标系下的车头方向 单位：弧度
            /// </summary>
            public double rCar;
        }

        /// <summary>
        /// 把 POINT 结构体转换成 DOUBLE 数组
        /// </summary>
        /// <param name="point">POINT 型数据</param>
        /// <returns></returns>
        public static List<double> Point2Double(POINT point)
        {
            List<double> data = new List<double>();
            data.Add(point.x);
            data.Add(point.y);
            data.Add(point.a);
            data.Add(point.r);
            data.Add(point.d);
            data.Add(point.aCar);
            data.Add(point.rCar);

            return data;
        }
        /// <summary>
        /// 把 DOUBLE 型数组转换成 POINT 结构体
        /// </summary>
        /// <param name="data">DOUBLE 型数据</param>
        /// <returns></returns>
        public static POINT Double2Point(List<double> data)
        {
            POINT point = new POINT();
            if (data == null || data.Count < 7) { return point; }
            point.x = data[0];
            point.y = data[1];
            point.a = data[2];
            point.r = data[3];
            point.d = data[4];
            point.aCar = data[5];
            point.rCar = data[6];
            return point;
        }

        public static POINT Create_XY(double x, double y)
        {
            POINT point = new POINT();

            point.x = x;
            point.y = y;

            point.r = Math.Atan(y / x);
            if (x < 0) { point.r += Math.PI; }
            if (x == 0 && y == 0) { point.r = 0; }

            point.a = point.r * 180 / Math.PI;
            point.d = Math.Sqrt(x * x + y * y);
            return point;
        }
        public static POINT Create_DA(double d, double a)
        {
            POINT point = new POINT();

            point.d = d;
            point.a = a;
            point.r = a * Math.PI / 180;
            point.x = d * Math.Cos(point.r);
            point.y = d * Math.Sin(point.r);

            return point;
        }
        public static POINT Create_DR(double d, double r)
        {
            POINT point = new POINT();

            point.d = d;
            point.r = r;
            point.a = r / Math.PI * 180;
            point.x = d * Math.Cos(point.r);
            point.y = d * Math.Sin(point.r);

            return point;
        }

        /// <summary>
        /// 获取一个无效点（点中所有成员的值为 NaN）
        /// </summary>
        /// <returns></returns>
        public static POINT getNegPoint()
        {
            POINT point = new POINT();

            point.x = double.NaN;
            point.y = double.NaN;
            point.a = double.NaN;
            point.r = double.NaN;
            point.d = double.NaN;

            point.aCar = double.NaN;
            point.rCar = double.NaN;

            return point;
        }
        /// <summary>
        /// 判断输入点是否无效
        /// </summary>
        /// <param name="point">输入点</param>
        /// <returns></returns>
        public static bool IsNegPoint(POINT point)
        {
            if (double.IsNaN(point.x)) { return true; }
            if (double.IsNaN(point.y)) { return true; }
            if (double.IsNaN(point.a)) { return true; }
            if (double.IsNaN(point.d)) { return true; }
            if (double.IsNaN(point.r)) { return true; }

            if (double.IsNaN(point.aCar)) { return true; }
            if (double.IsNaN(point.rCar)) { return true; }
            return false;
        }
        /// <summary>
        /// 判断两个点的值是否相同
        /// </summary>
        /// <param name="point1">点 1</param>
        /// <param name="point2">点 2</param>
        /// <returns></returns>
        public static bool Equal(POINT point1, POINT point2)
        {
            if (point1.x != point2.x) { return false; }
            if (point1.y != point2.y) { return false; }
            if (point1.a != point2.a) { return false; }
            if (point1.r != point2.r) { return false; }
            if (point1.d != point2.d) { return false; }
            if (point1.aCar != point2.aCar) { return false; }
            if (point1.rCar != point2.rCar) { return false; }
            return true;
        }

        public static List<POINT> SelectX(double BG, double ED, List<POINT> points)
        {
            List<POINT> SelectedPoints = new List<POINT>();

            if (points == null) { return SelectedPoints; }

            foreach (POINT point in points)
            {
                if (point.x < BG || point.x > ED) { continue; }
                SelectedPoints.Add(point);
            }

            return SelectedPoints;
        }
        public static List<POINT> SelectY(double BG, double ED, List<POINT> points)
        {
            List<POINT> SelectedPoints = new List<POINT>();

            if (points == null) { return SelectedPoints; }

            foreach (POINT point in points)
            {
                if (point.y < BG || point.y > ED) { continue; }
                SelectedPoints.Add(point);
            }

            return SelectedPoints;
        }
        public static List<POINT> SelectA(double BG, double ED, List<POINT> points)
        {
            List<POINT> SelectedPoints = new List<POINT>();
            if (points == null) { return SelectedPoints; }

            if (BG > ED)
            {
                foreach (POINT point in points)
                {
                    if (ED < point.a && point.a < BG) { continue; }
                    SelectedPoints.Add(point);
                }
                return SelectedPoints;
            }

            foreach (POINT point in points)
            {
                if (point.a < BG || point.a > ED) { continue; }
                SelectedPoints.Add(point);
            }

            return SelectedPoints;
        }
        public static List<POINT> SelectR(double BG, double ED, List<POINT> points)
        {
            List<POINT> SelectedPoints = new List<POINT>();
            if (points == null) { return SelectedPoints; }

            if (BG > ED)
            {
                foreach (POINT point in points)
                {
                    if (ED < point.r && point.r < BG) { continue; }
                    SelectedPoints.Add(point);
                }
                return SelectedPoints;
            }

            foreach (POINT point in points)
            {
                if (point.r < BG || point.r > ED) { continue; }
                SelectedPoints.Add(point);
            }

            return SelectedPoints;
        }
        public static List<POINT> SelectD(double BG, double ED, List<POINT> points)
        {
            List<POINT> SelectedPoints = new List<POINT>();

            if (points == null) { return SelectedPoints; }

            foreach (POINT point in points)
            {
                if (point.d < BG || point.d > ED) { continue; }
                SelectedPoints.Add(point);
            }

            return SelectedPoints;
        }

        public static double MaxX(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MinValue; }

            double max = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].x > max) { max = points[i].x; }
            }

            return max;
        }
        public static double MaxY(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MinValue; }

            double max = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].y > max) { max = points[i].y; }
            }

            return max;
        }
        public static double MaxA(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MinValue; }

            double max = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].a > max) { max = points[i].a; }
            }

            return max;
        }
        public static double MaxD(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MinValue; }

            double max = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].d > max) { max = points[i].d; }
            }

            return max;
        }

        public static double MinX(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MaxValue; }

            double min = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].x < min) { min = points[i].x; }
            }

            return min;
        }
        public static double MinY(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MaxValue; }

            double min = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].y < min) { min = points[i].y; }
            }

            return min;
        }
        public static double MinA(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MaxValue; }

            double min = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].a < min) { min = points[i].a; }
            }

            return min;
        }
        public static double MinD(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return double.MaxValue; }

            double min = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].d < min) { min = points[i].d; }
            }

            return min;
        }

        public static List<POINT> AbsX(List<POINT> points)
        {
            List<POINT> newPoints = new List<POINT>();
            if (points == null) { return newPoints; }

            for (int i = 0; i < points.Count; i++)
            {
                POINT point = Create_XY(Math.Abs(points[i].x), points[i].y);
                newPoints.Add(point);
            }

            return newPoints;
        }
        public static List<POINT> AbsY(List<POINT> points)
        {
            List<POINT> newPoints = new List<POINT>();
            if (points == null) { return newPoints; }

            for (int i = 0; i < points.Count; i++)
            {
                POINT point = Create_XY(points[i].x, Math.Abs(points[i].y));
                newPoints.Add(point);
            }

            return newPoints;
        }
        public static List<POINT> AbsXY(List<POINT> points)
        {
            List<POINT> newPoints = new List<POINT>();
            if (points == null) { return newPoints; }

            for (int i = 0; i < points.Count; i++)
            {
                POINT point = Create_XY(Math.Abs(points[i].x), Math.Abs(points[i].y));
                newPoints.Add(point);
            }

            return newPoints;
        }

        public static List<POINT> ExChangeXY(List<POINT> points)
        {
            if (points == null) { return new List<POINT>(); }
            for (int i = 0; i < points.Count; i++) {  points[i] = Create_XY(points[i].y, points[i].x); }
            return points;
        }

        public static List<POINT> SortX(List<POINT> points, bool increase = true)
        {
            if (points == null) { return new List<POINT>(); }

            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (increase && points[j].x <= points[j + 1].x) { continue; }
                    if (!increase && points[j].x >= points[j + 1].x) { continue; }

                    POINT temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            return points;
        }
        public static List<POINT> SortY(List<POINT> points, bool increase = true)
        {
            if (points == null) { return new List<POINT>(); }

            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (increase && points[j].y <= points[j + 1].y) { continue; }
                    if (!increase && points[j].y >= points[j + 1].y) { continue; }

                    POINT temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            return points;
        }
        public static List<POINT> SortA(List<POINT> points, bool increase = true)
        {
            if (points == null) { return new List<POINT>(); }

            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (increase && points[j].a <= points[j + 1].a) { continue; }
                    if (!increase && points[j].a >= points[j + 1].a) { continue; }

                    POINT temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            return points;
        }
        public static List<POINT> SortD(List<POINT> points, bool increase = true)
        {
            if (points == null) { return new List<POINT>(); }

            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (increase && points[j].d <= points[j + 1].d) { continue; }
                    if (!increase && points[j].d >= points[j + 1].d) { continue; }

                    POINT temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            return points;
        }

        public static bool MaxGapX(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MinValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].x - points[i + 1].x);
                if (gap > Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MaxGapY(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MinValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].y - points[i + 1].y);
                if (gap > Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MaxGapA(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MinValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].a - points[i + 1].a);
                if (gap > Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MaxGapD(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MinValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].d - points[i + 1].d);
                if (gap > Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }

        public static bool MinGapX(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].x - points[i + 1].x);
                if (gap < Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MinGapY(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].y - points[i + 1].y);
                if (gap < Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MinGapA(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].a - points[i + 1].a);
                if (gap < Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }
        public static bool MinGapD(List<POINT> points, ref double Gap, ref int indexL, ref int indexR)
        {
            if (points == null || points.Count <= 1) { return false; }

            Gap = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double gap = Math.Abs(points[i].d - points[i + 1].d);
                if (gap < Gap) { Gap = gap; indexL = i; indexR = i + 1; }
            }

            return true;
        }

        /// <summary>
        /// 对所有给出点的 x 值取平均
        /// </summary>
        /// <param name="points">给出点</param>
        /// <returns></returns>
        public static double AverageX(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return 0; }

            double sum = 0;
            for (int i = 0; i < points.Count; i++) { sum += points[i].x; }
            return sum / points.Count;
        }
        public static double AverageY(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return 0; }

            double sum = 0;
            for (int i = 0; i < points.Count; i++) { sum += points[i].y; }
            return sum / points.Count;
        }
        public static double AverageD(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return 0; }

            double sum = 0;
            for (int i = 0; i < points.Count; i++) { sum += points[i].d; }
            return sum / points.Count;
        }
        public static double AverageA(List<POINT> points)
        {
            if (points == null || points.Count == 0) { return 0; }

            double sum = 0;
            for (int i = 0; i < points.Count; i++) { sum += points[i].a; }
            return sum / points.Count;
        }

        /// <summary>
        /// 点对点进行移动，从源坐标移动到目标坐标的移动策略
        /// </summary>
        /// <param name="sour">源坐标</param>
        /// <param name="dest">目的坐标</param>
        /// <returns></returns>
        public static POINT MoveMethod(POINT sour, POINT dest)
        {
            double xMove = dest.x - sour.x;
            double yMove = dest.y - sour.y;

            double aMove = dest.aCar - sour.aCar;
            double rMove = dest.rCar - sour.rCar;

            POINT Move = Create_XY(xMove, yMove);
            Move.aCar = aMove;
            Move.rCar = rMove;
            return TransformCoordinate(Move, 0, 0, dest.rCar);
        }
        /// <summary>
        /// 变换小车坐标系，返回变化后目标位置
        /// </summary>
        /// <param name="pointO">把该点设定为原点</param>
        /// <param name="pos">原系中的位置</param>
        /// <returns></returns>
        public static POINT TransformCoordinate(POINT pointO, POINT pos)
        {
            double xMove = pos.x - pointO.x;
            double yMove = pos.y - pointO.y;

            double aMove = pos.aCar - pointO.aCar;
            double rMove = pos.rCar - pointO.rCar;

            POINT Move = Create_XY(xMove, yMove);
            Move.aCar = aMove;
            Move.rCar = rMove;
            return TransformCoordinate(Move, 0, 0, pos.rCar);
        }
        /// <summary>
        /// 变换小车坐标系
        /// </summary>
        /// <param name="pointO"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<POINT> TransformCoordinate(POINT pointO, List<POINT> points)
        {
            if (points == null) { return new List<POINT>(); }
            for (int i = 0; i < points.Count; i++) { points[i] = TransformCoordinate(pointO, points[i]); }
            return points;
        }
        /// <summary>
        /// 变换坐标系
        /// </summary>
        /// <param name="point">需要变换的点（原坐标系中的点）</param>
        /// <param name="xMove">原坐标系 X 轴变化量，向原坐标系 X 轴正方向移动为正</param>
        /// <param name="yMove">原坐标系 Y 轴变化量，向原坐标系 Y 轴正方向移动为正</param>
        /// <param name="rMove">原坐标系角度变化量，逆时针旋转为正</param>
        /// <returns>新坐标系下的点坐标</returns>
        public static POINT TransformCoordinate(POINT point, double xMove, double yMove, double rMove)
        {
            double x = point.x - xMove;
            double y = point.y - yMove;

            point.x = x * Math.Cos(rMove) + y * Math.Sin(rMove);
            point.y = y * Math.Cos(rMove) - x * Math.Sin(rMove);
            
            POINT newPoint = Create_XY(point.x, point.y);
            newPoint.aCar = point.aCar;
            newPoint.rCar = point.rCar;

            return newPoint;
        }
        /// <summary>
        /// 变换坐标系
        /// </summary>
        /// <param name="points">需要变换的点（原坐标系中的点）</param>
        /// <param name="xMove">原坐标系 X 轴变化量，向原坐标系 X 轴正方向移动为正</param>
        /// <param name="yMove">原坐标系 Y 轴变化量，向原坐标系 Y 轴正方向移动为正</param>
        /// <param name="rMove">原坐标系角度变化量，逆时针旋转为正</param>
        /// <returns>新坐标系下的点坐标</returns>
        public static List<POINT> TransformCoordinate(List<POINT> points, double xMove, double yMove, double rMove)
        {
            if (points == null) { return new List<POINT>(); }

            for (int i = 0; i < points.Count; i++)
            { points[i] = TransformCoordinate(points[i], xMove, yMove, rMove); }
            return points;
        }

        /// <summary>
        /// 二维平面直线拟合，返回 KAB 值（斜率、夹角、截距）
        /// </summary>
        /// <param name="points">待拟合的点</param>
        /// <returns></returns>
        public static double[] Fit(List<POINT> points)
        {
            if (points == null || points.Count < 2) { return new double[3] { 0, 0, 0 }; }
            
            double sumX = 0, sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0;
            int N = points.Count;
            for (int i = 0; i < N; i++)
            {
                sumX += points[i].x;
                sumY += points[i].y;

                sumXX += points[i].x * points[i].x;
                sumXY += points[i].x * points[i].y;
                sumYY += points[i].y * points[i].y;
            }

            double denominator = N * sumXX - sumX * sumX;
            if (denominator == 0) { denominator = 0.000000001; }

            // 计算斜率和截距
            double K = (N * sumXY - sumX * sumY) / denominator;
            double B = (sumXX * sumY - sumX * sumXY) / denominator;
            double A = Math.Atan(K) * 180 / Math.PI;

            return new double[3] { K, A, B };
        }
    }
}
