using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 位置导航
    /// </summary>
    class AST_GuideByPosition
    {
        /// <summary>
        /// 调节起点
        /// </summary>
        public static CoordinatePoint.POINT StartPosition;
        /// <summary>
        /// 调节终点
        /// </summary>
        public static CoordinatePoint.POINT TargetPosition;
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
        /// 向左 / 向右 行进一段距离（向左为负）
        /// </summary>
        /// <param name="xMove">右移距离 单位：mm</param>
        /// <returns></returns>
        public static int getSpeedX(double xMove)
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().x - StartPosition.x;
            double target = xMove;
            double Kp = 1;
            double adjust = -Kp * (current - target);
            
            // 调整极限
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        /// <summary>
        /// 向前 / 向后 行进一段距离（向后为负）
        /// </summary>
        /// <param name="yMove">前进距离 单位：mm</param>
        /// <returns></returns>
        public static int getSpeedY(double yMove)
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().y - StartPosition.y;
            double target = yMove;
            double Kp = 0.7;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachY = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        /// <summary>
        /// 向左 / 向右 旋转一定角度（向右为负）
        /// </summary>
        /// <param name="aMove">左转角度 单位：度</param>
        /// <returns></returns>
        public static int getSpeedA(double aMove)
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().aCar - StartPosition.aCar;
            double target = aMove;
            double Kp = 80;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 防撞
            return AST_GuideBySpeed.getSpeedA((int)adjust);
        }

        /// <summary>
        /// 到达目标点的 X 位置
        /// </summary>
        /// <returns></returns>
        public static int getSpeedX()
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().x;
            double target = TargetPosition.x;
            double Kp = 1;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachX = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedX(adjust);
        }
        /// <summary>
        /// 到达目标点的 Y 位置
        /// </summary>
        /// <returns></returns>
        public static int getSpeedY()
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().y;
            double target = TargetPosition.y;
            double Kp = 0.7;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachY = Math.Abs(current - target) < 10;

            // 防撞
            return AST_GuideBySpeed.getSpeedY(adjust);
        }
        /// <summary>
        /// 到达目标点的 A 位置
        /// </summary>
        /// <returns></returns>
        public static int getSpeedA()
        {
            // 获取控制
            double current = TH_MeasurePosition.getPosition().aCar;
            double target = TargetPosition.aCar;
            double Kp = 80;

            double adjust = -Kp * (current - target);

            // 调整极限
            ApproachA = Math.Abs(current - target) < 1;

            // 防撞
            return AST_GuideBySpeed.getSpeedA(adjust);
        }
    }
}
