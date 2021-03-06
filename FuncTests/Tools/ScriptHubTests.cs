﻿using CallOfCthulhu;
using CardWizard.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using XLua;

namespace CardWizard.Tools.Tests
{
    /// <summary>
    /// 测试 <see cref="ScriptHub"/> 类
    /// </summary>
    [TestClass()]
    public class ScriptHubTests
    {
        /// <summary>
        /// 脚本执行器
        /// </summary>
        public ScriptHub Hub { get; set; }

        /// <summary>
        /// 骰点
        /// </summary>
        /// <param name="count"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        private int Roll(int count, int upper)
        {
            var die = new Random(Guid.NewGuid().GetHashCode() % 100);
            int sum = 0;
            for (int i = count; i > 0; i--)
            {
                sum += die.Next(1, upper + 1);
            }
            return sum;
        }

        public ScriptHubTests()
        {
            Hub = new XLuaHub();
            Hub.Set<Func<int, int, int>>(nameof(Roll), Roll);
        }

        [TestCleanup]
        public void CleanUp()
        {
            Hub.Dispose();
        }

        [TestMethod]
        public void FormulaTest()
        {
            var config = new Config();
            config.Process();
            var paths = config.Paths;
            Assert.IsTrue(File.Exists(paths.ScriptFormula), $"找不到公式文件 {paths.ScriptFormula}");
            Hub.DoFile(paths.ScriptFormula, "FORMULA", isGlobal: true);
            Assert.IsNotNull(Hub.Get<LuaTable>("DATA_AGEBONUS"), "DATA_AGEBONUS 为空");
            for (int i = 7; i < 100; i += 10)
            {
                using var subhub = Hub.CreateSubEnv(null);
                // 查询年龄奖励
                var text = subhub.DoString($"return AgeBonus({i})", isGlobal: true).FirstOrDefault();
                Assert.IsNotNull(text, $"在 i == {i} 时, 无返回值");
                Assert.IsTrue(text is string, $"在 i == {i} 时, 返回值不是 字符串, text is {text}");
                // 解析返回的字符串
                var context = YamlKit.Parse<ContextDict>((string)text);
                // 取出 Bonus 表
                context.TryGet<ICollection>("Bonus", out var bonus);
                Assert.IsNotNull(bonus, $"在 i == {i} 时, Bonus 解析失败");
                // 遍历 Bonus
                foreach (var item in bonus)
                {
                    Assert.IsTrue(item is IDictionary, $"i == {i}, Bonus 的元素解析失败, item is {item}");
                    var idict = item as IDictionary;
                    var key = idict["key"];
                    Assert.IsTrue(key is string, $"i == {i}, key 不是 字符串, key is {key}");
                    var formula = idict["formula"];
                    Assert.IsTrue(formula is string, $"i == {i}, formula 不是 字符串, formula is {formula}");
                }
            }
        }
    }
}