using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallOfCthulhu
{
    /// <summary>
    /// 职业
    /// </summary>
    public class Occupation
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// 职业名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 职业技能
        /// </summary>
        public string[] Skills
        {
            get;
            set;
        }

        /// <summary>
        /// 信用评级范围
        /// <para>形如: 30 ~ 80</para>
        /// </summary>
        public string CreditRatingRange
        {
            get;
            set;
        }

        /// <summary>
        /// 职业技能点计算公式
        /// <para>形如: <code>EDU * 2 + APP * 2</code></para>
        /// </summary>
        public string PointFormula
        {
            get;
            set;
        }

        private int[] validSkillIDs;
        private Dictionary<Skill.Categories, int> validSkillCategories;

        /// <summary>
        /// 该职业的可选技能 ID
        /// </summary>
        private int[] ValidSkillIDs
        {
            get
            {
                if (validSkillIDs == null)
                {
                    InitializeSkills(this, Skills);
                }
                return validSkillIDs;
            }
            set
            {
                validSkillIDs = value;
            }
        }

        /// <summary>
        /// 该职业可自选的技能类型
        /// </summary>
        private Dictionary<Skill.Categories, int> ValidSkillCategories
        {
            get
            {
                if (validSkillCategories == null)
                {
                    InitializeSkills(this, Skills);
                }
                return validSkillCategories;
            }
            set
            {
                validSkillCategories = value;
            }
        }

        private static void InitializeSkills(Occupation target, IEnumerable<string> skillTexts)
        {
            var ids = new List<int>();
            var categories = new Dictionary<Skill.Categories, int>();
            for (int i = 0, len = skillTexts.Count(); i < len; i++)
            {
                var segments = skillTexts.ElementAt(i).Split();
                if (int.TryParse(segments[0], out int r))
                {
                    ids.Add(r);
                }
                else if (Enum.TryParse<Skill.Categories>(segments[0], out var c) && c != Skill.Categories.Any)
                {
                    if (categories.ContainsKey(c)) categories[c] += 1;
                    else categories[c] = 1;
                }
            }
            target.ValidSkillIDs = ids.ToArray();
            target.ValidSkillCategories = categories;
        }

        /// <summary>
        /// 判断技能是否合法, 返回值是给定的技能中所有合法的技能
        /// </summary>
        /// <param name="skills"></param>
        /// <returns></returns>
        public Skill[] IsSkillValidate(IEnumerable<Skill> skills)
        {
            var validateSkills = new List<Skill>(skills.Count());
            var counter = new Dictionary<Skill.Categories, int>(from kvp in ValidSkillCategories select KeyValuePair.Create(kvp.Key, 0));

            foreach (var skill in skills)
            {
                if (skill == null) continue;
                if (ValidSkillIDs.Contains(skill.ID)) validateSkills.Add(skill);
                var category = skill.Category;
                if (ValidSkillCategories.ContainsKey(category) && counter[category] < ValidSkillCategories[category])
                {
                    counter[category]++;
                    validateSkills.Add(skill);
                }
            }
            return validateSkills.ToArray();
        }

        /// <summary>
        /// 该职业所有技能的 ID
        /// </summary>
        /// <returns></returns>
        public object[] GetSkillFilter()
        {
            var ids = new object[Skills.Length];
            var skillTexts = Skills;
            for (int i = 0, len = skillTexts.Length; i < len; i++)
            {
                var segments = skillTexts[i].Split();
                if (int.TryParse(segments[0], out int r))
                {
                    ids[i] = r;
                }
                else
                {
                    ids[i] = Enum.TryParse<Skill.Categories>(segments[0], out var c) ? c : Skill.Categories.Any;
                }
            }
            return ids;
        }

        /// <summary>
        /// 该职业所有技能的名称
        /// </summary>
        /// <returns></returns>
        public string[] GetSkillNames()
        {
            var names = new string[Skills.Length];
            var skillTexts = Skills;
            for (int i = 0, len = skillTexts.Length; i < len; i++)
            {
                var segments = skillTexts[i].Split();
                names[i] = segments.Length > 1 ? segments[1] : skillTexts[i];
            }
            return names;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Name);
            builder.AppendLine(Description);
            var skillNames = GetSkillNames();
            for (int i = 0, len = skillNames.Length, last = skillNames.Length - 1; i < len; i++)
            {
                builder.Append(skillNames[i]);
                builder.Append(i < last ? ", " : "\n");
            }
            builder.AppendLine($"Credit Rating: {CreditRatingRange}");
            builder.AppendLine($"Points: {PointFormula}");
            return builder.ToString();
        }
    }
}
