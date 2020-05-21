using Microsoft.VisualStudio.TestTools.UnitTesting;
using CallOfCthulhu;
using CardWizard;
using CardWizard.Data;
using CardWizard.Properties;
using CardWizard.Tools;
using CardWizard.View;
using XLua;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using System;

namespace FuncTests
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
        }

        [TestMethod]
        public void Test_Serialization()
        {
            foreach (var kvp in Examples)
            {
                YamlKit.TryParse<Dictionary<string, object>>(kvp.Key, out var item);
                Assert.IsTrue(kvp.Value(item), $"{kvp.Key} got {item}");
            }
        }
    }
}
