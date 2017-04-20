using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_AlignAisleForward
    {
        public static void Start()
        {
            #region 初始化设置

            TH_AutoSearchTrack.control.NextSubAction = 0;

            #endregion

            #region 左右平移找到通道的入口

            TH_AutoSearchTrack.control.NextSubAction++;

            while (true)
            {
                // 是否有外部命令
                if (TH_AutoSearchTrack.IsStopAction()) { return; }

                // 退出条件
                bool Finished = false;
                int xSpeed = getSpeedX(TH_AutoSearchTrack.control.MaxSpeed_X, ref Finished);
                if (Finished) { break; }

                // 控制

                int ySpeed = 0;
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            if (TH_AutoSearchTrack.control.SubAction <= TH_AutoSearchTrack.control.NextSubAction)
            {
                TH_AutoSearchTrack.Direction lastDir = HouseMap.getDirection();
                TH_AutoSearchTrack.Direction nextDir = lastDir == TH_AutoSearchTrack.Direction.Up ?
                    TH_AutoSearchTrack.Direction.Down :
                    TH_AutoSearchTrack.Direction.Up;

                HouseMap.setDirection(nextDir);
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Forward);
            }

            #endregion
        }

        private static int getSpeedX(int keepSpeed,ref bool Finished)
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
            if (Gap < Hardware_PlatForm.Width) { return keepSpeed; }

            // 是否已经满足退出条件
            double current = L;
            double target = TH_AutoSearchTrack.control.KeepDistance_UD + Hardware_PlatForm.AxisSideR;
            if (Math.Abs(current - target) < 20) { Finished = true; }

            // 获取控制
            double Kp = 0.5;
            int xSpeed = (int)(Kp * (current - target));

            // 限速
            if (xSpeed > TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = TH_AutoSearchTrack.control.MaxSpeed_X; }
            if (xSpeed < -TH_AutoSearchTrack.control.MaxSpeed_X) { xSpeed = -TH_AutoSearchTrack.control.MaxSpeed_X; }
            return -xSpeed;
        }
    }

    class AST_AlignAisleBackward
    {
        private static bool ForwardToSearchEnterance;

        public static void Start()
        {
            #region 初始化设置

            TH_AutoSearchTrack.control.NextSubAction = 0;
            getSearchEnteranceDirection();

            #endregion

            #region 前进去找通道口（先靠近目标堆垛，再向前找到入口）

            TH_AutoSearchTrack.control.NextSubAction++;

            if (ForwardToSearchEnterance)
            {
                TH_AutoSearchTrack.Direction lastDir = HouseMap.getDirection(TH_AutoSearchTrack.control.NearStack);
                TH_AutoSearchTrack.Direction nextDir = lastDir == TH_AutoSearchTrack.Direction.Up ?
                     TH_AutoSearchTrack.Direction.Down :
                     TH_AutoSearchTrack.Direction.Up;

                TH_AutoSearchTrack.control.NearStack++;
                HouseMap.setDirection(nextDir);
                
                ApproachNextStack(); Forward();
            }

            #endregion

            #region 后退去找通道口（先沿着当前通道倒退直到找到通道口，再靠近目标堆垛）

            TH_AutoSearchTrack.control.NextSubAction++;

            if (!ForwardToSearchEnterance)
            {
                Backward();
                
                TH_AutoSearchTrack.Direction lastDir = HouseMap.getDirection(TH_AutoSearchTrack.control.NearStack);
                TH_AutoSearchTrack.Direction nextDir = lastDir == TH_AutoSearchTrack.Direction.Up ?
                     TH_AutoSearchTrack.Direction.Down :
                     TH_AutoSearchTrack.Direction.Up;

                TH_AutoSearchTrack.control.NearStack++;
                HouseMap.setDirection(nextDir); ApproachNextStack();
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Backward);
            }

            #endregion
        }


        private static void getSearchEnteranceDirection()
        {
            bool nearLeft = HouseMap.CarSideL_NearStack();
            double disEmpty = HouseMap.getAisleWidth();

            double minDis = nearLeft ?
                CoordinatePoint.MinX(TH_MeasureSurrounding.getSurroundingA(0, 10)) :
                -CoordinatePoint.MaxX(TH_MeasureSurrounding.getSurroundingA(170, 180));

            ForwardToSearchEnterance = minDis > HouseMap.getAisleWidth();
        }
        private static void ApproachNextStack()
        {
            while (true)
            {
                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 是否调整完毕
                bool nearLeft = HouseMap.CarSideL_NearStack();
                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(170, 180);
                pointsL = CoordinatePoint.AbsX(pointsL);
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 10);
                pointsR = CoordinatePoint.AbsX(pointsR);

                double minL = CoordinatePoint.MinX(pointsL);
                double minR = CoordinatePoint.MinX(pointsR);

                double reqL = TH_AutoSearchTrack.control.KeepDistance_UD - Hardware_PlatForm.AxisSideL;
                double reqR = TH_AutoSearchTrack.control.KeepDistance_UD + Hardware_PlatForm.AxisSideR;

                if (nearLeft && Math.Abs(minR - reqR) < 20) { break; }
                if (!nearLeft && Math.Abs(minL - reqL) < 20) { break; }

                // 控制
                int xSpeed = nearLeft ?
                    AST_GuideBySurrounding.getSpeedX_KeepR(reqR) :
                    AST_GuideBySurrounding.getSpeedX_KeepL(reqL);
                int ySpeed = 0;
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void Forward()
        {
            while (true)
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 是否满足退出条件
                double disEmpty = HouseMap.getAisleWidth();
                bool EmptyL = TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(170, 180));
                bool EmptyR = TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(0, 10));

                bool nearLeft = HouseMap.CarSideL_NearStack();
                if ((nearLeft && EmptyL) || (!nearLeft && EmptyR)) { break; }

                // 控制
                int xSpeed = 0;
                int ySpeed = AST_GuideBySpeed.getSpeedY(TH_AutoSearchTrack.control.MaxSpeed_Y);
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void Backward()
        {
            while (true)
            {
                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 是否满足退出条件
                double disEmpty = HouseMap.getAisleWidth();
                bool EmptyL = TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(170, 180));
                bool EmptyR = TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(0, 10));

                bool nearLeft = HouseMap.CarSideL_NearStack();
                if ((nearLeft && !EmptyR) || (!nearLeft && EmptyL)) { break; }

                // 控制
                int xSpeed = 0;
                int ySpeed = AST_GuideBySpeed.getSpeedY(-TH_AutoSearchTrack.control.MaxSpeed_Y);
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
    }
}
