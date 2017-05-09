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
        /// 小车行进过程所使用的方法
        /// </summary>
        public static AST_Process ProcessHandle;
        /// <summary>
        /// 小车行进过程所使用的方法
        /// </summary>
        public delegate void AST_Process();

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
            /// <summary>
            /// Y 方向最大允许速度
            /// </summary>
            public int MaxSpeed_Y;
            /// <summary>
            /// A 方向最大允许速度
            /// </summary>
            public int MaxSpeed_A;
            
            /// <summary>
            /// 正在执行的动作
            /// </summary>
            public Action Action;
            /// <summary>
            /// 紧急动作发生标志
            /// </summary>
            public bool EMA;
            /// <summary>
            /// 当前小车靠近的堆垛编号
            /// </summary>
            public int StackNo;
            /// <summary>
            /// 当前小车的路径编号
            /// </summary>
            public int TrackNo;
            /// <summary>
            /// 事件通知，在界面中提示当前小车的状态
            /// </summary>
            public string Event;
            /// <summary>
            /// 通知事件时，所用字体颜色
            /// </summary>
            public System.Drawing.Color EventColor;
            
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
        public enum Action
        {
            /// <summary>
            /// 一切正常
            /// </summary>
            Normal,
            /// <summary>
            /// 一直等待，直到此命令被清除
            /// </summary>
            Wait,
            /// <summary>
            /// 扫描并建立地图
            /// </summary>
            Scan,
            /// <summary>
            /// 退出通道
            /// </summary>
            OutAisle,
            /// <summary>
            /// 返回起点
            /// </summary>
            Return,
            /// <summary>
            /// 切换手动控制
            /// </summary>
            ByHand,
            /// <summary>
            /// 暂停
            /// </summary>
            Stop,
            /// <summary>
            /// 继续
            /// </summary>
            Continue,
            /// <summary>
            /// 退出
            /// </summary>
            Abort,
            /// <summary>
            /// 出错
            /// </summary>
            Error
        }
        /// <summary>
        /// 方向或位置信息
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// 在左边或者向左
            /// </summary>
            Left,
            /// <summary>
            /// 在右边或者向右
            /// </summary>
            Right,
            /// <summary>
            /// 在上边或者向上
            /// </summary>
            Up,
            /// <summary>
            /// 在下边或者向下
            /// </summary>
            Down,
            /// <summary>
            /// 方向未知或者正在调整
            /// </summary>
            Tuning
        }

        ////////////////////////////////////////// private attribute ///////////////////////////////////////////////
        
        ////////////////////////////////////////// public method ///////////////////////////////////////////////
       
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initial()
        {
            ProcessHandle = Solution_FollowTrack.FollowTrack.Start;

            control.Action = Action.Normal;
            control.EMA = false;
            control.StackNo = 0;
            control.TrackNo = 0;
            control.Event = "Normal";
            control.EventColor = System.Drawing.Color.Black;
        }

        /// <summary>
        /// 启动行进线程
        /// </summary>
        public static void Start()
        {
            // 开线程
            control.Abort = false;
            control.Thread = new System.Threading.Thread(AST);
            control.Thread.Start();
        }
        /// <summary>
        /// 复位行进线程
        /// </summary>
        public static void Restart()
        {
            // 关闭线程
            control.Abort = true;
            while (control.Thread != null && control.Thread.ThreadState == System.Threading.ThreadState.Running) ;
            control.Abort = false;

            // 初始化
            Initial();
            
            // 开线程
            control.Abort = false;
            control.Thread = new System.Threading.Thread(AST);
            control.Thread.Start();
        }

        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        public static void Load()
        {
            Initial();

            control.MinDistance_H = Configuration.getFieldValue1_DOUBLE("AST.MinDistance_H");
            control.MinDistance_T = Configuration.getFieldValue1_DOUBLE("AST.MinDistance_T");
            control.MinDistance_L = Configuration.getFieldValue1_DOUBLE("AST.MinDistance_L");
            control.MinDistance_R = Configuration.getFieldValue1_DOUBLE("AST.MinDistance_R");

            control.MaxSpeed_X = Configuration.getFieldValue1_INT("AST.MaxSpeed_X");
            control.MaxSpeed_Y = Configuration.getFieldValue1_INT("AST.MaxSpeed_Y");
            control.MaxSpeed_A = Configuration.getFieldValue1_INT("AST.MaxSpeed_A");
        }
        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        public static void Save()
        {
            Configuration.setFieldValue("AST.MinDistance_H", control.MinDistance_H);
            Configuration.setFieldValue("AST.MinDistance_T", control.MinDistance_T);
            Configuration.setFieldValue("AST.MinDistance_L", control.MinDistance_L);
            Configuration.setFieldValue("AST.MinDistance_R", control.MinDistance_R);

            Configuration.setFieldValue("AST.MaxSpeed_X", control.MaxSpeed_X);
            Configuration.setFieldValue("AST.MaxSpeed_Y", control.MaxSpeed_Y);
            Configuration.setFieldValue("AST.MaxSpeed_A", control.MaxSpeed_A);
        }

        ////////////////////////////////////////// private method ///////////////////////////////////////////////

        private static void AST()
        {
            while (true)
            {
                // 检测线程状态
                if (control.Abort) { control.Thread.Abort();  control.Abort = false; return; }
                
                // 检测串口状态
                if (!TH_SendCommand.IsOpen)
                {
                    control.Event = "Error: Control Port Closed !";
                    control.EventColor = System.Drawing.Color.Red;
                    //control.Thread.Abort();
                    //control.Abort = false; return;
                    control.Action = Action.Error;
                    //continue;
                }
                if (!TH_MeasureSurrounding.IsOpen)
                {
                    control.Event = "Error: URG Port Closed !";
                    control.EventColor = System.Drawing.Color.Red;
                    //control.Thread.Abort();
                    //control.Abort = false; return;
                    control.Action = Action.Error;
                    //continue;
                }
                if (!TH_MeasurePosition.IsOpen)
                {
                    control.Event = "Error: Locate Port Closed !";
                    control.EventColor = System.Drawing.Color.Red;
                    //control.Thread.Abort();
                    //control.Abort = false; return;
                    control.Action = Action.Error;
                    //continue;
                }

                ProcessHandle();

                // 测试自动记录路径并按路径行进
                //Solution_FollowTrack.BuildTrack.Start();
                //Solution_FollowTrack.FollowTrack.Start();

                // 取动作序列，并控制按动作序列行进
                //if (control.Action == Action.Normal) { control.EMA = false; }
                //if (control.Action == Action.Wait) { continue; }
                //if (control.Action == Action.OutAisle) {  }
                //if (control.Action == Action.Return) { }
                //if (control.Action == Action.ByHand) {  }
                //if (control.Action == Action.Stop) { continue; }
                //if (control.Action == Action.Continue) { control.EMA = false; control.Action = Action.Normal; }
                //if (control.Action == Action.Abort) { control.Thread.Abort(); control.Abort = false; return; }
                //if (control.Action == Action.Error) { continue; }
            }
        }
    }
}
