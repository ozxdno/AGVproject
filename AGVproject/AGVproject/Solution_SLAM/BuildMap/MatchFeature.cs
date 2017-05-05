using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 对某一特征进行匹配（只适用于小步伐移动）
    /// </summary>
    class MatchFeature
    {
        /// <summary>
        /// 待匹配的特征
        /// </summary>
        public static List<Feature.Feature> Features;

        /// <summary>
        /// 比较两个特征的相似程度，认为是同一特征返回 True
        /// </summary>
        /// <param name="feature1">特征 1</param>
        /// <param name="feature2">特征 2</param>
        /// <returns></returns>
        public static bool Start(Feature.Feature feature1, Feature.Feature feature2)
        {
            ERROR error = getError(feature1, feature2); return error.Samilar;
        }
        /// <summary>
        /// 在待匹配特征中寻找与目标特征最匹配的特征，找到了返回 True
        /// </summary>
        /// <param name="targetFeature">目标特征</param>
        /// <param name="matchFeature">寻找到的匹配特征</param>
        /// <returns></returns>
        public static bool Start(Feature.Feature targetFeature, ref Feature.Feature matchFeature)
        {
            // 源特征不存在
            if (Features == null || Features.Count == 0)
            { matchFeature = new Feature.Feature(); return false; }

            // 获取误差
            List<ERROR> Errors = new List<ERROR>();
            foreach (Feature.Feature f in Features) { Errors.Add(getError(f, targetFeature)); }

            // 寻找最小误差项
            int index = -1; double minError = double.MaxValue;
            for (int i = 0; i < Errors.Count; i++)
            {
                if (Errors[i].Factor < minError) { index = i; minError = Errors[i].Factor; }
            }

            // 返回找到的最小误差项
            matchFeature = Features[index]; return Errors[index].Samilar;
        }

        private static Feature.Feature LastFeature;
        private static Feature.Feature NextFeature;
        private struct ERROR
        {
            public double eLength;
            public double eDirection;
            public double eA;
            public double eD;

            public double Factor;
            public bool Samilar;
        }

        private static ERROR getError(Feature.Feature sour, Feature.Feature dest)
        {
            ERROR error = new ERROR();

            error.eLength = Math.Abs(dest.Length - sour.Length);
            error.eDirection = Math.Abs((dest.DirectionBG + dest.DirectionED) / 2 - (sour.DirectionBG + sour.DirectionED) / 2);
            error.eA = Math.Abs(dest.A - sour.A);
            error.eD = Math.Abs(dest.D - sour.D);

            getFactor(ref error);
            error.Samilar = error.Factor < 30;
            return error;
        }
        private static void getFactor(ref ERROR error)
        {
            double K_len = 1.0;
            double K_dir = 1.0;
            double K_a = 1.0;
            double K_d = 1.0;

            error.Factor = K_len * error.eLength + K_dir * error.eDirection + K_a * error.eA + K_d * error.eD;
        }
    }
}
