using System;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using CardWizard.Data;
using CardWizard.Tools;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;

namespace CardWizard.View
{
    /// <summary>
    /// ListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchGenerationWindow : Window
    {
        /// <summary>
        /// 选择结果
        /// </summary>
        public Dictionary<string, int> Selection { get; set; }

        public BatchGenerationWindow(IEnumerable<DataModel> dataModels, Func<string, int> calculator, Config.Localization localization = null)
        {
            InitializeComponent();
            Width = MinWidth;
            Height = MinHeight;
            ListMain.SelectedItem = ListMain.Items[0];
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Closed += ListWindow_Closed;
            if (localization.TryTranslate($"{nameof(BatchGenerationWindow)}.{nameof(Title)}", out var titleText))
            {
                Title = titleText;
            }

            var datas = dataModels.ToDictionary(m => m.Name);
            // 初始化标题行
            Headers.Process(_ =>
            {
                var properties = (from m in dataModels 
                                  where !m.Derived || m.Name.EqualsIgnoreCase(Config.KEY_ASSET)
                                  select m.Name).ToList();
                properties.Add("SUM");
                Headers.InitAsHeaders(properties.ToArray(), localization);
            });
            // 提示语句
            MessageBox.Process(control =>
            {
                var tkey = control.DataContext?.ToString() ?? string.Empty;
                if (localization.TryTranslate(tkey, out var message))
                {
                    //Label_Message.Document.Blocks.Clear();
                    //Label_Message.Document.Blocks.Add(new Paragraph(new Run(tipText)));
                    control.Text = message;
                }
            });
            // 列表
            List<TraitsViewItem> items = new List<TraitsViewItem>();
            foreach (var item in ListMain.Items)
            {
                if (item is TraitsViewItem listItem)
                {
                    items.Add(listItem);
                }
            }

            int Roll(Dictionary<string, int> properties, string key) 
                => calculator(MainManager.FormatScript(properties, datas[key].Formula));
            // 生成几组角色的属性
            for (int i = ListMain.Items.Count; i > 0; i--)
            {
                var properties = new Dictionary<string, int>(
                    from m in dataModels where !m.Derived || m.Name.EqualsIgnoreCase(Config.KEY_ASSET)
                    select new KeyValuePair<string, int>(m.Name, 0));;
                foreach (var key in properties.Keys.ToArray())
                {
                    var value = Roll(properties, key);
                    properties[key] = Convert.ToInt32(value);
                }
                var sum = properties.Sum(kvp => kvp.Key != Config.KEY_ASSET ? kvp.Value : 0);
                properties["SUM"] = sum;
                items[i - 1].Process(item => {
                    item.MouseDown += (o, e) =>
                    {
                        Selection = properties;
                    };
                    item.InitAsDatas(properties, false);
                });
            }

            Selection = (ListMain.Items[0] as TraitsViewItem).Values;
            GC.Collect();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ListWindow_Closed(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}
