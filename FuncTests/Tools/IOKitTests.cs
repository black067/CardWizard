using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CardWizard.Tools
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
            if (File.Exists(ZipFileName))
            {
                File.Delete(ZipFileName);
            }
        }

        [TestMethod]
        public void FindAllFiles()
        {
            // 创建测试用的目录及文件
            var root = $"./{nameof(IOKitTests)}_{nameof(FindAllFiles)}";
            var testFiles = new string[]
            {
                $"{root}/a.0.txt", $"{root}/a.1.txt", $"{root}/a.2.txt",
                $"{root}/b/b.0.txt",$"{root}/b/b.1.txt",
                $"{root}/c/c.0.txt",
                $"{root}/c/d/d.0.txt", $"{root}/c/d/d.1.txt",
            };
            foreach (var item in testFiles)
            {
                if (File.Exists(item))
                {
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(item));
                using var writer = File.CreateText(item);
                writer.Write($"test for {nameof(FindAllFiles)}");
            }
            // 文件创建完毕, 记录其路径
            var expectedItems = Directory.GetFiles(root).Select(f => new FileInfo(f).FullName).ToList();
            // FUNCTION BEGIN
            var actualItems = IOKit.GetAllFiles(root).Select(f => f.FullName);
            // FUNCTION END
            foreach (var item in expectedItems)
            {
                Assert.IsTrue(actualItems.Contains(item), $"没有找到文件 {item}");
            }
            // 删除创建的文件
            Directory.Delete(root, true);
        }

        [TestMethod]
        public void ZipAndExtract()
        {
            var files = Directory.GetFiles(Resource);
            var filesSource = files.ToDictionary(s => Path.GetFileName(s));
            // FUNCTION BEGIN
            var threadZip = IOKit.Zip(ZipFileName, "zip file for test", files);
            while (threadZip.IsAlive)
            {

            }
            Thread.Sleep(500);
            var threadExt = IOKit.Extract(ZipFileName);
            // FUNCTION END
            var filesExtracted = Directory.GetFiles("./test").ToDictionary(s => Path.GetFileName(s));

            foreach (var kvp in filesSource)
            {
                if (!filesExtracted.ContainsKey(kvp.Key)) throw new Exception($"解压前后文件数量不一致, 找不到文件 {kvp.Value}");
                string[] linesSource = File.ReadAllLines(kvp.Value);
                string[] linesExtracted = File.ReadAllLines(filesExtracted[kvp.Key]);

                Assert.AreEqual(linesSource.Length, linesExtracted.Length, $"文件长度不一致: {kvp.Value}");
                for (int i = 0, len = linesSource.Length; i < len; i++)
                {
                    Assert.AreEqual(linesSource[i], linesExtracted[i], message: $"在第 {i} 行出现差异");
                }

            }
        }
    }
}
