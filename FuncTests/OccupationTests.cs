using Microsoft.VisualStudio.TestTools.UnitTesting;
using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Text;

namespace CallOfCthulhu.Tests
{
    [TestClass()]
    public class OccupationTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            var occu = new Occupation()
            {
                Name = "警探",
                Description = "执行侦探破案工作的警察。",
                Skills = new string[]
                {
                    "艺术与手艺(表演)", "乔装", "射击", "法律", "聆听", "心理学", "侦察", Skill.CUSTOM_SOCIAL, Skill.CUSTOM
                },
                CreditRatingRange = "20 ~ 50",
                PointFormula = "EDU * 2 + math.max(DEX, STR) * 2",
            };
            var text = occu.ToString();
            
        }
    }
}