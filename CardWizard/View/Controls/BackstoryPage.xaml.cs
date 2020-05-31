using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardWizard.View
{
    /// <summary>
    /// BackstoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class BackstoryPage : UserControl
    {
        public BackstoryPage()
        {
            InitializeComponent();
            Button_NewIitem.Click += Button_NewIitem_Click;
        }

        private void Button_NewIitem_Click(object sender, RoutedEventArgs e)
        {
            var itemName = Text_NewItem.Text;
            if (string.IsNullOrWhiteSpace(itemName)) return;
            var inlines = UIExtension.ResolveTextElements($"{itemName} # FontSize: 14");
            var textblock = UIExtension.AddBlock(Panel_Gears, $"item_{Panel_Gears.Children.Count}", inlines: inlines);
        }
    }
}
