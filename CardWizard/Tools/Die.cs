using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWizard.Tools
{
    /// <summary>
    /// 随机数工具 (骰子)
    /// </summary>
    public class Die
    {
        private int seed;

        /// <summary>
        /// 构造新的随机数工具
        /// </summary>
        /// <param name="seed">随机数种子</param>
        public Die(int seed)
        {
            Seed = seed;
        }

        /// <summary>
        /// 用新GUID为随机数种子构造随机数工具
        /// </summary>
        public Die() : this(Guid.NewGuid().GetHashCode() % 100) { }

        /// <summary>
        /// 随机数产生工具（如果要重新设置随机数种子, 直接为Seed赋值）
        /// </summary>
        private Random Tool { get; set; }

        /// <summary>
        /// 随机数种子, 对此赋值会重新构造随机数工具
        /// </summary>
        public virtual int Seed { get => seed; set { seed = value; Tool = new Random(seed); } }

        /// <summary>
        /// 返回范围内的随机整数, 不含上限
        /// </summary>
        /// <param name="min">下限, 包含</param>
        /// <param name="max">上限, 不含</param>
        /// <returns></returns>
        public virtual int Range(int min, int max) => Tool.Next(min, max);

        /// <summary>
        /// 返回范围内的随机浮点数, 不含上限
        /// </summary>
        /// <param name="min">下限, 包含</param>
        /// <param name="max">上限, 不含</param>
        /// <returns></returns>
        public virtual float Range(float min, float max)
        {
            if (max < min)
            {
                max = min - max;
                min = min - max;
                max = min + max;
            }
            var rst = Convert.ToSingle(Tool.NextDouble());
            return rst * (max - min) + min;
        }

        /// <summary>
        /// 完全随机选取一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public T SelectRandom<T>(IEnumerable<T> list)
        {
            if (list != null)
            {
                int count = list.Count();
                if (count > 0)
                {
                    return list.ElementAtOrDefault(Range(0, count));
                }
            }
            return default;
        }

        /// <summary>
        /// 完全随机选取一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T SelectRandom<T>(params T[] args)
            => SelectRandom(list: args);

        /// <summary>
        /// 根据权重计算函数, 从集合中选出一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">集合</param>
        /// <param name="weightGetter">权重计算函数</param>
        /// <param name="point">输出, 随机值</param>
        /// <param name="sum">输出, 每个元素权重之和</param>
        /// <param name="probabilities">输出, 每个元素概率的合集</param>
        /// <returns></returns>
        public T Select<T>(IEnumerable<T> collection, Func<T, float> weightGetter, out float point, out float sum, out float[] probabilities)
        {
            var weights = from t in collection select weightGetter.Invoke(t);
            return Select(collection, weights, out point, out sum, out probabilities);
        }

        /// <summary>
        /// 根据 浮点型 的权重数组, 从集合中选出一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">元素集合</param>
        /// <param name="weights">权重集合</param>
        /// <param name="point">随机值</param>
        /// <param name="sum">权重之和</param>
        /// <param name="probabilities">概率的合集</param>
        /// <returns></returns>
        public T Select<T>(IEnumerable<T> collection, IEnumerable<float> weights, out float point, out float sum, out float[] probabilities)
        {
            point = 0;
            sum = 0;
            if (collection == null) { probabilities = Array.Empty<float>(); return default; }
            int count = collection.Count();
            probabilities = new float[count];
            if (count <= 1) { return collection.FirstOrDefault(); }
            if (weights == null || !weights.Any())
            {
                sum = count;
                var index = Range(0, count);
                point = index;
                for (int i = 0; i < count; i++)
                {
                    probabilities[i] = 1 / sum;
                }
                return collection.ElementAt(index);
            }
            var weightsArray = weights.ToArray();

            float[] scale = new float[count];
            scale[0] = weightsArray[0];
            sum = weightsArray[0];
            for (int i = 1; i < count; i++)
            {
                scale[i] = scale[i - 1] + weightsArray[i];
                sum += weightsArray[i];
            }
            for (int i = 0; i < count; i++)
            {
                probabilities[i] = weightsArray[i] / sum;
            }
            point = Range(0f, sum);
            int j = 0;
            foreach (var item in collection)
            {
                if (point < scale[j])
                {
                    return item;
                }
                j++;
            }
            return collection.LastOrDefault();
        }
    }
}
