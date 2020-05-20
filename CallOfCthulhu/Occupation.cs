using System.Collections.Generic;


namespace CallOfCthulhu
{
    /// <summary>
    /// 职业
    /// </summary>
    public class Occupation
    {
        /// <summary>
        /// 职业名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 职业技能
        /// </summary>
        public List<string> Skills { get; set; }

        /// <summary>
        /// 客制化数据
        /// </summary>
        public Dictionary<string, object> ContextData { get; set; }
    }
}
