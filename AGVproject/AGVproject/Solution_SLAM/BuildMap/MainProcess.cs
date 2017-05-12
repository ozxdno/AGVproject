using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 自动建图主函数
    /// </summary>
    class MainProcess
    {
        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        ////////////////////////////////////////////// private attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public CoordinatePoint.POINT LastPosition;
            public CoordinatePoint.POINT NextPosition;
        }

        ////////////////////////////////////////////// public method ///////////////////////////////////////////

        public static void Start()
        {
            // 初始化
            UpdataMap.InitBuildMap();

            AST_GuideBySpeed.ApproachX = false;
            AST_GuideBySpeed.ApproachY = false;
            AST_GuideBySpeed.ApproachA = false;

            config.LastPosition = TH_MeasurePosition.getPosition();

            while (true)
            {
                ScanMethod.Start();
                config.NextPosition = TH_MeasurePosition.getPosition();

                MeasureMove.Start();
            }
        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////
    }
}
