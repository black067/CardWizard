using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CallOfCthulhu
{
    /// <summary>
    /// 技能
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// 自选任意技能
        /// </summary>
        public const string CUSTOM = "{Custom}";

        /// <summary>
        /// 自选专业技能
        /// </summary>
        public const string CUSTOM_PRO = "{Custom.Professional}";

        /// <summary>
        /// 自选社交技能
        /// </summary>
        public const string CUSTOM_SOCIAL = "{Custom.Social}";

        /// <summary>
        /// 自选艺术与手艺技能
        /// </summary>
        public const string CUSTOM_ARTISTIC = "{Custom.Artistic}";

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 技能类别
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 技能基础值
        /// </summary>
        public int BaseValue { get; set; }

        public static Skill Parse(string text)
        {
            var s = new Skill();
            var match = Regex.Match(text, @"(\-|\+)?\d+(\.\d+)?\%", RegexOptions.Multiline);
            if (match.Success)
            {
                s.Name = text.Substring(0, match.Index);
                s.BaseValue = int.Parse(match.Value.Replace("%", string.Empty));
            }
            else
            {
                s.Name = text;
            }
            return s;
        }
    }
}
