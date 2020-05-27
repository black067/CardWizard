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
    public partial class GenerationWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// 选择结果
        /// </summary>
        public Dictionary<string, int> Selection { get; set; }
        private int age = Character.DEFAULT_AGE;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 初始年龄
        /// </summary>
        public int Age
        {
            get
            {
                return age;
            }
            set
            {
                age = value;
                OnPropertyChanged();
            }
        }
        public string Rule { get; set; }

        public List<(string key, string formula)> Bonus { get; set; }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 用来筛选可重生成的属性
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool Filter(Trait m) => !m.Derived && !m.Name.EqualsIgnoreCase("LUCK");

        /// <summary>
        /// 构造一个角色生成器窗口
        /// </summary>
        /// <param name="manager"></param>
        public GenerationWindow(MainManager manager, CalculateTrait TraitRoller)
        {
            if (manager == null) throw new NullReferenceException("manager is null");
            InitializeComponent();
            Width = MinWidth;
            Height = MinHeight;
            ListMain.SelectedItem = ListMain.Items[0];
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Closed += ListWindow_Closed;
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
            List<TraitsViewItem> items = (from object i in ListMain.Items where i is TraitsViewItem select i as TraitsViewItem).ToList();
            // 初始化年龄惩罚的显示列
            BindAgeBox(manager.LuaHub, Text_Age, Label_AgeBonus, Button_AgeCheck, AdjustmentsEditor, dataModels);
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

        /// <summary>
        /// 查询角色的年龄奖励
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="age"></param>
        /// <param name="rule"></param>
        /// <param name="bonus"></param>
        public static void GetAgeBonus(ScriptHub hub, int age, out string comment, out string rule, out List<(string key, string formula)> bonus)
        {
            if (hub == null) throw new NullReferenceException("hub is null");
            bonus = new List<(string, string)>();
            var ageBonus = hub.Get<LuaFunction>("AgeBonus");
            var text = ageBonus.Call(age).FirstOrDefault().ToString();
            var dict = YamlKit.Parse<ContextData>(text);
            dict.TryGet<string>("Comment", out comment);
            if (dict.ContainsKey("Rule"))
                rule = $"return {dict["Rule"]}";
            else
                rule = "return true";
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
        private void BindAgeBox(ScriptHub hub, TextBox inputField, Label description, Button checkButton, TraitsViewItem traitsView, List<Trait> models)
        {
            // 设置数据绑定
            var binding = new Binding(nameof(Age)) { Source = this, };
            binding.NotifyOnValidationError = true;
            var validation = new IntRangeRule(1, 99);
            binding.ValidationRules.Add(validation);
            inputField.SetBinding(TextBox.TextProperty, binding);
            UIExtension.OnClickSelectAll(inputField);
            // 绑定调整值输入框
            var properties = new Dictionary<string, int>(from m in models
                                                         where Filter(m)
                                                         select new KeyValuePair<string, int>(m.Name, 0));
            traitsView.InitAsDatas(properties, true);
            // 设置编辑结束时执行的事件
            void ValidCheck()
            {
                using var subhub = hub.CreateSubEnv(traitsView.Values);
                bool valid = (bool)subhub.DoString(Rule, isGlobal: true).First();
                Button_Confirm.IsEnabled = valid;
                description.DataContext = valid ? string.Empty : "Invalid";
            }
            void CheckAgeBonus(int age)
            {
                GetAgeBonus(hub, age, out var comment, out var r, out var b);
                Rule = r;
                Bonus = b;
                description.Content = comment;
                ValidCheck();
            }
            // 更新显示的提示文字
            CheckAgeBonus(Age);
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != nameof(Age)) return;
                CheckAgeBonus(Age);
            };
            // 
            checkButton.Click += (o, e) =>
            {
                ValidCheck();
            };
            var gestures = new InputGestureCollection();
            gestures.Add(new MouseGesture(MouseAction.LeftClick));
            this.AddCommandsBindings(new RoutedCommand("CatchMouse", this.GetType(), gestures), (o, e) =>
            {
                if (inputField.IsFocused) { checkButton.Focus(); }
            });
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
