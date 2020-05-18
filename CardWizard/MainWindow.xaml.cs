using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using CardWizard.Data;
using CardWizard.View;
using CardWizard.Tools;

namespace CardWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 处理逻辑与数据
        /// </summary>
        public MainManager manager;

        public MainWindow()
        {
            InitializeComponent();
            // UI 逻辑处理
            manager = new MainManager(this);
        }
    }
}
