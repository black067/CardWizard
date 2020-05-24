using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardWizard.Tools.Tests
{
    [TestClass()]
    public class CodeBuilderTests
    {
        [TestMethod]
        public void GenerateCodeTest()
        {
            var className = "TestClass";
            var builder = new CodeBuilder(className)
            {
                Comment = "这是自动生成的代码，用于测试",
                DestPath = "./Gen"
            };
            var memberPairs = new Dictionary<string, object>()
            {
                { "A", typeof(string) },
                { "B", typeof(int) },
                { "C", typeof(double) },
            };
            var unit = builder.GenerateUnit(memberPairs);
            var file = CodeBuilder.GenerateCode(unit, builder.DestPath, className, true);

            var assembly = CodeBuilder.GenerateAssembly(file);
        }
    }
}