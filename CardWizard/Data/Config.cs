using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using YamlDotNet.Serialization;
using CallOfCthulhu;

namespace CardWizard.Data
{
    /// <summary>
    /// 配置数据
    /// </summary>
    public partial class Config
    {
        /// <summary>
        /// 垃圾回收的周期
        /// </summary>
        public int GCInterval = 30;

        /// <summary>
        /// 是否保存翻译后的文本
        /// </summary>
        public bool SaveTranslationDoc = true;

        /// <summary>
        /// 角色存档文件的标准后缀名
        /// </summary>
        public string FileExtensionForCard = ".ivg.yaml";

        /// <summary>
        /// 角色存档的翻译文件的标准后缀名
        /// </summary>
        public string FileExtensionForCardDoc = ".ivg.trans.yaml";

        /// <summary>
        /// 角色图像的后缀名
        /// </summary>
        public string FileExtensionForCardPic = ".doc.png";

        /// <summary>
        /// 调查员的图像文档在打印时的页面DPI
        /// </summary>
        public double PrintSettings_Dpi = 300;

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
            new Trait() { Name = "STR", Formula = "5*(3D6)", Derived = false },
            new Trait() { Name = "CON", Formula = "5*(3D6)", Derived = false },
            new Trait() { Name = "DEX", Formula = "5*(3D6)", Derived = false },
            new Trait() { Name = "APP", Formula = "5*(3D6)", Derived = false },
            new Trait() { Name = "POW", Formula = "5*(3D6)", Derived = false },
            new Trait() { Name = "SIZ", Formula = "5*(2D6+6)", Derived = false },
            new Trait() { Name = "INT", Formula = "5*(2D6+6)",Derived = false },
            new Trait() { Name = "EDU", Formula = "5*(2D6+6)", Derived = false, Upper = 99},
            new Trait() { Name = "AST", Formula = "1D10", Derived = true},
            new Trait() { Name = "SAN", Formula = "$POW", Derived = true, Upper = 99},
            new Trait() { Name = "IDEA", Formula = "$INT",Derived = true },
            new Trait() { Name = "LUCK", Formula = "5*(3D6)", Derived = true },
            //new Trait() { Name = "MOV", Formula = "GetMOV($C)", Derived = true},
            new Trait() { Name = "MP", Formula = "$POW/5", Derived = true},
            new Trait() { Name = "HP", Formula = "($SIZ+$CON)/5", Derived = true},
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
        /// 用于指定 <see cref="Process"/> 中, 处理此字段的顺序 (数字升序)
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
            var gettersdict = new Dictionary<string, Func<string>>(
                from f in fields where f.FieldType == typeofStr
                select new KeyValuePair<string, Func<string>>(f.Name,
                                                              () => f.GetValue(target)?.ToString() ?? string.Empty));
            foreach (var field in fields)
            {
                if (field.FieldType != typeofStr) { continue; }
                var value = field.GetValue(target).ToString();
                value = Translator.MapKeywords(value, gettersdict);
                field.SetValue(target, value);
            }

            Translator.Process();
            return this;
        }
    }
}
