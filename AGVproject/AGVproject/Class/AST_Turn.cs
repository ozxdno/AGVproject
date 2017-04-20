using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    using SURROUNDING_POINT = TH_RefreshUrgData.TH_DATA.UrgPoint;

    class AST_TurnL
    {
        private static ProcessStateInfo PSI;
        private struct ProcessStateInfo
        {
            public bool DoingSubAction;
            public int No_SubAction;
            
            public bool PermitTurn;
            public double disRemain;

            public AutoSearchTrack.PD_PARAMETER PD_X, PD_Y, PD_A;
            public TH_MeasurePosition.TH_DATA.POSITION pBG,pED;
        }

        public static void Start()
        {
            #region 初始化设置

            PSI.DoingSubAction = false;
            PSI.No_SubAction = 0;
            PSI.disRemain = 100;
            
            #endregion
            
            #region 判断能否进行转弯动作

            PSI.No_SubAction++;
            if (AutoSearchTrack.control.SubAction <= PSI.No_SubAction) { PSI.PermitTurn = permitThrough(); }

            #endregion

            #region 转弯过程

            PSI.DoingSubAction = true;
            PSI.No_SubAction++;

            while (PSI.DoingSubAction && PSI.PermitTurn)
            {
                // 是否执行本子动作
                if (AutoSearchTrack.control.SubAction > PSI.No_SubAction) { PSI.DoingSubAction = false; break; }

                // 紧急动作
                if (AutoSearchTrack.control.EMA) { return; }

                // 判断退出条件
                PSI.pED = AutoSearchTrack.getPosition();
                if (PSI.pED.a - PSI.pBG.a < 5) { PSI.DoingSubAction = false; break; }

                // 获取控制
                int ForwardSpeed = getForwardSpeed(AutoSearchTrack.control.MaxSpeed_Forward, ref PSI.PD_X);
                int TranslateSpeed = getTranslateSpeed(0, ref PSI.PD_Y);
                int RotateSpeed = getRotateSpeed(0, ref PSI.PD_A);

                TH_SendCommand.AGV_MoveControl_0x70(ForwardSpeed, TranslateSpeed, RotateSpeed);
            }

            #endregion

            #region 转弯完成，确认对准通道入口

            PSI.DoingSubAction = true;
            PSI.No_SubAction++;

            while (PSI.DoingSubAction)
            {
                // 是否执行本子动作
                if (AutoSearchTrack.control.SubAction > PSI.No_SubAction) { PSI.DoingSubAction = false; break; }

                // 紧急动作
                if (AutoSearchTrack.control.EMA) { return; }

                // 判断退出条件
                double disEmpty = WareHouseMap.DefaultAisleWidth_Row;
                bool EmptyH = AutoSearchTrack.IsEmpty_Y(75, 105, disEmpty);
                if (EmptyH) { PSI.DoingSubAction = false; break; }

                // 寻找通道入口
                bool EmptyL = AutoSearchTrack.IsEmpty_X(190, 220, disEmpty);
                if (!EmptyL)
                {
                    AutoSearchTrack.control.EMA = true;
                    AutoSearchTrack.control.NextAction = AutoSearchTrack.Action.Error; return;
                }

                TH_SendCommand.AGV_MoveControl_0x70(0, AutoSearchTrack.control.MaxSpeed_Translate, 0);
            }

            #endregion

            #region 填充下一个动作

            if (AutoSearchTrack.control.ActionList.Count == 0)
            {
                if (!PSI.PermitTurn) { AutoSearchTrack.control.ActionList.Add(AutoSearchTrack.Action.Backward); return; }

                bool Upward = WareHouseMap.Upward(AutoSearchTrack.control.StackL) ||
                    WareHouseMap.Upward(AutoSearchTrack.control.StackR);
                if (Upward) { AutoSearchTrack.control.ActionList.Add(AutoSearchTrack.Action.Upward); return; }
                AutoSearchTrack.control.ActionList.Add(AutoSearchTrack.Action.Downward);
            }

            #endregion
        }

        private static double getAisleWidth1()
        {
            double yBG = HardwareConfig.PlatForm.ySpan / 2;
            double yED = HardwareConfig.PlatForm.ySpan;
            List<SURROUNDING_POINT> points = AutoSearchTrack.getPoints_Y(yBG, yED, AutoSearchTrack.SurroundingPoints);
            
            double minL = double.MaxValue, minR = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                double x = points[i].x;
                if (x < 0 && -x < minL) { minL = -x; continue; }
                if (x > 0 && x < minR) { minR = x; continue; }
            }

            return minL + minR;
        }
        private static double getAisleWidth2()
        {
            // 取点
            List<SURROUNDING_POINT> points = AutoSearchTrack.getPoints_A(90, 180, AutoSearchTrack.SurroundingPoints);

            // 点的数量不够，则能进行转弯。
            if (points.Count <= 3)
            {
                return HardwareConfig.PlatForm.xSpan + HardwareConfig.PlatForm.ySpan + 2 * PSI.disRemain;
            }

            // 绝对化
            for (int i = 0; i < points.Count; i++)
            {
                SURROUNDING_POINT point = points[i];
                point.y = Math.Abs(point.y);
                points[i] = point;
            }

            // 对 Y 坐标进行排序
            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (points[j].y <= points[j + 1].y) { continue; }
                    SURROUNDING_POINT temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            // 首端距离
            double HeadGap = points[0].y;

            // 获取首尾两端的距离
            double TailGap = 0;
            if (Math.Abs(points[points.Count - 1].a - 90) > 10)
            {
                TailGap = HardwareConfig.Urg.max - points[points.Count - 1].y;
            }

            // 获取中间的最大间隙
            double MiddleGap = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double iGap = Math.Abs(points[i].y - points[i + 1].y);
                if (iGap > MiddleGap) { MiddleGap = iGap; }
            }

            // 返回最大间隙
            return Math.Max(MiddleGap, Math.Max(HeadGap, TailGap));
        }
        private static bool permitThrough()
        {
            double carL = HardwareConfig.PlatForm.ySpan + PSI.disRemain;
            double carW = HardwareConfig.PlatForm.xSpan + PSI.disRemain;

            double w1 = getAisleWidth1();
            double w2 = getAisleWidth2();

            if (w1 >= Math.Sqrt(carL * carL + carW * carW) && w2 >= carW) { return true; }
            
            double temp = carL - Math.Sqrt(w1 * w1 - carW * carW);
            double reqW = Math.Sqrt(carW * carW + temp * temp);

            return w2 >= reqW;
        }

        public static int getForwardSpeed(int keepSpeed, ref AutoSearchTrack.PD_PARAMETER PD, double target = double.NaN)
        {
            // 取点
            List<SURROUNDING_POINT> pointsH = new List<TH_RefreshUrgData.TH_DATA.UrgPoint>();
            pointsH = AutoSearchTrack.getPoints_A(75, 105);

            if (pointsH.Count == 0) { return keepSpeed; }

            // 换成绝对距离
            for (int i = 0; i < pointsH.Count; i++)
            { SURROUNDING_POINT point = pointsH[i]; point.y = Math.Abs(point.y); pointsH[i] = point; }

            // 获取最小距离
            double minH = double.MaxValue;
            for (int i = 0; i < pointsH.Count; i++)
            {
                if (pointsH[i].y < 0) { continue; }
                if (minH < pointsH[i].y) { minH = pointsH[i].y; }
            }
            if (minH == double.MaxValue) { return keepSpeed; }

            // PD 控制
            double current = minH;
            if (!double.IsNaN(target)) { current = AutoSearchTrack.getPosition().y; }
            if (double.IsNaN(target)) { target = AutoSearchTrack.control.MinDistance_H; }

            double adjust = AutoSearchTrack.PDcontroller(current, target, ref PD);
            int ForwardSpeed = (int)(adjust / TH_SendCommand.TH_data.TimeForControl);

            // 限速
            if (minH < AutoSearchTrack.control.MinDistance_H) { return 0; }
            if (ForwardSpeed > AutoSearchTrack.control.MaxSpeed_Forward) { return AutoSearchTrack.control.MaxSpeed_Forward; }
            if (ForwardSpeed < -AutoSearchTrack.control.MaxSpeed_Forward) { return -AutoSearchTrack.control.MaxSpeed_Forward; }

            return ForwardSpeed;
        }
        public static int getTranslateSpeed(int keepSpeed, ref AutoSearchTrack.PD_PARAMETER PD, double target = double.NaN)
        {
            // 取点
            List<SURROUNDING_POINT> points = AutoSearchTrack.getPoints_Y(-HardwareConfig.ySpan_UltraSonic, 0);
            List<SURROUNDING_POINT> pointsL = AutoSearchTrack.getPoints_A(90, 270, points);
            List<SURROUNDING_POINT> pointsR = AutoSearchTrack.getPoints_A(-90, 90, points);

            // 取最近距离
            double minL = double.MaxValue, minR = double.MaxValue;
            for (int i = 0; i < pointsL.Count; i++)
            {
                double ix = Math.Abs(pointsL[i].x); if (ix < minL) { minL = ix; }
            }
            for (int i = 0; i < pointsR.Count; i++)
            {
                double ix = Math.Abs(pointsR[i].x); if (ix < minR) { minR = ix; }
            }
            if (minL == double.MaxValue && minR == double.MaxValue) { return keepSpeed; }

            // 把上下距离换算成左右距离
            double KeepDistance_L = 0.0, KeepDistance_R = 0.0;
            if (AutoSearchTrack.control.Direction == AutoSearchTrack.Direction.Left)
            {
                KeepDistance_L = AutoSearchTrack.control.SetKeepDistance_D;
                KeepDistance_R = AutoSearchTrack.control.SetKeepDistance_U;
            }
            if (AutoSearchTrack.control.Direction == AutoSearchTrack.Direction.Right)
            {
                KeepDistance_L = AutoSearchTrack.control.SetKeepDistance_U;
                KeepDistance_R = AutoSearchTrack.control.SetKeepDistance_D;
            }

            // PD控制
            bool KeepC = AutoSearchTrack.control.KeepCentre && minL != double.MaxValue && minR != double.MaxValue;
            bool KeepL = !KeepC && minL != double.MaxValue && KeepDistance_L != 0;
            bool KeepR = !KeepC && minR != double.MaxValue && KeepDistance_R != 0;

            if (!KeepC && !KeepL && !KeepR && minL != double.MaxValue)
            { KeepL = true; KeepDistance_L = AutoSearchTrack.control.DefaultKeepDistance; }
            if (!KeepC && !KeepL && !KeepR && minR != double.MaxValue)
            { KeepR = true; KeepDistance_R = AutoSearchTrack.control.DefaultKeepDistance; }

            double current = 0.0;
            if (KeepC) { current = minL; }
            if (KeepL) { current = minL; }
            if (KeepR) { current = minR; }
            if (!double.IsNaN(target)) { current = AutoSearchTrack.getPosition().x; KeepC = false; KeepL = true; KeepR = false; }

            if (KeepC && double.IsNaN(target)) { target = (minL + minR) / 2; }
            if (KeepL && double.IsNaN(target)) { target = KeepDistance_L; }
            if (KeepR && double.IsNaN(target)) { target = KeepDistance_R; }

            double adjust = AutoSearchTrack.PDcontroller(current, target, ref PD);
            int TranslateSpeed = KeepR ?
                (int)(adjust / TH_SendCommand.TH_data.TimeForControl) :
                (int)(-adjust / TH_SendCommand.TH_data.TimeForControl);

            // 限速
            if (minL < AutoSearchTrack.control.MinDistance_L && TranslateSpeed < 0) { TranslateSpeed = 0; }
            if (minR < AutoSearchTrack.control.MinDistance_R && TranslateSpeed > 0) { TranslateSpeed = 0; }

            if (TranslateSpeed > AutoSearchTrack.control.MaxSpeed_Translate) { return AutoSearchTrack.control.MaxSpeed_Translate; }
            if (TranslateSpeed < -AutoSearchTrack.control.MaxSpeed_Translate) { return -AutoSearchTrack.control.MaxSpeed_Translate; }
            return TranslateSpeed;
        }
        public static int getRotateSpeed(int keepSpeed, ref AutoSearchTrack.PD_PARAMETER PD, double target = double.NaN)
        {
            int RotateSpeed = keepSpeed;

            // PD 控制
            if (!double.IsNaN(target))
            {
                double current = AutoSearchTrack.getPosition().a;
                double adjust = AutoSearchTrack.PDcontroller(current, target, ref PSI.PD_A);
                RotateSpeed = (int)(adjust / TH_SendCommand.TH_data.TimeForControl);
            }

            // 判断是否允许旋转
            while (TH_SendCommand.TH_data.IsSetting) ;
            TH_SendCommand.TH_data.IsGetting = true;

            int Head_L_X = TH_SendCommand.TH_data.Head_L_X;
            int Head_L_Y = TH_SendCommand.TH_data.Head_L_Y;
            int Head_R_X = TH_SendCommand.TH_data.Head_R_X;
            int Head_R_Y = TH_SendCommand.TH_data.Head_R_Y;
            int Tail_L_X = TH_SendCommand.TH_data.Tail_L_X;
            int Tail_L_Y = TH_SendCommand.TH_data.Tail_L_Y;
            int Tail_R_X = TH_SendCommand.TH_data.Tail_R_X;
            int Tail_R_Y = TH_SendCommand.TH_data.Tail_R_Y;

            TH_SendCommand.TH_data.IsGetting = false;

            if (Head_L_X < AutoSearchTrack.control.MinDistance_L) { return 0; }
            if (Head_L_Y < AutoSearchTrack.control.MinDistance_H) { return 0; }
            if (Head_R_X < AutoSearchTrack.control.MinDistance_R) { return 0; }
            if (Head_R_Y < AutoSearchTrack.control.MinDistance_H) { return 0; }
            if (Tail_L_X < AutoSearchTrack.control.MinDistance_L) { return 0; }
            if (Tail_L_Y < AutoSearchTrack.control.MinDistance_T) { return 0; }
            if (Tail_R_X < AutoSearchTrack.control.MinDistance_R) { return 0; }
            if (Tail_R_Y < AutoSearchTrack.control.MinDistance_T) { return 0; }

            // 限速
            if (RotateSpeed > AutoSearchTrack.control.MaxSpeed_Rotate) { return AutoSearchTrack.control.MaxSpeed_Rotate; }
            if (RotateSpeed < -AutoSearchTrack.control.MaxSpeed_Rotate) { return -AutoSearchTrack.control.MaxSpeed_Rotate; }
            return RotateSpeed;
        }

        private static List<SURROUNDING_POINT> SortPoints(List<SURROUNDING_POINT> points)
        {
            // 点的数量不够，直接返回。
            if (points.Count <= 3) { return points; }

            // 距离排序
            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - i; j++)
                {
                    if (points[j].y <= points[j + 1].y) { continue; }

                    TH_RefreshUrgData.TH_DATA.UrgPoint temp = new TH_RefreshUrgData.TH_DATA.UrgPoint();
                    temp = points[j];
                    points[j] = points[j + 1];
                    points[j + 1] = temp;
                }
            }

            // 选取距离
            //int indexofcut = points.Count;
            //for (int i = 0; i < points.Count - 1; i++)
            //{
            //    if (points[i + 1].y - points[i].y > 200) { indexofcut = i + 1; break; }
            //}
            //points.RemoveRange(indexofcut, points.Count - indexofcut);

            return points;
        }
        private static List<SURROUNDING_POINT> getFitPoints(List<SURROUNDING_POINT> points)
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
            double targetAngle = x0 < HardwareConfig.ForeSight ? MaxAngle : MinAngle;
            angles[closest] = targetAngle;

            // 取出斜率符合的点
            List<TH_RefreshUrgData.TH_DATA.UrgPoint> fitpoints = new List<TH_RefreshUrgData.TH_DATA.UrgPoint>();
            for (int i = 1; i < angles.Count; i++)
            {
                if (Math.Abs(angles[i] - targetAngle) < 1.0) { fitpoints.Add(points[i]); continue; }
                if (Math.Abs(angles[i] + targetAngle - 180) < 1.0) { fitpoints.Add(points[i]); }
            }

            // 返回
            return fitpoints;
        }
        private static double[] getFitLine(List<SURROUNDING_POINT> points)
        {
            // 点数量不够
            if (points.Count <= 3) { return new double[3] { 0, 0, 0 }; }

            // 拟合直线
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
            double UrgK = (N * sumXY - sumX * sumY) / denominator;
            double UrgB = (sumXX * sumY - sumX * sumXY) / denominator;

            double UrgA = Math.Atan(UrgK) * 180 / Math.PI;
            return new double[3] { UrgK, UrgA, UrgB };
        }
    }

    class AST_TurnR
    {

    }
}
