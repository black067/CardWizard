using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using YamlDotNet.Serialization;
using System.Windows.Data;

namespace CardWizard.Data
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Character : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性值发生变化时的事件消息
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:嵌套类型应不可见", Justification = "<挂起>")]
        public class TraitChangedEventArgs : EventArgs
        {
            /// <summary>
            /// 原来的特点基础值
            /// </summary>
            public int OriginalBase { get; set; }
            /// <summary>
            /// 原来的特点成长值
            /// </summary>
            public int OriginalGrowth { get; set; }
            /// <summary>
            /// 新的特点基础值
            /// </summary>
            public int NewBase { get; set; }
            /// <summary>
            /// 新的特点成长值
            /// </summary>
            public int NewGrowth { get; set; }
            /// <summary>
            /// 原有的衰老惩罚值
            /// </summary>
            public int OriginalAdjustment { get; set; }
            /// <summary>
            /// 新的衰老惩罚值
            /// </summary>
            public int NewAdjustment { get; set; }
            /// <summary>
            /// 发生变化的特点名称
            /// </summary>
            public string Key { get; set; }
            /// <summary>
            /// 角色属性变动事件的参数
            /// </summary>
            /// <param name="key"></param>
            public TraitChangedEventArgs(string key)
            {
                Key = key;
            }
        }

        private string name;

        private string era;

        private string gender;

        private int age;

        private string education;

        private Occupation occupation;

        private string homeland;

        #region 角色属性值与其成长值的获取与赋值

        /// <summary>
        /// 取得角色特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTraitBase(string key) => Traits.TryGetValue(key, out int value) ? value : 0;

        /// <summary>
        /// 设置特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetTraitBase(string key, int value)
        {
            var original = Traits.ContainsKey(key) ? Traits[key] : 0;
            Traits[key] = value;
            UpdateTrait(new TraitChangedEventArgs(key)
            {
                OriginalBase = original,
                NewBase = value,
            });
        }

        /// <summary>
        /// 取得特点的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTraitGrowth(string key)
        {
            if (Growths == null) return 0;
            return Growths.TryGetValue(key, out int value) ? value : 0;
        }

        /// <summary>
        /// 设置特点的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetTraitGrowth(string key, int value)
        {
            if (Growths == null) Growths = new Dictionary<string, int>();
            int original = Growths.ContainsKey(key) ? Growths[key] : 0;
            Growths[key] = value;
            UpdateTrait(new TraitChangedEventArgs(key)
            {
                OriginalGrowth = original,
                NewGrowth = value,
            });
        }

        /// <summary>
        /// 取得特点的衰老惩罚值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTraitAdjustment(string key)
        {
            if (Adjustment == null) return 0;
            return Adjustment.TryGetValue(key, out int v) ? v : 0;
        }

        /// <summary>
        /// 设置特点的衰老惩罚值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetTraitAdjustment(string key, int value)
        {
            if (Adjustment == null) Adjustment = new Dictionary<string, int>();
            int original = Adjustment.ContainsKey(key) ? Adjustment[key] : 0;
            Adjustment[key] = value;
            UpdateTrait(new TraitChangedEventArgs(key)
            {
                OriginalAdjustment = original,
                NewAdjustment = value,
            });
        }

        /// <summary>
        /// 取得特点的最终值, 包括基础与成长
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTrait(string key) => GetTraitBase(key) + GetTraitGrowth(key) + GetTraitAdjustment(key);

        /// <summary>
        /// 取得属性值的字符串表达式
        /// <para>形如: {base}+{growth}</para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTraitText(string key)
        {
            int @base = GetTraitBase(key), growth = GetTraitGrowth(key), adjustment = GetTraitAdjustment(key);
            string growthText = growth > 0 ? $"+{growth}" : growth.ToString();
            string adjustmentText = adjustment > 0 ? $"+{adjustment}" : adjustment.ToString();
            if (@base == 0 && growth == 0 && adjustment == 0) return "0";
            if (@base == 0 && growth == 0 && adjustment != 0) return $"0+0{adjustmentText}";
            if (@base == 0 && growth != 0 && adjustment == 0) return $"0{growthText}";
            if (@base == 0 && growth != 0 && adjustment != 0) return $"0{growthText}{adjustmentText}";
            if (@base != 0 && growth == 0 && adjustment == 0) return $"{@base}";
            if (@base != 0 && growth == 0 && adjustment != 0) return $"{@base}+0{adjustmentText}";
            if (@base != 0 && growth != 0 && adjustment == 0) return $"{@base}{growthText}";
            return $"{@base}{growthText}{adjustmentText}";
        }

        /// <summary>
        /// 分割记录了角色特点的字符串
        /// </summary>
        /// <param name="valueText"></param>
        /// <returns></returns>
        public static (int @base, int growth, int adjustment) SplitTraitText(string valueText)
        {
            var result = (@base: 0, growth: 0, adjustment: 0);
            var matches = Regex.Matches(valueText, @"(\-|\+)?\d+(\.\d+)?");
            for (int i = 0, length = matches.Count; i < length; i++)
            {
                var m = matches[i];
                if (i == 0) result.@base = int.TryParse(m.Value, out int v) ? v : 0;
                else if (i == 1) result.growth = int.TryParse(m.Value, out int v) ? v : 0;
                else if (i == 2) result.adjustment = int.TryParse(m.Value, out int v) ? v : 0;
            }

            return result;
        }

        /// <summary>
        /// 解析字符串, 设置特点的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueText"></param>
        public void SetTrait(string key, string valueText)
        {
            var (@base, growth, adjustment) = SplitTraitText(valueText);
            SetTraitBase(key, @base);
            SetTraitGrowth(key, growth);
            SetTraitAdjustment(key, adjustment);
        }

        /// <summary>
        /// 设置特点的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="base"></param>
        /// <param name="growth"></param>
        /// <param name="adjustment"></param>
        public void SetTrait(string key, int @base, int growth, int adjustment)
        {
            SetTraitBase(key, @base);
            SetTraitGrowth(key, growth);
            SetTraitAdjustment(key, adjustment);
        }
        #endregion

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                UpdateData(nameof(Name));
            }
        }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age
        {
            get => age;
            set
            {
                age = value;
                UpdateData(nameof(Age));
            }
        }

        /// <summary>
        /// 时代
        /// </summary>
        public string Era
        {
            get => era;
            set
            {
                era = value;
                UpdateData(nameof(Era));
            }
        }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender
        {
            get => gender;
            set
            {
                gender = value;
                UpdateData(nameof(Gender));
            }
        }

        /// <summary>
        /// 学历
        /// </summary>
        public string Education
        {
            get => education;
            set
            {
                education = value;
                UpdateData(nameof(Education));
            }
        }

        /// <summary>
        /// 职业
        /// </summary>
        public Occupation Occupation
        {
            get => occupation;
            set
            {
                occupation = value;
                UpdateData(nameof(Occupation));
            }
        }

        /// <summary>
        /// 故乡
        /// </summary>
        public string Homeland
        {
            get => homeland;
            set
            {
                homeland = value;
                UpdateData(nameof(Homeland));
            }
        }

        /// <summary>
        /// 角色的属性值
        /// </summary>
        public Dictionary<string, int> Traits { get; set; }

        /// <summary>
        /// 特点成长值
        /// </summary>
        public Dictionary<string, int> Growths { get; set; }

        /// <summary>
        /// 衰老惩罚
        /// </summary>
        public Dictionary<string, int> Adjustment { get; set; }

        /// <summary>
        /// 职业点数
        /// </summary>
        public Dictionary<string, int> OccupationPoints { get; set; }

        /// <summary>
        /// 兴趣点数
        /// </summary>
        public Dictionary<string, int> InterestPoints { get; set; }

        /// <summary>
        /// 个人技能
        /// </summary>
        [YamlIgnore]
        public List<Skill> Skills { get; set; }

        /// <summary>
        /// 角色的所有个人技能
        /// </summary>
        public List<string> skills = new List<string>();

        /// <summary>
        /// 初始化角色的技能数据
        /// </summary>
        /// <param name="skillDict"></param>
        public void InitializeSkills(Dictionary<string, Skill> skillDict)
        {
            Skills = new List<Skill>();
            foreach (var item in skills)
            {
                if (skillDict != null && skillDict.ContainsKey(item))
                {
                    Skills.Add(skillDict[item]);
                }
                else
                {
                    Skills.Add(new Skill()
                    {
                        Name = item,
                        BaseValue = 5,
                    });
                }
            }
        }

        /// <summary>
        /// 角色名称等信息发生改变时, 触发的事件
        /// <para>注意, 角色的特点数值变化需要监听事件 <see cref="TraitChanged"/> </para>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 角色的特点数值发生改变时, 触发的事件
        /// </summary>
        public event Action<Character, TraitChangedEventArgs> TraitChanged;

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="name"></param>
        protected void UpdateData(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 更新特点数值
        /// </summary>
        /// <param name="e"></param>
        private void UpdateTrait(TraitChangedEventArgs e) => TraitChanged?.Invoke(this, e);

        /// <summary>
        /// 清空所有对角色数值/属性改变的监听
        /// <para>清空 <see cref="TraitChanged"/> 与 <see cref="PropertyChanged"/></para>
        /// </summary>
        public void ClearObservers()
        {
            TraitChanged = null;
            PropertyChanged = null;
        }

        /// <summary>
        /// 创建一个新角色
        /// </summary>
        /// <param name="baseModelDict"></param>
        /// <param name="calculator"></param>
        /// <returns></returns>
        public static Character Create(Dictionary<string, DataModel> baseModelDict, Func<Dictionary<string, int>, string, int> calculator)
        {
            var character = new Character()
            {
                Traits = new Dictionary<string, int>(),
            };
            if (calculator != null && baseModelDict != null)
            {
                foreach (var prop in baseModelDict.Values)
                {
                    character.SetTraitBase(prop.Name, calculator(character.Traits, prop.Formula));
                    character.SetTraitGrowth(prop.Name, 0);
                    character.SetTraitAdjustment(prop.Name, 0);
                }
            }
            return character;
        }
    }
}
