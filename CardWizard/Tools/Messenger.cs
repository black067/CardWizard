using System;
using System.Collections.Generic;
using System.Text;

namespace CardWizard.Tools
{
    /// <summary>
    /// 信息管理器
    /// </summary>
    public static class Messenger
    {
        /// <summary>
        /// 信息入列前的预处理事件
        /// </summary>
        private static Func<string, string> PretreatmentHandler { get; set; }

        /// <summary>
        /// 信息入列时的事件
        /// </summary>
        private static Action<string> EnqueueHandler { get; set; }

        /// <summary>
        /// 信息出列时的事件
        /// </summary>
        private static Action<string> DequeueHandler { get; set; }

        /// <summary>
        /// 消息进入队列之前的预处理事件
        /// </summary>
        public static event Func<string, string> OnPretreatment
        {
            add { PretreatmentHandler += value; }
            remove { PretreatmentHandler -= value; }
        }

        /// <summary>
        /// 消息进入队列时的事件
        /// </summary>
        public static event Action<string> OnEnqueue
        {
            add { EnqueueHandler += value; }
            remove { EnqueueHandler -= value; }
        }

        /// <summary>
        /// 消息出列时的事件
        /// </summary>
        public static event Action<string> OnDequeue
        {
            add { DequeueHandler += value; }
            remove { DequeueHandler -= value; }
        }

        /// <summary>
        /// 信息队列
        /// </summary>
        private static Queue<string> Queue { get; set; } = new Queue<string>();

        /// <summary>
        /// 信息出列, 如果队列中有信息, 会触发出列事件 OnDequeue
        /// </summary>
        /// <returns></returns>
        public static string Dequeue()
        {
            var rst = string.Empty;
            if (Queue.Count > 0)
            {
                rst = Queue.Dequeue();
                DequeueHandler?.Invoke(rst);
            }
            return rst;
        }
        /// <summary>
        /// 将信息放入信息队列, 会触发入列事件 OnEnqueue
        /// </summary>
        /// <param name="value"></param>
        public static void Enqueue(string value)
        {
            value = PretreatmentHandler?.Invoke(value) ?? value;
            Queue.Enqueue(value);
            EnqueueHandler?.Invoke(value);
        }

        /// <summary>
        /// 将物件转为字符串, 放入信息队列, 会触发入列事件 OnEnqueue
        /// </summary>
        /// <param name="item"></param>
        public static void Enqueue(object item)
            => Enqueue(item?.ToString());

        /// <summary>
        /// 将多个物件转为字符串, 放入信息队列, 每一个都会触发入列事件 OnEnqueue
        /// </summary>
        /// <param name="objects">多个物件</param>
        public static void Enqueue(params object[] objects)
        {
            if (objects == null) return;
            var builder = new StringBuilder();
            foreach (var item in objects)
            {
                builder.Append(item.ToString());
                builder.Append(" ");
            }
            Enqueue(item: builder.ToString().Trim());
        }

        /// <summary>
        /// 将信息格式化, 放入信息队列, 会触发入列事件 OnEnqueue
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public static void EnqueueFormat(string format, params object[] args)
            => Enqueue(string.Format(format, args));

        /// <summary>
        /// 返回信息队列的开始处信息, 不取出
        /// </summary>
        /// <returns></returns>
        public static string Peek
            => Queue.Count > 0 ? Queue.Peek() : string.Empty;

        /// <summary>
        /// 取出所有信息, 并格式化输出每一条
        /// </summary>
        /// <returns></returns>
        public static string DequeueAll(string lineFormat = "{0}\n")
        {
            var builder = new StringBuilder();
            for (int i = Queue.Count - 1; i >= 0; i--)
            {
                builder.AppendLine(string.Format(lineFormat, Dequeue()));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 清空信息队列
        /// </summary>
        public static void Clear()
            => Queue.Clear();

        /// <summary>
        /// 重置信息入列/出列事件
        /// </summary>
        public static void Reset()
        {
            Clear();
            EnqueueHandler = null;
            DequeueHandler = null;
        }
    }
}
