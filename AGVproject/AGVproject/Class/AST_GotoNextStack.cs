using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_GotoNextStack
    {
        public static void Scan_JumpLR()
        {
            // 保证有足够的空间够旋转
            int[] dis = TH_SendCommand.getUltraSonicData();
            for (int i = 0; i < 8; i++) { if (dis[i] == 0) { dis[i] = (int)Hardware_UltraSonic.Max; } }

            double disL = Math.Min(dis[(int)TH_SendCommand.Sonic.Head_L_X], dis[(int)TH_SendCommand.Sonic.Tail_L_X]);
            double disR = Math.Min(dis[(int)TH_SendCommand.Sonic.Head_R_X], dis[(int)TH_SendCommand.Sonic.Head_R_X]);
            double disU = Math.Min(dis[(int)TH_SendCommand.Sonic.Head_L_Y], dis[(int)TH_SendCommand.Sonic.Head_R_Y]);
            double disD = Math.Min(dis[(int)TH_SendCommand.Sonic.Tail_L_Y], dis[(int)TH_SendCommand.Sonic.Tail_R_Y]);

            bool PermitRotate = (disL + disR) >= Hardware_PlatForm.Length && (disU + disD) > Hardware_PlatForm.Width;
            if (!PermitRotate)
            {
                TH_AutoSearchTrack.control.EMA = true;
                TH_AutoSearchTrack.control.Action = TH_AutoSearchTrack.Action.Error;
                TH_AutoSearchTrack.control.Event = "Error: No Enough Space For Rotate !";
                return;
            }

            // 到达目标位置
            CoordinatePoint.POINT StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.StartPosition = StartPosition;

            AST_GuideByPosition.ApproachX = false;
            AST_GuideByPosition.ApproachY = false;

            double disX = 0, disY = 0;
            if (disL < Hardware_PlatForm.Length / 2) { disX = Hardware_PlatForm.Length / 2 - disL; }
            if (disR < Hardware_PlatForm.Length / 2) { disX = disR - Hardware_PlatForm.Length / 2; }
            if (disU < Hardware_PlatForm.Width / 2) { disY = disU - Hardware_PlatForm.Width / 2; }
            if (disD < Hardware_PlatForm.Width / 2) { disY = Hardware_PlatForm.Width / 2 - disD; }
            
            while (!AST_GuideByPosition.ApproachX || !AST_GuideByPosition.ApproachY)
            {
                int xSpeed = AST_GuideByPosition.getSpeedX(disX);
                int ySpeed = AST_GuideByPosition.getSpeedY(disY);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, 0);
            }

            // 旋转
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.ApproachA = false;
            while (!AST_GuideByPosition.ApproachA)
            {
                int aSpeed = AST_GuideByPosition.getSpeedA(180);
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }

            // 回到原位置
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.TargetPosition = StartPosition;
            AST_GuideByPosition.ApproachX = false;
            AST_GuideByPosition.ApproachY = false;

            while (!AST_GuideByPosition.ApproachX || !AST_GuideByPosition.ApproachY)
            {
                int xSpeed = AST_GuideByPosition.getSpeedX();
                int ySpeed = AST_GuideByPosition.getSpeedY();

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, 0);
            }
        }
        public static void Scan_Jump()
        {

        }
    }
}
