using System.Collections.Generic;

namespace CallOfCthulhu
{
    /// <summary>
    /// 技能
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 技能基础值
        /// </summary>
        public int BaseValue { get; set; }

        /// <summary>
        /// 客制化数据, 比如成长值等
        /// </summary>
        public Dictionary<string, object> ContextData { get; set; }

        /// <summary>
        /// 取得拷贝
        /// </summary>
        /// <returns></returns>
        public Skill Clone()
        {
            return new Skill()
            {
                Name = Name,
                Description = Description,
                BaseValue = BaseValue,
                ContextData = new Dictionary<string, object>(ContextData),
            };
        }

    }
}
