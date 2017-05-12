using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Solution_SLAM.Feature
{
    /// <summary>
    /// 对特征进行匹配
    /// </summary>
    class MatchFeature
    {
        /////////////////////////////////////////////// public attribute /////////////////////////////////////////

        /// <summary>
        /// 待匹配的特征
        /// </summary>
        public static List<Feature> Features;

        /////////////////////////////////////////////// private attribute /////////////////////////////////////////

        private static double SamilarFactor = 100;
        private struct ERROR
        {
            public double eLength;
            public double eDirection;
            public double eAngleP;
            public double eAngleN;

            public double eK;
            public double eA;
            public double eB;
            public double eD;

            public double Factor;
        }

        /////////////////////////////////////////////// public method /////////////////////////////////////////

        /// <summary>
        /// 比较两个特征的相似程度，返回匹配误差
        /// </summary>
        /// <param name="feature1">特征 1</param>
        /// <param name="feature2">特征 2</param>
        /// <returns></returns>
        public static double Start(Feature feature1, Feature feature2)
        {
            ERROR error = getError(feature1, feature2); return error.Factor;
        }
        /// <summary>
        /// 在待匹配特征中寻找与目标特征最匹配的特征，返回匹配误差
        /// </summary>
        /// <param name="targetFeature">目标特征</param>
        /// <param name="matchFeature">寻找到的匹配特征</param>
        /// <returns></returns>
        public static double Start(Feature targetFeature, ref Feature matchFeature)
        {
            // 源特征不存在
            if (Features == null || Features.Count == 0)
            { matchFeature = new Feature(); return double.PositiveInfinity; }

            // 获取误差
            List<ERROR> Errors = new List<ERROR>();
            foreach (Feature f in Features) { Errors.Add(getError(f, targetFeature)); }

            // 寻找最小误差项
            int index = -1; double minError = double.MaxValue;
            for (int i = 0; i < Errors.Count; i++)
            {
                if (Errors[i].Factor < minError) { index = i; minError = Errors[i].Factor; }
            }

            // 返回找到的最小误差项
            matchFeature = Features[index]; return Errors[index].Factor;
        }

        /// <summary>
        /// 判断具有该匹配误差的两个特征是否是同一特征
        /// </summary>
        /// <param name="Factor">匹配误差</param>
        /// <returns></returns>
        public static bool IsSamilar(double Factor)
        {
            return Factor < SamilarFactor;
        }

        /////////////////////////////////////////////// private method /////////////////////////////////////////
        
        private static ERROR getError(Feature sour, Feature dest)
        {
            ERROR error = new ERROR();

            error.eLength = Math.Abs(dest.Length - sour.Length);
            error.eDirection = Math.Abs((dest.DirectionBG + dest.DirectionED) / 2 - (sour.DirectionBG + sour.DirectionED) / 2);
            error.eAngleP = Math.Abs(dest.AngleP - sour.AngleP);
            error.eAngleN = Math.Abs(dest.AngleN - sour.AngleN);
            
            error.eK = Math.Abs(dest.K - sour.K);
            error.eA = Math.Abs(dest.A - sour.A);
            error.eB = Math.Abs(dest.B - sour.B);
            error.eD = Math.Abs(dest.D - sour.D);

            getFactor(ref error); return error;
        }
        private static void getFactor(ref ERROR error)
        {
            double K_len = 1.0;
            double K_dir = 1.0;
            double K_ap = 1.0;
            double K_an = 1.0;
            double K_a = 1.0;
            double K_b = 1.0;
            double K_d = 1.0;

            error.Factor =
                K_len * error.eLength +
                K_dir * error.eDirection +
                K_ap * error.eAngleP +
                K_an * error.eAngleN +
                K_a * error.eA +
                K_b * error.eB +
                K_d * error.eD;
        }
    }
}
