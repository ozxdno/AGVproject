using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 通道入口处校准
    /// </summary>
    class AST_AlignAisle
    {
        /// <summary>
        /// 已达 X 方向校准极限
        /// </summary>
        public static bool ApproachX;
        /// <summary>
        /// 已达 Y 方向校准极限
        /// </summary>
        public static bool ApproachY;
        /// <summary>
        /// 已达 A 方向校准极限
        /// </summary>
        public static bool ApproachA;

        /// <summary>
        /// 开始寻找并校准通道入口
        /// </summary>
        /// <param name="keepDistanceH">维持与通道口的前方距离 单位：mm</param>
        /// <param name="keepDistanceR">维持与通道口的右边距离（小于 0 为左边）单位：mm</param>
        /// <param name="searchLeft">向左搜寻通道入口</param>
        public static void Start(double keepDistanceH, double keepDistanceR, bool searchLeft = false)
        {
            ApproachY = false;
            while (!ApproachY)
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }
                int ySpeed = getSpeedY(keepDistanceH);
                TH_SendCommand.AGV_MoveControl_0x70(0, ySpeed, 0);
            }

            ApproachX = false;
            while (!ApproachX)
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }
                int xSpeed = getSpeedX(keepDistanceR, searchLeft);
                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, 0, 0);
            }

            ApproachA = false;
            while (!ApproachA)
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }
                int aSpeed = getSpeedA(keepDistanceR < 0);
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }
        }

        /// <summary>
        /// 获取对准通道时的 X 方向速度，必须先满足 Y 方向距离条件
        /// </summary>
        /// <param name="keepDistance">维持距离，维持右边距为正，单位：mm</param>
        /// <param name="left">小车 X 轴平移方向</param>
        /// <returns></returns>
        public static int getSpeedX(double keepDistance, bool left = false)
        {
            // 取点
            List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(45, 135);

            // 去掉 Y 方向跨度过大的点
            double minY = CoordinatePoint.MinY(pointsH);
            pointsH = CoordinatePoint.SelectY(0, minY + Hardware_PlatForm.Width, pointsH);

            // 点数量不够
            if (pointsH.Count <= 3) { return 0; }

            // 对 X 坐标从小到大排序
            pointsH = CoordinatePoint.SortX(pointsH);

            // 获取最大间隙
            double Gap = 0, L = 0, R = 0;
            int indexL = -1, indexR = -1;
            CoordinatePoint.MaxGapX(pointsH, ref Gap, ref indexL, ref indexR);
            L = Math.Abs(pointsH[indexL].x);
            R = Math.Abs(pointsH[indexR].x);

            // 判断间隙是否足够大
            if (Gap < Hardware_PlatForm.Width)
            {
                return AST_GuideBySpeed.getSpeedX(TH_AutoSearchTrack.control.MaxSpeed_X);
            }
            
            // 获取控制
            double current = 0, target = 0;
            double Kp = 0.5;
            if (keepDistance > 0) { current = R; target = keepDistance; }
            if (keepDistance < 0) { current = L; target = -keepDistance; }

            int xSpeed = (int)(Kp * (current - target));
            if (keepDistance < 0) { xSpeed = -xSpeed; }

            // 是否已经满足退出条件
            if (Math.Abs(current - target) < 20) { ApproachX = true; }

            // 限速
            if (xSpeed > TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = TH_AutoSearchTrack.control.MaxSpeed_X; }
            if (xSpeed < -TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = -TH_AutoSearchTrack.control.MaxSpeed_X; }
            return -xSpeed;
        }
        /// <summary>
        /// 获取对准通道时的 Y 方向速度
        /// </summary>
        /// <param name="keepDistance">维持与通道口的 Y 方向距离 单位：mm</param>
        /// <returns></returns>
        public static int getSpeedY(double keepDistance)
        {
            // 取点
            List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(45, 135);

            // 最近距离
            double minH = CoordinatePoint.MinY(pointsH);

            // 获取控制
            double current = minH;
            double target = keepDistance;
            double Kp = 1;

            double adjust = Kp * (current - target);

            // 到达目标点
            ApproachY = Math.Abs(current - target) < 20;

            // 防撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        /// <summary>
        /// 获取对准通道时的 A 方向速度，必须满足 X 方向距离条件
        /// </summary>
        /// <param name="keepleft">与左边平行</param>
        /// <returns></returns>
        public static int getSpeedA(bool keepleft = false)
        {
            AST_GuideBySurrounding.ApproachA = false;
            int aSpeed = 0;

            if (keepleft) { aSpeed = AST_GuideBySurrounding.getSpeedA_KeepL_Forward(); }
            else { aSpeed = AST_GuideBySurrounding.getSpeedA_KeepR_Forward(); }

            ApproachA = AST_GuideBySurrounding.ApproachA;
            return aSpeed;
        }
    }
}
