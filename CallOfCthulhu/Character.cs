using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private string damageBonus;
        private List<string> gearAndPossessions;
        private ObservableCollection<Skill> skills;
        private ObservableCollection<Weapon> weapons;
        private ContextDict backstory;

        /// <summary>
        /// 名称
        /// </summary>
        [Description("名称")]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 年龄
        /// </summary>
        [Description("年龄")]
        public int Age
        {
            get => age;
            set
            {
                age = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 时代
        /// </summary>
        [Description("时代")]
        public string Era
        {
            get => era;
            set
            {
                era = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 性别
        /// </summary>
        [Description("性别")]
        public string Gender
        {
            get => gender;
            set
            {
                gender = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 学历
        /// </summary>
        [Description("学历")]
        public string Education
        {
            get => education;
            set
            {
                education = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 职业
        /// </summary>
        [Description("职业")]
        public string Occupation
        {
            get => occupation;
            set
            {
                occupation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 角色的现居地
        /// </summary>
        [Description("角色的现居地")]
        public string Address
        {
            get => address;
            set
            {
                address = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 故乡
        /// </summary>
        [Description("故乡")]
        public string Homeland
        {
            get => homeland;
            set
            {
                homeland = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 背景故事
        /// </summary>
        [Description("背景故事")]
        public ContextDict Backstory
        {
            get
            {
                if (backstory == null) backstory = new ContextDict(15);
                return backstory;
            }

            set
            {
                backstory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 装备和物品
        /// </summary>
        [Description("装备和物品")]
        public List<string> GearAndPossessions
        {
            get
            {
                if (gearAndPossessions == null) gearAndPossessions = new List<string>();
                return gearAndPossessions;
            }

            set
            {
                gearAndPossessions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 技能列表
        /// </summary>
        [Description("技能列表")]
        public ObservableCollection<Skill> Skills
        {
            get
            {
                if (skills == null) skills = new ObservableCollection<Skill>();
                return skills;
            }

            set
            {
                skills = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 根据ID查找技能
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGetSkill(int id, out Skill skill)
        {
            skill = Skills.FirstOrDefault(s => s.ID == id);
            return skill != default;
        }

        public void SetSkill(Skill template, Skill.Segment segment, int value)
        {
            SkillChangedEventArgs args;
            if (!TryGetSkill(template.ID, out Skill skill))
            {
                skill = template.Clone() as Skill;
                Skills.Add(skill);
            }
            args = new SkillChangedEventArgs(template.ID)
            {
                Segment = segment,
                OldValue = skill.GetPoints(segment),
                NewValue = value,
            };
            switch (segment)
            {
                case Skill.Segment.OCCUPATION:
                    skill.OccupationPoints = value;
                    break;
                case Skill.Segment.PERSONAL:
                    skill.PersonalPoints = value;
                    break;
                case Skill.Segment.GROWTH:
                    skill.GrowthPoints = value;
                    break;
            }
            OnSkillChanged(args);
        }

        /// <summary>
        /// 武器列表
        /// </summary>
        [Description("武器列表")]
        public ObservableCollection<Weapon> Weapons
        {
            get
            {
                if (weapons == null) weapons = new ObservableCollection<Weapon>();
                return weapons;
            }

            set
            {
                weapons = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 角色名称等信息发生改变时, 触发的事件
        /// <para>注意, 角色的属性 (Characteristic) 数值变化需要监听事件 <see cref="CharacteristicChanged"/> </para>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #region 角色属性值与其成长值的获取与赋值

        /// <summary>
        /// 取得角色属性的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInitial(string key) => Initials.TryGetValue(key, out int value) ? value : 0;

        /// <summary>
        /// 设置属性的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetInitial(string key, int value)
        {
            var original = Initials.ContainsKey(key) ? Initials[key] : 0;
            Initials[key] = value;
            OnCharacteristicChanged(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.INITIAL,
                NewValue = value,
                OldValue = original,
            });
        }

        /// <summary>
        /// 取得属性的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetGrowth(string key)
        {
            if (Growths == null) return 0;
            return Growths.TryGetValue(key, out int value) ? value : 0;
        }

        /// <summary>
        /// 设置属性的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetGrowth(string key, int value)
        {
            if (Growths == null) Growths = new Dictionary<string, int>();
            int original = Growths.ContainsKey(key) ? Growths[key] : 0;
            Growths[key] = value;
            OnCharacteristicChanged(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.GROWTH,
                NewValue = value,
                OldValue = original,
            });
        }

        /// <summary>
        /// 取得角色属性的调整值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetAdjustment(string key)
        {
            if (Adjustments == null) return 0;
            return Adjustments.TryGetValue(key, out int v) ? v : 0;
        }

        /// <summary>
        /// 设置角色属性的调整值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetAdjustment(string key, int value)
        {
            if (Adjustments == null) Adjustments = new Dictionary<string, int>();
            int original = Adjustments.ContainsKey(key) ? Adjustments[key] : 0;
            Adjustments[key] = value;
            OnCharacteristicChanged(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.ADJUSTMENT,
                NewValue = value,
                OldValue = original,
            });
        }

        /// <summary>
        /// 设置角色属性的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segment"></param>
        /// <param name="value"></param>
        public void SetCharacteristic(string key, Characteristic.Segment segment, int value)
        {
            switch (segment)
            {
                case Characteristic.Segment.INITIAL:
                    SetInitial(key, value);
                    break;
                case Characteristic.Segment.ADJUSTMENT:
                    SetAdjustment(key, value);
                    break;
                case Characteristic.Segment.GROWTH:
                    SetGrowth(key, value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 取得属性的最终值, 包括基础与成长
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTotal(string key) => GetInitial(key) + GetGrowth(key) + GetAdjustment(key);

        /// <summary>
        /// 设置属性的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="base"></param>
        /// <param name="growth"></param>
        /// <param name="adjustment"></param>
        public void SetTotal(string key, int initial, int growth, int adjustment)
        {
            SetInitial(key, initial);
            SetGrowth(key, growth);
            SetAdjustment(key, adjustment);
        }
        #endregion

        /// <summary>
        /// 角色属性初始值
        /// </summary>
        [Description("角色属性初始值")]
        public Dictionary<string, int> Initials { get; set; }

        /// <summary>
        /// 角色属性成长值
        /// </summary>
        [Description("角色属性成长值")]
        public Dictionary<string, int> Growths { get; set; }

        /// <summary>
        /// 角色属性调整值
        /// </summary>
        [Description("角色属性调整值")]
        public Dictionary<string, int> Adjustments { get; set; }

        /// <summary>
        /// 伤害加值公式
        /// </summary>
        [Description("伤害加值公式")]
        public string DamageBonus
        {
            get => damageBonus;
            set
            {
                damageBonus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取数据的总值
        /// </summary>
        public Dictionary<string, int> GetCharacteristicTotal(Func<string, bool> filterForKey = null)
        {
            return new Dictionary<string, int>(from k in Initials.Keys where filterForKey?.Invoke(k) ?? true select KeyValuePair.Create(k, GetTotal(k)));
        }

        /// <summary>
        /// 角色的特点数值发生改变时, 触发的事件
        /// </summary>
        public event CharacteristicChangedEventHandler CharacteristicChanged;

        /// <summary>
        /// 更新特点数值
        /// </summary>
        /// <param name="e"></param>
        protected void OnCharacteristicChanged(CharacteristicChangedEventArgs e) => CharacteristicChanged?.Invoke(this, e);

        /// <summary>
        /// 角色的技能数值发生改变时, 触发的事件
        /// </summary>
        public event SkillChangedEventHandler SkillChanged;

        /// <summary>
        /// 更新技能数值
        /// </summary>
        /// <param name="e"></param>
        protected void OnSkillChanged(SkillChangedEventArgs e) => SkillChanged?.Invoke(this, e);

        /// <summary>
        /// 清空所有对角色数值/属性改变的监听
        /// <para>清空 <see cref="CharacteristicChanged"/> 与 <see cref="PropertyChanged"/></para>
        /// </summary>
        public void ClearObservers()
        {
            CharacteristicChanged = null;
            SkillChanged = null;
            PropertyChanged = null;
            Backstory.ClearObservers();
        }

        /// <summary>
        /// 此构造器仅用于数据序列化, 请避免直接使用此方法构造角色
        /// <para>使用 <see cref="Create(Dictionary{string, Characteristic}, CalculateCharacteristic)"/></para>
        /// </summary>
        public Character() { }

        /// <summary>
        /// 创建一个新角色
        /// </summary>
        /// <param name="baseModelDict"></param>
        /// <param name="calculator">属性计算器</param>
        /// <returns></returns>
        public static Character Create(Dictionary<string, Characteristic> baseModelDict, CalculateCharacteristic calculator = null)
        {
            var character = new Character()
            {
                Initials = new Dictionary<string, int>(),
                Age = DEFAULT_AGE,
            };
            if (baseModelDict != null)
            {
                bool hasCalculator = calculator != null;
                foreach (var prop in baseModelDict.Values)
                {
                    character.SetInitial(prop.Name, hasCalculator ? calculator(prop.Formula, character.Initials) : 0);
                    character.SetGrowth(prop.Name, 0);
                    character.SetAdjustment(prop.Name, 0);
                }
            }
            return character;
        }
    }

    /// <summary>
    /// 委托: 属性计算器, 可以根据现有属性的值计算派生属性的值
    /// </summary>
    /// <param name="traits"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate int CalculateCharacteristic(string formula, Dictionary<string, int> traits);
}
