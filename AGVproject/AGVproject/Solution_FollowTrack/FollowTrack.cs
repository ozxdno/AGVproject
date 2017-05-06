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
                CoordinatePoint.POINT move = CoordinatePoint.MoveMethod(sour, dest);

                AST_GuideByPosition.ApproachX = false;
                AST_GuideByPosition.ApproachY = false;
                AST_GuideByPosition.ApproachA = false;

                while (!AST_GuideByPosition.ApproachA)
                {
                    if (Math.Abs(move.aCar) < 5) { break; }

                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideByPosition.getSpeedA(move.aCar);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (!AST_GuideByPosition.ApproachX)
                {
                    int xSpeed = AST_GuideByPosition.getSpeedX(move.x);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (!AST_GuideByPosition.ApproachY)
                {
                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideByPosition.getSpeedY(move.y);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                CorrectPosition.Start((CorrectPosition.CORRECT)HouseTrack.getExtra(i));
            }
        }
    }
}
