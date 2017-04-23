using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AGVproject.Class
{
    class HouseMap
    {
        ///////////////////////////////////////////// attribute //////////////////////////////////////////////////

        /// <summary>
        /// 仓库的前后距离 单位：mm
        /// </summary>
        public static double HouseLength;
        /// <summary>
        /// 仓库的左右距离 单位：mm
        /// </summary>
        public static double HouseWidth;
        
        /// <summary>
        /// 堆垛信息
        /// </summary>
        public static List<STACK> Stacks = new List<STACK>();

        /// <summary>
        /// 站在仓库门口并背对仓库门口，左手边堆垛数量
        /// </summary>
        public static int TotalStacksL;
        /// <summary>
        /// 站在仓库门口并背对仓库门口，右手边堆垛数量
        /// </summary>
        public static int TotalStacksR;
        /// <summary>
        /// 总的堆垛数量
        /// </summary>
        public static int TotalStacks { get { return TotalStacksL + TotalStacksR; } }

        public static double DefaultCentreRoadWidth;
        public static double DefaultAisleWidth;
        public static double DefaultStackLength;
        public static double DefaultStackWidth;
        
        /// <summary>
        /// 堆垛参数
        /// </summary>
        public struct STACK
        {
            /// <summary>
            /// 是否为左边的堆垛
            /// </summary>
            public bool IsLeft;
            /// <summary>
            /// 堆垛编号
            /// </summary>
            public int No;

            /// <summary>
            /// 堆垛长度 单位：mm
            /// </summary>
            public double Length;
            /// <summary>
            /// 堆垛宽度 单位：mm
            /// </summary>
            public double Width;

            /// <summary>
            /// 该堆垛左方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_L;
            /// <summary>
            /// 该堆垛右方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_R;
            /// <summary>
            /// 该堆垛上方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_U;
            /// <summary>
            /// 该堆垛下方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_D;
            
            /// <summary>
            /// 车与垛区的相对位置（上、下、左、右、无法确定）
            /// </summary>
            public TH_AutoSearchTrack.Direction CarPosition;
            /// <summary>
            /// 车头的方向
            /// </summary>
            public TH_AutoSearchTrack.Direction CarDirection;
            /// <summary>
            /// 与参考点的相对距离 单位：mm
            /// </summary>
            public double Distance;
            /// <summary>
            /// 参考点仓库坐标
            /// </summary>
            public CoordinatePoint.POINT ReferencePoint;

            /// <summary>
            /// 小车在该堆垛时，小车与堆垛左方保持的距离 单位：mm
            /// </summary>
            public double KeepDistanceL;
            /// <summary>
            /// 小车在该堆垛时，小车与堆垛右方保持的距离 单位：mm
            /// </summary>
            public double KeepDistanceR;
            /// <summary>
            /// 小车在该堆垛时，小车与堆垛上方保持的距离 单位：mm
            /// </summary>
            public double KeepDistanceU;
            /// <summary>
            /// 小车在该堆垛时，小车与堆垛下方保持的距离 单位：mm
            /// </summary>
            public double KeepDistanceD;
        }

        ///////////////////////////////////////////// Stacks //////////////////////////////////////////////////
        
        public static void getDefaultStacks()
        {
            Stacks = new List<STACK>();

            STACK stack0 = new STACK();
            stack0.IsLeft = false;
            stack0.No = 0;
            stack0.AisleWidth_L = (HouseWidth - 2000) / 2;
            stack0.AisleWidth_R = (HouseWidth - 2000) / 2;
            stack0.AisleWidth_U = 0;
            stack0.AisleWidth_D = 0;
            stack0.CarPosition = TH_AutoSearchTrack.Direction.Tuning;
            stack0.Length = 2000;
            stack0.Width = 100;
            Stacks.Add(stack0);

            for (int i = 1; i <= TotalStacksR; i++)
            {
                STACK NewStack = new STACK();

                NewStack.IsLeft = false;
                NewStack.No = i;
                NewStack.Length = DefaultStackLength;
                NewStack.Width = DefaultStackWidth;
                NewStack.AisleWidth_R = (HouseWidth - DefaultCentreRoadWidth - DefaultStackLength * 2) / 2;
                NewStack.AisleWidth_L = DefaultCentreRoadWidth;
                NewStack.AisleWidth_U = DefaultAisleWidth;
                NewStack.AisleWidth_D = DefaultAisleWidth;

                NewStack.KeepDistanceU = NewStack.AisleWidth_U / 2;
                NewStack.KeepDistanceD = NewStack.AisleWidth_D / 2;
                NewStack.KeepDistanceL = NewStack.AisleWidth_L / 2;
                NewStack.KeepDistanceR = NewStack.AisleWidth_R / 2;

                Stacks.Add(NewStack);
            }
            for (int i = TotalStacksR + 1; i <= TotalStacks; i++)
            {
                STACK NewStack = new STACK();

                NewStack.IsLeft = true;
                NewStack.No = i;
                NewStack.Length = DefaultStackLength;
                NewStack.Width = DefaultStackWidth;
                NewStack.AisleWidth_L = (HouseWidth - DefaultCentreRoadWidth - DefaultStackLength * 2) / 2;
                NewStack.AisleWidth_R = DefaultCentreRoadWidth;
                NewStack.AisleWidth_U = DefaultAisleWidth;
                NewStack.AisleWidth_D = DefaultAisleWidth;

                NewStack.KeepDistanceU = NewStack.AisleWidth_U / 2;
                NewStack.KeepDistanceD = NewStack.AisleWidth_D / 2;
                NewStack.KeepDistanceL = NewStack.AisleWidth_L / 2;
                NewStack.KeepDistanceR = NewStack.AisleWidth_R / 2;

                Stacks.Add(NewStack);
            }
        }

        public static bool getAisleWidth(ref double AisleWidth)
        {
            // 取点
            List<CoordinatePoint.POINT> pointsL = TH_MeasureSurrounding.getSurroundingA(150, 180);
            List<CoordinatePoint.POINT> pointsR = TH_MeasureSurrounding.getSurroundingA(0, 30);

            // 点的数量不够
            int reqAmount = 30;
            if (pointsL.Count < reqAmount) { return false; }
            if (pointsR.Count < reqAmount) { return false; }

            // 取最近距离
            double minL = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsL));
            double minR = CoordinatePoint.MinX(CoordinatePoint.AbsX(pointsR));

            AisleWidth = minL + minR;
            return true;
        }
        public static double getAisleWidth()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return -1; }

            TH_AutoSearchTrack.Direction d = Stacks[No].CarPosition;

            if (d == TH_AutoSearchTrack.Direction.Left) { return Stacks[No].AisleWidth_L; }
            if (d == TH_AutoSearchTrack.Direction.Right) { return Stacks[No].AisleWidth_R; }
            if (d == TH_AutoSearchTrack.Direction.Up) { return Stacks[No].AisleWidth_U; }
            if (d == TH_AutoSearchTrack.Direction.Down) { return Stacks[No].AisleWidth_D; }

            return -1;
        }
        public static double getAisleWidth(TH_AutoSearchTrack.Direction direction)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return -1; }
            
            if (direction == TH_AutoSearchTrack.Direction.Left) { return Stacks[No].AisleWidth_L; }
            if (direction == TH_AutoSearchTrack.Direction.Right) { return Stacks[No].AisleWidth_R; }
            if (direction == TH_AutoSearchTrack.Direction.Up) { return Stacks[No].AisleWidth_U; }
            if (direction == TH_AutoSearchTrack.Direction.Down) { return Stacks[No].AisleWidth_D; }

            return -1;
        }
        public static double getAisleWidth(int No, TH_AutoSearchTrack.Direction direction)
        {
            if (No < 0 || No > TotalStacks) { return -1; }

            if (direction == TH_AutoSearchTrack.Direction.Left) { return Stacks[No].AisleWidth_L; }
            if (direction == TH_AutoSearchTrack.Direction.Right) { return Stacks[No].AisleWidth_R; }
            if (direction == TH_AutoSearchTrack.Direction.Up) { return Stacks[No].AisleWidth_U; }
            if (direction == TH_AutoSearchTrack.Direction.Down) { return Stacks[No].AisleWidth_D; }

            return -1;
        }

        public static bool setAisleWidth()
        {
            double width = 0;
            bool get = getAisleWidth(ref width); if (!get) { return get; }
            setAisleWidth(width);
            return get;
        }
        public static void setAisleWidth(double width)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];

            TH_AutoSearchTrack.Direction direction = iStack.CarPosition;

            if (direction == TH_AutoSearchTrack.Direction.Left) { iStack.AisleWidth_L = width; }
            if (direction == TH_AutoSearchTrack.Direction.Right) { iStack.AisleWidth_R = width; }
            if (direction == TH_AutoSearchTrack.Direction.Up) { iStack.AisleWidth_U = width; }
            if (direction == TH_AutoSearchTrack.Direction.Down) { iStack.AisleWidth_D = width; }

            Stacks[No] = iStack;
        }
        public static void setAisleWidth(double width, TH_AutoSearchTrack.Direction direction)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            
            if (direction == TH_AutoSearchTrack.Direction.Left) { iStack.AisleWidth_L = width; }
            if (direction == TH_AutoSearchTrack.Direction.Right) { iStack.AisleWidth_R = width; }
            if (direction == TH_AutoSearchTrack.Direction.Up) { iStack.AisleWidth_U = width; }
            if (direction == TH_AutoSearchTrack.Direction.Down) { iStack.AisleWidth_D = width; }

            Stacks[No] = iStack;
        }
        public static void setAisleWidth(double width, int No, TH_AutoSearchTrack.Direction direction)
        {
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];

            if (direction == TH_AutoSearchTrack.Direction.Left) { iStack.AisleWidth_L = width; }
            if (direction == TH_AutoSearchTrack.Direction.Right) { iStack.AisleWidth_R = width; }
            if (direction == TH_AutoSearchTrack.Direction.Up) { iStack.AisleWidth_U = width; }
            if (direction == TH_AutoSearchTrack.Direction.Down) { iStack.AisleWidth_D = width; }

            Stacks[No] = iStack;
        }

        public static double getKeepDistance(bool fromcar = false)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return -1; }

            TH_AutoSearchTrack.Direction pos = Stacks[No].CarPosition;
            TH_AutoSearchTrack.Direction dir = Stacks[No].CarDirection;
            bool IsLeft = Stacks[No].IsLeft;

            if (pos == TH_AutoSearchTrack.Direction.Up)
            {
                if (fromcar) { return Stacks[No].KeepDistanceU; }
                if (IsLeft) { return Stacks[No].KeepDistanceU - Hardware_PlatForm.AxisSideL; }
                else { return Stacks[No].KeepDistanceU + Hardware_PlatForm.AxisSideR; }
            }
            if (pos == TH_AutoSearchTrack.Direction.Down)
            {
                if (fromcar) { return Stacks[No].KeepDistanceD; }
                if (IsLeft) { return Stacks[No].KeepDistanceD + Hardware_PlatForm.AxisSideR; }
                else { return Stacks[No].KeepDistanceD - Hardware_PlatForm.AxisSideL; }
            }
            if (pos == TH_AutoSearchTrack.Direction.Left)
            {
                if (fromcar) { return Stacks[No].KeepDistanceL; }
                if (dir == TH_AutoSearchTrack.Direction.Up) { return Stacks[No].KeepDistanceL + Hardware_PlatForm.AxisSideR; }
                if (dir == TH_AutoSearchTrack.Direction.Down) { return Stacks[No].KeepDistanceL - Hardware_PlatForm.AxisSideL; }
            }
            if (pos == TH_AutoSearchTrack.Direction.Right)
            {
                if (fromcar) { return Stacks[No].KeepDistanceR; }
                if (dir == TH_AutoSearchTrack.Direction.Down) { return Stacks[No].KeepDistanceL + Hardware_PlatForm.AxisSideR; }
                if (dir == TH_AutoSearchTrack.Direction.Up) { return Stacks[No].KeepDistanceL - Hardware_PlatForm.AxisSideL; }
            }
            return -1;
        }

        public static double getStackWidth()
        {
            return DefaultStackWidth;
        }
        public static double getStackWidth(int No)
        {
            return DefaultStackWidth;
        }

        public static double getStackLength()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return -1; }

            return Stacks[No].Length;
        }
        public static double getStackLength(int No)
        {
            if (No < 0 || No > TotalStacks) { return -1; }

            return Stacks[No].Length;
        }

        public static void setStackLength(double length)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            iStack.Length = length;
            Stacks[No] = iStack;
        }
        public static void setStackLength(double length, int No)
        {
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            iStack.Length = length;
            Stacks[No] = iStack;
        }

        public static TH_AutoSearchTrack.Direction getCarPosition()
        {
            int No = TH_AutoSearchTrack.control.NearStack;

            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }
            return Stacks[No].CarPosition;
        }
        public static TH_AutoSearchTrack.Direction getCarPosition(int No)
        {
            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }
            return Stacks[No].CarPosition;
        }

        public static void setCarPosition(TH_AutoSearchTrack.Direction direction)
        {
            int No = TH_AutoSearchTrack.control.NearStack;

            if (No < 0 || No > TotalStacks) { return; }
            STACK iStack = Stacks[No];
            iStack.CarPosition = direction; Stacks[No] = iStack;
        }
        public static void setCarPosition(TH_AutoSearchTrack.Direction direction, int No)
        {
            if (No < 0 || No > TotalStacks) { return; }
            STACK iStack = Stacks[No];
            iStack.CarPosition = direction; Stacks[No] = iStack;
        }

        public static TH_AutoSearchTrack.Direction getCarDirection()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }

            return Stacks[No].CarDirection;
        }
        public static TH_AutoSearchTrack.Direction getCarDirection(int No)
        {
            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }
            return Stacks[No].CarDirection;
        }

        public static void setCarDirection(TH_AutoSearchTrack.Direction direction)
        {
            int No = TH_AutoSearchTrack.control.NearStack;

            if (No < 0 || No > TotalStacks) { return; }
            STACK iStack = Stacks[No];
            iStack.CarDirection = direction; Stacks[No] = iStack;
        }

        public static void setReferencePoint()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            iStack.ReferencePoint = TH_MeasurePosition.getPosition(); Stacks[No] = iStack;
        }
        public static void setReferencePoint(CoordinatePoint.POINT point)
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            iStack.ReferencePoint = point; Stacks[No] = iStack;
        }
        public static void setReferencePoint(CoordinatePoint.POINT point, int No)
        {
            if (No < 0 || No > TotalStacks) { return; }

            STACK iStack = Stacks[No];
            iStack.ReferencePoint = point; Stacks[No] = iStack;
        }

        public static CoordinatePoint.POINT getReferencePoint()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return CoordinatePoint.getNegPoint(); }

            return Stacks[No].ReferencePoint;
        }
        public static CoordinatePoint.POINT getReferencePoint(int No)
        {
            if (No < 0 || No > TotalStacks) { return CoordinatePoint.getNegPoint(); }

            return Stacks[No].ReferencePoint;
        }
    }
}
