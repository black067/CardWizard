using System;
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
        public Dictionary<string, Skill> Skills { get => skills; set => skills = value; }

        /// <summary>
        /// 职业数据
        /// </summary>
        public Dictionary<string, Occupation> Occupations { get => occupations; set => occupations = value; }

        /// <summary>
        /// 武器数据
        /// </summary>
        public Dictionary<string, Weapon> Weapons { get => weapons; set => weapons = value; }

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
        public void LoadAll(string path)
        {
            if (Directory.Exists(path))
            {
                var pathos = Directory.GetFiles(path);
                pathos.ToList().AddRange(Directory.GetDirectories(path));
                foreach (var p in pathos)
                {
                    LoadAll(p);
                }
                return;
            }
            if (path == null) { return; }
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
        
        private void SolveRaw<T>(string path)
        {
            var datas = YamlKit.LoadFile<IEnumerable<T>>(path);
            if (datas == null || datas.Any()) return;
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
            if (item is Occupation occupation)
            {
                Occupations[occupation.Name] = occupation;
            }
            else if (item is Skill skill)
            {
                Skills[skill.Name] = skill;
            }
            else if (item is Weapon weapon)
            {
                Weapons[weapon.Name] = weapon;
            }
        }
    }
}
