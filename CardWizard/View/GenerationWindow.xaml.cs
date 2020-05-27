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
using System.Windows.Data;
using System.Windows.Input;
using XLua;
using XLua.LuaDLL;

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

        public int Age { get; set; }

        public string Rule { get; set; }

        public List<(string key, string formula)> Bonus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public GenerationWindow(MainManager manager, CalculateTrait TraitRoller)
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
            BindAgeBox(manager.LuaHub, Text_Age, Block_AgeBonus);
            // 生成几组角色的属性
            var datas = dataModels.ToDictionary(m => m.Name);
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var properties = new Dictionary<string, int>(from m in dataModels
                                                             where Filter(m)
                                                             select new KeyValuePair<string, int>(m.Name, 0));
                foreach (var key in properties.Keys.ToArray())
                {
                    var value = TraitRoller(datas[key].Formula, properties);
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

        public static void GetAgeBonus(ScriptHub hub, int age, out string rule, out List<(string key, string formula)> bonus)
        {
            if (hub == null) throw new NullReferenceException("hub is null");
            bonus = new List<(string, string)>();
            var text = (string)hub.DoString($"return AgeBonus({age})").FirstOrDefault();
            var dict = YamlKit.Parse<ContextData>(text);
            if (dict.ContainsKey("Rule"))
                rule = (string)dict["Rule"];
            else
                rule = "true";
            dict.TryGet<ICollection>("Bonus", out var bonusRaw);
            foreach (IDictionary item in bonusRaw)
            {
                var key = (string)item["key"];
                var formula = (string)item["formula"];
                bonus.Add((key, formula));
            }
        }

        /// <summary>
        /// 初始化年龄惩罚列
        /// </summary>
        /// <param name="traitsView"></param>
        private void BindAgeBox(ScriptHub hub, TextBox inputField, TextBlock description)
        {
            void onEndEdit(object sender, RoutedEventArgs e)
            {
                GetAgeBonus(hub, Age, out var r, out var b);
                Rule = r;
                Bonus = b;
                description.Inlines.Clear();
                description = 
            }
            var binding = new Binding(nameof(Age)) { Source = this, Mode = BindingMode.TwoWay };
            binding.ValidationRules.Add(new IntRangeRule(1, 99));
            inputField.SetBinding(TextBox.TextProperty, binding);
            inputField.LostFocus += onEndEdit;
            UIExtension.OnClickSelectAll(inputField);
        }

        /// <summary>
        /// 用来筛选可重生成的属性
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool Filter(Trait m) => !m.Derived && !m.Name.EqualsIgnoreCase("LUCK") || m.Name.EqualsIgnoreCase("AST");

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
