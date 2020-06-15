using CallOfCthulhu;
using CardWizard.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CardWizard.Data
{
    /// <summary>
    /// 数据总线, 用于存储武器/技能/职业等数据
    /// </summary>
    public class DataBus
    {
        private Dictionary<int, Skill> skills;
        private Dictionary<int, Occupation> occupations;
        private Dictionary<int, Weapon> weapons;

        /// <summary>
        /// 技能数据
        /// </summary>
        public Dictionary<int, Skill> Skills { get => skills; private set => skills = value; }

        /// <summary>
        /// 职业数据
        /// </summary>
        public Dictionary<int, Occupation> Occupations { get => occupations; private set => occupations = value; }

        /// <summary>
        /// 武器数据
        /// </summary>
        public Dictionary<int, Weapon> Weapons { get => weapons; private set => weapons = value; }

        /// <summary>
        /// 根据技能名称查询技能
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool TryGetSkill(int id, out Skill skill) => Skills.TryGetValue(id, out skill);

        /// <summary>
        /// 根据职业 ID 查询职业
        /// </summary>
        /// <param name="id"></param>
        /// <param name="occupation"></param>
        /// <returns></returns>
        public bool TryGetOccupation(int id, out Occupation occupation) => Occupations.TryGetValue(id, out occupation);

        /// <summary>
        /// 根据职业名称查询职业数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="occupation"></param>
        /// <returns></returns>
        public bool TryGetOccupation(string name, out Occupation occupation)
        {
            var sql = (from val in Occupations.Values where val.Name.EqualsIgnoreCase(name) select val);
            if (sql.Any())
            {
                occupation = sql.FirstOrDefault();
                return true;
            }
            occupation = default;
            return false;
        }

        /// <summary>
        /// 根据武器名称查询武器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool TryGetWeapon(int key, out Weapon weapon) => Weapons.TryGetValue(key, out weapon);

        /// <summary>
        /// 初始化数据总线
        /// </summary>
        public DataBus()
        {
            Skills = new Dictionary<int, Skill>();
            Occupations = new Dictionary<int, Occupation>();
            Weapons = new Dictionary<int, Weapon>();
        }

        /// <summary>
        /// 从文件/文件夹中读取数据
        /// <para>如果是文件夹, 会读取文件夹内的所有子目录与子文件</para>
        /// </summary>
        /// <param name="path"></param>
        public void LoadFrom(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            // 如果是目录
            if (Directory.Exists(path))
            {
                var paths = Directory.GetFiles(path).ToList();
                paths.AddRange(Directory.GetDirectories(path));
                foreach (var p in paths)
                {
                    LoadFrom(p);
                }
                return;
            }
            // 如果是文件
            else if (File.Exists(path))
            {
                var name = Path.GetFileName(path);
                if (name.StartsWith(nameof(Weapon)))
                {
                    SolveRaw<Weapon>(path);
                }
                else if (name.StartsWith(nameof(Occupation)))
                {
                    SolveRaw<Occupation>(path);
                }
                else if (name.StartsWith(nameof(Skill)))
                {
                    SolveRaw<Skill>(path);
                }
            }
        }

        private void SolveRaw<T>(string path)
        {
            var datas = YamlKit.LoadFile<IEnumerable<T>>(path);
            if (datas == null || !datas.Any()) return;
            foreach (var item in datas)
            {
                CacheData(item);
            }
        }

        /// <summary>
        /// 将数据储存到库中
        /// </summary>
        /// <param name="item"></param>
        public void CacheData(object item)
        {
            switch (item)
            {
                case Weapon w:
                    w.ID = w.ID > 0 ? w.ID : (Weapons.Count + 1);
                    Weapons[w.ID] = w;
                    break;
                case Skill s:
                    s.ID = s.ID > 0 ? s.ID : (Skills.Count + 1);
                    Skills[s.ID] = s;
                    break;
                case Occupation o:
                    o.ID = o.ID > 0 ? o.ID : (Occupations.Count + 1);
                    Occupations[o.ID] = o;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 生成默认数据
        /// </summary>
        public void GenerateDefaultOccupations()
        {
            var defaults = new Occupation[]
            {
                new Occupation()
                {
                    Name = "医生",
                    Description = "钻研学习医学科学技术，挽救生命，以治病为业。",
                    Skills = new string[]
                    {
                        "急救",
                        "其它语言(拉丁文)",
                        "医学",
                        "心理学",
                        "科学(生物学)",
                        "科学(药学)",
                        "自选专业技能",
                        "自选专业技能",
                    },
                    CreditRatingRange = "30 ~ 80",
                    PointFormula = "EDU * 4",
                },
                new Occupation()
                {
                    Name = "警探",
                    Description = "执行侦探破案工作的警察。",
                    Skills = new string[]
                    {
                        "艺术与手艺(表演)",
                        "乔装",
                        "射击",
                        "法律",
                        "聆听",
                        "心理学",
                        "侦察",
                        "自选社交技能",
                        "自选任意技能",
                    },
                    CreditRatingRange = "20 ~ 50",
                    PointFormula = "EDU * 2 + math.max(DEX, STR) * 2",
                },
            };

            foreach (var item in defaults)
            {
                item.ID = Occupations.Count + 1;
                Occupations.Add(item.ID, item);
            }
        }

        /// <summary>
        /// 生成默认数据
        /// </summary>
        public void GenerateDefaultWeapons()
        {
            var defaults = new Weapon[]
            {
                new Weapon()
                {
                    Name = "徒手格斗",
                    HitrateNormal = "25",
                    Damage = "1D3 + DB",
                    BaseRange = "",
                    AttacksPerRound = "1"
                },
            };

            foreach (var item in defaults)
            {
                item.ID = Weapons.Count + 1;
                Weapons.Add(item.ID, item);
            }
        }
    }
}
