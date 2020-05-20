using System;

namespace CallOfCthulhu
{
    /// <summary>
    /// 属性值发生变化时的事件消息
    /// </summary>
    public class TraitChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 原来的特点基础值
        /// </summary>
        public int OriginalBase { get; set; }

        /// <summary>
        /// 原来的特点成长值
        /// </summary>
        public int OriginalGrowth { get; set; }

        /// <summary>
        /// 新的特点基础值
        /// </summary>
        public int NewBase { get; set; }

        /// <summary>
        /// 新的特点成长值
        /// </summary>
        public int NewGrowth { get; set; }

        /// <summary>
        /// 原有的衰老惩罚值
        /// </summary>
        public int OriginalAdjustment { get; set; }

        /// <summary>
        /// 新的衰老惩罚值
        /// </summary>
        public int NewAdjustment { get; set; }

        /// <summary>
        /// 发生变化的特点名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 角色属性变动事件的参数
        /// </summary>
        /// <param name="key"></param>
        public TraitChangedEventArgs(string key)
        {
            Key = key;
        }
    }
}
