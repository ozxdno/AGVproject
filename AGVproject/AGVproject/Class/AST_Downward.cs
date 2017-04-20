using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Downward
    {
        private static int SubAction;

        public static void Start()
        {
            #region 初始化设置

            SubAction = 0;

            #endregion

            #region 找到开始点

            SubAction++;

            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 达到退出条件
                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(170, 180);
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 10);

                bool nearLeft = HouseMap.CarSideL_NearStack();

                double disEmpty = TH_AutoSearchTrack.control.KeepDistance_LR + 200;
                bool Empty = nearLeft ?
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, pointsL) :
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, pointsR);

                if (!Empty) { break; }

                // 控制
                int xSpeed = 0;
                int ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(TH_AutoSearchTrack.control.MinDistance_H);
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            #endregion

            #region 找到结束点

            SubAction++;

            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 达到退出条件
                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(170, 180);
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 10);

                bool nearLeft = HouseMap.CarSideL_NearStack();

                double disEmpty = TH_AutoSearchTrack.control.KeepDistance_LR + 200;
                bool Empty = nearLeft ?
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, pointsL) :
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, pointsR);

                if (Empty) { break; }

                // 控制
                int xSpeed = 0;
                int ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(TH_AutoSearchTrack.control.MinDistance_H);
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            #endregion

            #region 停下来准备旋转

            SubAction++;
            AST_SlowDown.Start();

            #endregion

            #region 下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.RotateL);
            }

            #endregion
        }
    }
}
