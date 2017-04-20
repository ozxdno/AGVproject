using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Forward
    {
        private static bool FinishedGetAisleWidth;

        /// <summary>
        /// 1 进入通道，记录起始位置，记录行通道宽度
        /// 2 通道内自动行走直至刚出通道
        /// 3 进行校准，记录列通道宽度
        /// </summary>
        public static void Start()
        {
            #region 初始化设置

            TH_AutoSearchTrack.control.NextSubAction = 0;
            HouseMap.setReferencePoint(CoordinatePoint.getNegPoint());

            FinishedGetAisleWidth = false;

            #endregion

            #region 小车头部进入通道，记录起始位置。

            TH_AutoSearchTrack.control.NextSubAction++;

            while (true)
            {
                // 是否有外部命令
                if (TH_AutoSearchTrack.IsStopAction()) { return; }

                // 执行完毕
                bool nearLeft = HouseMap.CarSideL_NearStack();

                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(170, 180);
                pointsL = CoordinatePoint.AbsX(pointsL);
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 10);
                pointsR = CoordinatePoint.AbsX(pointsR);

                double minL = CoordinatePoint.MinX(pointsL);
                double minR = CoordinatePoint.MinX(pointsR);

                double aisleWidth = HouseMap.getAisleWidth();

                if (nearLeft && minL < aisleWidth) { break; }
                if (!nearLeft && minR < aisleWidth) { break; }

                // 获取控制
                int xSpeed = 0;
                int ySpeed = AST_GuideBySpeed.getSpeedY(TH_AutoSearchTrack.control.MaxSpeed_Y);
                int aSpeed = 0;

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            //if()

            #endregion

            #region 通道内自动行走，记录通道长度，宽度。

            TH_AutoSearchTrack.control.NextSubAction++;

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

                // 是否已经满足条件
                bool nearLeft = HouseMap.CarSideL_NearStack();

                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(170, 180);
                pointsL = CoordinatePoint.AbsX(pointsL);
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 10);
                pointsR = CoordinatePoint.AbsX(pointsR);
                List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getSurroundingA(85, 95);
                pointsH = CoordinatePoint.AbsY(pointsH);

                double minL = CoordinatePoint.MinX(pointsL);
                double minR = CoordinatePoint.MinX(pointsR);
                double minH = CoordinatePoint.MinX(pointsH);

                double aisleWidth = HouseMap.getAisleWidth();

                if (nearLeft && minL < aisleWidth) { break; }
                if (!nearLeft && minR < aisleWidth) { break; }
                if (minH < TH_AutoSearchTrack.control.KeepDistance_LR) { break; }

                // 控制策略
                int xSpeed = AST_GuideBySurrounding.getSpeedX_KeepL(TH_AutoSearchTrack.control.MaxSpeed_X);
                int ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(TH_AutoSearchTrack.control.MaxSpeed_Y);
                int aSpeed = AST_GuideBySurrounding.getSpeedA_KeepL_Forward();

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

            #region 通道尽头校准，记录通道宽度

            TH_AutoSearchTrack.control.NextSubAction++;

            if (TH_AutoSearchTrack.control.SubAction <= TH_AutoSearchTrack.control.NextSubAction)
            {
                
                double K = 0, A = 0, B = 0;
                AST_CorrectPosition.Start();
                if (TH_AutoSearchTrack.IsStopAction()) { return; }
                AST_CorrectPosition.getKAB(ref K, ref A, ref B);
                
                TH_AutoSearchTrack.Direction direction = HouseMap.ScanningL() ?
                     TH_AutoSearchTrack.Direction.Left :
                     TH_AutoSearchTrack.Direction.Right;
                
                HouseMap.setAisleWidth(Math.Abs(B), direction);
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.AlignB);
            }

            #endregion
        }
    }
}
