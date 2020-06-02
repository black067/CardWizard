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
    [Description("配置数据")]
    public partial class Config
    {
        /// <summary>
        /// Lua 垃圾回收的周期
        /// </summary>
        [Description("Lua 垃圾回收的周期, 单位: 秒")]
        public int GCInterval = 20;

        /// <summary>
        /// 是否显示提示信息
        /// </summary>
        [Description("是否显示提示信息")]
        public bool ToolTipAvailable = true;

        /// <summary>
        /// 提示浮窗出现时, 其背景的透明度
        /// </summary>
        [Description("提示浮窗出现时, 其背景的透明度")]
        public double ToolTipOpacity = 0.87;

        /// <summary>
        /// 角色存档文件的标准后缀名
        /// </summary>
        [Description("角色存档文件的标准后缀名")]
        public string FileExtensionForCard = ".i.yaml";

        /// <summary>
        /// 角色存档的翻译文件的标准后缀名
        /// </summary>
        [Description("角色存档的翻译文件的标准后缀名")]
        public string FileExtensionForCardDoc = ".i.trans.yaml";

        /// <summary>
        /// 角色图像的后缀名
        /// </summary>
        [Description("角色图像的后缀名")]
        public string FileExtensionForCardPic = ".i.png";

        /// <summary>
        /// 调查员的图像文档在打印时的页面DPI
        /// </summary>
        [Description("打印时的页面DPI")]
        public double PrintSettings_Dpi = 300;

        /// <summary>
        /// 打印时的高宽比值
        /// </summary>
        [Description("打印时页面高度与宽度的比值")]
        public double PrintSettings_H_W_Scale = 297.0 / 210.0;

        /// <summary>
        /// 调查员的图像文档在打印时的页面背景色
        /// </summary>
        [Description("调查员的图像文档在打印时的页面背景色")]
        public string PrintSettings_BackgroundColor = "#ffffffff";

        /// <summary>
        /// 文本翻译工具
        /// </summary>
        [Description("文本字典")]
        public Translator Translator = new Translator();

        /// <summary>
        /// 路径数据, 缓存了应用的特殊路径
        /// </summary>
        [Description("重要文件与文件夹的路径")]
        public Paths Paths = new Paths();

        /// <summary>
        /// 基础属性模型
        /// </summary>
        [Description("基础属性模型")]
        public List<Characteristic> DataModels = new List<Characteristic>()
        {
            new Characteristic() { Name = "STR", Formula = "5 * (3D6)", Derived = false },
            new Characteristic() { Name = "CON", Formula = "5 * (3D6)", Derived = false },
            new Characteristic() { Name = "DEX", Formula = "5 * (3D6)", Derived = false },
            new Characteristic() { Name = "APP", Formula = "5 * (3D6)", Derived = false },
            new Characteristic() { Name = "POW", Formula = "5 * (3D6)", Derived = false },
            new Characteristic() { Name = "SIZ", Formula = "5 * (2D6 + 6)", Derived = false },
            new Characteristic() { Name = "INT", Formula = "5 * (2D6 + 6)",Derived = false },
            new Characteristic() { Name = "EDU", Formula = "5 * (2D6 + 6)", Derived = false, Upper = 99},
            new Characteristic() { Name = "LUCK", Formula = "5 * (3D6)", Derived = false, Upper = 99 },
            new Characteristic() { Name = "AST", Formula = "1D10", Derived = true},
            new Characteristic() { Name = "SAN", Formula = "POW", Derived = true, Upper = 99},
            new Characteristic() { Name = "IDEA", Formula = "INT",Derived = true },
            new Characteristic() { Name = "MOV", Formula = "GetMOV(SIZ, DEX, STR, AGE)", Derived = true},
            new Characteristic() { Name = "MP", Formula = "POW / 5", Derived = true},
            new Characteristic() { Name = "HP", Formula = "(SIZ + CON) / 5", Derived = true},
            new Characteristic() { Name = "DODGE", Formula = "DEX / 2", Derived = true},
            new Characteristic() { Name = "Build", Formula="0", Derived = true, },
        };

        [YamlIgnore]
        private Dictionary<string, Characteristic> baseModelDict;

        /// <summary>
        /// 属性模型的字典
        /// </summary>
        [YamlIgnore]
        public Dictionary<string, Characteristic> BaseModelDict
        {
            get
            {
                if (baseModelDict == null)
                {
                    baseModelDict = new Dictionary<string, Characteristic>(from m in DataModels select KeyValuePair.Create(m.Name, m));
                }
                return baseModelDict;
            }
        }

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
                from f in fields
                where f.FieldType == typeofStr
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
