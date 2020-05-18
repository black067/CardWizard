using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YamlDotNet.Serialization;


namespace CardWizard.Data
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
                { Config.KEY_ASSET, "资产" },
                { "Growths", "成长值" },
                { "Adjustment", "调整值" },
                { "ToolTip.Trait", "最终值依次由 初始值, 成长值, 调整值 构成, 初始值不可变更" },
                { "OccupationPoints", "职业点数" },
                { "InterestPoints", "兴趣点数" },
                { "true", "是" },
                { "false", "否" },
                { "Valid", "✅" },
                { "Invalid", "❎" },
                { "MenuBar.Button.Create", "新建" },
                { "MenuBar.Button.Save", "保存" },
                { "MenuBar.Button.SavePicture", "生成图片" },
                { "Card.Image.Avatar", "点击可导入新头像" },
                { "Card.Label.BasePropSum", "基础属性总计" },
                { "Card.Button.Regenerate", "重新生成" },
                { "Card.Button.DMGBonus", "掷一次" },
                { "Card.Label.DamageBonus", "伤害奖励" },
                { "Message.RollADMGBonus", "根据公式 {0}, 本轮掷出了 {1}" },
                { "BatchGenerationWindow.Title", "批量生成属性" },
                { "Dialog.Import.Avatar.Title", "选择调查员的照片 (*.png)" },
                { "Dialog.Import.Avatar.Confirmation", "是否确认用\n{0}\n覆盖现有的文件?" },
                { "Message.Regenerate", $"请选择一组数据作为角色的基础属性\n* 当前角色的成长值将被完全清空\n* {{{Config.KEY_ASSET}}} 不计入属性总计" },
                { "Message.Character.Saved", "调查员的文档已保存至: {0}" },
                { "Message.Character.SavedPic", "调查员的图像档案已保存至: {0}" },
                { "Message.Character.Switched", "已切换至调查员: {0}" },
            };

        [YamlIgnore]
        internal Dictionary<string, string> dictionaryIgnoreCase = new Dictionary<string, string>();


        /// <summary>
        /// 查询翻译文字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:指定 CultureInfo", Justification = "<挂起>")]
        public bool TryTranslate(string path, out string text, bool ignoreCase = false)
        {
            path = path?.Replace('_', '.') ?? string.Empty;
            text = null;
            if (ignoreCase)
            {
                return !string.IsNullOrWhiteSpace(path) && dictionaryIgnoreCase.TryGetValue(path.ToUpper(), out text);
            }
            return !string.IsNullOrWhiteSpace(path) && dictionary.TryGetValue(path, out text);
        }

        /// <summary>
        /// 用递归的方式, 对目标字典的键进行翻译
        /// </summary>
        /// <param name="target"></param>
        public void Translate(Dictionary<string, object> target)
        {
            if (target == null) return;
            var keys = target.Keys.ToArray();
            foreach (var key in keys)
            {
                var value = target[key];
                if (value is Dictionary<string, object> child)
                {
                    Translate(child);
                }
                target.Remove(key);
                var newKey = TryTranslate(key, out var msg) ? msg : key;
                target[newKey] = value;
            }
        }
    }
}
