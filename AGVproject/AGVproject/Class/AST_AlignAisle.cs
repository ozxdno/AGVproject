using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_AlignAisle
    {
        public static bool ApproachX;
        public static bool ApproachY;
        public static bool ApproachA;

        public static int getSpeedX(double keepDistance)
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
            if (Gap < Hardware_PlatForm.Width) { return 0; }

            // 是否已经满足退出条件
            double current = L;
            double target = HouseMap.getKeepDistance();
            if (Math.Abs(current - target) < 20) { ApproachX = true; }

            // 获取控制
            double Kp = 0.5;
            int xSpeed = (int)(Kp * (current - target));

            // 限速
            if (xSpeed > TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = TH_AutoSearchTrack.control.MaxSpeed_X; }
            if (xSpeed < -TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = -TH_AutoSearchTrack.control.MaxSpeed_X; }
            return -xSpeed;
        }
        public static int getSpeedY(double keepDistance)
        {
            return 0;
        }
        public static int getSpeedA()
        {
            return 0;
        }
    }
}
