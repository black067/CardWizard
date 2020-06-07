using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CallOfCthulhu
{
    /// <summary>
    /// 武器的数据结构
    /// </summary>
    public class Weapon : INotifyPropertyChanged
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

        private int id;
        private string name;
        private WEAPONTYPE weaponType;
        private string description;
        private string hitrateNormal;
        private string damage;
        private string baseRange;
        private string attacksPerRound;
        private int bullets;
        private int resistance;

        /// <summary>
        /// 编号
        /// </summary>
        [Description("编号")]
        public int ID
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 武器名称
        /// </summary>
        [Description("武器名称")]
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
        /// 武器类型
        /// </summary>
        [Description("武器类型")]
        public WEAPONTYPE WeaponType
        {
            get => weaponType;
            set
            {
                weaponType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 武器描述
        /// </summary>
        [Description("武器描述")]
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 常规命中率公式
        /// </summary>
        [Description("常规命中率公式")]
        public string HitrateNormal
        {
            get => hitrateNormal;
            set
            {
                hitrateNormal = value;
                OnPropertyChanged();
            }
        }

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
        public string Damage
        {
            get => damage;
            set
            {
                damage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 基础射程
        /// </summary>
        [Description("基础射程")]
        public string BaseRange
        {
            get => baseRange;
            set
            {
                baseRange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 每轮攻击次数
        /// </summary>
        [Description("每轮攻击次数")]
        public string AttacksPerRound
        {
            get => attacksPerRound;
            set
            {
                attacksPerRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 装弹数
        /// </summary>
        [Description("装弹数")]
        public int Bullets
        {
            get => bullets;
            set
            {
                bullets = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 耐久度
        /// </summary>
        [Description("耐久度")]
        public int Resistance
        {
            get => resistance;
            set
            {
                resistance = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 在各个年代的价格
        /// </summary>
        [Description("在各个年代的价格")]
        public float[] Prices { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
