﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using CardWizard.Tools;
using CallOfCthulhu;

namespace CardWizard.Data
{
    /// <summary>
    /// 数据总线, 用于存储武器/技能/职业等数据
    /// </summary>
    public class DataBus
    {
        private Dictionary<string, Skill> skills;
        private Dictionary<string, Occupation> occupations;
        private Dictionary<string, Weapon> weapons;

        /// <summary>
        /// 技能数据
        /// </summary>
        public Dictionary<string, Skill> Skills { get => skills; private set => skills = value; }

        /// <summary>
        /// 职业数据
        /// </summary>
        public Dictionary<string, Occupation> Occupations { get => occupations; private set => occupations = value; }

        /// <summary>
        /// 武器数据
        /// </summary>
        public Dictionary<string, Weapon> Weapons { get => weapons; private set => weapons = value; }

        /// <summary>
        /// 根据技能名称查询技能
        /// </summary>
        /// <param name="key"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool TryGetSkill(string key, out Skill skill) => Skills.TryGetValue(key, out skill);

        /// <summary>
        /// 根据职业名称查询职业
        /// </summary>
        /// <param name="key"></param>
        /// <param name="occupation"></param>
        /// <returns></returns>
        public bool TryGetOccupation(string key, out Occupation occupation) => Occupations.TryGetValue(key, out occupation);

        /// <summary>
        /// 根据武器名称查询武器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool TryGetWeapon(string key, out Weapon weapon) => Weapons.TryGetValue(key, out weapon);

        /// <summary>
        /// 初始化数据总线
        /// </summary>
        public DataBus()
        {
            Skills = new Dictionary<string, Skill>();
            Occupations = new Dictionary<string, Occupation>();
            Weapons = new Dictionary<string, Weapon>();
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
                if (path.StartsWith(nameof(Weapon)))
                {
                    SolveRaw<Weapon>(path);
                }
                else if (path.StartsWith(nameof(Occupation)))
                {
                    SolveRaw<Occupation>(path);
                }
                else if (path.StartsWith(nameof(Skill)))
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
                    Weapons[w.Name] = w;
                    break;
                case Skill s:
                    s.ID = s.ID > 0 ? s.ID : (Skills.Count + 1);
                    Skills[s.Name] = s;
                    break;
                case Occupation o:
                    o.ID = o.ID > 0 ? o.ID : (Occupations.Count + 1);
                    Occupations[o.Name] = o; 
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 生成默认数据
        /// </summary>
        public void GenerateDefaultData()
        {
            var models = new object[]
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
                new Weapon()
                {
                    Name = "徒手格斗",
                    HitrateNormal = "25",
                    Damage = "1D3 + DB",
                    BaseRange = "",
                    AttacksPerRound = "1"
                },
                new Skill()
                {
                    Name = "Accounting",
                    BaseValue = 25,
                },
                new Skill()
                {
                    Name = "Anthropology",
                    BaseValue = 1,
                },
                new Skill()
                {
                    Name = "Appraise",
                    BaseValue = 5,
                },
                new Skill()
                {
                    Name = "Archaeology",
                    BaseValue = 1,
                },
                new Skill()
                {
                    Name = "Credit Rating",
                    BaseValue = 0,
                    Growable = false,
                },
                new Skill()
                {
                    Name = "Cthulhu Mythos",
                    BaseValue = 0,
                    Growable = false,
                },
            };

            foreach (var item in models)
            {
                CacheData(item);
            }
        }
    }
}
