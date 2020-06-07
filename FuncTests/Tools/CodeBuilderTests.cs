using CardWizard.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CardWizard.Tools.Tests
{
    [TestClass()]
    public class CodeBuilderTests
    {
        [TestMethod]
        public void GenerateCodeTest()
        {
            var meta = new Config();
            var typeMeta = meta.GetType();
            var descrAttr = typeMeta.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
            var descrText = string.Empty;
            if (descrAttr is DescriptionAttribute description) descrText = description.Description;
            var className = "ConfigBase";
            var builder = new CodeBuilder(className)
            {
                Comment = descrText,
                DestPath = "./Gen",
                Namespace = typeMeta.Namespace,
            };
            var members = from f in typeMeta.GetFields() select new MemberDefinition(f, meta);
            var unit = builder.GenerateUnit(members);
            var file = CodeBuilder.GenerateCode(unit, builder.DestPath, className, true);
        }
    }
}