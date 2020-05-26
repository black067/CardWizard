using Microsoft.VisualStudio.TestTools.UnitTesting;
using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Text;

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
            var context = new ContextData();
            var text = "射击（步枪/霰弹枪）25% # { Spacial：0, Interest: 0, Increased: true }";
            var skill = Skill.Resolve(text);
        }
    }
}