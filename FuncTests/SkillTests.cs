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
            var segments = text.Split('#', 2, StringSplitOptions.RemoveEmptyEntries);
            var skill = Skill.Parse(segments[0]);
            Assert.AreEqual(skill.Name, "射击（步枪/霰弹枪）", "名称处理失败");
            Assert.AreEqual(skill.BaseValue, 25, "基本值处理失败");

            var context = YamlKit.Parse<ContextData>(segments[1]);
            foreach (var item in new string[] { "Spacial", "Interest", "Increased" })
            {
                Assert.IsTrue(context.ContainsKey(item), $"找不到键: {item}");
            }
        }
    }
}