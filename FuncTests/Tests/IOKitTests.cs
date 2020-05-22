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
using System.IO;

namespace FuncTests
{
    [TestClass]
    public class IOKitTests
    {
        public string ZipFileName { get; set; }
        public string Resource { get; set; }

        public IOKitTests()
        {
            Resource = "./Resources/Scripts";
            ZipFileName = "test.zip";
        }

        [TestMethod]
        public void ZipAndExtract()
        {
            var files = Directory.GetFiles(Resource);
            var filesSource = files.ToDictionary(s => Path.GetFileName(s));
            // FUNCTION BEGIN
            IOKit.Zip(ZipFileName, "zip file for test", files);
            IOKit.Extract(ZipFileName);
            // FUNCTION END
            var filesExtracted = Directory.GetFiles("./test").ToDictionary(s => Path.GetFileName(s));

            foreach (var kvp in filesSource)
            {
                if (!filesExtracted.ContainsKey(kvp.Key)) throw new Exception($"解压前后文件数量不一致, 找不到文件 {kvp.Value}");
                string[] linesSource = File.ReadAllLines(kvp.Value);
                string[] linesExtracted = File.ReadAllLines(filesExtracted[kvp.Key]);

                Assert.AreEqual(linesSource.Length, linesSource.Length, $"文件长度不一致: {kvp.Value}");
                for (int i = 0, len = linesSource.Length; i < len; i++)
                {
                    Assert.AreEqual(linesSource[i], linesExtracted[i], message: $"在第 {i} 行出现差异");
                }

            }
        }
    }
}
