using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 能否获得幕间提升
        /// </summary>
        public bool Growable { get; set; } = true;

        /// <summary>
        /// 将文本转化为 Skill
        /// <para><paramref name="parser"/>: 函数, 可以将形如 "{ a: 1, b: true, c: C }" 的字符串转化为 <see cref="ContextDict"/> </para>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="context"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Skill Parse(string text, out ContextDict context, Func<string, ContextDict> parser = null)
        {
            var s = new Skill();

            if (string.IsNullOrWhiteSpace(text))
            {
                context = new ContextDict();
                return s;
            }
            var segments = text.Split('#', 2, StringSplitOptions.RemoveEmptyEntries);
            var first = segments.First();
            var match = Regex.Match(first, @"(\-|\+)?\d+(\.\d+)?\%", RegexOptions.Multiline);
            if (match.Success)
            {
                s.Name = first.Substring(0, match.Index).Trim();
                s.BaseValue = int.Parse(match.Value.Replace("%", string.Empty));
            }
            else
            {
                s.Name = first;
            }
            if (segments.Length > 1 && !string.IsNullOrWhiteSpace(segments[1]) && parser != null)
            {
                context = parser.Invoke(segments[1]);
                context?.ExportValues(s);
            }
            else
            {
                context = new ContextDict();
            }
            return s;
        }
    }
}
