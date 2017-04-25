using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    class AST_Forward
    {
        public static void Scan()
        {
            // 对准通道口
            AST_AlignAisle.ApproachX = false;
            AST_AlignAisle.ApproachY = false;

            while (!AST_AlignAisle.ApproachX && !AST_AlignAisle.ApproachY)
            {
                int xSpeed = AST_AlignAisle.getSpeedX(HouseMap.DefaultAisleWidth / 2);
                int ySpeed = AST_AlignAisle.getSpeedY(Hardware_PlatForm.Width);

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, 0);
            }

            //前进
            AST_GuideBySurrounding.ApproachY = false;
            bool SetAisleWidthFinished = false;
            bool RecordedStartPos = false;
            bool RecordedEndPos = false;
            CoordinatePoint.POINT posBG = CoordinatePoint.getNegPoint();
            CoordinatePoint.POINT posED = CoordinatePoint.getNegPoint();

            while (!AST_GuideBySurrounding.ApproachY)
            {
                // 记录起点
                if (!RecordedStartPos)
                {
                    List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(170, 180);
                    bool EmptyL = TH_MeasureSurrounding.IsEmptyX(HouseMap.DefaultAisleWidth, points);
                    if (!EmptyL) { posBG = TH_MeasurePosition.getPosition(); RecordedStartPos = true; }
                }

                // 记录通道宽度
                if (!SetAisleWidthFinished) { SetAisleWidthFinished = HouseMap.setAisleWidth(); }

                // 记录终点
                if (!RecordedEndPos)
                {
                    List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingA(170, 180);
                    bool EmptyL = TH_MeasureSurrounding.IsEmptyX(HouseMap.DefaultAisleWidth, points);
                    if (EmptyL) { posED = TH_MeasurePosition.getPosition(); RecordedEndPos = true; }
                }

                int xSpeed = AST_GuideBySurrounding.getSpeedX_KeepL_Forward(HouseMap.DefaultAisleWidth / 2);
                int ySpeed = AST_GuideBySurrounding.getSpeedY_KeepU(TH_AutoSearchTrack.control.MinDistance_H);
                int aSpeed = AST_GuideBySurrounding.getSpeedA_KeepL_Forward();

                TH_SendCommand.AGV_MoveControl_0x70(xSpeed, ySpeed, aSpeed);
            }


        }
    }
}
