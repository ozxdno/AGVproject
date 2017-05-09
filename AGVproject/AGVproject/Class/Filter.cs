using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVproject.Class
{
    /// <summary>
    /// 跳变滤波器
    /// </summary>
    class AbruptFilter
    {
        /// <summary>
        /// 移除无效数据
        /// </summary>
        public bool RemoveNeg;
        /// <summary>
        /// 无效数据的填充值
        /// </summary>
        public double Fill;
        /// <summary>
        /// 最多可忽略连续点的个数
        /// </summary>
        public double NegAmount;
        /// <summary>
        /// 最大允许的跳变误差
        /// </summary>
        public double MaxError;

        /// <summary>
        /// 滤除输入数据中的跳变数据
        /// </summary>
        /// <param name="data">原数据</param>
        /// <returns></returns>
        public List<double> Start(List<double> data)
        {
            // 滤除跳变
            int N = data.Count;

            List<double> diff = new List<double>();
            for (int i = 1; i < N; i++) { diff.Add(Math.Abs(data[i] - data[i - 1])); }

            List<int> P = new List<int>();
            for (int i = 0; i < N - 1; i++) { if (diff[i] > MaxError) { P.Add(i); } }

            // 填充默认值
            for (int i = 0; i < P.Count - 1; i++)
            {
                if (P[i + 1] - P[i] > NegAmount) { continue; }
                for (int j = P[i] + 1; j <= P[i + 1]; j++) { data[j] = Fill; }
            }

            // 是否删除无效数据
            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (!RemoveNeg) { break; }
                if (data[i] == Fill) { data.RemoveAt(i); }
            }

            // 返回
            return data;
        }

        /// <summary>
        /// 滤除目标点中 X 值跳变的点
        /// </summary>
        /// <param name="points">目标点</param>
        /// <param name="negAmount">最多可忽略连续点的个数</param>
        /// <param name="maxError">最大允许的跳变误差</param>
        /// <returns></returns>
        public List<CoordinatePoint.POINT> FilterX(List<CoordinatePoint.POINT> points, double negAmount, double maxError)
        {
            if (points == null || points.Count == 0)
            { points = new List<CoordinatePoint.POINT>(); return points; }

            RemoveNeg = false; Fill = double.NaN; NegAmount = negAmount; MaxError = maxError;

            List<double> data = new List<double>();
            for (int i = 0; i < points.Count; i++) { data.Add(points[i].x); }

            data = Start(data);
            for (int i = data.Count - 1; i >= 0; i--) { if (Fill == data[i]) { points.RemoveAt(i); } }
            return points;
        }
        /// <summary>
        /// 滤除目标点中 D 值跳变的点
        /// </summary>
        /// <param name="points">目标点</param>
        /// <param name="negAmount">最多可忽略连续点的个数</param>
        /// <param name="maxError">最大允许的跳变误差</param>
        /// <returns></returns>
        public List<CoordinatePoint.POINT> FilterD(List<CoordinatePoint.POINT> points, double negAmount, double maxError)
        {
            if (points == null || points.Count == 0)
            { points = new List<CoordinatePoint.POINT>(); return points; }

            RemoveNeg = false; Fill = double.NaN; NegAmount = negAmount; MaxError = maxError;

            List<double> data = new List<double>();
            for (int i = 0; i < points.Count; i++) { data.Add(points[i].d); }

            data = Start(data);
            for (int i = data.Count - 1; i >= 0; i--) { if (Fill == data[i]) { points.RemoveAt(i); } }
            return points;
        }
    }

    /// <summary>
    /// 卡尔曼滤波器
    /// </summary>
    class KalmanFilter
    {
        /// <summary>
        /// 暂时没弄清楚
        /// </summary>
        public double P;
        /// <summary>
        /// 系统噪声方差
        /// </summary>
        public double Q;
        /// <summary>
        /// 观测噪声方差
        /// </summary>
        public double R;

        /// <summary>
        /// 上一时刻的估计值
        /// </summary>
        public double Last;
        /// <summary>
        /// 这一时刻的估计值
        /// </summary>
        public double Next;

        /// <summary>
        /// 卡尔曼滤波器，返回本次的估计结果
        /// </summary>
        /// <param name="measure">测量值</param>
        /// <returns></returns>
        public double Start(double measure)
        {
            // 更新 Last
            Last = Next;

            // 用系统方程估计下一时刻的状态值
            double sX_next = Last;

            // 估计下一时刻 P
            double pNext = P + Q;

            // 得到卡尔曼增益
            double KalmanGain = pNext / (pNext + R);

            // 得到下一时刻的估计值
            Next = sX_next + KalmanGain * (measure - sX_next);

            // 更新 P
            P = (1 - KalmanGain) * pNext;

            // 返回估计值
            return Next;
        }
    }
}
