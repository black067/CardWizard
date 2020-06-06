using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace CallOfCthulhu
{
    /// <summary>
    /// 本地化文本相关数据
    /// </summary>
    public class Translator
    {
        /// <summary>
        /// 可选择的时代
        /// </summary>
        [Description("可选择的时代")]
        public string[] labelForEras = new string[]
        {
                "1890s", "1920s", "现代"
        };
        /// <summary>
        /// 名词翻译
        /// </summary>
        [Description("名词对照表")]
        public Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "Era", "时代" },
            { "Name", "名称" },
            { "Name.Placeholder", "请输入名称…" },
            { "Name.Default", "无名小卒" },
            { "Gender", "性别" },
            { "Gender.Male", "男" },
            { "Gender.Female", "女" },
            { "Gender.Others", "?" },
            { "Age", "年龄" },
            { "Age.ToolTip", "调查员的年龄应当在 15 至 90 之间。若调查员超过了这个年龄范围，请找您的守秘人进行调整吧。" },
            { "Education", "学历" },
            { "Education.Placeholder", "请输入学历…" },
            { "Address", "居住地" },
            { "Address.Placeholder", "请输入居住地…" },
            { "Homeland", "故乡" },
            { "Homeland.Placeholder", "请输入故乡…" },
            { "Occupation", "职业"},
            { "Occupation.Select", "选择职业" },
            { "Occupation.Select.ToolTip", "为你的角色指定职业" },
            { "Description", "描述" },
            { "Skills", "技能" },
            { "Characteristics", "属性" },
            // 力量
            { "STR", "力量" },
            { "STR.ToolTip", @"力量是调查员肌肉能力的量化。该属性会决定调查员在近战中造成的伤害。 " },
            { "STR.Block", "STR # FontSize: 12\n 力量 # FontSize: 16" },
            // 体质
            { "CON", "体质" },
            { "CON.ToolTip", @"体质意味着健康、生气和活力。" },
            { "CON.Block", "CON # FontSize: 12\n 体质 # FontSize: 16" },
            // 体型
            { "SIZ", "体型" },
            { "SIZ.ToolTip", @"{SIZ} 可以帮助决定生命值和伤害加值和体格。" },
            { "SIZ.Block", "SIZ # FontSize: 12\n 体型 # FontSize: 16" },
            // 智力
            { "INT", "智力" },
            { "INT.ToolTip", @"{INT}表示为调查员学习能力、理解 能力、信息分析能力 和解密能力的优劣度。" },
            { "INT.Block", "INT # FontSize: 10\n 智力 # FontSize: 12\n IDEA # FontSize: 10\n 灵感 # FontSize: 12" },
            // 意志
            { "POW", "意志" },
            { "POW.ToolTip", @"{POW}正是心意的力量；意志越高，学习和抵抗魔法的资质就越高。" },
            { "POW.Block", "POW # FontSize: 12\n 意志 # FontSize: 16" },
            // 敏捷
            { "DEX", "敏捷" },
            { "DEX.ToolTip", @"拥有高敏捷的调查员更快，灵巧，身体更加柔软。守秘人可以过一个敏捷鉴定判断他是否能抓住空中落物。" },
            { "DEX.Block", "DEX # FontSize: 12\n 敏捷 # FontSize: 16" },
            // 外表
            { "APP", "外表" },
            { "APP.ToolTip", @"外貌统括了肉体吸引力和人格魅力。高外貌的人潇洒而惹人喜爱，但不一定会有一副好面孔。" },
            { "APP.Block", "APP # FontSize: 12\n 外表 # FontSize: 16" },
            // 教育
            { "EDU", "教育" },
            { "EDU.ToolTip", @"{EDU} 代表了调查员的通过学校学习的知识。教育直接影响了调查员的职业技能点数的多少。。" },
            { "EDU.Block", "EDU # FontSize: 12\n 教育 # FontSize: 16" },
            // 理智
            { "SAN", "理智" },
            { "SAN.ToolTip", @"{SAN} 值用来衡量调查员是否能够承受克苏鲁神话的惊怖，以及能够勇敢地面对恐怖的情景而不转身逃走。" },
            { "SAN.Block", "SAN # FontSize: 12\n 理智 # FontSize: 16" },
            // 幸运
            { "LUCK", "幸运" },
            { "LUCK.ToolTip", @"在玩家进行技能检定后（使用技能或属性），可以花费幸运值来改变结果。" },
            { "LUCK.Block", "LUCK # FontSize: 12\n 幸运 # FontSize: 16" },
            // 灵感
            { "IDEA", "灵感" },
            { "IDEA.ToolTip", @"当没有什么技能鉴定比较合适的时候，可以用这个鉴定来解决问题。" },
            // 生命值
            { "HP", "生命值" },
            { "HP.ToolTip", @"{HP}用来计算一名调查员、NPC、怪物在游戏中累计受到的伤害量，用来表明这个家伙在被疼痛、疲惫和死亡压垮前还能走多久。" },
            { "HP.Block", "Hit # FontSize: 10\n Points # FontSize: 10 \n 生命 # FontSize: 14" },
            // 魔法值
            { "MP", "魔法值" },
            { "MP.ToolTip", @"（大多数情况下）施法时必须消耗{MP}，激活造物、为魔法通道充能等同样需要。" },
            { "MP.Block", "Magic # FontSize: 10\n Points # FontSize: 10 \n 魔法 # FontSize: 14" },
            // 移动速度
            { "MOV", "移动" },
            { "MOV.ToolTip", @"调查员每轮可以移动的码数（或米数）等于他的{MOV}值的五倍。" },
            { "MOV.Block", "MOV # FontSize: 12\n 移动 # FontSize: 16" },
            // 知识
            { "KNOW", "知识" },
            { "KNOW.ToolTip", @"所有人都了解一些不同方面的{KNOW}。因为没有人可以知道一切，{KNOW}检定永远不能到达99，哪怕一个调查员有21点教育。" },
            // 信用评级与资产相关
            { "CreditRating", "信用评级" },
            { "CreditRatingRange", "信用评级范围" },
            { "CreditRating.ToolTip", "调查员的日日所需取决于信用评级。你的主要所有物，包括房子和汽车，都取决于信用评级。" },
            { "Backstory.SpendingLevel", "消费水平" },
            { "Backstory.Cash", "现金" },
            { "Backstory.Assets", "资产" },
            { "AST", "资产" },
            { "AST.ToolTip", @"调查员拥有财产和其他价值年收入五倍的{AST}；" },
            { "SUM", "总计" },
            { "Initial", "初始" },
            { "Initials", "属性初始值" },
            { "Initial.ToolTip", "初始值, 撰写档案时掷骰得出" },
            { "Adjustments", "属性值调整" },
            { "Adjustment", "调整" },
            { "Adjustment.ToolTip", "调整值, 计算相关影响因素后得出" },
            { "Growths", "属性值成长" },
            { "Growth", "成长" },
            { "Growth.ToolTip", "成长值, 随着游戏进行可能获得成长" },
            { "OccupationPoints", "职业点数" },
            { "InterestPoints", "兴趣点数" },
            { "Value.Half.ToolTip", "半值" },
            { "Value.OneFifth.ToolTip", "五分之一值" },
            // 派生属性相关
            // 伤害加值
            { "DamageBonus", "伤害加值" },
            { "DamageBonus.Block", "Damage # FontSize: 10\nBonus # FontSize: 10\n {DamageBonus} # FontSize: 12" },
            { "DamageBonus.Block.ToolTip", "所有调查员，包括守秘人控制角色和怪物都有着名为“{DamageBonus}”（ＤＢ）和“{Build}”的两种属性。高大强壮的生物和人类可以比那些孱弱的同类造成更多的物理伤害。" },
            // 体格
            { "Build", "体格" },
            { "Build.Block", "Build # FontSize: 10\n {Build} # FontSize: 14" },
            { "Build.Block.ToolTip", "{Build}则会用在战技和追逐中，用它来进行比较和计算。" },
            // 闪避
            { "DODGE", "闪避" },
            { "DODGE.ToolTip", @"允许调查员本能地闪避攻击，投掷过来的投射物以及诸如此类的。
一名角色可以尝试在一场战斗轮中使用任何次数的闪避（但是对抗一次特定的攻击只能一次；见章节6：战斗）。
闪避可以通过经验来提升，就像其他的技能一样。如果一次攻击可以被看见，一名角色可以尝试闪避开它。
想要闪避子弹是不可能的，因为运动中的它们是不可能被看见的；一名角色所能做到的最好的是做逃避的行动来造成自己更难被命中——“寻找掩体（Diving for cover）”" },
            { "DODGE.Block", "DODGE # FontSize: 10\n 闪避 # FontSize: 14" },
            // 背景故事相关
            { "Backstory.PersonalDescription", "个人描述" },
            { "Backstory.Traits", "特质" },
            { "Backstory.IdeologyAndBeliefs", "思想 / 信仰" },
            { "Backstory.InjuriesAndScars", "伤口 & 疤痕" },
            { "Backstory.SignificantPeople", "重要之人" },
            { "Backstory.PhobiasAndManias", "恐惧症 & 躁狂症" },
            { "Backstory.MeaningfulLocations", "意义非凡之地" },
            { "Backstory.ArcaneTomesAndETC", "神话典籍、法术和魔法物品" },
            { "Backstory.TreasuredPossessions", "宝贵之物" },
            { "Backstory.EncountersWithStrangeEntities", "异常接触史" },
            // 职业相关
            { "Occupation.PointFormula", "技能点数公式" },
            // 技能相关
            { "Custom", "自选技能" },
            { "Custom.Professional", "自选专业技能" },
            { "Custom.Social", "自选社交型技能" },
            { "Custom.Artistic", "自选艺术与手艺" },
            { "OccupationPoints.Label", "职业"},
            { "PersonalPoints.Label", "兴趣"},
            { "GrowthPoints.Label", "成长"},
            // 其它语句
            { "true", "是" },
            { "false", "否" },
            { "Confirm", "确定" },
            { "Cancel", "取消" },
            { "Check", "检定" },
            // 菜单栏
            { "MenuBar.Button.Create", "新建" },
            { "MenuBar.Button.Create.ToolTip", "Ctrl + N\n创建一份新的调查员档案" },
            { "MenuBar.Button.Save", "保存" },
            { "MenuBar.Button.Save.ToolTip", "Ctrl + S\n保存你的调查员档案" },
            { "MenuBar.Button.SavePicture", "🖨" },
            { "MenuBar.Button.SavePicture.ToolTip", "Ctrl + Shift + S\n将你的调查员档案保存为一张图片" },
            { "MenuBar.Button.Settings", "设置" },
            { "MenuBar.Button.ShowToolTip", "显示 Tool Tip 提示" },
            { "Tab.Front.Header", "档案 正面" },
            { "Tab.Back.Header", "档案 背面" },
            // 界面元素
            { "Investigator.Document.Title", "DOCUMENT" },
            { "Investigator.Characteristics.Title", "CHARACTERISTICS" },
            { "Investigator.Combat.Title", "COMBAT" },
            { "Investigator.Weapons.Title", "WEAPONS" },
            { "Investigator.Skills.Title", "INVESTIGATOR SKILLS" },
            { "Investigator.Backstory.Title", "BACKSTORY" },
            { "Investigator.Gears.Title", "GEAR & POSSESSIONS" },
            { "Investigator.Assets.Title", "CASH & ASSETS" },
            { "Investigator.Rules.Title", "QUICK REFERENCE RULES" },
            { "Investigator.Partners.Title", "FELLOW INVESTIGATORS" },
            { "Investigator.PersonalPoints" , "个人技能点" },
            { "Investigator.OccupationPoints" , "职业技能点" },
            // 控件显示
            { "Card.Image.Avatar", "点击可导入新头像" },
            { "Card.Button.Regenerate", "🎲" },
            { "Card.Button.Regenerate.ToolTip", "重新生成角色属性" },
            { "Card.Button.DMGBonus", "🎲" },
            { "Card.Button.DMGBonus.ToolTip", "掷一次伤害加值" },
            { "Card.NewItem.Placeholder", "添加新物品?"},
            { "Card.Button.AddItem", "➕"},
            // 其它窗口
            { "GenerationWindow.Title", "生成了以下属性" },
            { "GenerationWindow.Message", $"选择一组数据作为调查员的基础属性\n* 请确认调查员年龄, 并按照年龄调整属性点\n* {{LUCK}} 将在确认后自动骰出" },
            { "GenerationWindow.Helper", "* 此行填写调整值" },
            { "GenerationWindow.ValidationCheck", "校验" },
            { "OccupationWindow.Title", "有以下职业可供选择" },
            { "OccupationWindow.Message", "" },
            { "Dialog.Import.Avatar.Title", "选择调查员的照片 (*.png)" },
            { "Dialog.Overwrite.Confirmation", "是否确认用\n{0}\n覆盖现有的文件?" },
            // 消息
            { "Message.RollADMGBonus", "根据公式 {0}, 本轮掷出了 {1}" },
            { "Message.Character.Saved", "调查员的文档已保存至: {0}" },
            { "Message.Character.SavedPic", "调查员的图像档案已保存至: {0}" },
            { "Message.Character.Switched", "已切换至调查员: {0}" },
            { "Message.Characteristic.Overflow", "属性: {0} 的值不能超过 {1}" },
            { "Message.Skill.Overflow", "技能: {0} 的值不能超过 {1}" },
            { "Message.Avatar.FileNotSupported", "不支持此文件格式: \"{0}\", 目前只支持 {1} 格式." },
            // 页首与页尾
            { "QuickReferenceRules", @"技能和属性检定 # { FontSize:16, FontWeight: Bold }
成功等级 [ 大失败 : 00/96+ ] [ 失败 : 点数 > 技能 ]
[ 常规 : 点数 <= 技能 ] [ 困难 : 点数 <= 技能 / 2 ] [ 极难 : 点数 <= 技能 / 5 ]

" },
            { "Page.Main.Head", "- Call of Cthulhu -" },
            { "Page.Main.Trail", "- ★ -" },
            { "Page.Backstory.Head", "- Call of Cthulhu -" },
            { "Page.Backstory.Trail", "- ⚝ -" },
        };

        /// <summary>
        /// 转换为包含了关键字与实际值的字典
        /// <para>可用于 <see cref="MapKeywords(string, Dictionary{string, Func{string}})"/></para>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Func<string>> ToKeywordsMap()
        {
            return new Dictionary<string, Func<string>>(from kvp in dictionary
                                                        select new KeyValuePair<string, Func<string>>(kvp.Key, () => kvp.Value));
        }

        /// <summary>
        /// 将语句中的关键字 (被 '{' 与 '}' 环绕的词) 替换为 <paramref name="getters"/> 中获取的值
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="getters"></param>
        /// <returns></returns>
        public static string MapKeywords(string sentence, Dictionary<string, Func<string>> getters)
        {
            var matches = Regex.Matches(sentence, @"\{[^\{\}]*\}");
            Match match;
            string keyword;
            Func<string> getter;
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                match = matches[i];
                keyword = match.Value.Trim('{', '}');
                if (getters.TryGetValue(keyword, out getter))
                {
                    sentence = Regex.Replace(sentence, match.Value, getter());
                }
            }
            return sentence;
        }

        /// <summary>
        /// 将语句中的关键字 (被 '{' 与 '}' 环绕的词) 替换为 <paramref name="getters"/> 中获取的值
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string MapKeywords(string sentence, IDictionary dictionary)
        {
            var words = new Dictionary<string, string>(from object k in dictionary.Keys select KeyValuePair.Create(k.ToString(), dictionary[k].ToString()));
            var matches = Regex.Matches(sentence, @"\{[^\{\}]*\}");
            Match match;
            string keyword;
            string value;
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                match = matches[i];
                keyword = match.Value.Trim('{', '}');
                if (words.TryGetValue(keyword, out value))
                {
                    sentence = Regex.Replace(sentence, match.Value, value);
                }
            }
            return sentence;
        }

        /// <summary>
        /// 替换翻译文本中的关键字
        /// </summary>
        public void Process()
        {
            var gettersdict = ToKeywordsMap();
            foreach (var item in dictionary.Keys.ToArray())
            {
                dictionary[item] = MapKeywords(dictionary[item], gettersdict);
            }
        }

        /// <summary>
        /// 查询翻译后的文字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:指定 CultureInfo", Justification = "<挂起>")]
        public bool TryTranslate(string path, out string text)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                text = string.Empty;
                return false;
            }
            path = path.Replace('_', '.');
            return dictionary.TryGetValue(path, out text);
        }

        /// <summary>
        /// 查询翻译后的文字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string Translate(string path, string @default) => TryTranslate(path, out var text) ? text : @default;

        /// <summary>
        /// 用递归的方式, 对目标字典的键进行翻译
        /// </summary>
        /// <param name="target"></param>
        public void TranslateKeys(IDictionary target)
        {
            if (target == null) return;
            var keys = new object[target.Count];
            target.Keys.CopyTo(keys, 0);
            foreach (var key in keys)
            {
                var value = target[key];
                if (value is IDictionary child)
                {
                    TranslateKeys(child);
                }
                if (TryTranslate(key.ToString(), out var newKey))
                {
                    target.Remove(key);
                    target[newKey] = value;
                }
            }
        }
    }
}
