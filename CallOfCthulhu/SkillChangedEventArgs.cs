using System;

namespace CallOfCthulhu
{
    /// <summary>
    /// 角色技能数值变动时触发的事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void SkillChangedEventHandler(Character sender, SkillChangedEventArgs args);

    /// <summary>
    /// 角色技能数值发生变动时的事件参数
    /// </summary>
    public class SkillChangedEventArgs : EventArgs
    {
        public SkillChangedEventArgs(int skillID)
        {
            SkillID = skillID;
        }

        /// <summary>
        /// 变动的技能的ID
        /// <para>如果用技能名初始化, 这个值是 -1</para>
        /// </summary>
        public int SkillID { get; set; }

        /// <summary>
        /// 技能的哪个数值发生了改变
        /// </summary>
        public Skill.Segment Segment { get; set; }

        /// <summary>
        /// 原来的值
        /// </summary>
        public int OldValue { get; set; }

        /// <summary>
        /// 新的值
        /// </summary>
        public int NewValue { get; set; }

        /// <summary>
        /// 判断是否是同一个技能
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameSkill(Skill other)
        {
            if (other == null) return false;
            return SkillID == other.ID;
        }
    }
}
