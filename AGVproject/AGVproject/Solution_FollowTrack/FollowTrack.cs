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
                AST_GuideByPosition.StartPosition = route.StartPosition;
                AST_GuideByPosition.TargetPosition = route.TargetPosition;
                AST_GuideByPosition.ApproachX = false;
                AST_GuideByPosition.ApproachY = false;
                AST_GuideByPosition.ApproachA = false;

                double xMove = route.TargetPosition.x - route.StartPosition.x;
                double yMove = route.TargetPosition.y - route.StartPosition.y;
                double aMove = route.TargetPosition.aCar - route.StartPosition.aCar;

                while (!AST_GuideByPosition.ApproachX && !AST_GuideByPosition.ApproachY && !AST_GuideByPosition.ApproachA)
                {
                    int xSpeed = AST_GuideByPosition.getSpeedX();
                    int ySpeed = AST_GuideByPosition.getSpeedY();
                    int aSpeed = AST_GuideByPosition.getSpeedA();

                    TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                }

                AST_CorrectPosition.Start(route.TargetCorrect);
            }
        }
    }
}
