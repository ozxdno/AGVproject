using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    class BuildRoute
    {
        public struct ROUTE
        {
            /// <summary>
            /// 小车当前位置
            /// </summary>
            public CoordinatePoint.POINT Current;
            /// <summary>
            /// 小车目标位置
            /// </summary>
            public CoordinatePoint.POINT Target;

            /// <summary>
            /// 目标点校准信息
            /// </summary>
            public AST_CorrectPosition.CORRECT Correct;
        }
    }
}
