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
using CallOfCthulhu;

namespace CardWizard.View
{
    /// <summary>
    /// ListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GenerationWindow : Window
    {
        /// <summary>
        /// 选择结果
        /// </summary>
        public Dictionary<string, int> Selection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public GenerationWindow(MainManager manager, Func<Dictionary<string, int>, string, int> TraitRoller)
        {
            InitializeComponent();
            Width = MinWidth;
            Height = MinHeight;
            ListMain.SelectedItem = ListMain.Items[0];
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Closed += ListWindow_Closed;
            if (manager == null) throw new NullReferenceException();
            var translator = manager.Translator;
            var dataModels = manager.Config.DataModels;
            // 初始化标题行
            Headers.Process(_ =>
            {
                var properties = (from m in dataModels
                                  where Filter(m)
                                  select m.Name).ToList();
                properties.Add("SUM");
                Headers.InitAsHeaders(properties.ToArray(), translator);
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
            // 初始化年龄惩罚的显示列
            InitAgePenaltyBlock();
            // 生成几组角色的属性
            var datas = dataModels.ToDictionary(m => m.Name);
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var properties = new Dictionary<string, int>(from m in dataModels where Filter(m)
                                                             select new KeyValuePair<string, int>(m.Name, 0));
                foreach (var key in properties.Keys.ToArray())
                {
                    var value = TraitRoller(properties, datas[key].Formula);
                    properties[key] = Convert.ToInt32(value);
                }
                var sum = properties.Sum(kvp => kvp.Key != "AST" ? kvp.Value : 0);
                properties["SUM"] = sum;

                items[i].MouseDown += (o, e) => Selection = properties;
                items[i].InitAsDatas(properties, false);
            }
            Selection = items[0].Values;

            MainManager.Localize(this, translator);
        }

        /// <summary>
        /// 初始化年龄惩罚列
        /// </summary>
        /// <param name="traitsView"></param>
        private void InitAgePenaltyBlock()
        {
            // TODO 撰写年龄的输入框逻辑
            AgeSettings.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 用来筛选可重生成的属性
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool Filter(Trait m) => !m.Derived || m.Name.EqualsIgnoreCase("AST");

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
