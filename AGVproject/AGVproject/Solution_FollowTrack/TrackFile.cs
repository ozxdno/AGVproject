using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_FollowTrack
{
    class TrackFile
    {
        /// <summary>
        /// 保存路径中的 Extra（CORRECT） 信息
        /// </summary>
        /// <param name="Value">Extra 信息</param>
        public static void Save(object Value)
        {
            CorrectPosition.CORRECT correct = Value == null ?
                new CorrectPosition.CORRECT() : (CorrectPosition.CORRECT)Value;

            Configuration.setFieldValue("xInvalid", correct.xInvalid);
            Configuration.setFieldValue("xK", correct.xK);
            Configuration.setFieldValue("xL", correct.xL);
            Configuration.setFieldValue("xA", correct.xA);
            Configuration.setFieldValue("xB", correct.xB);
            Configuration.setFieldValue("xC", correct.xC);
            Configuration.setFieldValue("xD", correct.xD);
            Configuration.setFieldValue("x1", correct.x1);
            Configuration.setFieldValue("x2", correct.x2);

            Configuration.setFieldValue("yInvalid", correct.yInvalid);
            Configuration.setFieldValue("yK", correct.yK);
            Configuration.setFieldValue("yL", correct.yL);
            Configuration.setFieldValue("yA", correct.yA);
            Configuration.setFieldValue("yB", correct.yB);
            Configuration.setFieldValue("yC", correct.yC);
            Configuration.setFieldValue("yD", correct.yD);
            Configuration.setFieldValue("y1", correct.y1);
            Configuration.setFieldValue("y2", correct.y2);
        }
        /// <summary>
        /// 读取路径文件中的 Extra（CORRECT） 信息
        /// </summary>
        /// <returns></returns>
        public static object Load()
        {
            CorrectPosition.CORRECT correct = new CorrectPosition.CORRECT();

            correct.xInvalid = Configuration.getFieldValue1_BOOL("xInvalid");
            correct.xK = Configuration.getFieldValue1_DOUBLE("xK");
            correct.xL = Configuration.getFieldValue1_DOUBLE("xL");
            correct.xA = Configuration.getFieldValue1_DOUBLE("xA");
            correct.xB = Configuration.getFieldValue1_DOUBLE("xB");
            correct.xC = Configuration.getFieldValue1_DOUBLE("xC");
            correct.xD = Configuration.getFieldValue1_DOUBLE("xD");
            correct.x1 = Configuration.getFieldValue1_DOUBLE("x1");
            correct.x2 = Configuration.getFieldValue1_DOUBLE("x2");

            correct.yInvalid = Configuration.getFieldValue1_BOOL("yInvalid");
            correct.yK = Configuration.getFieldValue1_DOUBLE("yK");
            correct.yL = Configuration.getFieldValue1_DOUBLE("yL");
            correct.yA = Configuration.getFieldValue1_DOUBLE("yA");
            correct.yB = Configuration.getFieldValue1_DOUBLE("yB");
            correct.yC = Configuration.getFieldValue1_DOUBLE("yC");
            correct.yD = Configuration.getFieldValue1_DOUBLE("yD");
            correct.y1 = Configuration.getFieldValue1_DOUBLE("y1");
            correct.y2 = Configuration.getFieldValue1_DOUBLE("y2");

            return correct;
        }
    }
}
