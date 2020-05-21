using System;
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
            { "Age.Min.ToolTip", "角色的最小年龄是 {0}" },
            { "Age.Penalty.ToolTip", "角色超过 40 岁时, 每超过 10 年, 从以下属性中选择 1 点减去: {STR} / {CON} / {DEX} / {APP}." },
            { "Education", "学历" },
            { "Address", "居住地" },
            { "Homeland", "故乡" },
            { "Occupation", "职业"},
            { "Occupation.Select", "选择职业" },
            { "Description", "描述" },
            { "Skills", "技能" },
            { "skills", "技能" },
            { "Traits", "属性" },
            // 力量
            { "STR", "力量" },
            { "STR.Description", @"力量是调查员肌肉能力的量化。力量越高，调查员就能举起更重的东西或更强有力的抓住物体。
该属性会决定调查员在近战中造成的伤害。
力量降低为0时，调查员就成为了一个无法离开床铺的病号。 " },
            { "STR.TraitBox", "力量\nSTR # FontSize: 14" },
            // 体质
            { "CON", "体质" },
            { "CON.Description", @"体质意味着健康、生气和活力。毒药和疾病会与 调查员的体质属性正面相斗。
高体质的调查员会有更 多的生命值——能承受更多伤害和攻击。严重的物理损伤或魔法攻击有可能降低该属性，而当体质降为0时，调查员就死咯。" },
            { "CON.TraitBox", "体质\nCON # FontSize: 14" },
            // 体型
            { "SIZ", "体型" },
            { "SIZ.Description", @"体型值将身高和体重整合成了一个数字。
伸长脖子越过矮墙观望，或者挤进狭窄的空间，或者判定谁的头在蹲下时也会高处草堆一个截时，就看体型了。
体型可以帮助决定生命值和伤害加值和体格。体型的减少通常意味着丢失肢体，当然这也意味着敏捷的减少。
对于调查员来说，失去所有体型，应该意味着他消失了——只有上帝知道他在哪！" },
            { "SIZ.TraitBox", "体型\nSIZ # FontSize: 14" },
            // 智力
            { "INT", "智力" },
            { "INT.Description", @"智力代表了这个调查员的学习能力，记忆力和分析力。以及他对周围事物的认知。为了帮助形容不同的局势，守秘人会将智力乘以各种倍数然后丢D100，要求检定结果等于或者小于这个数字。智力乘以5，是灵感。
困难的概念，计划和灵感暗示会可能会要求将智力乘以1或者2，然后D100要求低于那个难度等级。这种鉴定可以判定调查员是否能得到更多的信息或者情报。
一个没有智力的调查员就是白痴。
智力直接影响了一个调查员兴趣技能点数的多少，还有他们学习克苏鲁咒语的速度。
如果智力的数值跟玩家的角色风格不相符合，这里还有个机会帮助扮演：那既是高教育低智力，或者低教育高智力。前者是知道理论但不足了解现象的学者，后者是刚到大城市的农场男孩。" },
            { "INT.TraitBox", "智力\nINT\n灵感\nIDEA # FontSize: 10" },
            // 意志
            { "POW", "意志" },
            { "POW.Description", @"意志代表了意志力。意志越高，魔法值也就越高。意志并不代表领导力，后者只是扮演的问题。
一个没有意志的调查员就像一个僵尸一样，而且不能使用魔法。除非特殊的情况，否则意志的损失都是永久的。
意志乘以5得到的是幸运。这个数字也刚好等于角色的理智属性。魔法值，并不像意志值，是会不断地恢复的。普通人的意志值很少会有变化。
如果他能够熟练的使用某种克苏鲁巫术，便有可能增长他的意志值。守秘人要特别浏览p101的那个表格。" },
            { "POW.TraitBox", "意志\nPOW # FontSize: 14" },
            // 敏捷
            { "DEX", "敏捷" },
            { "DEX.Description", @"拥有高敏捷的调查员更快，灵巧，身体更加柔软。守秘人可以过一个敏捷鉴定判断他是否能抓住空中落物。或者在强风以及冰上保持平衡。完成灵敏的动作，或者不像被人注意到。
就像其他属性一样，敏捷的难度鉴定也源于守秘人挑选不同的倍数乘以敏捷值。一个没有敏捷的调查员非常的不协调，不能完成很多动作，也无法使用幸运鉴定。
在战斗中，高敏捷的角色先行动，而且有可能在敌人攻击前缴械或者使其失效。
敏捷乘以2也就是闪躲技能的起始值。" },
            { "DEX.TraitBox", "敏捷\nDEX # FontSize: 14" },
            // 外表
            { "APP", "外表" },
            { "APP.Description", @"外貌统括了肉体吸引力和人格魅力。高外貌的人潇洒而惹人喜爱，但不一定会有一副好面孔。
外貌降为０的人恐怖而丑陋，有着令人十分厌恶的举止，走到哪都会引发议论和震动。
外貌会在社交活动中发生效用，或在试图给某人 留下好印象时有所帮助。" },
            { "APP.TraitBox", "外表\nAPP # FontSize: 14" },
            // 教育
            { "EDU", "教育" },
            { "EDU.Description", @"教育代表了调查员的通过学校学习的知识。教育直接影响了调查员的职业技能点数的多少。教育乘以5便是知识鉴定。教育乘以5也是一个调查员起始状态下的母语的技能点数。
一个没有教育就像一个初生的婴儿，或者是对世界没有半点认知，失忆症患者。
教育达到12代表了高中毕业生的教育水平。如果高于12，则表示有一些大学生活了。教育达到16就可以拿到学位了。一个教育很高的调查员不见得非要上学，但是一定是严谨系统的学习过。" },
            { "EDU.TraitBox", "教育\nEDU # FontSize: 14" },
            // 理智
            { "SAN", "理智" },
            { "SAN.Description", @"理智等于意志乘以5。理智是派生属性，但它是游戏的重点和核心部分。理智有几个相关概念：理智，理智点，最大理智值。理智点会发生变化，但是理智不会发生变化。
一个调查员最大的理智点绝对不可能超过99。理智点为99代表了人类最坚强的意志，已经达到了接近于“钢铁意志”程度。
相反，一个理智只有30的人则更容易被打击，刺激乃至发疯。绝大多数的怪物和超自然时间都会让玩家丧失理智点，就连法术也必须用理智点为代价才能得以施展。" },
            { "SAN.TraitBox", "理智\nSAN # FontSize: 14" },
            // 幸运
            { "LUCK", "幸运" },
            { "LUCK.Description", @"调查员是否带来什么特殊的装备？他们哪个是空鬼（一种怪物）选择攻击的目标？调查员走上的楼梯是否会突然碎裂，或者是发生地震？幸运鉴定就能快速的得到答案。
幸运便是一项能力能使你在正确的时间呆在正确的地点：往往在紧急时刻使用它，特别是当守秘人希望调查员能有更高的机会通过，在仅用常用的跳跃和闪躲检定之上。" },
            // 灵感
            { "IDEA", "灵感" },
            { "IDEA.Description", @"当没有什么技能鉴定比较合适的时候，可以用这个鉴定来解决问题。例如：调查员是否能观察或理解他们所见到的事物？一个普通人是否会感受到一次特殊的召唤或者某个地点？是否有什么东西在山的另一头？
将侦查技能留给那些不会立刻明白的特定线索或物品。当与人交往的时候仍然需要运用心理学。" },
            // 灵感
            { "HP", "生命值" },
            { "HP.Description", @"生命值用来计算一名调查员、NPC、怪物在游戏中累计受到的伤害量，用来表明这个家伙在被疼痛、疲惫和死亡压垮前还能走多久。" },
            // 魔法值
            { "MP", "魔法值" },
            { "MP.Description", @"（大多数情况下）施法时必须消耗魔法值，激活造物、为魔法通道充能等同样需要。" },
            // 移动速度
            { "MOV", "移动速度" },
            { "MOV.Description", @"调查员每轮可以移动的码数（或米数）等于他的移动值的五倍。" },
            { "MOV.TraitBox", "移动\nMOVE # FontSize: 14" },
            // 知识
            { "KNOW", "知识" },
            { "KNOW.Description", @"所有人都了解一些不同方面的知识。这个知识检定表现了调查员大脑中储存的，能够提供各种信息的可能性。
调查员可能明白如何将硫酸倒入水中，或者将水倒入硫酸中（不管她是否学过化学），或者他可能知道西藏的地理信息（而不需要地质学检定），或者了解蜘蛛有多少只腿（哪怕只有一点生物学）。
辨认俗语俚语便是知识检定的一个绝佳的应用。
因为没有人可以知道一切，知识检定永远不能到达99，哪怕一个调查员有21点教育。" },
            // 信用评级
            { "CreditRating", "信用评级" },
            { "CreditRating.Description", "调查员的日日所需取决于信用评级。你的主要所有物，包括房子和汽车，都取决于信用评级。" },
            // 资产
            { "AST", "资产" },
            { $"AST.Description", @"调查员拥有财产和其他价值年收入五倍的资产；
一个现代的调查员投出55000美元拥有275000的资产。这些资产的十分之一存入银行当作现金。另外十分之一是股份和债券，可以在30天内转移。余下的是老书，房子或者是任何符合角色的东西。" },
            { "SUM", "总计" },
            { "Initial", "初始" },
            { "Initial.ToolTip", "初始值, 撰写档案时掷骰得出" },
            { "Adjustment", "调整" },
            { "Adjustment.ToolTip", "调整值, 计算相关影响因素后得出" },
            { "Growth", "成长" },
            { "Growth.ToolTip", "成长值, 随着游戏进行可能获得成长" },
            { "OccupationPoints", "职业点数" },
            { "InterestPoints", "兴趣点数" },
            { "true", "是" },
            { "false", "否" },
            { "Valid", "✔" },
            { "Invalid", "❌" },
            { "MenuBar.Button.Create", "新建" },
            { "MenuBar.Button.Save", "保存" },
            { "MenuBar.Button.SavePicture", "生成图片" },
            { "Page.Head", "- Call of Cthulhu -" },
            { "Page.Trail", "- ★ -" },
            { "Card.Image.Avatar", "点击可导入新头像" },
            { "Card.Label.Traits.Title", "属性" },
            { "Card.Button.Regenerate", "🔀" },
            { "Card.Button.DMGBonus", "掷一次" },
            { "Card.Label.DamageBonus", "伤害奖励" },
            { "Message.RollADMGBonus", "根据公式 {0}, 本轮掷出了 {1}" },
            { "GenerationWindow.Title", "批量生成属性" },
            { "Dialog.Import.Avatar.Title", "选择调查员的照片 (*.png)" },
            { "Dialog.Import.Avatar.Confirmation", "是否确认用\n{0}\n覆盖现有的文件?" },
            { "Message.Regenerate", $"请选择一组数据作为角色的基础属性\n* 当前角色的成长值将被完全清空\n* {{AST}} 不计入属性总计" },
            { "Message.Character.Saved", "调查员的文档已保存至: {0}" },
            { "Message.Character.SavedPic", "调查员的图像档案已保存至: {0}" },
            { "Message.Character.Switched", "已切换至调查员: {0}" },
        };

        private Dictionary<string, string> dictionaryIgnoreCase = new Dictionary<string, string>();

        /// <summary>
        /// 将语句中的关键字 (被 '{' 与 '}' 环绕的词) 替换为 <paramref name="getters"/> 中获取的值
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="getters"></param>
        /// <returns></returns>
        public static string MapKeywords(string sentence, Dictionary<string, Func<string>> getters)
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
        /// 替换翻译文本中的关键字
        /// </summary>
        public void Process()
        {
            var gettersdict = new Dictionary<string, Func<string>>(
                from f in dictionary
                select new KeyValuePair<string, Func<string>>(f.Key, () => f.Value));
            foreach (var item in dictionary.Keys.ToArray())
            {
                var value = MapKeywords(dictionary[item], gettersdict);
                dictionary[item] = value;
                dictionaryIgnoreCase[item.ToUpper()] = value;
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
        public bool TryTranslate(string path, out string text, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                text = string.Empty;
                return false;
            }
            path = path.Replace('_', '.');
            if (ignoreCase)
            {
                return dictionaryIgnoreCase.TryGetValue(path.ToUpper(), out text);
            }
            return dictionary.TryGetValue(path, out text);
        }

        /// <summary>
        /// 查询翻译后的文字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string Translate(string path, string @default, bool ignoreCase = false) => TryTranslate(path, out var text, ignoreCase) ? text : @default;

        /// <summary>
        /// 用递归的方式, 对目标字典的键进行翻译
        /// </summary>
        /// <param name="target"></param>
        public void TranslateKeys(IDictionary<string, object> target)
        {
            if (target == null) return;
            var keys = target.Keys.ToArray();
            foreach (var key in keys)
            {
                var value = target[key];
                if (value is IDictionary<string, object> child)
                {
                    TranslateKeys(child);
                }
                if (TryTranslate(key, out var newKey))
                {
                    target.Remove(key);
                    target[newKey] = value;
                }
            }
        }
    }
}
