using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Specialized;

namespace CallOfCthulhu
{
    /// <summary>
    /// 包装后的响应式字典
    /// <para>派生自: <see cref="Dictionary{TKey, TValue}"/>, <see cref="INotifyCollectionChanged"/></para>
    /// <para>TKey is <see cref="string"/>, TVallue is <see cref="Object"/></para>
    /// </summary>
    public class ContextData : Dictionary<string, object>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// 清空所有观察者
        /// </summary>
        public void ClearObservers() => CollectionChanged = null;

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
        /// 添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(string key, object value)
        {
            if (ContainsKey(key)) { throw new ArgumentException($"已存在键为 {key} 的元素"); }
            base.Add(key, value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, KeyValuePair.Create(key, value)));
        }

        /// <summary>
        /// 移除并取出指定的元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public new bool Remove(string key, out object item)
        {
            if (base.Remove(key, out item))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, KeyValuePair.Create(key, item)));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除指定元素
        /// </summary>
        /// <param name="key"></param>
        public new bool Remove(string key) => Remove(key, out _);

        public new object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (!ContainsKey(key))
                {
                    Add(key, value);
                    return;
                }
                var old = base[key];
                if (value != old)
                {
                    base[key] = value;
                    var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem: KeyValuePair.Create(key, value), oldItem: KeyValuePair.Create(key, old));
                    CollectionChanged?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// 设置目标的 字段/属性 的值
        /// </summary>
        /// <param name="target"></param>
        public void ExportValues(object target) => ExportValues(this, target);

        /// <summary>
        /// 设置目标的 字段/属性 的值
        /// </summary>
        /// <param name="values"></param>
        /// <param name="target"></param>
        public static void ExportValues(ContextData values, object target)
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
