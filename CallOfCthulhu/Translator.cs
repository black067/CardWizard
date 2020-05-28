using System;
using System.Collections;
using System.Collections.Generic;
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
            { "Age.ToolTip", "调查员的年龄应当在 15 至 90 之间。若调查员超过了这个年龄范围，请找您的守秘人进行调整吧。" },
            { "Education", "学历" },
            { "Address", "居住地" },
            { "Homeland", "故乡" },
            { "Occupation", "职业"},
            { "Occupation.Select", "选择职业" },
            { "Occupation.Select.ToolTip", "为你的角色指定职业" },
            { "Description", "描述" },
            { "Skills", "技能" },
            { "Traits", "属性" },
            // 力量
            { "STR", "力量" },
            { "STR.ToolTip", @"力量是调查员肌肉能力的量化。力量越高，调查员就能举起更重的东西或更强有力的抓住物体。
该属性会决定调查员在近战中造成的伤害。
力量降低为0时，调查员就成为了一个无法离开床铺的病号。 " },
            { "STR.Block", "STR # FontSize: 12\n 力量 # FontSize: 16" },
            // 体质
            { "CON", "体质" },
            { "CON.ToolTip", @"体质意味着健康、生气和活力。毒药和疾病会与 调查员的体质属性正面相斗。
高体质的调查员会有更多的生命值——能承受更多伤害和攻击。严重的物理损伤或魔法攻击有可能降低该属性，而当体质降为0时，调查员就死咯。" },
            { "CON.Block", "CON # FontSize: 12\n 体质 # FontSize: 16" },
            // 体型
            { "SIZ", "体型" },
            { "SIZ.ToolTip", @"体型值将身高和体重整合成了一个数字。
伸长脖子越过矮墙观望，或者挤进狭窄的空间，或者判定谁的头在蹲下时也会高处草堆一个截时，就看体型了。
体型可以帮助决定生命值和伤害加值和体格。体型的减少通常意味着丢失肢体，当然这也意味着敏捷的减少。
对于调查员来说，失去所有体型，应该意味着他消失了——只有上帝知道他在哪！" },
            { "SIZ.Block", "SIZ # FontSize: 12\n 体型 # FontSize: 16" },
            // 智力
            { "INT", "智力" },
            { "INT.ToolTip", @"智力代表了这个调查员的学习能力，记忆力和分析力。以及他对周围事物的认知。为了帮助形容不同的局势，守秘人会将智力乘以各种倍数然后丢D100，要求检定结果等于或者小于这个数字。智力乘以5，是灵感。
困难的概念，计划和灵感暗示会可能会要求将智力乘以1或者2，然后D100要求低于那个难度等级。这种鉴定可以判定调查员是否能得到更多的信息或者情报。
一个没有智力的调查员就是白痴。
智力直接影响了一个调查员兴趣技能点数的多少，还有他们学习克苏鲁咒语的速度。
如果智力的数值跟玩家的角色风格不相符合，这里还有个机会帮助扮演：那既是高教育低智力，或者低教育高智力。前者是知道理论但不足了解现象的学者，后者是刚到大城市的农场男孩。" },
            { "INT.Block", "INT # FontSize: 10\n 智力 # FontSize: 12\n IDEA # FontSize: 10\n 灵感 # FontSize: 12" },
            // 意志
            { "POW", "意志" },
            { "POW.ToolTip", @"{POW}代表了意志力。{POW}越高，{MP}也就越高。{POW}并不代表领导力，后者只是扮演的问题。
一个没有{POW}的调查员就像一个僵尸一样，而且不能使用魔法。除非特殊的情况，否则{POW}的损失都是永久的。
{MP}，并不像意志值，是会不断地恢复的。普通人的{POW}很少会有变化。
如果他能够熟练的使用某种克苏鲁巫术，便有可能增长他的{POW}。守秘人要特别浏览p101的那个表格。" },
            { "POW.Block", "POW # FontSize: 12\n 意志 # FontSize: 16" },
            // 敏捷
            { "DEX", "敏捷" },
            { "DEX.ToolTip", @"拥有高敏捷的调查员更快，灵巧，身体更加柔软。守秘人可以过一个敏捷鉴定判断他是否能抓住空中落物。或者在强风以及冰上保持平衡。完成灵敏的动作，或者不像被人注意到。
就像其他属性一样，敏捷的难度鉴定也源于守秘人挑选不同的倍数乘以敏捷值。一个没有敏捷的调查员非常的不协调，不能完成很多动作，也无法使用幸运鉴定。
在战斗中，高敏捷的角色先行动，而且有可能在敌人攻击前缴械或者使其失效。
敏捷乘以2也就是闪躲技能的起始值。" },
            { "DEX.Block", "DEX # FontSize: 12\n 敏捷 # FontSize: 16" },
            // 外表
            { "APP", "外表" },
            { "APP.ToolTip", @"外貌统括了肉体吸引力和人格魅力。高外貌的人潇洒而惹人喜爱，但不一定会有一副好面孔。
外貌降为０的人恐怖而丑陋，有着令人十分厌恶的举止，走到哪都会引发议论和震动。
外貌会在社交活动中发生效用，或在试图给某人 留下好印象时有所帮助。" },
            { "APP.Block", "APP # FontSize: 12\n 外表 # FontSize: 16" },
            // 教育
            { "EDU", "教育" },
            { "EDU.ToolTip", @"教育代表了调查员的通过学校学习的知识。教育直接影响了调查员的职业技能点数的多少。教育乘以5便是知识鉴定。教育乘以5也是一个调查员起始状态下的母语的技能点数。
一个没有教育就像一个初生的婴儿，或者是对世界没有半点认知，失忆症患者。
教育达到12代表了高中毕业生的教育水平。如果高于12，则表示有一些大学生活了。教育达到16就可以拿到学位了。一个教育很高的调查员不见得非要上学，但是一定是严谨系统的学习过。" },
            { "EDU.Block", "EDU # FontSize: 12\n 教育 # FontSize: 16" },
            // 理智
            { "SAN", "理智" },
            { "SAN.ToolTip", @"{SAN}等于{POW}乘以5。{SAN}是派生属性，但它是游戏的重点和核心部分。{SAN}有几个相关概念：{SAN}，理智点，最大理智值。理智点会发生变化，但是{SAN}不会发生变化。
一个调查员最大的理智点绝对不可能超过99。理智点为99代表了人类最坚强的意志，已经达到了接近于“钢铁意志”程度。
相反，一个{SAN}只有30的人则更容易被打击，刺激乃至发疯。绝大多数的怪物和超自然时间都会让玩家丧失理智点，就连法术也必须用理智点为代价才能得以施展。" },
            { "SAN.Block", "SAN # FontSize: 12\n 理智 # FontSize: 16" },
            // 幸运
            { "LUCK", "幸运" },
            { "LUCK.ToolTip", @"调查员是否带来什么特殊的装备？他们哪个是空鬼（一种怪物）选择攻击的目标？调查员走上的楼梯是否会突然碎裂，或者是发生地震？{LUCK}鉴定就能快速的得到答案。
幸运便是一项能力能使你在正确的时间呆在正确的地点：往往在紧急时刻使用它，特别是当守秘人希望调查员能有更高的机会通过，在仅用常用的跳跃和闪躲检定之上。" },
            { "LUCK.Block", "LUCK # FontSize: 12\n 幸运 # FontSize: 16" },
            // 灵感
            { "IDEA", "灵感" },
            { "IDEA.ToolTip", @"当没有什么技能鉴定比较合适的时候，可以用这个鉴定来解决问题。例如：调查员是否能观察或理解他们所见到的事物？一个普通人是否会感受到一次特殊的召唤或者某个地点？是否有什么东西在山的另一头？
将侦查技能留给那些不会立刻明白的特定线索或物品。当与人交往的时候仍然需要运用心理学。" },
            // 生命值
            { "HP", "生命值" },
            { "HP.ToolTip", @"{HP}用来计算一名调查员、NPC、怪物在游戏中累计受到的伤害量，用来表明这个家伙在被疼痛、疲惫和死亡压垮前还能走多久。" },
            { "HP.Block", "Hit # FontSize: 14\n Points # FontSize: 14" },
            // 魔法值
            { "MP", "魔法值" },
            { "MP.ToolTip", @"（大多数情况下）施法时必须消耗{MP}，激活造物、为魔法通道充能等同样需要。" },
            { "MP.Block", "Magic # FontSize: 14\n Points # FontSize: 14" },
            // 移动速度
            { "MOV", "移动" },
            { "MOV.ToolTip", @"调查员每轮可以移动的码数（或米数）等于他的{MOV}值的五倍。" },
            { "MOV.Block", "MOV # FontSize: 12\n 移动 # FontSize: 16" },
            // 知识
            { "KNOW", "知识" },
            { "KNOW.ToolTip", @"所有人都了解一些不同方面的{KNOW}。这个{KNOW}检定表现了调查员大脑中储存的，能够提供各种信息的可能性。
调查员可能明白如何将硫酸倒入水中，或者将水倒入硫酸中（不管她是否学过化学），或者他可能知道西藏的地理信息（而不需要地质学检定），或者了解蜘蛛有多少只腿（哪怕只有一点生物学）。
辨认俗语俚语便是知识检定的一个绝佳的应用。
因为没有人可以知道一切，{KNOW}检定永远不能到达99，哪怕一个调查员有21点教育。" },
            // 信用评级
            { "CreditRating", "信用评级" },
            { "CreditRating.ToolTip", "调查员的日日所需取决于信用评级。你的主要所有物，包括房子和汽车，都取决于信用评级。" },
            // 资产
            { "AST", "资产" },
            { "AST.ToolTip", @"调查员拥有财产和其他价值年收入五倍的{AST}；
一个现代的调查员投出55000美元拥有275000的{AST}。这些资产的十分之一存入银行当作现金。另外十分之一是股份和债券，可以在30天内转移。余下的是老书，房子或者是任何符合角色的东西。" },
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
            // 职业相关
            { "Occupation.PointFormula", "技能点数公式" },
            // 技能相关
            { "Custom", "自选技能" },
            { "Custom.Professional", "自选专业技能" },
            { "Custom.Social", "自选社交型技能" },
            { "Custom.Artistic", "自选艺术与手艺" },
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
            { "MenuBar.Button.SavePicture", "生成图片" },
            { "MenuBar.Button.SavePicture.ToolTip", "Ctrl + Shift + S\n将你的调查员档案保存为一张图片" },
            { "MenuBar.Button.Settings", "设置" },
            { "MenuBar.Button.ShowToolTip", "显示提示浮窗" },
            // 界面元素
            { "Investigator.Document.Title", "DOCUMENT 档案" },
            { "Investigator.Traits.Title", "CHARACTERISTICS 属性" },
            { "Investigator.Combat.Title", "COMBAT 格斗" },
            { "Investigator.Weapons.Title", "WEAPONS 武器" },
            { "Investigator.Skills.Title", "INVESTIGATOR SKILLS 武器" },
            // 控件显示
            { "Card.Image.Avatar", "点击可导入新头像" },
            { "Card.Button.Regenerate", "🎲" },
            { "Card.Button.Regenerate.ToolTip", "重新生成角色属性" },
            { "Card.Button.DMGBonus", "🎲" },
            { "Card.Button.DMGBonus.ToolTip", "掷一次伤害加值" },
            // 其它窗口
            { "GenerationWindow.Title", "生成了以下属性..." },
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
            { "Message.Trait.Overflow", "属性: {0} 的值不能超过 {1}" },
            // 页首与页尾
            { "Page.Head", "- Call of Cthulhu -" },
            { "Page.Trail", "- ★ -" },
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
