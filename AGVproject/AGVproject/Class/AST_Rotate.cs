using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_RotateL
    {
        private static int SubAction;

        public static void Start()
        {
            #region 初始化设置

            SubAction = 0;

            #endregion

            #region 找到一个空间，足够旋转

            SubAction++;
            
            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 判断退出条件
                List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getHead();
                List<CoordinatePoint.POINT> pointsT = TH_MeasureSurrounding.getTail();
                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getBodyL();
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getBodyR();

                double minH = CoordinatePoint.MinY(CoordinatePoint.AbsY(pointsH));
                double minT = CoordinatePoint.MinY(CoordinatePoint.AbsY(pointsT));
                double minL = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsL));
                double minR = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsR));

                double reqDistanceH = Hardware_PlatForm.Width + Hardware_PlatForm.AxisSideU;
                double reqDistanceT = Hardware_PlatForm.Width - Hardware_PlatForm.AxisSideD;
                double reqDistanceL = Hardware_PlatForm.Length - Hardware_PlatForm.AxisSideL;
                double reqDistanceR = Hardware_PlatForm.Length + Hardware_PlatForm.AxisSideR;

                bool SuitH = minH > reqDistanceH;
                bool SuitT = minT > reqDistanceT;
                bool SuitL = minL > reqDistanceL;
                bool SuitR = minR > reqDistanceR;

                if (SuitH && SuitT && SuitL && SuitR) { break; }

                // 获取控制
                int xSpeed = 0, ySpeed = 0;
                if (!SuitH) { ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(reqDistanceH); }
                if (!SuitT) { ySpeed = AST_GuideBySurrounding.getSpeedY_KeepD(reqDistanceT); }
                if (!SuitL) { xSpeed = AST_GuideBySurrounding.getSpeedX_KeepL(reqDistanceL); }
                if (!SuitR) { xSpeed = AST_GuideBySurrounding.getSpeedX_KeepR(reqDistanceR); }

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, 0);
            }

            #endregion

            #region 逆时针/向左 旋转 90 度（原地），更新状态

            SubAction++;
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();

            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 达到退出条件
                double current = TH_MeasurePosition.getPosition().aCar;
                double target = AST_GuideByPosition.StartPosition.aCar + 90;
                if (Math.Abs(target - current) < 1) { break; }

                // 获取控制
                int aSpeed = AST_GuideByPosition.getSpeedA(90);
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }

            if (TH_AutoSearchTrack.control.SubAction <= SubAction)
            {
                TH_AutoSearchTrack.Direction nextDir = HouseMap.getDirection() == TH_AutoSearchTrack.Direction.Up ?
                    TH_AutoSearchTrack.Direction.Right :
                    TH_AutoSearchTrack.Direction.Left;

                HouseMap.setDirection(nextDir);
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                if (HouseMap.CarSideL_NearStack())
                { TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Downward); }

                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Upward);
            }

            #endregion
        }

    }

    class AST_RotateR
    {
        private static int SubAction;

        public static void Start()
        {
            #region 初始化设置

            SubAction = 0;

            #endregion

            #region 找到一个空间，足够旋转

            SubAction++;

            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 判断退出条件
                List<CoordinatePoint.POINT> pointsH = TH_MeasureSurrounding.getHead();
                List<CoordinatePoint.POINT> pointsT = TH_MeasureSurrounding.getTail();
                List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getBodyL();
                List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getBodyR();

                double minH = CoordinatePoint.MinY(CoordinatePoint.AbsY(pointsH));
                double minT = CoordinatePoint.MinY(CoordinatePoint.AbsY(pointsT));
                double minL = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsL));
                double minR = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsR));

                double reqDistanceH = Hardware_PlatForm.Width + Hardware_PlatForm.AxisSideU;
                double reqDistanceT = Hardware_PlatForm.Width - Hardware_PlatForm.AxisSideD;
                double reqDistanceL = Hardware_PlatForm.Length - Hardware_PlatForm.AxisSideL;
                double reqDistanceR = Hardware_PlatForm.Length + Hardware_PlatForm.AxisSideR;

                bool SuitH = minH > reqDistanceH;
                bool SuitT = minT > reqDistanceT;
                bool SuitL = minL > reqDistanceL;
                bool SuitR = minR > reqDistanceR;

                if (SuitH && SuitT && SuitL && SuitR) { break; }

                // 获取控制
                int xSpeed = 0, ySpeed = 0;
                if (!SuitH) { ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(reqDistanceH); }
                if (!SuitT) { ySpeed = AST_GuideBySurrounding.getSpeedY_KeepD(reqDistanceT); }
                if (!SuitL) { xSpeed = AST_GuideBySurrounding.getSpeedX_KeepL(reqDistanceL); }
                if (!SuitR) { xSpeed = AST_GuideBySurrounding.getSpeedX_KeepR(reqDistanceR); }

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, 0);
            }

            #endregion

            #region 逆时针/向左 旋转 90 度（原地），更新状态

            SubAction++;
            AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();

            while (true)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > SubAction) { break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 达到退出条件
                double current = TH_MeasurePosition.getPosition().aCar;
                double target = AST_GuideByPosition.StartPosition.aCar + 90;
                if (Math.Abs(target - current) < 1) { break; }

                // 获取控制
                int aSpeed = AST_GuideByPosition.getSpeedA(90);
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
            }

            if (TH_AutoSearchTrack.control.SubAction <= SubAction)
            {
                TH_AutoSearchTrack.Direction nextDir = HouseMap.getDirection() == TH_AutoSearchTrack.Direction.Up ?
                    TH_AutoSearchTrack.Direction.Right :
                    TH_AutoSearchTrack.Direction.Left;

                HouseMap.setDirection(nextDir);
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                if (HouseMap.CarSideL_NearStack())
                { TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Downward); }

                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Upward);
            }

            #endregion
        }
    }
}
