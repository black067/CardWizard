using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using YamlDotNet.Serialization;


namespace CardWizard.Data
{
    /// <summary>
    /// 配置数据
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 调查员资产的缩写
        /// </summary>
        public const string KEY_ASSET = "AST";

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
        /// 本地化文本相关数据
        /// </summary>
        public class Localization
        {
            /// <summary>
            /// 可选择的时代
            /// </summary>
            public string[] labelForEras = new string[]
            {
                "1890s", "1920s", "现代"
            };
            /// <summary>
            /// 名词翻译
            /// </summary>
            public Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                { "Era", "时代" },
                { "Name", "名称" },
                { "Name.Default", "无名小卒" },
                { "Gender", "性别" },
                { "Gender.Male", "男" },
                { "Gender.Female", "女" },
                { "Gender.Others", "保密" },
                { "Age", "年龄" },
                { "ToolTip.Age.Min", "角色的最小年龄是 {0}" },
                { "ToolTip.Age.Penalty", "角色超过 40 岁时, 每超过 10 年, 从以下属性中选择 1 点减去: {STR} / {CON} / {DEX} / {APP}." },
                { "Education", "学历" },
                { "Homeland", "故乡" },
                { "Occupation", "职业"},
                { "Description", "描述" },
                { "Skills", "技能" },
                { "skills", "技能" },
                { "Traits", "属性" },
                { "STR", "力量" },
                { "CON", "体质" },
                { "SIZ", "体型" },
                { "INT", "智力" },
                { "POW", "意志" },
                { "DEX", "敏捷" },
                { "APP", "外表" },
                { "EDU", "教育" },
                { "SAN", "理智" },
                { "LUCK", "幸运" },
                { "IDEA", "灵感" },
                { "SUM", "总计" },
                { "KNOW", "知识" },
                { KEY_ASSET, "资产" },
                { "Growths", "成长值" },
                { "Senescence", "衰老惩罚" },
                { "OccupationPoints", "职业点数" },
                { "InterestPoints", "兴趣点数" },
                { "true", "是" },
                { "false", "否" },
                { "Valid", "✅" },
                { "Invalid", "❎" },
                { "MenuBar.Button.Create", "新建" },
                { "MenuBar.Button.Save", "保存" },
                { "Card.Image.Avatar", "点击可导入新头像" },
                { "Card.Label.BasePropSum", "基础属性总计" },
                { "Card.Button.Regenerate", "重新生成" },
                { "Card.Button.DMGBonus", "掷一次" },
                { "Card.Label.DamageBonus", "伤害奖励" },
                { "Message.RollADMGBonus", "根据公式 {0}, 本轮掷出了 {1}" },
                { "BatchGenerationWindow.Title", "批量生成属性" },
                { "Dialog.Import.Avatar.Title", "选择调查员的照片 (*.png)" },
                { "Dialog.Import.Avatar.Confirmation", "是否确认用\n{0}\n覆盖现有的文件?" },
                { "Message.Regenerate", $"请选择一组数据作为角色的基础属性\n* 当前角色的成长值将被完全清空\n* {{{KEY_ASSET}}} 不计入属性总计" },
                { "Message.Character.Saved", "角色数据已保存至: {0}" },
                { "Message.Character.Switched", "已切换至角色: {0}" },
            };

            [YamlIgnore]
            internal Dictionary<string, string> dictionaryIgnoreCase = new Dictionary<string, string>();

