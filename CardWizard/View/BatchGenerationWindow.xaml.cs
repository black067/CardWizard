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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public BatchGenerationWindow(Config config, Func<Dictionary<string, int>, string, int> TraitRoller)
        {
            InitializeComponent();
            Width = MinWidth;
            Height = MinHeight;
            ListMain.SelectedItem = ListMain.Items[0];
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Closed += ListWindow_Closed;
            if (config == null) throw new NullReferenceException();
            var translator = config.Translator;
            var dataModels = config.DataModels;
            
            if (translator.TryTranslate($"{nameof(BatchGenerationWindow)}.{nameof(Title)}", out var titleText))
            {
                Title = titleText;
            }

            // 初始化标题行
            Headers.Process(_ =>
            {
                var properties = (from m in dataModels where Filter(m)
                                  select m.Name).ToList();
                properties.Add("SUM");
                Headers.InitAsHeaders(properties.ToArray(), translator);
            });
            // 提示语句
            MessageBox.Process(control =>
            {
                var tkey = control.DataContext?.ToString() ?? string.Empty;
                if (translator.TryTranslate(tkey, out var message))
                {
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
            // 生成几组角色的属性
            var datas = dataModels.ToDictionary(m => m.Name);
            for (int i = ListMain.Items.Count; i > 0; i--)
            {
                var properties = new Dictionary<string, int>(
                    from m in dataModels where Filter(m)
                    select new KeyValuePair<string, int>(m.Name, 0));
                foreach (var key in properties.Keys.ToArray())
                {
                    var value = TraitRoller(properties, datas[key].Formula);
                    properties[key] = Convert.ToInt32(value);
                }
                var sum = properties.Sum(kvp => kvp.Key != Config.KEY_ASSET ? kvp.Value : 0);
                properties["SUM"] = sum;
                items[i - 1].Process(item => {
                    item.MouseDown += (o, e) => Selection = properties;
                    item.InitAsDatas(properties, false);
                });
            }

            Selection = (ListMain.Items[0] as TraitsViewItem).Values;
        }

        /// <summary>
        /// 用来筛选可重生成的属性
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool Filter(DataModel m) => !m.Derived || m.Name.EqualsIgnoreCase("AST");

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
