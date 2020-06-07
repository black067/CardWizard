using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CallOfCthulhu
{
    /// <summary>
    /// 技能
    /// </summary>
    public class Skill : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// 技能点数的类型
        /// </summary>
        public enum Segment
        {
            /// <summary>
            /// 职业点数
            /// </summary>
            OCCUPATION,
            /// <summary>
            /// 个人兴趣点数
            /// </summary>
            PERSONAL,
            /// <summary>
            /// 成长点数
            /// </summary>
            GROWTH,
        }

        private int id;
        private string name;
        private string category;
        private string description;
        private bool growable = true;
        private int baseValue;
        private int growthPoints;
        private int occupationPoints;
        private int personalPoints;
        private bool grown;
        private int upper;
        private int lower;

        /// <summary>
        /// 编号
        /// </summary>
        [Description("编号")]
        public int ID
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        [Description("名称")]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 技能类别
        /// </summary>
        [Description("技能类别")]
        public string Category
        {
            get => category;
            set
            {
                category = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 技能描述
        /// </summary>
        [Description("技能描述")]
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 能否获得幕间提升
        /// </summary>
        [Description("能否获得幕间提升")]
        public bool Growable
        {
            get => growable;
            set
            {
                growable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 上限
        /// </summary>
        [Description("点数上限")]
        public int Upper
        {
            get => upper;
            set
            {
                upper = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 点数下限
        /// </summary>
        [Description("点数下限")]
        public int Lower
        {
            get => lower;
            set
            {
                lower = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// 技能基础值
        /// </summary>
        [Description("技能基础值")]
        public int BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 职业点数
        /// </summary>
        [Description("职业点数")]
        public int OccupationPoints
        {
            get => occupationPoints;
            set
            {
                occupationPoints = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 兴趣点数
        /// </summary>
        [Description("兴趣点数")]
        public int PersonalPoints
        {
            get => personalPoints;
            set
            {
                personalPoints = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 成长点数
        /// </summary>
        [Description("成长点数")]
        public int GrowthPoints
        {
            get => growthPoints;
            set
            {
                growthPoints = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 技能是否已经过幕间成长
        /// </summary>
        [Description("技能是否已经过幕间成长")]
        public bool Grown
        {
            get => grown;
            set
            {
                grown = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 查询指定类型的值
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public int GetPoints(Segment segment)
        {
            return segment switch
            {
                Segment.OCCUPATION => OccupationPoints,
                Segment.PERSONAL => PersonalPoints,
                _ => growthPoints,
            };
        }

        /// <summary>
        /// 设置指定类型的值
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="value"></param>
        public void SetPoints(Segment segment, int value)
        {
            switch (segment)
            {
                case Segment.OCCUPATION:
                    OccupationPoints = value;
                    break;
                case Segment.PERSONAL:
                    PersonalPoints = value;
                    break;
                case Segment.GROWTH:
                    GrowthPoints = value;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 属性发生改变时触发的事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        public override string ToString()
        {
            var builder = new StringBuilder($"{Name} {BaseValue}% # {{");
            var props = typeof(Skill).GetProperties();
            foreach (var item in props)
            {
                builder.Append($"{item.Name}: {item.GetValue(this)}, ");
            }
            builder.Append("}");
            return builder.ToString();
        }

        /// <summary>
        /// 取得一份拷贝
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Skill
            {
                ID = id,
                Name = name,
                Category = category,
                Description = description,
                Growable = growable,
                Grown = grown,
                BaseValue = baseValue,
                GrowthPoints = growthPoints,
                OccupationPoints = occupationPoints,
                PersonalPoints = personalPoints,
            };
        }
    }
}
