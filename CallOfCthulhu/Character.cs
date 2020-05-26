using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CallOfCthulhu
{
    /// <summary>
    /// 角色
    /// </summary>
    public partial class Character : INotifyPropertyChanged
    {
        /// <summary>
        /// 默认的角色年龄
        /// </summary>
        public const int DEFAULT_AGE = 24;

        private string name;

        private string era;

        private string gender;

        private int age;

        private string education;

        private string occupation;

        private string address;

        private string homeland;

        private string[] skills;

        #region 角色属性值与其成长值的获取与赋值

        /// <summary>
        /// 取得角色特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTraitInitial(string key) => Initials.TryGetValue(key, out int value) ? value : 0;

        /// <summary>
        /// 设置特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetTraitInitial(string key, int value)
        {
            var original = Initials.ContainsKey(key) ? Initials[key] : 0;
            Initials[key] = value;
            UpdateTrait(new TraitChangedEventArgs(key)
            {
                Segment = Trait.Segment.INITIAL,
                NewValue = value,
                OriginValue = original,
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
                Segment = Trait.Segment.GROWTH,
                NewValue = value,
                OriginValue = original,
            });
        }

        /// <summary>
        /// 取得特点的衰老惩罚值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTraitAdjustment(string key)
        {
            if (Adjustments == null) return 0;
            return Adjustments.TryGetValue(key, out int v) ? v : 0;
        }

        /// <summary>
        /// 设置特点的衰老惩罚值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetTraitAdjustment(string key, int value)
        {
            if (Adjustments == null) Adjustments = new Dictionary<string, int>();
            int original = Adjustments.ContainsKey(key) ? Adjustments[key] : 0;
            Adjustments[key] = value;
            UpdateTrait(new TraitChangedEventArgs(key)
            {
                Segment = Trait.Segment.ADJUSTMENT,
                NewValue = value,
                OriginValue = original,
            });
        }

        /// <summary>
        /// 取得特点的最终值, 包括基础与成长
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTrait(string key) => GetTraitInitial(key) + GetTraitGrowth(key) + GetTraitAdjustment(key);

        /// <summary>
        /// 设置特点的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="base"></param>
        /// <param name="growth"></param>
        /// <param name="adjustment"></param>
        public void SetTrait(string key, int initial, int growth, int adjustment)
        {
            SetTraitInitial(key, initial);
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
                UpdateData();
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
                UpdateData();
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
                UpdateData();
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
                UpdateData();
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
                UpdateData();
            }
        }

        /// <summary>
        /// 职业
        /// </summary>
        public string Occupation
        {
            get => occupation;
            set
            {
                occupation = value;
                UpdateData();
            }
        }

        /// <summary>
        /// 角色的现居地
        /// </summary>
        public string Address
        {
            get => address;
            set
            {
                address = value;
                UpdateData();
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
                UpdateData();
            }
        }

        /// <summary>
        /// 技能列表
        /// </summary>
        public string[] Skills
        {
            get => skills;
            set
            {
                skills = value;
                UpdateData();
            }
        }

        /// <summary>
        /// 角色属性初始值
        /// </summary>
        public Dictionary<string, int> Initials { get; set; }

        /// <summary>
        /// 角色属性成长值
        /// </summary>
        public Dictionary<string, int> Growths { get; set; }

        /// <summary>
        /// 角色属性调整值
        /// </summary>
        public Dictionary<string, int> Adjustments { get; set; }

        /// <summary>
        /// 角色名称等信息发生改变时, 触发的事件
        /// <para>注意, 角色的属性 (Characteristic) 数值变化需要监听事件 <see cref="TraitChanged"/> </para>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 角色的特点数值发生改变时, 触发的事件
        /// </summary>
        public event TraitChangedEventHandler TraitChanged;

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="name"></param>
        protected void UpdateData([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
        public static Character Create(Dictionary<string, Trait> baseModelDict, Func<Dictionary<string, int>, string, int> calculator = null)
        {
            var character = new Character()
            {
                Initials = new Dictionary<string, int>(),
                Age = DEFAULT_AGE,
                Skills = new string[0],
            };
            if (baseModelDict != null)
            {
                bool hasCalculator = calculator != null;
                foreach (var prop in baseModelDict.Values)
                {
                    character.SetTraitInitial(prop.Name, hasCalculator ? calculator(character.Initials, prop.Formula) : 0);
                    character.SetTraitGrowth(prop.Name, 0);
                    character.SetTraitAdjustment(prop.Name, 0);
                }
            }
            return character;
        }
    }
}
