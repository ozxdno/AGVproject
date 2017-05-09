using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    class FollowTrack
    {
        public static void Start()
        {
            for (int i = 1; i < HouseTrack.TotalTrack; i++)
            {
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

                //CorrectPosition.Start((CorrectPosition.CORRECT)HouseTrack.getExtra(i));
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
