using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class CorrectKeyPosition
    {
        ////////////////////////////////////////// public attribute  ///////////////////////////////////////////////

        /// <summary>
        /// 当前位置的小车超声波数据和墙壁数据
        /// </summary>
        public KeyPoint currPoint;
        public struct KeyPoint
        {
            /// <summary>
            /// AGV 小车的 Tail_L_Y 号超声波测得数据
            /// </summary>
            public int UltraSonicL;
            /// <summary>
            /// AGV 小车的 Tail_R_Y 号超声波测得数据
            /// </summary>
            public int UltraSonicR;
            /// <summary>
            /// 前方墙壁直线拟合后的斜率
            /// </summary>
            public double UrgK;
            /// <summary>
            /// 前方墙壁直线拟合后的截距
            /// </summary>
            public double UrgB;

            /// <summary>
            /// 起始点旁边墙壁直线拟合后的斜率
            /// </summary>
            public double StartExtraK;
            /// <summary>
            /// 起始点旁边墙壁直线拟合后的截距
            /// </summary>
            public double StartExtraB;
        }

        ////////////////////////////////////////// private attribute ///////////////////////////////////////////////
        
        private KeyPoint prevPoint;

        private List<UrgPoint> urgPoints;
        private List<List<UrgPoint>> urgGroups;
        private PID_PARAMETER PID_parameter;
        private CONTROL_PARAMETER CON_parameter;

        private struct UrgPoint { public double X; public double Y; }
        private struct PID_PARAMETER
        {
            public double Kp;
            public double Ki;
            public double Kd;

            public double Error2;
            public double Error1;
            public double Error0;

            public double SumError;
        }
        private struct CONTROL_PARAMETER
        {
            public double Fit_Error; // 拟合允许误差
            public double Fit_Percent; // 要求拟合数据的靠近程度

            public double A_Error; // 角度调整允许误差 单位：0.01 度
            public double X_Error; // X 轴调整允许误差 单位：mm
            public double Y_Error; // Y 轴调整允许误差 单位：mm

            public int TimeFor_0x86; // 获取超声波数据时预留时间
            public int TimeFor_0x70; // 响应控制命令预留时间
            public int TimeFor_URG; // 获取激光雷达数据预留时间
            public int TimeFor_process;
            public int TimeFor_control;
        }

        ////////////////////////////////////////// public method    ////////////////////////////////////////////////

        /// <summary>
        /// 对小车当前位置进行校准。
        /// </summary>
        /// <param name="controlport">控制口</param>
        /// <param name="urgport">激光雷达口</param>
        /// <param name="point">关键点</param>
        public void Start(ControlPort controlport, UrgPort urgport, KeyPoint point)
        {
            prevPoint = point;

            // 粗调
            CON_parameter.Fit_Percent = 0.05;
            CON_parameter.A_Error = 50;
            CON_parameter.X_Error = 10;
            CON_parameter.Y_Error = 10;
            CON_parameter.TimeFor_0x86 = 100;
            CON_parameter.TimeFor_URG = 10;
            CON_parameter.TimeFor_process = 2;
            CON_parameter.TimeFor_0x70 = 50 - CON_parameter.TimeFor_URG - CON_parameter.TimeFor_process;

            Adjust_A(controlport, urgport);
            Adjust_X(controlport, urgport);
            Adjust_Y(controlport, urgport);

            // 细调
            CON_parameter.Fit_Percent = 0.02;
            CON_parameter.A_Error = 20;
            CON_parameter.X_Error = 5;
            CON_parameter.Y_Error = 5;
            CON_parameter.TimeFor_0x86 = 100;
            CON_parameter.TimeFor_URG = 10;
            CON_parameter.TimeFor_process = 2;
            CON_parameter.TimeFor_0x70 = 50 - CON_parameter.TimeFor_URG - CON_parameter.TimeFor_process;

            Adjust_A(controlport, urgport);
            Adjust_X(controlport, urgport);
            Adjust_Y(controlport, urgport);
        }

        /// <summary>
        /// 得到当前位置的超声波和墙壁信息，数据在 currPoint 中。
        /// </summary>
        /// <param name="controlport">控制口</param>
        /// <param name="urgport">激光雷达口</param>
        /// <returns></returns>
        public bool GetCurrentPoint(ControlPort controlport,UrgPort urgport)
        {
            CON_parameter.Fit_Percent = 0.01;
            CON_parameter.TimeFor_0x86 = 100;

            urgport.GetUrgData();
            //if (!GetSonicData(controlport)) { return false; }
            System.Threading.Thread.Sleep(100);

            currPoint.UltraSonicL = (controlport.UltraSonic.Head_L_Y + controlport.UltraSonic.Tail_L_Y) / 2;
            currPoint.UltraSonicR = (controlport.UltraSonic.Head_R_Y + controlport.UltraSonic.Tail_R_Y) / 2;

            return GetUrgData(urgport);
        }

        /// <summary>
        /// 对 AGV 小车的起始点进行校准。
        /// </summary>
        /// <param name="controlport">控制口</param>
        /// <param name="urgport">激光雷达口</param>
        /// <param name="point">起始关键点</param>
        public void Correct_1st(ControlPort controlport, UrgPort urgport, KeyPoint point)
        {

        }

        ////////////////////////////////////////// private method   ////////////////////////////////////////////////

        private bool GetSonicData(ControlPort controlport)
        {
            if (!controlport.GetUltraSonicData_0x86()) { return false; }
            System.Threading.Thread.Sleep(CON_parameter.TimeFor_0x86);
            if (controlport.Receiving) { return false; }

            currPoint.UltraSonicL = controlport.UltraSonic.Tail_L_Y;
            currPoint.UltraSonicR = controlport.UltraSonic.Tail_R_Y;

            return true;
        }
        private bool GetUrgData(UrgPort urgport)
        {
            // 获取原始数据
            if (!urgport.GetUrgData()) { return false; }
            urgport.MidFilter();
            
            // 获取拟合误差
            double sumDistance = 0;
            double N = 0;
            for (int i = 0; i < urgport.urgData.distance.Count; i++)
            {
                if (urgport.urgData.distance[i] == 0) { continue; }
                N++;
                sumDistance += urgport.urgData.distance[i];
            }
            CON_parameter.Fit_Error = sumDistance / N * CON_parameter.Fit_Percent;

            // 分割与拟合
            Pole2Rectangular(urgport.urgData.distance,urgport.urgData.StartAngle,urgport.urgData.AnglePace);
            urgGroups = new List<List<UrgPoint>>();
            CutGroup_UrgPoint(urgPoints);
            List<UrgPoint> linePoints = GetGroup_UrgPoint();
            Fit_UrgPoint(linePoints);

            return true;
        }

        private void Pole2Rectangular(List<long> polePoints, double angelStart, double anglePace)
        {
            urgPoints = new List<UrgPoint>();

            for (int i = 0; i < polePoints.Count; i++)
            {
                if (polePoints[i] == 0) { continue; }

                double angle = (angelStart + anglePace * i) * Math.PI / 180;
                UrgPoint urgPoint;

                urgPoint.X = polePoints[i] * Math.Cos(angle);
                urgPoint.Y = polePoints[i] * Math.Sin(angle);

                urgPoints.Add(urgPoint);
            }

            return;

            // 按X排序，由大到小
            for (int i = 1; i < urgPoints.Count; i++)
            {
                for (int j = 0; j < urgPoints.Count - i; j++)
                {
                    if (urgPoints[j].X >= urgPoints[j + 1].X) { continue; }

                    UrgPoint Temp_urgPoint = new UrgPoint();
                    Temp_urgPoint = urgPoints[j];
                    urgPoints[j] = urgPoints[j + 1];
                    urgPoints[j + 1] = Temp_urgPoint;
                }
            }
        }
        private void CutGroup_UrgPoint(List<UrgPoint> points)
        {
            // 点的数量不够
            if (points.Count == 0) { return; }
            if (points.Count == 1 || points.Count == 2) { urgGroups.Add(points); return; }

            // 基本参数
            double MaxDis = 0.0;
            int indexofmax = 0;

            // 直线参数
            double x1 = points[0].X, y1 = points[0].Y;
            double x2 = points[points.Count - 1].X, y2 = points[points.Count - 1].Y;

            double A = y2 - y1, B = -(x2 - x1), C = (x2 - x1) * y1 - (y2 - y1) * x1;

            // 寻找最大距离
            for (int i = 0; i < points.Count; i++)
            {
                double iDis = (A * points[i].X + B * points[i].Y + C) / Math.Sqrt(A * A + B * B);
                if (MaxDis > iDis) { continue; }

                indexofmax = i; MaxDis = iDis;
            }

            // 分割直线
            if (MaxDis <= CON_parameter.Fit_Error) { urgGroups.Add(points); return;  }

            List<UrgPoint> newLine = new List<UrgPoint>();
            for (int i = 0; i <= indexofmax; i++) { newLine.Add(points[i]); }
            CutGroup_UrgPoint(newLine);

            newLine = new List<UrgPoint>();
            for (int i = indexofmax; i < points.Count; i++) { newLine.Add(points[i]); }
            CutGroup_UrgPoint(newLine);
        }
        private List<UrgPoint> GetGroup_UrgPoint()
        {
            // 挑选直线
            for (int i = 0; i < urgGroups.Count; i++)
            {
                double bgX = urgGroups[i][0].X;
                double edX = urgGroups[i][urgGroups[i].Count - 1].X;

                if (bgX > 0 && edX < 0) { return urgGroups[i]; }
                if (bgX < 0 && edX > 0) { return urgGroups[i]; }

                if (bgX == 0)
                {
                    if (i == 0) { return urgGroups[i]; }
                    if (urgGroups[i].Count >= urgGroups[i - 1].Count) { return urgGroups[i]; }
                    return urgGroups[i - 1];
                }
                if (edX == 0)
                {
                    if (i == urgGroups.Count) { return urgGroups[i]; }
                    if (urgGroups[i].Count >= urgGroups[i + 1].Count) { return urgGroups[i]; }
                    return urgGroups[i + 1];
                }
            }
            return urgGroups[0];
        }
        private void Fit_UrgPoint(List<UrgPoint> points)
        {
            double sumX = 0,sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0;
            int N = points.Count;

            for (int i = 0; i < N; i++)
            {
                sumX += points[i].X;
                sumY += points[i].Y;

                sumXX += points[i].X * points[i].X;
                sumXY += points[i].X * points[i].Y;
                sumYY += points[i].Y * points[i].Y;
            }

            double denominator = N * sumXX - sumX * sumX;
            if (denominator == 0) { denominator = 0.01; }

            currPoint.UrgK = (N * sumXY - sumX * sumY) / denominator;
            currPoint.UrgB = (sumXX * sumY - sumX * sumXY) / denominator;

            currPoint.UrgK = Math.Atan(currPoint.UrgK) * 180 / Math.PI;
        }

        private void Adjust_A(ControlPort controlport, UrgPort urgport)
        {
            urgport.GetUrgData();
            System.Threading.Thread.Sleep(100);
            while (!GetUrgData(urgport)) ;

            CON_parameter.TimeFor_control = CON_parameter.TimeFor_0x70 + CON_parameter.TimeFor_URG + CON_parameter.TimeFor_process;
            Initial_PID_parameter(0.0, 0.0, 0.0);

            while (true)
            {
                double current = Math.Atan(currPoint.UrgK) * 18000 / Math.PI;
                double target = Math.Atan(prevPoint.UrgK) * 18000 / Math.PI;

                if (Math.Abs(current - target) <= CON_parameter.A_Error) { return; }

                double adjustA = PIDcontroller1(current, target);
                int aSpeed = (int)(adjustA / CON_parameter.TimeFor_control);
                if (aSpeed == 0) { return; }

                //controlport.MoveControl_0x70(0, 0, aSpeed);
                urgport.GetUrgData();
                System.Threading.Thread.Sleep(CON_parameter.TimeFor_0x70);
                
                while (!GetUrgData(urgport)) ;
            }
        }
        private void Adjust_X(ControlPort controlport, UrgPort urgport)
        {
            // 沿用调整 A 过程时的数据
            // while (!GetUrgData(urgport)) ;

            CON_parameter.TimeFor_control = CON_parameter.TimeFor_0x70 + CON_parameter.TimeFor_URG + CON_parameter.TimeFor_process;
            Initial_PID_parameter(0.0, 0.0, 0.0);

            while (true)
            {
                double current = currPoint.UrgB;
                double target = prevPoint.UrgB;

                if (Math.Abs(current - target) < CON_parameter.X_Error) { return; }

                double adjustX = PIDcontroller1(current, target);
                int xSpeed = (int)(adjustX / CON_parameter.TimeFor_control);
                if (xSpeed == 0) { return; }

                urgport.GetUrgData();
                //controlport.MoveControl_0x70(xSpeed, 0, 0);
                System.Threading.Thread.Sleep(CON_parameter.TimeFor_0x70);
                
                while (!GetUrgData(urgport)) ;
            }
        }
        private void Adjust_Y(ControlPort controlport, UrgPort urgport)
        {
            while (!GetSonicData(controlport)) ;

            CON_parameter.TimeFor_control = CON_parameter.TimeFor_0x70 + CON_parameter.TimeFor_0x86 + CON_parameter.TimeFor_process;
            Initial_PID_parameter(0.0, 0.0, 0.0);

            bool NearLeft = prevPoint.UltraSonicL >= prevPoint.UltraSonicR;
            while (true)
            {
                double current = NearLeft ? currPoint.UltraSonicL : currPoint.UltraSonicR;
                double target = NearLeft ? prevPoint.UltraSonicL : prevPoint.UltraSonicR;

                if (Math.Abs(current - target) < CON_parameter.Y_Error) { return; }

                double adjustY = PIDcontroller1(current, target);
                int ySpeed = (int)(adjustY / (CON_parameter.TimeFor_control));
                if (ySpeed == 0) { return; }

                //controlport.MoveControl_0x70(ySpeed, 0, 0);
                System.Threading.Thread.Sleep(CON_parameter.TimeFor_0x70);

                while (!GetSonicData(controlport)) ;
            }
        }

        private void Initial_PID_parameter(double Kp, double Ki, double Kd)
        {
            PID_parameter.Kp = Kp;
            PID_parameter.Ki = Ki;
            PID_parameter.Kd = Kd;

            PID_parameter.Error0 = 0;
            PID_parameter.Error1 = 0;
            PID_parameter.Error2 = 0;

            PID_parameter.SumError = 0;
        }
        private double PIDcontroller1(double current, double target) // 位置式
        {
            PID_parameter.Error2 = PID_parameter.Error1;
            PID_parameter.Error1 = PID_parameter.Error0;
            PID_parameter.Error0 = current - target;

            PID_parameter.SumError += PID_parameter.Error0;

            double pControl = PID_parameter.Kp * PID_parameter.Error0;
            double iControl = PID_parameter.Ki * PID_parameter.SumError;
            double dControl = PID_parameter.Kd * (PID_parameter.Error0 - PID_parameter.Error1);

            return -(pControl + iControl + dControl);
        }
        private double PIDcontroller2(double current, double target) // 增量式
        {
            PID_parameter.Error2 = PID_parameter.Error1;
            PID_parameter.Error1 = PID_parameter.Error0;
            PID_parameter.Error0 = current - target;

            double pControl = PID_parameter.Kp * (PID_parameter.Error0 - PID_parameter.Error1);
            double iControl = PID_parameter.Ki * PID_parameter.Error0;
            double dControl = PID_parameter.Kd * (PID_parameter.Error0 - 2 * PID_parameter.Error1 + PID_parameter.Error2);

            return -(pControl + iControl + dControl);
        }
    }
}
