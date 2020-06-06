using System;

namespace CallOfCthulhu
{
    /// <summary>
    /// 委托: 可绑定到 <see cref="Character.CharacteristicChanged"/> 事件上, 用于监听角色属性数值的变化
    /// </summary>
    /// <param name="character"></param>
    /// <param name="args"></param>
    public delegate void CharacteristicChangedEventHandler(Character character, CharacteristicChangedEventArgs args);

    /// <summary>
    /// 属性值发生变化时的事件消息
    /// </summary>
    public class CharacteristicChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 被修改的段落
        /// </summary>
        public Characteristic.Segment Segment { get; set; }

        /// <summary>
        /// 原来的值
        /// </summary>
        public int OldValue { get; set; }

        /// <summary>
        /// 新的值
        /// </summary>
        public int NewValue { get; set; }

        /// <summary>
        /// 发生变化的特点名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 角色属性变动事件的参数
        /// </summary>
        /// <param name="key"></param>
        public CharacteristicChangedEventArgs(string key)
        {
            Key = key;
        }
    }
}
