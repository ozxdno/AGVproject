using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGVproject.Class;

namespace AGVproject.Solution_SLAM.BuildMap
{
    /// <summary>
    /// 用环境特征来估计小车的移动量
    /// </summary>
    class MeasureMove
    {
        ///////////////////////////////////////////// public attribute ////////////////////////////////////////


        ///////////////////////////////////////////// private attribute ////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public List<Feature.Feature> LastFeatures;
            public List<Feature.Feature> NextFeatures;

            public List<FACTORS> Factors;

            public int xIndex;
            public int yIndex;
            public Feature.Feature xFeature;
            public Feature.Feature yFeature;
        }
        private struct FACTORS
        {
            public int Last;
            public int Next;
            public double Factor;
        }

        ///////////////////////////////////////////// public method ////////////////////////////////////////

        /// <summary>
        /// 利用前后两次的激光雷达数据来估计小车的位移量
        /// </summary>
        /// <returns></returns>
        public static FusionMove.MOVE Start()
        {
            FusionMove.MOVE move = new FusionMove.MOVE();
            move.xInvalid = true;
            move.yInvalid = true;
            move.aInvalid = true;

            if (config.LastFeatures == null || config.NextFeatures == null) { return move; }

            int N_Last = config.LastFeatures.Count;
            int N_Next = config.NextFeatures.Count;
            if (N_Last ==0 || N_Next < 10) { return move; }

            Feature.ExtractFeatures.Start();
            Feature.ExtractFeatures.CopyFeatures(ref config.NextFeatures);

            getFeatures();


            Feature.ExtractFeatures.CopyFeatures(config.LastFeatures, ref config.NextFeatures);
            return move;
        }

        ///////////////////////////////////////////// private method ////////////////////////////////////////
        
        private static void sortFeatures()
        {
            int N = config.NextFeatures.Count;

            for (int i = 1; i < N; i++)
            {
                for (int j = 0; j < N - i; j++)
                {
                    if (config.NextFeatures[i].N >= config.NextFeatures[i + 1].N) { continue; }
                    Feature.Feature TempF = config.NextFeatures[i];
                    config.NextFeatures[i] = config.NextFeatures[i + 1];
                    config.NextFeatures[i + 1] = TempF;
                }
            }
        }
        private static void getMatch()
        {
            config.Factors = new List<FACTORS>();

            Feature.Feature match = new Feature.Feature();
            int i = 0, index = -1;

            while (config.Factors.Count != config.NextFeatures.Count)
            {
                Feature.MatchFeature.Features = config.LastFeatures;
                double Factor = Feature.MatchFeature.Start(config.NextFeatures[i], ref index, ref match);
                bool Invalid = Feature.MatchFeature.IsInvalid(Factor);

                if (Invalid) { config.NextFeatures.RemoveAt(i);continue; }

                FACTORS factor = new FACTORS();
                factor.Last = index;
                factor.Next = i;
                factor.Factor = Factor;
                config.Factors.Add(factor); i++;
            }
        }
        private static void getFeatures()
        {
            // 排序并匹配特征
            sortFeatures(); getMatch();

            // 不存在任何特征
            config.xFeature.Type = Feature.TYPE.Invaild;
            config.yFeature.Type = Feature.TYPE.Invaild;
            config.xIndex = -1;
            config.yIndex = -1;
            if (config.NextFeatures.Count == 0) { return; }

            // 第一个特征
            if (config.NextFeatures[0].xA > 45) { config.yFeature = config.NextFeatures[0]; config.yIndex = 0; getFeatureX(); }
            else { config.xFeature = config.NextFeatures[0]; config.xIndex = 0; getFeatureY(); }
        }
        private static void getFeatureX()
        {
            double minAngle = double.MaxValue;
            int index = -1;

            for (int i = 1; i < config.NextFeatures.Count; i++)
            {
                double angle = Math.Abs(config.NextFeatures[i].xA);
                if (angle < minAngle) { minAngle = angle; index = i; }
            }

            config.xIndex = index;
            if (index != -1) { config.xFeature = config.NextFeatures[index]; return; }

            config.xFeature = new Feature.Feature();
            config.xFeature.Type = Feature.TYPE.Invaild;
        }
        private static void getFeatureY()
        {
            double minAngle = double.MaxValue;
            int index = -1;

            for (int i = 1; i < config.NextFeatures.Count; i++)
            {
                double angle = Math.Abs(config.NextFeatures[i].yA);
                if (angle < minAngle) { minAngle = angle; index = i; }
            }

            config.yIndex = index;
            if (index != -1) { config.yFeature = config.NextFeatures[index]; return; }

            config.yFeature = new Feature.Feature();
            config.yFeature.Type = Feature.TYPE.Invaild;
        }

        private static void getMoveX(ref FusionMove.MOVE move)
        {
            bool yInvalid = config.yFeature.Type == Feature.TYPE.Invaild || config.yIndex == -1;
            if (yInvalid) { move.xInvalid = true; return; }

            move.xInvalid = false;
        }
        private static void getMoveY(ref FusionMove.MOVE move)
        {

        }
        private static void getMoveA(ref FusionMove.MOVE move)
        {
            bool xInvalid = config.xFeature.Type == Feature.TYPE.Invaild || config.xIndex == -1;
            bool yInvalid = config.yFeature.Type == Feature.TYPE.Invaild || config.yIndex == -1;

            if (xInvalid && yInvalid) { move.aInvalid = true; return; }

            bool UseFeatureY = !yInvalid && config.yFeature.N > config.xFeature.N;
            
            move.aInvalid = false;
            move.a = UseFeatureY ?
                config.yFeature.xA - config.LastFeatures[config.yIndex].xA :
                config.xFeature.yA - config.LastFeatures[config.xIndex].yA;
        }
    }
}
