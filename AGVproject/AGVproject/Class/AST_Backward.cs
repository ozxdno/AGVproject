using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Backward
    {
        private static bool FinishedGetAisleWidth;

        /// <summary>
        /// AGV小车在通道内，1 自动倒退，2 整个车身退出通道
        /// </summary>
        public static void Start()
        {
            #region 初始化配置信息

            TH_AutoSearchTrack.control.NextSubAction = 0;
            FinishedGetAisleWidth = false;

            #endregion

            #region 从通道之中退出来

            TH_AutoSearchTrack.control.NextSubAction++;
            HouseMap.setReferencePoint(TH_MeasurePosition.getPosition());

            while (true)
            {
                // 是否有外部命令
                if (TH_AutoSearchTrack.IsStopAction()) { return; }

                // 获取通道宽度
                if (!FinishedGetAisleWidth)
                {
                    double AisleWidth = 0;
                    FinishedGetAisleWidth = TH_AutoSearchTrack.getAisleWidth(ref AisleWidth);
                    if (FinishedGetAisleWidth) { HouseMap.setAisleWidth(AisleWidth); }
                }

                // 是否已经满足要求
                double disEmpty = HouseMap.getAisleWidth();
                bool nearLeft = HouseMap.CarSideL_NearStack();

                bool Empty = nearLeft ?
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(170, 180)) :
                    TH_MeasureSurrounding.IsEmptyX(disEmpty, TH_MeasureSurrounding.getSurroundingA(0, 10));

                if (Empty) { break; }

                // 获取控制
                int xSpeed = nearLeft ?
                    AST_GuideBySurrounding.getSpeedX_KeepL(TH_AutoSearchTrack.control.KeepDistance_UD - Hardware_PlatForm.AxisSideL) :
                    AST_GuideBySurrounding.getSpeedX_KeepR(TH_AutoSearchTrack.control.KeepDistance_UD + Hardware_PlatForm.AxisSideR);
                int ySpeed = AST_GuideBySurrounding.getSpeedY_KeepD(TH_AutoSearchTrack.control.KeepDistance_LR - Hardware_PlatForm.AxisSideD);
                int aSpeed = nearLeft ?
                    AST_GuideBySurrounding.getSpeedA_KeepL_Backward() :
                    AST_GuideBySurrounding.getSpeedA_KeepR_Backward();

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            if (TH_AutoSearchTrack.control.SubAction <= TH_AutoSearchTrack.control.NextSubAction)
            {
                CoordinatePoint.POINT ptBG = HouseMap.getReferencePoint();
                CoordinatePoint.POINT ptED = TH_MeasurePosition.getPosition();

                if (!CoordinatePoint.IsNegPoint(ptBG))
                {
                    double AisleWidth = Math.Abs(ptBG.y - ptED.y);
                    HouseMap.setStackLength(AisleWidth);
                }
            }

            #endregion
            
            #region 决定下一动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                bool ReachBegin = TH_AutoSearchTrack.control.NearStack == HouseMap.TotalStacks &&
                    HouseMap.getDirection(HouseMap.TotalStacks) == TH_AutoSearchTrack.Direction.Up;

                bool ReachEnd = TH_AutoSearchTrack.control.NearStack == HouseMap.TotalStacksR &&
                    HouseMap.getDirection(HouseMap.TotalStacksR) == TH_AutoSearchTrack.Direction.Down;

                if (ReachBegin || ReachEnd) { TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Reverse); return; }
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.RotateR);
            }

            #endregion
        }
    }
}
