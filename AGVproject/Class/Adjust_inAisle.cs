using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class Adjust_inAisle
    {
        ////////////////////////////////////////// public attribute ///////////////////////////////////////////////

        public ADJUST_CONFIG config;

        public struct ADJUST_CONFIG
        {
            public double TrackLength;
            public double Error_A;

            public bool KeepL;
            public bool KeepR;
            public bool KeepCentre;

            public double CurrentL;
            public double CurrentR;
            public double TargetL;
            public double TargetR;

            public double PreviousL;
            public double PreviousR;
        }

        ////////////////////////////////////////// private attribute ///////////////////////////////////////////////

        private PID_PARAMETER PID_parameter;

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

        ////////////////////////////////////////// public method ///////////////////////////////////////////////

        public void Start(int xSpeed, KeyPoint point, TH_SendCommand TH_command)
        {
            // 目标信息
            config.TargetL = point.UrgL;
            config.TargetR = point.UrgR;

            while (true)
            {
                // 判断结束信息

                // 获取两侧距离
                GetSideDistance();

                // 按要求调整(要考虑没收到数据)
                if (config.KeepL)
                {
                    double current = Math.Atan(config.CurrentL - config.PreviousL) / (xSpeed * TH_SendCommand.TH_data.TimeForControl / 1000);
                    double target = Math.Atan((config.CurrentL * Math.Cos(current) - config.TargetL) / config.TrackLength);

                    if (Math.Abs(current - target) < config.Error_A) { return; }

                    double adjust = PIDcontroller1(current, target);
                    int aSpeed = (int)(adjust * 100);

                    TH_command.AGV_MoveControl_0x70(xSpeed, 0, aSpeed);
                }
            }
        }

        ////////////////////////////////////////// private method ///////////////////////////////////////////////

        private void GetSideDistance()
        {
            // 取数据
            List<double> X = new List<double>();
            TH_RefreshUrgData.TH_data.IsGetting = true;
            while (TH_RefreshUrgData.TH_data.IsSetting) ;
            foreach (double iX in TH_RefreshUrgData.TH_data.x) { X.Add(iX); }
            TH_RefreshUrgData.TH_data.IsGetting = false;

            // 分成大于0和小于0两组，取最小距离
            double MeasureL = double.MaxValue, MeasureR = double.MaxValue;
            foreach (double iX in X)
            {
                if (iX > 0 && iX < MeasureR) { MeasureR = iX; }
                if (iX < 0 && -iX > MeasureL) { MeasureL = -iX; }
            }

            config.PreviousL = config.CurrentL;
            config.PreviousR = config.CurrentR;
            config.CurrentL = MeasureL;
            config.CurrentR = MeasureR;
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
