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
        
        public static List<STACK> Stacks = new List<STACK>();

        /// <summary>
        /// 背对仓库门口，左手边堆垛数量
        /// </summary>
        public static int TotalStacksL;
        public static int TotalStacksR;
        public static int TotalStacks { get { return TotalStacksL + TotalStacksR; } }

        public static double DefaultCentreRoadWidth;
        public static double DefaultAisleWidth;
        public static double DefaultStackLength;
        public static double DefaultStackWidth;

        /// <summary>
        /// 每个像素点代表的实际长度 单位：mm
        /// </summary>
        public static double PixLength;
        
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
            /// 该堆垛上方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_L;
            /// <summary>
            /// 该堆垛下方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_R;
            /// <summary>
            /// 该堆垛左方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_U;
            /// <summary>
            /// 该堆垛右方的通道宽度 单位：mm
            /// </summary>
            public double AisleWidth_D;
            
            /// <summary>
            /// 车与垛区的相对位置（上、下、左、右、无法确定）
            /// </summary>
            public TH_AutoSearchTrack.Direction CarPosition;
            /// <summary>
            /// 参考点仓库坐标
            /// </summary>
            public CoordinatePoint.POINT ReferencePoint;
            /// <summary>
            /// 当前点仓库坐标
            /// </summary>
            public CoordinatePoint.POINT CurrentPoint;

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

        public static void Initial()
        {
            Stacks = new List<STACK>();

            STACK stack0 = new STACK();
            stack0.IsLeft = false;
            stack0.No = 0;
            stack0.AisleWidth_R = (HouseWidth - DefaultCentreRoadWidth - DefaultStackLength * 2) / 2;
            stack0.AisleWidth_L = DefaultCentreRoadWidth;
            stack0.AisleWidth_U = DefaultAisleWidth;
            stack0.AisleWidth_D = DefaultAisleWidth;
            stack0.CarPosition = TH_AutoSearchTrack.Direction.Up;
            Stacks.Add(stack0);

            for (int i = 1; i <= TotalStacksL; i++)
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

                Stacks.Add(NewStack);
            }
            for (int i = TotalStacksL + 1; i <= TotalStacks; i++)
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

                Stacks.Add(NewStack);
            }
        }

        public static bool CarSideL_NearStack()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return false; }

            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Up && Stacks[No].IsLeft == true) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Down && Stacks[No].IsLeft == false) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Left && Stacks[No].IsLeft == false) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Right && Stacks[No].IsLeft == true) { return true; }
            return false;
        }
        public static bool CarSideR_NearStack()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return false; }

            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Up && Stacks[No].IsLeft == false) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Down && Stacks[No].IsLeft == true) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Left && Stacks[No].IsLeft == true) { return true; }
            if (Stacks[No].CarPosition == TH_AutoSearchTrack.Direction.Right && Stacks[No].IsLeft == false) { return true; }
            return false;
        }
        public static bool ScanningL()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return false; }

            return Stacks[No].IsLeft;
        }
        public static bool ScanningR()
        {
            int No = TH_AutoSearchTrack.control.NearStack;
            if (No < 0 || No > TotalStacks) { return false; }

            return !Stacks[No].IsLeft;
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

        public static TH_AutoSearchTrack.Direction getDirection()
        {
            int No = TH_AutoSearchTrack.control.NearStack;

            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }
            return Stacks[No].CarPosition;
        }
        public static TH_AutoSearchTrack.Direction getDirection(int No)
        {
            if (No < 0 || No > TotalStacks) { return TH_AutoSearchTrack.Direction.Tuning; }
            return Stacks[No].CarPosition;
        }

        public static void setDirection(TH_AutoSearchTrack.Direction direction)
        {
            int No = TH_AutoSearchTrack.control.NearStack;

            if (No < 0 || No > TotalStacks) { return; }
            STACK iStack = Stacks[No];
            iStack.CarPosition = direction; Stacks[No] = iStack;
        }
        public static void setDirection(TH_AutoSearchTrack.Direction direction, int No)
        {
            if (No < 0 || No > TotalStacks) { return; }
            STACK iStack = Stacks[No];
            iStack.CarPosition = direction; Stacks[No] = iStack;
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
