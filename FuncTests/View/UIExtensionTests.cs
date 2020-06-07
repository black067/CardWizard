using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace CardWizard.View.Tests
{
    [TestClass()]
    public class UIExtensionTests
    {
        [TestMethod()]
        public void ResolveTextElementPropertiesTest()
        {
            var text = @"# 测试
FontSize: 16
FontWeight: Bold
FontFamily: Consolas
Background: Transparent
Foreground: '#ff00ff00'";
            // FUNCTION BEGIN
            var dp = UIExtension.ResolveTextElementProperties(text);
            // FUNCTION END
            var conditions = new Dictionary<string, Func<bool>>()
            {
                {nameof(TextElement.FontSize), () => dp[TextElement.FontSizeProperty].Equals(16.0) },
                {nameof(TextElement.FontWeight), () => dp[TextElement.FontWeightProperty].Equals(System.Windows.FontWeights.Bold) },
                {nameof(TextElement.FontFamily), () => dp[TextElement.FontFamilyProperty] is FontFamily },
                {nameof(TextElement.Background), () => dp[TextElement.BackgroundProperty] is Brush},
                {nameof(TextElement.Foreground), () => dp[TextElement.ForegroundProperty] is Brush },
            };
            foreach (var (propertyName, condition) in conditions)
            {
                Assert.IsTrue(condition(), $"{propertyName} 的解析不符合预期");
            }
        }

        [TestMethod()]
        public void ResolveTextElementsTest()
        {
            var text = @"Title # { FontSize: 12 }
First line # { FontSize: 8, Background: Black }
Title B # { FontSize: 12 }
Second line # { FontSize: 8, Background: Transparent }";
            // FUNCTION BEGIN
            var inlines = UIExtension.ResolveTextElements(text);
            var runs = (from i in inlines where i is Run select i as Run).ToList();
            // FUNCTION END
            var conditions = new Dictionary<string, Func<bool>>()
            {
                { $"0: {nameof(TextElement.FontSize)} convert fail",
                    () => runs[0].FontSize.Equals(12.0) },
                { $"1: {nameof(TextElement.Background)} convert fail",
                    () => runs[1].Background is SolidColorBrush colorBrush },
            };
            foreach (var (msg, condition) in conditions)
            {
                Assert.IsTrue(condition(), msg);
            }
        }
    }
}