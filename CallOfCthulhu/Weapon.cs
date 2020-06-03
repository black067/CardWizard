using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CallOfCthulhu
{
    /// <summary>
    /// 武器的数据结构
    /// </summary>
    public class Weapon
    {
        /// <summary>
        /// 枚举: 武器类型
        /// </summary>
        public enum WEAPONTYPE
        {
            /// <summary>
            /// 近战武器
            /// </summary>
            Melee,
            /// <summary>
            /// 手枪
            /// </summary>
            Pistol,
            /// <summary>
            /// 霰弹枪
            /// </summary>
            Shotgun,
            /// <summary>
            /// 步枪
            /// </summary>
            Rifle,
            /// <summary>
            /// 冲锋枪
            /// </summary>
            SubmachineGun,
            /// <summary>
            /// 机枪
            /// </summary>
            MachineGun,
            /// <summary>
            /// 重武器
            /// </summary>
            HeavyWeapon,
            /// <summary>
            /// 爆炸物
            /// </summary>
            Explosive,
            /// <summary>
            /// 其它
            /// </summary>
            Misc,
        }

        /// <summary>
        /// 武器名称
        /// </summary>
        [Description("武器名称")]
        public string Name { get; set; }

        /// <summary>
        /// 武器类型
        /// </summary>
        [Description("武器类型")]
        public WEAPONTYPE WeaponType { get; set; }

        /// <summary>
        /// 武器描述
        /// </summary>
        [Description("武器描述")]
        public string Description { get; set; }

        /// <summary>
        /// 常规命中率公式
        /// </summary>
        [Description("常规命中率公式")]
        public string HitrateNormal { get; set; }

        /// <summary>
        /// 困难命中率公式
        /// </summary>
        [Description("困难命中率公式")]
        public string HitrateHard { get => $"({ HitrateNormal ?? "0"}) / 2"; }

        /// <summary>
        /// 极难命中率公式
        /// </summary>
        [Description("极难命中率公式")]
        public string HitrateExtreme { get => $"({ HitrateNormal ?? "0"}) / 5"; }

        /// <summary>
        /// 伤害公式
        /// </summary>
        [Description("伤害公式")]
        public string Damage { get; set; }

        /// <summary>
        /// 基础射程
        /// </summary>
        [Description("基础射程")]
        public string BaseRange { get; set; }

        /// <summary>
        /// 每轮攻击次数
        /// </summary>
        [Description("每轮攻击次数")]
        public string AttacksPerRound { get; set; }

        /// <summary>
        /// 装弹数
        /// </summary>
        [Description("装弹数")]
        public int Bullets { get; set; }

        /// <summary>
        /// 耐久度
        /// </summary>
        [Description("耐久度")]
        public int Resistance { get; set; }

        /// <summary>
        /// 在各个年代的价格
        /// </summary>
        [Description("在各个年代的价格")]
        public float[] Prices { get; set; }

        /// <summary>
        /// 用字符串设置武器类型 <see cref="WeaponType"/> 的值
        /// </summary>
        /// <param name="value"></param>
        public void SetType(string value)
        {
            WeaponType = Enum.TryParse<WEAPONTYPE>(value, out var t) ? t : WEAPONTYPE.Misc;
        }

        /// <summary>
        /// 用 <see cref="int"/> 设置武器类型 <see cref="WeaponType"/> 的值
        /// </summary>
        /// <param name="value"></param>
        public void SetType(int value)
        {
            if (value >= 0 && value < Enum.GetValues(typeof(WEAPONTYPE)).Length)
                WeaponType = (WEAPONTYPE)value;
        }
    }
}
