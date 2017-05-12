using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

// 1 多次获取校正信息取中值作为该点的校准标准
// 2 调整过程过抖，可以减小精度或进行滤波
// 3 校准过程调整策略问题，到底先校准哪个，后校准哪个
// 4 校准过程单因素阈值，单个因素误差过大，不考虑

namespace AGVproject.Solution_FollowTrack
{
    class FollowTrack
    {
        public static void Start()
        {
            // 初始点校准
            CorrectPosition.Start((CorrectPosition.CORRECT)HouseTrack.getExtra(0));

            TH_AutoSearchTrack.control.Event = "0";

            for (int i = 1; i < HouseTrack.TotalTrack; i++)
            {
                TH_AutoSearchTrack.control.Event = (i - 1).ToString() + "--->" + i.ToString();


                CoordinatePoint.POINT sour = HouseTrack.getTargetPosition(i - 1);
                CoordinatePoint.POINT dest = HouseTrack.getTargetPosition(i);
                CoordinatePoint.POINT move = CoordinatePoint.TransformCoordinate(sour, dest);

                TH_MeasurePosition.setPosition(sour);
                AST_GuideByPosition.setStartPosition(sour);
                AST_GuideByPosition.setTargetPosition(dest);
                AST_GuideByPosition.ApproachX = false;
                AST_GuideByPosition.ApproachY = false;
                AST_GuideByPosition.ApproachA = false;



                if (Math.Abs(dest.aCar - sour.aCar) > 5) { AdjustA(); }
                if (Math.Abs(move.x) > Math.Abs(move.y)) { AdjustX(); AdjustY(); }
                else { AdjustY(); AdjustX(); }

                CorrectPosition.Start((CorrectPosition.CORRECT)HouseTrack.getExtra(i));
            }
        }

        private static void AdjustX()
        {
            while (!AST_GuideByPosition.ApproachX)
            {
                int xSpeed = AST_GuideByPosition.getSpeedX();
                int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void AdjustY()
        {
            while (!AST_GuideByPosition.ApproachY)
            {
                int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                int ySpeed = AST_GuideByPosition.getSpeedY();
                int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
        private static void AdjustA()
        {
            while (!AST_GuideByPosition.ApproachA)
            {
                int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                int aSpeed = AST_GuideByPosition.getSpeedA();

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }
        }
    }
}
