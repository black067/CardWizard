using CardWizard.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CallOfCthulhu;
using CardWizard;
using CardWizard.Data;
using CardWizard.Properties;
using CardWizard.View;
using XLua;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using System;

namespace CardWizard.Tools.Tests
{
    [TestClass]
    public class YamlKitTests
    {
        public Dictionary<string, Func<object, bool>> Examples { get; private set; }

        public YamlKitTests()
        {
            Examples = new Dictionary<string, Func<object, bool>>();
            Examples.Add("FontSize: 16", o =>
            {
                return o is Dictionary<string, object> dict && dict.ContainsKey("FontSize");
            });
            Examples.Add("{ FontSize: 16, FontWeight: Bold }", o =>
            {
                return o is Dictionary<string, object> dict && dict.ContainsKey("FontSize") && dict.ContainsKey("FontWeight");
            });
        }

        [TestMethod]
        public void Test_Serialization()
        {
            foreach (var kvp in Examples)
            {
                YamlKit.TryParse<Dictionary<string, object>>(kvp.Key, out var item, YamlKit.ParseFail.Throw);
                Assert.IsTrue(kvp.Value(item), $"{kvp.Key} got {item}");
            }
        }
    }
}
