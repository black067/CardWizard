using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
        public string[] Skills { get; set; }

        /// <summary>
        /// 信用评级范围
        /// <para>形如: 30 ~ 80</para>
        /// </summary>
        public string CreditRatingRange { get; set; }

        /// <summary>
        /// 职业技能点计算公式
        /// <para>形如: <code>EDU * 2 + APP * 2</code></para>
        /// </summary>
        public string PointFormula { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Name);
            builder.AppendLine(Description);
            for (int i = 0, len = Skills.Length, last = Skills.Length - 1; i < len; i++)
            {
                builder.Append($"{Skills[i]}");
                builder.Append(i < last ? ", " : "\n");
            }
            builder.AppendLine($"Credit Rating: {CreditRatingRange}");
            builder.AppendLine($"Points: {PointFormula}");
            return builder.ToString();
        }
    }
}
