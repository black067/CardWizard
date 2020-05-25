using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using YamlDotNet.Serialization;
using CallOfCthulhu;
using System.ComponentModel;

namespace CardWizard.Data
{
    /// <summary>
    /// 配置数据
    /// </summary>
    public partial class Config
    {
        /// <summary>
        /// Lua 垃圾回收的周期
        /// </summary>
        [Description("Lua 垃圾回收的周期")]
        public int GCInterval = 20;

        /// <summary>
        /// 是否保存翻译后的文本
        /// </summary>
        public bool SaveTranslationDoc = true;

        /// <summary>
        /// 是否显示提示信息
        /// </summary>
        public bool ShowToolTips = true;

        /// <summary>
        /// 角色存档文件的标准后缀名
        /// </summary>
        public string FileExtensionForCard = ".i.yaml";

        /// <summary>
        /// 角色存档的翻译文件的标准后缀名
        /// </summary>
        public string FileExtensionForCardDoc = ".i.trans.yaml";

        /// <summary>
        /// 角色图像的后缀名
        /// </summary>
        public string FileExtensionForCardPic = ".i.png";

        /// <summary>
        /// 调查员的图像文档在打印时的页面DPI
        /// </summary>
        public double PrintSettings_Dpi = 300;

        /// <summary>
        /// 打印时的尺寸
        /// </summary>
        public int[] PrintSettings_Size = new int[] { 2480, 3508 };

        /// <summary>
        /// 调查员的图像文档在打印时的页面背景色
        /// </summary>
        public string PrintSettings_BackgroundColor = "#ffffffff";

        /// <summary>
        /// 文本翻译工具
        /// </summary>
        public Translator Translator = new Translator();

        /// <summary>
        /// 路径数据, 缓存了应用的特殊路径
        /// </summary>
        public Paths Paths = new Paths();

        /// <summary>
        /// 基础属性模型
        /// </summary>
        public List<Trait> DataModels = new List<Trait>()
        {
            new Trait() { Name = "STR", Formula = "5 * (3D6)", Derived = false },
            new Trait() { Name = "CON", Formula = "5 * (3D6)", Derived = false },
            new Trait() { Name = "DEX", Formula = "5 * (3D6)", Derived = false },
            new Trait() { Name = "APP", Formula = "5 * (3D6)", Derived = false },
            new Trait() { Name = "POW", Formula = "5 * (3D6)", Derived = false },
            new Trait() { Name = "SIZ", Formula = "5 * (2D6 + 6)", Derived = false },
            new Trait() { Name = "INT", Formula = "5 * (2D6 + 6)",Derived = false },
            new Trait() { Name = "EDU", Formula = "5 * (2D6 + 6)", Derived = false, Upper = 99},
            new Trait() { Name = "LUCK", Formula = "5 * (3D6)", Derived = false, Upper = 99 },
            new Trait() { Name = "AST", Formula = "1D10", Derived = false},
            new Trait() { Name = "SAN", Formula = "POW", Derived = true, Upper = 99},
            new Trait() { Name = "IDEA", Formula = "INT",Derived = true },
            new Trait() { Name = "MOV", Formula = "GetMOV(SIZ, DEX, STR, AGE)", Derived = true},
            new Trait() { Name = "MP", Formula = "POW / 5", Derived = true},
            new Trait() { Name = "HP", Formula = "(SIZ + CON) / 5", Derived = true},
            new Trait() { Name = "DODGE", Formula = "DEX / 2", Derived = true},
        };

        [YamlIgnore]
        private Dictionary<string, Trait> baseModelDict;

        /// <summary>
        /// 属性模型的字典
        /// </summary>
        [YamlIgnore]
        public Dictionary<string, Trait> BaseModelDict
        {
            get
            {
                if (baseModelDict == null)
                {
                    baseModelDict = new Dictionary<string, Trait>(DataModels.Select(m => new KeyValuePair<string, Trait>(m.Name, m)));
                }
                return baseModelDict;
            }
        }

        /// <summary>
        /// 所有职业
        /// </summary>
        [Description("所有职业数据")]
        public List<Occupation> OccupationModels = new List<Occupation>()
        {
            new Occupation()
            {
                Name = "医生",
                Description = "钻研学习医学科学技术，挽救生命，以治病为业。",
                Skills = new string[]
                {
                    "急救", "其它语言(拉丁文)", "医学", "心理学", "科学(生物学)", "科学(药学)", Skill.CUSTOM_PRO, Skill.CUSTOM_PRO
                },
                CreditRatingRange = "30 ~ 80",
                PointFormula = "EDU * 4",
            },
            new Occupation()
            {
                Name = "警探",
                Description = "执行侦探破案工作的警察。",
                Skills = new string[]
                {
                    "艺术与手艺(表演)", "乔装", "射击", "法律", "聆听", "心理学", "侦察", Skill.CUSTOM_SOCIAL, Skill.CUSTOM
                },
                CreditRatingRange = "20 ~ 50",
                PointFormula = "EDU * 2 + math.max(DEX, STR) * 2",
            },
        };

        /// <summary>
        /// 标签: 在 <see cref="Process"/> 方法中, 处理此字段的顺序 (按照 <see cref="Index"/> 升序)
        /// </summary>
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        internal sealed class ProcessIndexAttribute : Attribute
        {
            public ProcessIndexAttribute(int index)
            {
                Index = index;
            }
            /// <summary>
            /// 生成的顺序, 从 0 开始
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// 获取 <paramref name="target"/> 的所有字段信息, 按照此标签的 Index 排序
            /// </summary>
            /// <param name="target"></param>
            /// <param name="bindingFlags"></param>
            /// <returns></returns>
            public static FieldInfo[] SortField(object target, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
            {
                var typeofProcessIndexAttr = typeof(ProcessIndexAttribute);
                var fields = target.GetType().GetFields(bindingFlags).ToList();
                fields.Sort((field1, field2) =>
                {
                    var attr1 = field1.GetCustomAttributes(typeofProcessIndexAttr)
                                .FirstOrDefault() as ProcessIndexAttribute;
                    var attr2 = field2.GetCustomAttributes(typeofProcessIndexAttr)
                                .FirstOrDefault() as ProcessIndexAttribute;
                    if (attr1 != null && attr2 != null) return attr1.Index.CompareTo(attr2.Index);
                    if (attr1 == null && attr2 != null) return -1;
                    if (attr1 != null && attr2 == null) return 1;
                    return 0;
                });
                return fields.ToArray();
            }
        }

        /// <summary>
        /// 对数据进行处理: 把文本中的关键字替换为实际值
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:指定 CultureInfo", Justification = "<挂起>")]
        public Config Process()
        {
            var typeofStr = typeof(string);
            var target = Paths;
            var fields = ProcessIndexAttribute.SortField(target);
            var getters = new Dictionary<string, Func<string>>(
                from f in fields where f.FieldType == typeofStr
                select new KeyValuePair<string, Func<string>>(f.Name, () => f.GetValue(target)?.ToString() ?? f.Name));
            foreach (var field in fields)
            {
                if (field.FieldType != typeofStr) { continue; }
                var value = field.GetValue(target).ToString();
                value = Translator.MapKeywords(value, getters);
                field.SetValue(target, value);
            }

            Translator.Process();
            return this;
        }
    }
}