            /// <summary>
            /// 查询翻译文字
            /// </summary>
            /// <param name="path"></param>
            /// <param name="text"></param>
            /// <returns></returns>
            public bool TryTranslate(string path, out string text, bool ignoreCase = false)
            {
                path = path.Replace('_', '.');
                text = null;
                if (ignoreCase)
                {
                    return !string.IsNullOrWhiteSpace(path) && dictionaryIgnoreCase.TryGetValue(path.ToUpper(), out text);
                }
                return !string.IsNullOrWhiteSpace(path) && dictionary.TryGetValue(path, out text);
            }
        }

        /// <summary>
        /// 文本翻译工具
        /// </summary>
        public Localization Translator = new Localization();


        /// <summary>
        /// 路径数据库, 缓存了应用的特殊路径
        /// </summary>
        public class PathsDatabase
        {
            /// <summary>
            /// 存档的根目录
            /// </summary>
            public string PathSave = "./Save";

            /// <summary>
            /// 资源的根目录
            /// </summary>
            public string PathResources = "./Resources";

            /// <summary>
            /// 数据存放的根目录
            /// </summary>
            [ProcessIndex(1)]
            public string PathData = "{PathResources}/Data";

            /// <summary>
            /// 技能数据位置
            /// </summary>
            [ProcessIndex(2)]
            public string FileSkills = $"{{PathData}}/{nameof(Skill)}.All.yaml";

            /// <summary>
            /// 职业数据的位置
            /// </summary>
            [ProcessIndex(2)]
            public string FileOccupations = $"{{PathData}}/{nameof(Occupation)}.All.yaml";

            /// <summary>
            /// 武器数据的位置
            /// </summary>
            [ProcessIndex(2)]
            public string FileWeapons = $"{{PathData}}/{nameof(Weapon)}.All.yaml";

            /// <summary>
            /// 脚本的目录
            /// </summary>
            [ProcessIndex(1)]
            public string PathScripts = "{PathResources}/Scripts";

            /// <summary>
            /// 启动脚本的位置
            /// </summary>
            [ProcessIndex(2)]
            public string ScriptFormula = "{PathScripts}/formula.lua";

            /// <summary>
            /// 启动完毕脚本的位置
            /// </summary>
            [ProcessIndex(2)]
            public string ScriptStartup = "{PathScripts}/startup.lua";

            /// <summary>
            /// Debug 脚本的位置
            /// </summary>
            [ProcessIndex(2)]
            public string ScriptDebug = "{PathScripts}/debug.lua";

            /// <summary>
            /// 图片资源的位置
            /// </summary>
            [ProcessIndex(1)]
            public string PathTextures = "{PathResources}/Textures";
        }

        /// <summary>
        /// 路径数据, 缓存了应用的特殊路径
        /// </summary>
        public PathsDatabase Paths = new PathsDatabase();

        /// <summary>
        /// 基础属性模型
        /// </summary>
        public List<DataModel> DataModels = new List<DataModel>()
        {
            new DataModel() { Name = "STR", Formula = "3D6", Description = "力量值", Derived = false },
            new DataModel() { Name = "CON", Formula = "3D6", Description = "体质值", Derived = false },
            new DataModel() { Name = "POW", Formula = "3D6", Description = "意志值", Derived = false },
            new DataModel() { Name = "DEX", Formula = "3D6", Description = "敏捷值", Derived = false },
            new DataModel() { Name = "APP", Formula = "3D6", Description = "外貌值", Derived = false },
            new DataModel() { Name = "SIZ", Formula = "2D6+6", Description = "体型值", Derived = false },
            new DataModel() { Name = "INT", Formula = "2D6+6", Description = "智力值", Derived = false },
            new DataModel() { Name = "EDU", Formula = "3D6+3", Description = "教育值", Derived = false },
            new DataModel() { Name = KEY_ASSET, Formula = "1D10", Description = "资产", Derived = true},
            new DataModel() { Name = "SAN", Formula = "POW*5", Description = @"This score is used as a percentile roll that presents your investigator’s ability to remain stoic in the face of horrors. As you encounter the monstrosities of the Cthulhu Mythos your SAN score fluctuates.", Derived = true },
            new DataModel() { Name = "LUCK", Formula = "POW*5", Description = @"A Luck roll is often used to determine whether external circumstances are in your favour or against you.", Derived = true },
            new DataModel() { Name = "IDEA", Formula = "INT*5", Description = @"当没有什么技能鉴定比较合适的时候，可以用这个鉴定来解决问题。
将侦查技能留给那些不会立刻明白的特定线索或物品。当与人交往的时候仍然需要运用心理学。", Derived = true },
            new DataModel() { Name = "KNOW", Formula = "EDU*5", Description = @"所有人都了解一些不同方面的知识。
这个知识检定表现了调查员大脑中储存的，能够提供各种信息的可能性。
因为没有人可以知道一切，知识检定永远不能到达99，哪怕一个调查员有21点教育。", Derived = true, Upper = 99},
        };

        [YamlIgnore]
        private Dictionary<string, DataModel> baseModelDict;

        /// <summary>
        /// 属性模型的字典
        /// </summary>
        [YamlIgnore]
        public Dictionary<string, DataModel> BaseModelDict
        {
            get
            {
                if (baseModelDict == null)
                {
                    baseModelDict = new Dictionary<string, DataModel>(DataModels.Select(m => new KeyValuePair<string, DataModel>(m.Name, m)));
                }
                return baseModelDict;
            }
        }

        /// <summary>
        /// 用于指定 <see cref="Process"/> 中, 处理此字段的顺序 (数字升序)
        /// </summary>
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        sealed class ProcessIndexAttribute : Attribute
        {
            public ProcessIndexAttribute(int index)
            {
                Index = index;
            }

            public int Index { get; set; }

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
        /// 将语句中的关键字 (被 '{' 与 '}' 环绕的词) 替换为 <paramref name="getters"/> 中获取的值
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="getters"></param>
        /// <returns></returns>
        protected static string MapKeywords(string sentence, Dictionary<string, Func<string>> getters)
        {
            var matches = Regex.Matches(sentence, @"\{[^\{\}]*\}");
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var m = matches[i];
                var r = m.Value.Trim('{', '}');
                if (getters.Keys.Contains(r))
                {
                    sentence = Regex.Replace(sentence, m.Value, getters[r]());
                }
            }
            return sentence;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        public Config Process()
        {
            var typeofProcessIndexAttr = typeof(ProcessIndexAttribute);
            var typeofStr = typeof(string);
            var target = Paths;
            var fields = ProcessIndexAttribute.SortField(target);
            var gettersdict = new Dictionary<string, Func<string>>(
                from f in fields
                where f.FieldType == typeofStr
                select new KeyValuePair<string, Func<string>>(f.Name,
                                                              () => f.GetValue(target)?.ToString() ?? string.Empty));
            foreach (var field in fields)
            {
                if (field.FieldType != typeofStr) { continue; }
                var value = field.GetValue(target).ToString();
                value = MapKeywords(value, gettersdict);
                field.SetValue(target, value);
            }

            gettersdict = new Dictionary<string, Func<string>>(
                from f in Translator.dictionary
                select new KeyValuePair<string, Func<string>>(f.Key, () => f.Value));
            foreach (var item in Translator.dictionary.Keys.ToArray())
            {
                var value = MapKeywords(Translator.dictionary[item], gettersdict);
                Translator.dictionary[item] = value;
                Translator.dictionaryIgnoreCase[item.ToUpper()] = value;
            }
            return this;
        }
    }
}
