using System;
using System.Collections.Generic;

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
        public string Name { get; set; }

        /// <summary>
        /// 武器类型
        /// </summary>
        public WEAPONTYPE WeaponType { get; set; }

        /// <summary>
        /// 武器描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 基本命中率公式
        /// </summary>
        public string Hitrate { get; set; }

        /// <summary>
        /// 伤害公式
        /// </summary>
        public string Damage { get; set; }

        /// <summary>
        /// 基础射程
        /// </summary>
        public string BaseRange { get; set; }

        /// <summary>
        /// 每轮攻击次数
        /// </summary>
        public string AttacksPerRound { get; set; }

        /// <summary>
        /// 装弹数
        /// </summary>
        public int Bullets { get; set; }

        /// <summary>
        /// 耐久度
        /// </summary>
        public int Resistance { get; set; }

        /// <summary>
        /// 在各个年代的价格
        /// </summary>
        public float[] Prices { get; set; }

        /// <summary>
        /// 客制化数据
        /// </summary>
        public Dictionary<string, object> ContextData { get; set; }

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
