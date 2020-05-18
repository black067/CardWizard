using System;

namespace CardWizard.Tools
{
    public static class ProcessKit
    {
        /// <summary>
        /// 创建一个新的不同名变量
        /// <para>等价于 <typeparamref name="T"/> <paramref name="other"/> = <paramref name="self"/> </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T AssignAs<T>(this T self, out T other)
        {
            other = self;
            return self;
        }

        /// <summary>
        /// 对该对象进行处理, 再返回对象自身
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public static T Process<T>(this T self, Action<T> process)
        {
            process?.Invoke(self);
            return self;
        }
    }
}
