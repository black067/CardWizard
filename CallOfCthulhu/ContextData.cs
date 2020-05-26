using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CallOfCthulhu
{
    public class ContextData : Dictionary<string, object>
    {
        /// <summary>
        /// 尝试根据 <paramref name="key"/> 查找出对应的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T item, T defaultValue = default)
        {
            bool contains = TryGetValue(key, out var value);
            if (value is T instance) item = instance;
            else item = defaultValue;
            return contains;
        }

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary`2 class 
        /// that is empty, has the default initial capacity, 
        /// and uses the default equality comparer for the key type.
        /// </summary>
        public ContextData() : base() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public ContextData(int capacity) : base(capacity) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public ContextData(IDictionary<string, object> dictionary) : base(dictionary) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        public ContextData(IEnumerable<KeyValuePair<string, object>> collection) : base(collection) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuples"></param>
        public ContextData(IEnumerable<Tuple<string, object>> tuples) : base(from t in tuples select new KeyValuePair<string, object>(t.Item1, t.Item2)) { }
    }
}
