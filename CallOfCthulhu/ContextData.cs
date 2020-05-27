using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace CallOfCthulhu
{
    public class ContextData : Dictionary<string, object>
    {
        /// <summary>
        /// 尝试根据 <paramref name="key"/> 查找出对应的值, 否则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetOrDefault<T>(string key, T defaultValue = default)
        {
            TryGetValue(key, out var value);
            if (value is T instance) return instance;
            else return defaultValue;
        }

        /// <summary>
        /// 尝试根据 <paramref name="key"/> 查找出对应的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T item)
        {
            bool contains = TryGetValue(key, out var value);
            if (value is T instance) item = instance;
            else item = default;
            return contains;
        }

        /// <summary>
        /// 设置目标的 字段/属性 的值
        /// </summary>
        /// <param name="target"></param>
        public void SetValues(object target) => SetValues(this, target);

        /// <summary>
        /// 设置目标的 字段/属性 的值
        /// </summary>
        /// <param name="values"></param>
        /// <param name="target"></param>
        public static void SetValues(ContextData values, object target)
        {
            if (target == null || values == null || values.Count == 0) return; 
            var typeofTarget = target.GetType();
            var members = typeofTarget.GetMembers();
            Dictionary<MemberInfo, Action<object, object>> setters = new Dictionary<MemberInfo, Action<object, object>>(values.Count);
            foreach (var m in members)
            {
                // 如果 values 中不包含成员的名称, 直接跳过
                if (!values.ContainsKey(m.Name)) continue;
                if (m is PropertyInfo pinfo) setters.Add(pinfo, pinfo.SetValue);
                if (m is FieldInfo finfo) setters.Add(finfo, finfo.SetValue);
            }
            // 如果没有可用的 setter, 直接返回
            if (setters.Count == 0) return;
            StringBuilder errors = new StringBuilder();
            foreach (var kvp in setters)
            {
                var name = kvp.Key.Name;
                if (!values.TryGetValue(name, out var value)) continue;
                var itemtype = kvp.Key is FieldInfo ? (kvp.Key as FieldInfo).FieldType : (kvp.Key as PropertyInfo).PropertyType;
                var valuetype = value.GetType();
                if (valuetype.IsSubclassOf(itemtype))
                {
                    kvp.Value.Invoke(target, value);
                }
                else
                {
                    errors.AppendLine($"设置字段 {kvp.Key.Name} 的值时, 需要类型为 {itemtype.FullName} 的值, 但给的值类型为 {valuetype.FullName}");
                }
            }
            if (errors.Length > 0)
                throw new TargetException(errors.ToString());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey, TValue}"/> class 
        /// that is empty, has the default initial capacity, 
        /// and uses the default equality comparer for the key type.
        /// <para>TKey is <see cref="String"/>, TValue is <see cref="Object"/></para>
        /// </summary>
        public ContextData() : base() { }

        /// <summary>
        /// 以指定的容量初始化
        /// </summary>
        /// <param name="capacity"></param>
        public ContextData(int capacity) : base(capacity) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public ContextData(IDictionary<string, object> dictionary) : base(dictionary) { }

        /// <summary>
        /// 通过键值对创建
        /// </summary>
        /// <param name="collection"></param>
        public ContextData(IEnumerable<KeyValuePair<string, object>> collection) : base(collection) { }

        /// <summary>
        /// 通过 <see cref="Tuple{T1, T2}"/> 来创建
        /// <para>T1 is <see cref="string"/>, T2 is <see cref="Object"/></para>
        /// </summary>
        /// <param name="tuples"></param>
        public ContextData(IEnumerable<Tuple<string, object>> tuples) : base(from t in tuples select new KeyValuePair<string, object>(t.Item1, t.Item2)) { }
    }
}
