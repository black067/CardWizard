using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CardWizard.View
{
    /// <summary>
    /// BackstoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class BackstoryPage : UserControl
    {

        /// <summary>
        /// 装备面板元素发生变化时触发的事件
        /// </summary>
        public event NotifyCollectionChangedEventHandler GearsCollectionChanged;

        public BackstoryPage()
        {
            InitializeComponent();
            Panel_Gears.ClearAllChildren();
            Button_NewIitem.Click += Button_NewIitem_Click;
        }

        public Label AddGearToPanel(params string[] gears)
        {
            if (gears == null || !gears.Any()) return null;
            foreach (var item in gears)
            {
                var style = (Style)Panel_Gears.FindResource("PanelItem");
                var itemlabel = UIExtension.AddItem<Label>(Panel_Gears, $"item_{Panel_Gears.Children.Count}", style);
                itemlabel.Content = item;
                return itemlabel;
            }
            return null;
        }

        private void Button_NewIitem_Click(object sender, RoutedEventArgs e)
        {
            var itemName = Text_NewItem.Text;
            Text_NewItem.Text = string.Empty;
            if (string.IsNullOrWhiteSpace(itemName)) return;
            var item = AddGearToPanel(itemName);
            var index = Panel_Gears.Children.IndexOf(item);
            GearsCollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemName, index: index));
        }

        /// <summary>
        /// 用于道具的删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button delButton && delButton.TemplatedParent is FrameworkElement fe && fe.Parent == Panel_Gears)
            {
                var index = Panel_Gears.Children.IndexOf(fe);
                Panel_Gears.UnregisterName(fe.Name);
                Panel_Gears.Children.Remove(fe);
                if (fe is ContentControl control)
                    GearsCollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, fe.Name, index: index));
            }
        }
    }
}
