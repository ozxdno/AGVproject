using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class TH_AutoSearchTrack
    {
        ////////////////////////////////////////// public attribute ///////////////////////////////////////////////

        /// <summary>
        /// 小车控制过程所需要的变量集合
        /// </summary>
        public static AST_CONTROL control;

        /// <summary>
        /// 小车控制过程所需要的变量集合
        /// </summary>
        public struct AST_CONTROL
        {
            /// <summary>
            /// 距车头的允许最短安全距离，单位：mm
            /// </summary>
            public double MinDistance_H;
            /// <summary>
            /// 距车尾的允许最短安全距离，单位：mm
            /// </summary>
            public double MinDistance_T;
            /// <summary>
            /// 车左边的允许最短安全距离，单位：mm
            /// </summary>
            public double MinDistance_L;
            /// <summary>
            /// 车右边的允许最短安全距离，单位：mm
            /// </summary>
            public double MinDistance_R;

            /// <summary>
            /// X 方向最大允许速度
            /// </summary>
            public int MaxSpeed_X;
            public int MaxSpeed_Y;
            public int MaxSpeed_A;
            
            /// <summary>
            /// 动作列表，设置了小车的行动方案，此列表为空会自动填充
            /// </summary>
            public List<Action> ActionList;
            /// <summary>
            /// 下一个动作，取动作列表中的第一个动作
            /// </summary>
            public Action NextAction;
            /// <summary>
            /// 下一个动作的子动作，决定了从某个动作的某个子动作开始
            /// </summary>
            public int SubAction;
            /// <summary>
            /// 下一个待执行的子动作，由所属动作函数自动填充
            /// </summary>
            public int NextSubAction;
            /// <summary>
            /// 紧急动作发生标志
            /// </summary>
            public bool EMA;
            /// <summary>
            /// 当前小车靠近的堆垛编号
            /// </summary>
            public int NearStack;
            /// <summary>
            /// 事件通知，在界面中提示当前小车的状态
            /// </summary>
            public string Event;
            
            /// <summary>
            /// 小车在堆垛上端或者下端时，小车与堆垛所保持的距离 单位：mm
            /// </summary>
            public double KeepDistance_UD;
            /// <summary>
            /// 小车在堆垛的左边或者右边时，小车与堆垛所保持的距离 单位：mm
            /// </summary>
            public double KeepDistance_LR;

            /// <summary>
            /// 主控线程命令：关闭主控线程
            /// </summary>
            public bool Abort;
            /// <summary>
            /// 主控线程
            /// </summary>
            public System.Threading.Thread Thread;
        }

        /// <summary>
        /// 所有的动作集合
        /// </summary>
        public enum Action { Wait,Begin,Forward,Backward,Upward,Downward,RotateL,Reverse,RotateR,AlignF,AlignB,OutAisle,Return,ByHand,Stop,Continue,Abort,Error }
        /// <summary>
        /// 方向或位置信息
        /// </summary>
        public enum Direction { Left,Right,Up,Down,Tuning }

        ////////////////////////////////////////// private attribute ///////////////////////////////////////////////
        
        ////////////////////////////////////////// public method ///////////////////////////////////////////////
        
        public static void Start()
        {
            // 开线程
            control.Abort = false;
            control.Thread = new System.Threading.Thread(AST);
            control.Thread.Start();
        }
        public static void Restart()
        {
            // 关闭线程
            control.Abort = true;
            while (control.Thread != null && control.Thread.ThreadState == System.Threading.ThreadState.Running) ;
            control.Abort = false;

            // 加载参数
            Configuration.Load();

            // 清除动作列表
            control.NearStack = 0;
            control.ActionList.Clear();
            control.ActionList.Add(Action.AlignF);

            // 开线程
            control.Abort = false;
            control.Thread = new System.Threading.Thread(AST);
            control.Thread.Start();
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

            AisleWidth = minL + minR + Hardware_UltraSonic.xSpan;
            return true;
        }
        public static bool IsStopAction()
        {
            if (control.Abort) { control.SubAction = control.NextSubAction; return true; }
            if (control.EMA) { control.SubAction = control.NextSubAction; return true; }
            if (control.NextSubAction < control.SubAction) { return true; }
            return false;
        }
        
        ////////////////////////////////////////// private method ///////////////////////////////////////////////
        
        private static void AST()
        {
            while (true)
            {
                // 检测线程状态
                if (control.Abort) { control.Thread.Abort();  control.Abort = false; return; }
                
                // 检测串口状态
                if (!TH_SendCommand.IsOpen) { control.Event = "Control Port Closed !"; continue; }
                if (!TH_MeasureSurrounding.IsOpen) { control.Event = "URG Port Closed !"; continue; }
                if (!TH_MeasurePosition.IsOpen) { control.Event = "Locate Port Closed !"; continue; }

                // 测试

                while (false)
                {
                    int xSpeed = AST_GuideBySpeed.getSpeedX(0);
                    int ySpeed = AST_GuideBySpeed.getSpeedY(0);
                    int aSpeed = AST_GuideBySpeed.getSpeedA(0);

                    TH_SendCommand.AGV_MoveControl_0x70(0, 0, 100);
                }

                AST_GuideByPosition.ApproachX = false;
                AST_GuideByPosition.ApproachY = false;
                AST_GuideByPosition.ApproachA = false;

                AST_GuideByPosition.StartPosition = TH_MeasurePosition.getPosition();

                AST_GuideByPosition.TargetPosition.x = AST_GuideByPosition.StartPosition.x - 1000;
                AST_GuideByPosition.TargetPosition.y = AST_GuideByPosition.StartPosition.y - 1000;
                AST_GuideByPosition.TargetPosition.aCar = AST_GuideByPosition.StartPosition.aCar - 90;

                while (false)
                {
                    //int xSpeed = AST_GuideByPosition.getSpeedX();
                    //int ySpeed = AST_GuideByPosition.getSpeedY();
                    //int aSpeed = AST_GuideByPosition.getSpeedA();

                    //TH_SendCommand.AGV_MoveControl_0x70(xSpeed, 0, 0);
                }

                AST_GuideBySurrounding.ApproachX = false;
                AST_GuideBySurrounding.ApproachY = false;
                AST_GuideBySurrounding.ApproachA = false;

                while (true)
                {
                    int aSpeed = AST_GuideBySurrounding.getSpeedY_KeepD(400);

                    TH_SendCommand.AGV_MoveControl_0x70(0, 0, aSpeed);
                }

                // 取动作
                if (control.ActionList.Count == 0) { continue; }
                control.NextAction = control.ActionList[0];

                // 动作处理优先级
                if (control.NextAction == Action.Abort) { control.Thread.Abort(); control.ActionList.RemoveAt(0); return; }

                if (control.NextAction == Action.Continue) { continue; }
                if (control.NextAction == Action.Wait) { continue; }
                if (control.NextAction == Action.Stop) { continue; }
                if (control.NextAction == Action.OutAisle) { continue; }
                if (control.NextAction == Action.Return) { continue; }
                if (control.NextAction == Action.ByHand) { continue; }
                
                if (control.NextAction == Action.Begin) { continue; }


                

                
                //if (control.NextAction == Action.AlignF) { AST_AlignAisleForward.Start(); continue; }
                //if (control.NextAction == Action.AlignB) { AST_AlignAisleBackward.Start(); continue; }
                //if (control.NextAction == Action.Forward) { AST_Forward.Start(); continue; }
                //if (control.NextAction == Action.Backward) { AST_Backward.Start(); continue; }
                //if (control.NextAction == Action.Upward) { AST_Upward.Start(); continue; }
                //if (control.NextAction == Action.Downward) { AST_Downward.Start(); continue; }
                //if (control.NextAction == Action.RotateL) { AST_RotateL.Start(); continue; }
                //if (control.NextAction == Action.RotateR) { AST_RotateR.Start(); continue; }
                //if (control.NextAction == Action.Reverse) { AST_Reverse.Start(); continue; }
            }
        }
    }
}
