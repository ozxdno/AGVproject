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
        private static CoordinatePoint.POINT StartPosition;
        /// <summary>
        /// 调节终点
        /// </summary>
        private static CoordinatePoint.POINT TargetPosition;

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
        /// 到达目标点的 X 位置
        /// </summary>
        /// <returns></returns>
        public static int getSpeedX()
        {
            // 获取数据
            CoordinatePoint.POINT currpos = TH_MeasurePosition.getPosition();

            // 获取控制
            double current = CoordinatePoint.TransformCoordinate(StartPosition, currpos).x;
            double target = CoordinatePoint.TransformCoordinate(StartPosition, TargetPosition).x;
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
            // 获取数据
            CoordinatePoint.POINT currpos = TH_MeasurePosition.getPosition();

            // 获取控制
            double current = CoordinatePoint.TransformCoordinate(StartPosition, currpos).y;
            double target = CoordinatePoint.TransformCoordinate(StartPosition, TargetPosition).y;
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

        /// <summary>
        /// 把当前位置设定为起始点仓库坐标
        /// </summary>
        public static void setStartPosition()
        {
            StartPosition = TH_MeasurePosition.getPosition();
        }
        /// <summary>
        /// 设定起始点仓库坐标
        /// </summary>
        /// <param name="pos">起始点仓库坐标</param>
        public static void setStartPosition(CoordinatePoint.POINT pos)
        {
            StartPosition = pos;
        }
        /// <summary>
        /// 设定目标点仓库坐标（必须先把起始点设定完毕）
        /// </summary>
        /// <param name="pos">目标点仓库坐标</param>
        public static void setTargetPosition(CoordinatePoint.POINT pos)
        {
            TargetPosition = pos;
        }
        /// <summary>
        /// 按 X / Y / A 三个方向的移动量来设定目标点位置（必须先把起始点设定完毕）
        /// </summary>
        /// <param name="xMove">X 方向移动量 单位：mm</param>
        /// <param name="yMove">Y 方向移动量 单位：mm</param>
        /// <param name="aMove">A 方向移动量 单位：度</param>
        public static void setTargetPosition(double xMove, double yMove, double aMove)
        {
            TargetPosition.x = StartPosition.x + xMove;
            TargetPosition.y = StartPosition.y + yMove;
            TargetPosition = CoordinatePoint.Create_XY(TargetPosition.x, TargetPosition.y);

            TargetPosition.aCar = StartPosition.aCar + aMove;
            TargetPosition.rCar = TargetPosition.aCar * Math.PI / 180;
        }
    }
}
