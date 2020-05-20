using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CardWizard.Tools
{
    /// <summary>
    /// Lua / C# 公用的工具类
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// 使用类型的名称生成字典
        /// </summary>
        /// <param name="keyTypeName"></param>
        /// <param name="valueTypeName"></param>
        /// <returns></returns>
        public static object NewDict(string keyTypeName, string valueTypeName)
        {
            var keyType = CodeBuilder.ResolveType(keyTypeName);
            var valueType = CodeBuilder.ResolveType(valueTypeName);
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return dictType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
        }

        /// <summary>
        /// 计算集合中的的元素个数
        /// <para><em>dirty</em></para>
        /// </summary>
        /// <param name="someItems"></param>
        /// <returns></returns>
        public static int GetCount(object someItems)
        {
            if (someItems is IDictionary dict)
            {
                return dict.Count;
            }
            if (someItems is ICollection collection)
            {
                return collection.Count;
            }
            if (someItems is IList arr)
            {
                return arr.Count;
            }
            if (someItems is IEnumerable enumerable)
            {
                var array = enumerable.Cast<object>();
                return array.Count();
            }
            return 0;
        }
    }
}
