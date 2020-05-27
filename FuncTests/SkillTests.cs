using Microsoft.VisualStudio.TestTools.UnitTesting;
using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Text;
using CardWizard.Tools;

namespace CallOfCthulhu.Tests
{
    /// <summary>
    /// 技能的测试
    /// </summary>
    [TestClass()]
    public class SkillTests
    {
        [TestMethod()]
        public void ResolveTest()
        {
            var text = "射击（步枪/霰弹枪）25% # { Spacial: 0, Interest: 0, Increased: true }";
            var skill = Skill.Parse(text, out var context, YamlKit.Parse<ContextDict>);
            Assert.AreEqual(skill.Name, "射击（步枪/霰弹枪）", "Name 处理失败");
            Assert.AreEqual(skill.BaseValue, 25, "BaseValue 处理失败");

            foreach (var item in new string[] { "Spacial", "Interest", "Increased" })
            {
                Assert.IsTrue(context.ContainsKey(item), $"找不到键: {item}");
            }
        }
    }
}