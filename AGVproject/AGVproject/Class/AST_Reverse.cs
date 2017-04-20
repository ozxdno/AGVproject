using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Reverse
    {
        private static ProcessStateInfo PSI;

        private struct ProcessStateInfo
        {
            // 公共变量
            public bool DoingSubAction;
            public int No_SubAction;

            public TH_AutoSearchTrack.PD_PARAMETER PD_F, PD_T, PD_R;
            public double TargetPos;
        }

        public static void Start()
        {
            #region 初始化配置信息

            PSI.DoingSubAction = false;
            PSI.No_SubAction = 0;

            #endregion

            #region 掉头

            PSI.DoingSubAction = true;
            PSI.No_SubAction++;
            TH_AutoSearchTrack.Initial_PD_parameter(1.0, 0.0, ref PSI.PD_F);
            TH_AutoSearchTrack.Initial_PD_parameter(1.0, 0.0, ref PSI.PD_T);
            TH_AutoSearchTrack.Initial_PD_parameter(1.0, 0.0, ref PSI.PD_R);

            AST_KeepSpeed.PD_F = PSI.PD_F;
            AST_KeepSpeed.PD_T = PSI.PD_T;
            AST_KeepSpeed.PD_R = PSI.PD_R;

            PSI.TargetPos = TH_AutoSearchTrack.getPosition().a + 180;

            while (PSI.DoingSubAction)
            {
                // 是否执行本子动作
                if (TH_AutoSearchTrack.control.SubAction > PSI.No_SubAction) { PSI.DoingSubAction = false; break; }

                // 紧急动作
                if (TH_AutoSearchTrack.control.EMA) { return; }

                // 是否满足退出条件
                double current = TH_AutoSearchTrack.getPosition().a;
                if (Math.Abs(current - PSI.TargetPos) < 1) { PSI.DoingSubAction = false; break; }

                // 控制
                int RotateSpeed = AST_KeepSpeed.getTranslateSpeed(TH_AutoSearchTrack.control.MaxSpeed_Rotate);
                TH_SendCommand.AGV_MoveControl_0x70(0, 0, RotateSpeed);
            }

            #endregion

            #region 填充下一个动作

            if (TH_AutoSearchTrack.control.ActionList.Count != 0)
            { TH_AutoSearchTrack.control.ActionList.RemoveAt(0); }

            if (TH_AutoSearchTrack.control.ActionList.Count == 0)
            {
                if (TH_AutoSearchTrack.control.NearStack == 1) { TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Return); }
                TH_AutoSearchTrack.control.ActionList.Add(TH_AutoSearchTrack.Action.Forward);
            }

            #endregion
        }
    }
}
