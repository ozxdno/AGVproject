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

                double xMove = route.TargetPosition.x - route.StartPosition.x;
                double yMove = route.TargetPosition.y - route.StartPosition.y;
                double aMove = route.TargetPosition.aCar - route.StartPosition.aCar;

                int test = 0; test++;

                while (!AST_GuideByPosition.ApproachA)
                {
                    //if (aMove < 5) { break; }

                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideByPosition.getSpeedA(aMove);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (!AST_GuideByPosition.ApproachX || !AST_GuideByPosition.ApproachY)
                {
                    //if (yMove < 50) { break; }

                    int xSpeed = AST_GuideByPosition.getSpeedX(xMove);
                    int ySpeed = AST_GuideByPosition.getSpeedY(yMove);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                while (false)
                {
                    //if (xMove < 50) { break; }

                    int xSpeed = AST_GuideByPosition.getSpeedX(xMove);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                AST_CorrectPosition.Start(route.TargetCorrect);
            }
        }
    }
}
