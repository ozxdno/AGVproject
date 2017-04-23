using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 环境信息导航
    /// </summary>
    class AST_GuideBySurrounding
    {
        /// <summary>
        /// 已到达 X 方向调整极限
        /// </summary>
        public static bool ApproachX;
        /// <summary>
        /// 已到达 Y 方向调整极限
        /// </summary>
        public static bool ApproachY;
        /// <summary>
        /// 已到达 A 方向调整极限
        /// </summary>
        public static bool ApproachA;

        /// <summary>
        /// 全速靠近左边，直到到达维持距离，然后维持该距离
        /// </summary>
        /// <param name="keepDistance">维持车身与左边障碍物距离</param>
        /// <returns></returns>
        public static int getSpeedX_KeepL_Forward(double keepDistance)
        {
            // 获取数据
            List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(150, 180);
            List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 30);
            
            pointsL = CoordinatePoint.AbsX(pointsL);
            pointsR = CoordinatePoint.AbsX(pointsR);

            // 获取最近距离
            double minL = CoordinatePoint.MinX(pointsL);
            double minR = CoordinatePoint.MinX(pointsR);

            // 点数量不够
            bool acceptL = pointsL.Count != 0;
            bool acceptR = pointsR.Count != 0;

            if (!acceptL) { minL = Hardware_URG.max; }
            if (!acceptR) { minR = Hardware_URG.max; }

            // 默认参数
            double current = minL;
            double target = keepDistance - Hardware_PlatForm.AxisSideL;
            double Kp = 0.6;

            double adjust = -Kp * (current - target);
            
            // 判断是否已经到达
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        /// <summary>
        /// 全速靠近右边，直到到达维持距离，然后维持该距离
        /// </summary>
        /// <param name="keepDistance">维持车身与右边障碍物距离</param>
        /// <returns></returns>
        public static int getSpeedX_KeepR_Forward(double keepDistance)
        {
            // 获取数据
            List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(150,180);
            List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 30);
            
            pointsL = CoordinatePoint.AbsX(pointsL);
            pointsR = CoordinatePoint.AbsX(pointsR);

            // 获取最近距离
            double minL = CoordinatePoint.MinX(pointsL);
            double minR = CoordinatePoint.MinX(pointsR);

            // 点数量不够
            bool acceptL = pointsL.Count != 0;
            bool acceptR = pointsR.Count != 0;

            if (!acceptL) { minL = Hardware_URG.max; }
            if (!acceptR) { minR = Hardware_URG.max; }

            // 默认参数
            double current = minR;
            double target = keepDistance + Hardware_PlatForm.AxisSideR;
            double Kp = 0.6;

            double adjust = Kp * (current - target);
            
            // 判断是否已经到达
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        /// <summary>
        /// 全速靠近左边，直到到达维持距离，然后维持该距离
        /// </summary>
        /// <param name="keepDistance">维持车身与左边障碍物距离</param>
        /// <returns></returns>
        public static int getSpeedX_KeepL_Backward(double keepDistance)
        {
            // 获取数据
            double yBG = Hardware_PlatForm.AxisSideD;
            double yED = yBG + Hardware_PlatForm.Length / 4;
            List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingY(yBG, yED);
            pointsL = CoordinatePoint.SelectX(double.MinValue, 0, pointsL);
            pointsL = CoordinatePoint.AbsX(pointsL);

            // 获取最近距离
            double minL = CoordinatePoint.MinX(pointsL);

            // 点数量不够
            bool acceptL = pointsL.Count != 0;
            if (!acceptL) { minL = Hardware_UltraSonic.Max; }

            // 获取控制
            double current = minL;
            double target = keepDistance - Hardware_PlatForm.AxisSideL;
            double Kp = 0.6;

            double adjust = -Kp * (current - target);
            
            // 判断是否已经到达
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        /// <summary>
        /// 全速靠近右边，直到到达维持距离，然后维持该距离
        /// </summary>
        /// <param name="keepDistance">维持车身与右边障碍物距离</param>
        /// <returns></returns>
        public static int getSpeedX_KeepR_Backward(double keepDistance)
        {
            // 获取数据
            double yBG = Hardware_PlatForm.AxisSideD;
            double yED = yBG + Hardware_PlatForm.Length / 4;
            List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingY(yBG, yED);
            pointsR = CoordinatePoint.SelectX(0, double.MaxValue, pointsR);
            pointsR = CoordinatePoint.AbsX(pointsR);

            // 获取最近距离
            double minR = CoordinatePoint.MinX(pointsR);

            // 点数量不够
            bool acceptR = pointsR.Count != 0;
            if (!acceptR) { minR = Hardware_UltraSonic.Max; }

            // 获取控制
            double current = minR;
            double target = keepDistance + Hardware_PlatForm.AxisSideR;
            double Kp = 0.6;

            double adjust = Kp * (current - target);

            // 判断是否已经到达
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }




        /// <summary>
        /// 全速前进，直至遇到障碍物，再缓慢停止
        /// </summary>
        /// <param name="keepDistance">车头与障碍物的保持距离</param>
        /// <returns></returns>
        public static int getSpeedY_KeepU(double keepDistance)
        {
            // 获取数据
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(85, 95);

            // 获取距离
            double minH = CoordinatePoint.MinY(points);
            if (points.Count == 3) { minH = Hardware_URG.max; }

            // 获取控制
            double current = minH;
            double target = keepDistance + Hardware_PlatForm.AxisSideU;
            double Kp = 1;

            double adjust = Kp * (current - target);
            
            // 判断是否已经到达
            ApproachY = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        /// <summary>
        /// 全速后退，直至遇到障碍物，再缓慢停止
        /// </summary>
        /// <param name="keepDistance">车尾与障碍物的保持距离</param>
        /// <returns></returns>
        public static int getSpeedY_KeepD(double keepDistance)
        {
            // 获取数据
            int Tail_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);
            int Tail_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);

            // 数据量不够
            if (Tail_L_Y == 0) { Tail_L_Y = (int)Hardware_UltraSonic.Tail_L_Y.max; }
            if (Tail_R_Y == 0) { Tail_R_Y = (int)Hardware_UltraSonic.Tail_R_Y.max; }

            double minT = Math.Min(Tail_L_Y, Tail_R_Y);

            // 获取控制
            double current = minT;
            double target = keepDistance;
            double Kp = 1;

            double adjust = -Kp * (current - target);

            // 判断是否已经到达
            ApproachY = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        



        /// <summary>
        /// 小车在前进时，将与左边对齐。前方出现墙壁时，不进行控制
        /// </summary>
        /// <returns></returns>
        public static int getSpeedA_KeepL_Forward()
        {
            // 纠偏
            List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(85, 95);
            double minH = CoordinatePoint.MinY(pointsH);
            if (minH < Hardware_PlatForm.ForeSightED) { return AST_GuideBySpeed.getSpeedA(0); }

            // 取点
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(120, 180);
            points = CoordinatePoint.ExChangeXY(CoordinatePoint.AbsXY(points));

            // Y 轴限幅（去除远处的点）
            points = CoordinatePoint.SortY(points);
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i + 1].y - points[i].y < 200) { continue; }
                points.RemoveRange(i, points.Count - i); break;
            }

            // 控制策略
            double current = getFitAngle(points);
            double target = 0;
            double Kp = 30;
            double adjust = Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 超声波车身偏离补偿
            //int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            //int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            //int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);
            //int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);

            //double minL = TH_AutoSearchTrack.control.MinDistance_L;
            //double minR = TH_AutoSearchTrack.control.MinDistance_R;

            //if (0 < Head_L_X && Head_L_X < minL) { aSpeed -= (int)(3 * (minL - Head_L_X)); }
            //if (0 < Head_R_X && Head_R_X < minR) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_L_X && Tail_L_X < minL) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_R_X && Tail_R_X < minR) { aSpeed -= (int)(3 * (minL - Head_L_X)); }

            // 防撞
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
        /// <summary>
        /// 小车在后退时，将与左边对齐。后方出现墙壁时，不进行控制
        /// </summary>
        /// <returns></returns>
        public static int getSpeedA_KeepL_Backward()
        {
            // 后方出现墙壁
            int Tail_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);
            int Tail_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_Y);

            if (0 < Tail_L_Y && Tail_L_Y < Hardware_PlatForm.ForeSightED &&
                0 < Tail_R_Y && Tail_R_Y < Hardware_PlatForm.ForeSightED) { return AST_GuideBySpeed.getSpeedA(0); }

            // 获取数据
            double yBG = Hardware_PlatForm.AxisSideD - Hardware_PlatForm.ForeSightBG;
            double yED = Hardware_PlatForm.AxisSideD + Hardware_PlatForm.Length / 4;
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingY(yBG, yED);
            points = CoordinatePoint.SelectX(double.MinValue, 0, points);
            points = CoordinatePoint.ExChangeXY(CoordinatePoint.AbsXY(points));

            // Y 轴限幅（去除远处的点）
            points = CoordinatePoint.SortY(points);
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i + 1].y - points[i].y < 200) { continue; }
                points.RemoveRange(i, points.Count - i); break;
            }

            // 控制策略
            double current = getFitAngle(points);
            double target = 0;
            double Kp = 30;

            double adjust = Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 超声波车身偏离补偿
            //int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            //int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            //int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);
            //int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);

            //double minL = TH_AutoSearchTrack.control.MinDistance_L;
            //double minR = TH_AutoSearchTrack.control.MinDistance_R;

            //if (0 < Head_L_X && Head_L_X < minL) { aSpeed -= (int)(3 * (minL - Head_L_X)); }
            //if (0 < Head_R_X && Head_R_X < minR) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_L_X && Tail_L_X < minL) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_R_X && Tail_R_X < minR) { aSpeed -= (int)(3 * (minL - Head_L_X)); }

            // 限速
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
        /// <summary>
        /// 小车在前进时，将与右边对齐。前方出现墙壁时，不进行控制
        /// </summary>
        /// <returns></returns>
        public static int getSpeedA_KeepR_Forward()
        {
            // 纠偏
            List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(85, 95);
            double minH = CoordinatePoint.MinY(pointsH);
            if (minH < Hardware_PlatForm.ForeSightED) { return AST_GuideBySpeed.getSpeedA(0); }

            // 取点
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(0, 60);
            points = CoordinatePoint.ExChangeXY(CoordinatePoint.AbsXY(points));

            // Y 轴限幅（去除远处的点）
            points = CoordinatePoint.SortY(points);
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i + 1].y - points[i].y < 200) { continue; }
                points.RemoveRange(i, points.Count - i); break;
            }

            // 控制策略
            double current = getFitAngle(points);
            double target = 0;
            double Kp = 30;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 超声波车身偏离补偿
            //int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            //int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            //int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);
            //int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);

            //double minL = TH_AutoSearchTrack.control.MinDistance_L;
            //double minR = TH_AutoSearchTrack.control.MinDistance_R;

            //if (0 < Head_L_X && Head_L_X < minL) { aSpeed -= (int)(3 * (minL - Head_L_X)); }
            //if (0 < Head_R_X && Head_R_X < minR) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_L_X && Tail_L_X < minL) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_R_X && Tail_R_X < minR) { aSpeed -= (int)(3 * (minL - Head_L_X)); }

            // 限速
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
        /// <summary>
        /// 小车在后退时，将与右边对齐。后方出现墙壁时，不进行控制
        /// </summary>
        /// <returns></returns>
        public static int getSpeedA_KeepR_Backward()
        {
            // 后方出现墙壁
            int Tail_L_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_Y);
            int Tail_R_Y = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_Y);
            if (0 < Tail_L_Y && Tail_L_Y < Hardware_PlatForm.ForeSightED && 
                0 < Tail_R_Y && Tail_R_Y < Hardware_PlatForm.ForeSightED) { return 0; }

            // 获取数据
            double yBG = Hardware_PlatForm.AxisSideD - Hardware_PlatForm.ForeSightBG;
            double yED = Hardware_PlatForm.AxisSideD + Hardware_PlatForm.Length / 4;
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingY(yBG, yED);
            points = CoordinatePoint.SelectX(0, double.MaxValue, points);
            points = CoordinatePoint.ExChangeXY(CoordinatePoint.AbsXY(points));

            // Y 轴限幅（去除远处的点）
            points = CoordinatePoint.SortY(points);
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i + 1].y - points[i].y < 200) { continue; }
                points.RemoveRange(i, points.Count - i); break;
            }

            // 控制策略
            double current = getFitAngle(points);
            double target = 0;
            double Kp = 30;

            double adjust = -Kp * (current - target);
            
            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 超声波车身偏离补偿
            //int Head_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_L_X);
            //int Head_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Head_R_X);
            //int Tail_L_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_L_X);
            //int Tail_R_X = TH_SendCommand.getUltraSonicData(TH_SendCommand.Sonic.Tail_R_X);

            //double minL = TH_AutoSearchTrack.control.MinDistance_L;
            //double minR = TH_AutoSearchTrack.control.MinDistance_R;

            //if (0 < Head_L_X && Head_L_X < minL) { aSpeed -= (int)(3 * (minL - Head_L_X)); }
            //if (0 < Head_R_X && Head_R_X < minR) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_L_X && Tail_L_X < minL) { aSpeed += (int)(3 * (minL - Head_L_X)); }
            //if (0 < Tail_R_X && Tail_R_X < minR) { aSpeed -= (int)(3 * (minL - Head_L_X)); }

            // 限速
            return AST_GuideBySpeed.getSpeedA(adjust);
        }

        private static double getFitAngle(List<CoordinatePoint.POINT> points)
        {
            // 点数量不足
            if (points.Count <= 3) { return 0; }

            // 若跨度满足要求
            double xSpan = CoordinatePoint.MaxX(points) - CoordinatePoint.MinX(points);
            if (xSpan > Hardware_PlatForm.ForeSightBG)
            {
                points = getFitPoints(points);
                if (points.Count <= 3) { return 0; }
                double[] KAB = CoordinatePoint.Fit(points);
                return KAB[1];
            }

            // 设定基准点
            double BaseX = Hardware_PlatForm.ForeSightBG;
            double BaseY = HouseMap.getKeepDistance();
            
            // 获取相对角度
            double minAngle = double.MaxValue, maxAngle = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                double angle = Math.Atan((points[i].y - BaseY) / (points[i].x - BaseX)) * 180 / Math.PI;
                if (angle < 0) { angle += 180; }

                if (angle < minAngle) { minAngle = angle; }
                if (angle > minAngle) { maxAngle = angle; }
            }

            // 返回相对角度
            if (maxAngle > 120) { return 90 - maxAngle; }
            return minAngle;
        }
        private static List<CoordinatePoint.POINT> getFitPoints(List<CoordinatePoint.POINT> points)
        {
            // 点的数量不够，直接返回。
            if (points.Count <= 3) { return points; }

            // 最近的点作为基准点
            double x0 = 0.0, y0 = double.MaxValue;
            int closest = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].y > y0) { continue; }

                x0 = points[i].x;
                y0 = points[i].y;
                closest = i;
            }

            // 其余点相对角度
            List<double> angles = new List<double>();
            double MaxAngle = 0;
            double MinAngle = double.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == closest) { angles.Add(0.0); continue; }

                double angle = Math.Atan((points[i].y - y0) / (points[i].x - x0));
                if (points[i].x < x0) { angle += 180; }

                angles.Add(angle);
                if (angle > MaxAngle) { MaxAngle = angle; }
                if (angle < MinAngle) { MinAngle = angle; }
            }

            // 根据最近点的相对小车距离决定选取那个角度
            double targetAngle = x0 < Hardware_PlatForm.ForeSightBG ? MaxAngle : MinAngle;
            angles[closest] = targetAngle;

            // 取出斜率符合的点
            List<CoordinatePoint.POINT> fitpoints = new List<CoordinatePoint.POINT>();
            for (int i = 1; i < angles.Count; i++)
            {
                if (Math.Abs(angles[i] - targetAngle) < 1.0) { fitpoints.Add(points[i]); continue; }
                if (Math.Abs(angles[i] + targetAngle - 180) < 1.0) { fitpoints.Add(points[i]); }
            }

            // 返回
            return fitpoints;
        }
        
    }
}
