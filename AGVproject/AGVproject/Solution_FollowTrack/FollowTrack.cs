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
            foreach (BuildRoute.ROUTE route in BuildRoute.Route)
            {
                AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
                AST_GuideByPosition.ApproachX = false;
                AST_GuideByPosition.ApproachY = false;
                AST_GuideByPosition.ApproachA = false;

                while (!AST_GuideByPosition.ApproachA)
                {
                    if (Math.Abs(route.aMove) < 5) { break; }

                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideByPosition.getSpeedA(route.aMove);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (!AST_GuideByPosition.ApproachX)
                {
                    int xSpeed = AST_GuideByPosition.getSpeedX(route.xMove);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (!AST_GuideByPosition.ApproachY)
                {
                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideByPosition.getSpeedY(route.yMove);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                //AST_CorrectPosition.Start(route.TargetCorrect);
            }
        }
    }
}
