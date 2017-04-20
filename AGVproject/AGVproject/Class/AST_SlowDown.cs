using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_SlowDown
    {
        /// <summary>
        /// 缓慢地使小车停止运动
        /// </summary>
        /// <param name="rate">变缓速率</param>
        public static void Start(double rate = 0.8)
        {
            while (true) 
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }

                int xSpeed = getSpeedX(rate);
                int ySpeed = getSpeedY(rate);
                int aSpeed = getSpeedA(rate);

                if (xSpeed < 10 && ySpeed < 10 && aSpeed < 50) { xSpeed = 0; ySpeed = 0; aSpeed = 0; }

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
                if (xSpeed == 0 && ySpeed == 0 && aSpeed == 0) { return; }
                System.Threading.Thread.Sleep(TH_SendCommand.TimeForControl);
            }
        }

        public static int getSpeedX(double rate = 0.8)
        {
            return (int)(rate * TH_SendCommand.xSpeed);
        }
        public static int getSpeedY(double rate = 0.8)
        {
            return (int)(rate * TH_SendCommand.ySpeed);
        }
        public static int getSpeedA(double rate = 0.8)
        {
            return (int)(rate * TH_SendCommand.aSpeed);
        }
    }
}
