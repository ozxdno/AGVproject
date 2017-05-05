﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AGVproject.Class
{
    class HouseStack
    {
        ///////////////////////////////////////////// public attribute //////////////////////////////////////////////////

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

        /// <summary>
        /// 堆垛左右的靠墙通道默认宽度 单位：mm
        /// </summary>
        public static double DefaultAisleWidthLR;
        /// <summary>
        /// 堆垛前后的通道默认宽度 单位：mm
        /// </summary>
        public static double DefaultAisleWidthUD;
        /// <summary>
        /// 默认堆垛长度 单位：mm
        /// </summary>
        public static double DefaultStackLength;
        /// <summary>
        /// 默认堆垛宽度 单位：mm
        /// </summary>
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
            /// 堆垛左上角位置
            /// </summary>
            public CoordinatePoint.POINT Position;

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

        ///////////////////////////////////////////// private attribute //////////////////////////////////////////////////

        private static List<STACK> Stacks;
        private static CONFIG config;
        private struct CONFIG
        {
            public object StacksLock;
        }

        ///////////////////////////////////////////// Stacks //////////////////////////////////////////////////

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initial()
        {
            TotalStacksL = 0;
            TotalStacksR = 0;
            DefaultAisleWidthLR = 0;
            DefaultAisleWidthUD = 0;
            DefaultStackLength = 0;
            DefaultStackWidth = 0;

            Stacks = new List<STACK>();

            config.StacksLock = new object();
        }

        /// <summary>
        /// 获取默认堆垛参数
        /// </summary>
        public static void getDefaultStacks()
        {
            lock (config.StacksLock)
            {
                Stacks = new List<STACK>();

                STACK stack0 = new STACK();
                stack0.IsLeft = false;
                stack0.No = 0;
                stack0.Length = 2000;
                stack0.Width = 200;
                stack0.AisleWidth_L = (HouseMap.HouseWidth - stack0.Length) / 2;
                stack0.AisleWidth_R = (HouseMap.HouseWidth - stack0.Length) / 2;
                stack0.AisleWidth_U = 0;
                stack0.AisleWidth_D = 0;
                stack0.CarPosition = TH_AutoSearchTrack.Direction.Tuning;
                stack0.CarDirection = TH_AutoSearchTrack.Direction.Tuning;
                stack0.Distance = 0;
                stack0.ReferencePoint = new CoordinatePoint.POINT();
                stack0.KeepDistanceU = 0;
                stack0.KeepDistanceD = 0;
                stack0.KeepDistanceL = 0;
                stack0.KeepDistanceR = 0;
                stack0.Position = CoordinatePoint.Create_XY(stack0.AisleWidth_L, 0);
                Stacks.Add(stack0);

                double ptX = HouseMap.HouseWidth - DefaultStackLength - DefaultAisleWidthLR;
                double ptY = DefaultAisleWidthUD;

                for (int i = 1; i <= TotalStacksR; i++)
                {
                    STACK NewStack = new STACK();

                    NewStack.IsLeft = false;
                    NewStack.No = i;
                    NewStack.Length = DefaultStackLength;
                    NewStack.Width = DefaultStackWidth;
                    NewStack.Position = CoordinatePoint.Create_XY(ptX, ptY);
                    NewStack.AisleWidth_R = DefaultAisleWidthLR;
                    NewStack.AisleWidth_L = HouseMap.HouseWidth - 2 * DefaultAisleWidthLR - 2 * DefaultStackLength;
                    NewStack.AisleWidth_U = DefaultAisleWidthUD;
                    NewStack.AisleWidth_D = DefaultAisleWidthUD;
                    NewStack.CarPosition = TH_AutoSearchTrack.Direction.Tuning;
                    NewStack.CarDirection = TH_AutoSearchTrack.Direction.Tuning;
                    NewStack.Distance = 0;
                    NewStack.ReferencePoint = new CoordinatePoint.POINT();
                    NewStack.KeepDistanceU = NewStack.AisleWidth_U / 2;
                    NewStack.KeepDistanceD = NewStack.AisleWidth_D / 2;
                    NewStack.KeepDistanceL = NewStack.AisleWidth_L / 2;
                    NewStack.KeepDistanceR = NewStack.AisleWidth_R / 2;
                    Stacks.Add(NewStack);

                    ptY += DefaultStackWidth + DefaultAisleWidthUD;
                }

                ptX = DefaultAisleWidthLR;
                ptY = DefaultAisleWidthUD;

                for (int i = TotalStacks; i > TotalStacksR; i--)
                {
                    STACK NewStack = new STACK();

                    NewStack.IsLeft = true;
                    NewStack.No = i;
                    NewStack.Length = DefaultStackLength;
                    NewStack.Width = DefaultStackWidth;
                    NewStack.Position = CoordinatePoint.Create_XY(ptX, ptY);
                    NewStack.AisleWidth_L = DefaultAisleWidthLR;
                    NewStack.AisleWidth_R = HouseMap.HouseWidth - 2 * DefaultAisleWidthLR - 2 * DefaultStackLength;
                    NewStack.AisleWidth_U = DefaultAisleWidthUD;
                    NewStack.AisleWidth_D = DefaultAisleWidthUD;
                    NewStack.CarPosition = TH_AutoSearchTrack.Direction.Tuning;
                    NewStack.CarDirection = TH_AutoSearchTrack.Direction.Tuning;
                    NewStack.Distance = 0;
                    NewStack.ReferencePoint = new CoordinatePoint.POINT();
                    NewStack.KeepDistanceU = NewStack.AisleWidth_U / 2;
                    NewStack.KeepDistanceD = NewStack.AisleWidth_D / 2;
                    NewStack.KeepDistanceL = NewStack.AisleWidth_L / 2;
                    NewStack.KeepDistanceR = NewStack.AisleWidth_R / 2;
                    Stacks.Insert(TotalStacksR, NewStack);

                    ptY += DefaultStackWidth + DefaultAisleWidthUD;
                }
            }
        }

        /// <summary>
        /// 清除堆垛信息
        /// </summary>
        public static void Clear()
        {
            lock (config.StacksLock) { Stacks.Clear(); }
        }
        /// <summary>
        /// 获取整个堆垛的信息
        /// </summary>
        /// <returns></returns>
        public static List<STACK> Get()
        {
            if (Stacks == null) { return new List<STACK>(); }

            List<STACK> stacks = new List<STACK>();
            lock (config.StacksLock) { foreach (STACK i in Stacks) { stacks.Add(i); } }
            return stacks;
        }
        /// <summary>
        /// 设置整个堆垛的信息
        /// </summary>
        /// <param name="stacks">堆垛信息</param>
        public static void Set(List<STACK> stacks)
        {
            lock (config.StacksLock) { Stacks = stacks; }
        }

        /// <summary>
        /// 添加堆垛
        /// </summary>
        /// <param name="stack">堆垛参数</param>
        public static void addStack(STACK stack)
        {
            lock (config.StacksLock) { Stacks.Add(stack); }
        }
        /// <summary>
        /// 插入堆垛
        /// </summary>
        /// <param name="No">插入位置</param>
        /// <param name="stack">堆垛参数</param>
        public static void addStack(int No, STACK stack)
        {
            if (No < 0 || No > TotalStacks) { return; }
            lock (config.StacksLock) { Stacks.Insert(No, stack); }
        }
        /// <summary>
        /// 删除堆垛
        /// </summary>
        /// <param name="No"></param>
        public static void delStack(int No)
        {
            if (No < 0 || No > TotalStacks) { return; }
            lock (config.StacksLock) { Stacks.RemoveAt(No); }
        }
        /// <summary>
        /// 获取堆垛信息
        /// </summary>
        /// <param name="No">堆垛编号</param>
        /// <returns></returns>
        public static STACK getStack(int No)
        {
            STACK stack = new STACK();
            stack.No = -1;
            if (No < 0 || No > TotalStacks) { return stack; }

            lock (config.StacksLock) { stack = Stacks[No]; }
            return stack;
        }
        /// <summary>
        /// 重设堆垛信息
        /// </summary>
        /// <param name="No">堆垛编号</param>
        /// <param name="stack">堆垛参数</param>
        public static void setStack(int No, STACK stack)
        {
            if (No < 0 || No > TotalStacks) { return; }
            lock (config.StacksLock) { Stacks[No] = stack; }
        }


        public static bool getIsLeft(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return false; }

            return stack.IsLeft;
        }
        public static double getLength(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.Length;
        }
        public static double getWidth(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.Width;
        }
        public static CoordinatePoint.POINT getPosition(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return new CoordinatePoint.POINT(); }

            return stack.Position;
        }
        public static double getPositionX(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.Position.x;
        }
        public static double getPositionY(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.Position.y;
        }
        public static double getAisleWidthU(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.AisleWidth_U;
        }
        public static double getAisleWidthD(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.AisleWidth_D;
        }
        public static double getAisleWidthL(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.AisleWidth_L;
        }
        public static double getAisleWidthR(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.AisleWidth_R;
        }
        public static TH_AutoSearchTrack.Direction getCarPosition(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return TH_AutoSearchTrack.Direction.Tuning; }

            return stack.CarPosition;
        }
        public static TH_AutoSearchTrack.Direction getCarDirection(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return TH_AutoSearchTrack.Direction.Tuning; }

            return stack.CarDirection;
        }
        public static double getDistance(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.Distance;
        }
        public static CoordinatePoint.POINT getReferencePoint(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return new CoordinatePoint.POINT(); }

            return stack.ReferencePoint;
        }
        public static double getReferencePointX(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.ReferencePoint.x;
        }
        public static double getReferencePointY(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.ReferencePoint.y;
        }
        public static double getKeepDistanceU(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.KeepDistanceU;
        }
        public static double getKeepDistanceD(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.KeepDistanceD;
        }
        public static double getKeepDistanceL(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.KeepDistanceL;
        }
        public static double getKeepDistanceR(int No)
        {
            STACK stack = getStack(No);
            if (stack.No == -1) { return -1; }

            return stack.KeepDistanceR;
        }
    }
}