using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Go
    {
        public static TH_UpdataPictureBox.ROUTE Last;
        public static TH_UpdataPictureBox.ROUTE Next;

        public static void Track()
        {
            for (int i = 1; i <= HouseMap.TotalStacks; i++) { ResetRouteInfo(i); }

            while (true)
            {
                if (TH_AutoSearchTrack.control.EMA) { return; }
                if (TH_AutoSearchTrack.control.Target > TH_UpdataPictureBox.Route.Count) { return; }

                Last = TH_UpdataPictureBox.Route[TH_AutoSearchTrack.control.Current];
                Next = TH_UpdataPictureBox.Route[TH_AutoSearchTrack.control.Target];
                if (Last.Distance == Next.Distance) { continue; }

                if (Last.No != Next.No) { GotoNextStack(); continue; }

                bool scan = (Last.Direction == TH_AutoSearchTrack.Direction.Up || Last.Direction == TH_AutoSearchTrack.Direction.Down) &&
                    Last.Direction == Next.Direction;
                bool forward = false;

                if (scan && forward) { ScanForward(); continue; }
                if (scan && !forward) { ScanBackward(); continue; }
                GoAroundStack(); continue;
            }
        }
        public static void Scan()
        {
            // 寻找 1 通道入口
            AST_AlignAisle.Start(Hardware_PlatForm.Width, HouseMap.DefaultAisleWidth / 2, false);

            // 前进
            AST_Forward.Scan();

            while (true)
            {
                AST_Backward.Scan();
                if (true) { break; }
                AST_Side.Scan();
                AST_Forward.Scan();
            }

            // 后退
            AST_Backward.Scan();

            // 沿堆垛边行进
            AST_Side.Scan();
        }
        
        private static void ResetRouteInfo(int No)
        {
            TH_UpdataPictureBox.STACK stack = TH_UpdataPictureBox.Stacks[No];
            TH_UpdataPictureBox.ROUTE route = TH_UpdataPictureBox.Route[No];
            
            if (route.Direction == TH_AutoSearchTrack.Direction.Up) { return; }
            if (route.Direction == TH_AutoSearchTrack.Direction.Down) { return; }
            
            if (route.Direction == TH_AutoSearchTrack.Direction.Left)
            {
                if (route.Distance == stack.Width + stack.SetKeepU)
                {
                    route.Direction = TH_AutoSearchTrack.Direction.Up;
                    route.Distance = -stack.SetKeepL;
                    TH_UpdataPictureBox.Route[No] = route; return;
                }
                if (route.Distance == -stack.SetKeepD)
                {
                    route.Direction = TH_AutoSearchTrack.Direction.Down;
                    route.Distance = stack.Length + stack.SetKeepL;
                    TH_UpdataPictureBox.Route[No] = route; return;
                }
            }
            if (route.Direction == TH_AutoSearchTrack.Direction.Right)
            {
                if (route.Distance == -stack.SetKeepU)
                {
                    route.Direction = TH_AutoSearchTrack.Direction.Up;
                    route.Distance = stack.Length + stack.SetKeepR;
                    TH_UpdataPictureBox.Route[No] = route; return;
                }
                if (route.Distance == stack.Width + stack.SetKeepD)
                {
                    route.Direction = TH_AutoSearchTrack.Direction.Down;
                    route.Distance = -stack.SetKeepR;
                    TH_UpdataPictureBox.Route[No] = route; return;
                }
            }
        }
        private static void GotoNextStack()
        {
            // 设定参考点坐标
            HouseMap.setReferencePoint();

            // 获取距离
            double disX = (Next.MapPoint.X - Last.MapPoint.X) * Form_Start.config.PixLength;
            double disY = (Next.MapPoint.Y - Last.MapPoint.Y) * Form_Start.config.PixLength;

            // 前进
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.ApproachX = false;
            AST_GuideByPosition.ApproachY = false;

            while (!AST_GuideByPosition.ApproachX || !AST_GuideByPosition.ApproachY)
            {
                int xSpeed = AST_GuideByPosition.getSpeedX(disX);
                int ySpeed = AST_GuideByPosition.getSpeedY(disY);
                int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            // 更新 AST 数据
            TH_AutoSearchTrack.control.Current = TH_AutoSearchTrack.control.Target;
            TH_AutoSearchTrack.control.Target++;
            TH_AutoSearchTrack.control.NearStack = Next.No;

            // 更新 STACK 数据
            HouseMap.setCarPosition(Next.Direction);
            HouseMap.setCarDirection(HouseMap.getCarDirection(Last.No));
        }
        private static void ScanForward()
        {
            // 设定参考点坐标
            HouseMap.setReferencePoint();

            // 寻找并对准通道入口


            // 获取距离
            double disY = (Next.MapPoint.Y - Last.MapPoint.Y) * Form_Start.config.PixLength;

            // 前进
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();
            AST_GuideByPosition.ApproachY = false;

            bool SetAisleWidthFinished = false;
            CoordinatePoint.POINT posI = CoordinatePoint.getNegPoint();
            CoordinatePoint.POINT posO = CoordinatePoint.getNegPoint();

            while (!AST_GuideByPosition.ApproachY)
            {
                // 记录通道宽度
                if (!SetAisleWidthFinished) { SetAisleWidthFinished = HouseMap.setAisleWidth(); }

                // 记录进通道 位置/时间

                // 记录出通道 位置/时间

                TH_AutoSearchTrack.Direction dir = HouseMap.getCarPosition();

                int xSpeed = dir == TH_AutoSearchTrack.Direction.Down ?
                    AST_GuideBySurrounding.getSpeedX_KeepL_Forward(HouseMap.getKeepDistance()) :
                    AST_GuideBySurrounding.getSpeedX_KeepR_Forward(HouseMap.getKeepDistance());
                int ySpeed = AST_GuideByPosition.getSpeedY(disY);
                int aSpeed = AST_GuideBySurrounding.getSpeedA_KeepL_Forward();

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }

            // 更新 AST 数据
            TH_AutoSearchTrack.control.Current = TH_AutoSearchTrack.control.Target;
            TH_AutoSearchTrack.control.Target++;
            TH_AutoSearchTrack.control.NearStack = Next.No;
        }
        private static void ScanBackward()
        {

        }
        private static void GoAroundStack()
        {
            // 设定参考点坐标
            HouseMap.setReferencePoint();

            // 调整车身方向

            // 
        }
    }
}
