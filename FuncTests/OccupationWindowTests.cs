using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardWizard.View;
using System;
using System.Collections.Generic;
using System.Text;
using CardWizard.Data;

namespace CardWizard.View.Tests
{
    [TestClass()]
    public class OccupationWindowTests
    {
        [TestMethod()]
        public void OccupationWindowTest()
        {
            var config = new Config();
            var window = new OccupationWindow(config.OccupationModels, config.Translator);
            window.Show();
        }

        [TestMethod()]
        public void AddBlockTest()
        {

        }

        [TestMethod()]
        public void ConvertOccupationTest()
        {

        }
    }
}