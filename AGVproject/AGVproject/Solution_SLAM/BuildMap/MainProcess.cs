using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

// 1 编码器卡尔曼滤波
// 2 激光雷达突变滤波

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 自动建图主函数
    /// </summary>
    class MainProcess
    {
        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        /// <summary>
        /// 控制周期起始点坐标
        /// </summary>
        public static CoordinatePoint.POINT ptBG;
        /// <summary>
        /// 控制周期终止点坐标
        /// </summary>
        public static CoordinatePoint.POINT ptED;

        ////////////////////////////////////////////// private attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
        }

        ////////////////////////////////////////////// public method ///////////////////////////////////////////

        public static void Start()
        {
            // 初始化
            UpdataMap.InitBuildMap();

            AST_GuideBySpeed.ApproachX = false;
            AST_GuideBySpeed.ApproachY = false;
            AST_GuideBySpeed.ApproachA = false;

            ScanMethod.xSpeed = TH_AutoSearchTrack.control.MaxSpeed_X;
            ScanMethod.ySpeed = TH_AutoSearchTrack.control.MaxSpeed_Y;
            ScanMethod.aSpeed = TH_AutoSearchTrack.control.MaxSpeed_A;
            
            while (true)
            {
                ptBG = TH_MeasurePosition.getPosition();
                ScanMethod.Start();
                ptED = TH_MeasurePosition.getPosition();

                FusionMove.Urg = MeasureMove.Start();
                
            }
        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////
    }
}
