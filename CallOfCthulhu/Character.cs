﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string[] skills;
        private ContextDict backstory = new ContextDict(10);

        #region 角色属性值与其成长值的获取与赋值

        /// <summary>
        /// 取得角色特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInitial(string key) => Initials.TryGetValue(key, out int value) ? value : 0;

        /// <summary>
        /// 设置特点的基础值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetInitial(string key, int value)
        {
            var original = Initials.ContainsKey(key) ? Initials[key] : 0;
            Initials[key] = value;
            UpdateCharacteristic(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.INITIAL,
                NewValue = value,
                OriginValue = original,
            });
        }

        /// <summary>
        /// 取得特点的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetGrowth(string key)
        {
            if (Growths == null) return 0;
            return Growths.TryGetValue(key, out int value) ? value : 0;
        }

        /// <summary>
        /// 设置特点的成长值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetGrowth(string key, int value)
        {
            if (Growths == null) Growths = new Dictionary<string, int>();
            int original = Growths.ContainsKey(key) ? Growths[key] : 0;
            Growths[key] = value;
            UpdateCharacteristic(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.GROWTH,
                NewValue = value,
                OriginValue = original,
            });
        }

        /// <summary>
        /// 取得特点的衰老惩罚值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetAdjustment(string key)
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
        public void SetAdjustment(string key, int value)
        {
            if (Adjustments == null) Adjustments = new Dictionary<string, int>();
            int original = Adjustments.ContainsKey(key) ? Adjustments[key] : 0;
            Adjustments[key] = value;
            UpdateCharacteristic(new CharacteristicChangedEventArgs(key)
            {
                Segment = Characteristic.Segment.ADJUSTMENT,
                NewValue = value,
                OriginValue = original,
            });
        }

        /// <summary>
        /// 取得特点的最终值, 包括基础与成长
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTotal(string key) => GetInitial(key) + GetGrowth(key) + GetAdjustment(key);

        /// <summary>
        /// 设置特点的值
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
        /// 名称
        /// </summary>
        [Description("名称")]
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
        [Description("年龄")]
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
        [Description("时代")]
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
        [Description("性别")]
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
        [Description("学历")]
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
        [Description("职业")]
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
        [Description("角色的现居地")]
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
        [Description("故乡")]
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
        /// 背景故事
        /// </summary>
        [Description("背景故事")]
        public ContextDict Backstory
        {
            get
            {
                if (backstory == null) backstory = new ContextDict(10);
                return backstory;
            }

            set
            {
                backstory = value;
                UpdateData();
            }
        }
        /// <summary>
        /// 技能列表
        /// </summary>
        [Description("技能列表")]
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
        /// 角色名称等信息发生改变时, 触发的事件
        /// <para>注意, 角色的属性 (Characteristic) 数值变化需要监听事件 <see cref="CharacteristicChanged"/> </para>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 角色的特点数值发生改变时, 触发的事件
        /// </summary>
        public event CharacteristicChangedEventHandler CharacteristicChanged;

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="name"></param>
        protected void UpdateData([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 更新特点数值
        /// </summary>
        /// <param name="e"></param>
        protected void UpdateCharacteristic(CharacteristicChangedEventArgs e) => CharacteristicChanged?.Invoke(this, e);

        /// <summary>
        /// 清空所有对角色数值/属性改变的监听
        /// <para>清空 <see cref="CharacteristicChanged"/> 与 <see cref="PropertyChanged"/></para>
        /// </summary>
        public void ClearObservers()
        {
            CharacteristicChanged = null;
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
                Skills = new string[0],
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
