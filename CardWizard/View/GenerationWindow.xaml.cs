using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using XLua;

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

        public Dictionary<string, string> Bonus { get; set; } = new Dictionary<string, string>();

        private CalculateCharacteristic CharacteristicsRoller { get; set; }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 用来筛选可重生成的属性
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool Filter(Characteristic m) => !m.Derived && !m.Name.EqualsIgnoreCase("LUCK");

        /// <summary>
        /// 构造一个角色生成器窗口
        /// </summary>
        /// <param name="manager"></param>
        public GenerationWindow(MainManager manager, CalculateCharacteristic traitRoller)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            InitializeComponent();
            Width = MinWidth;
            Height = MinHeight;
            ListMain.SelectedItem = ListMain.Items[0];
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Closed += ListWindow_Closed;
            var translator = manager.Translator;
            var dataModels = manager.Config.DataModels;
            CharacteristicsRoller = traitRoller;
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
            List<CustomRowView> items = (from object i in ListMain.Items where i is CustomRowView select i as CustomRowView).ToList();
            // 初始化年龄惩罚的显示列
            BindAgeBox(manager.LuaHub, Text_Age, Label_AgeBonus, Button_AgeCheck, AdjustmentsEditor, dataModels);
            // 生成几组角色的属性
            var datas = dataModels.ToDictionary(m => m.Name);
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                var properties = new Dictionary<string, int>(from m in dataModels
                                                             where Filter(m)
                                                             select new KeyValuePair<string, int>(m.Name, 0));
                foreach (var key in properties.Keys.ToArray())
                {
                    properties[key] = CharacteristicsRoller(datas[key].Formula, properties);
                }
                var sum = properties.Sum(kvp => kvp.Value);
                properties["SUM"] = sum;
                item.InitAsDatas(properties, false);
                item.MouseDown += (o, e) => Selection = item.Values;
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
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            bonus = new List<(string, string)>();
            var ageBonus = hub.Get<LuaFunction>("AgeBonus");
            var text = ageBonus.Call(age).FirstOrDefault().ToString();
            var dict = YamlKit.Parse<ContextDict>(text);
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
        /// <param name="CustomRowView"></param>
        private void BindAgeBox(ScriptHub hub, TextBox inputField, Label description, Button checkButton, CustomRowView CustomRowView, List<Characteristic> models)
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
            CustomRowView.InitAsDatas(properties, true);
            // 设置编辑结束时执行的事件
            bool ValidCheck()
            {
                GetAgeBonus(hub, Age, out var comment, out var rule, out var bonus);
                Bonus.Clear();
                // 显示说明文字
                description.Content = comment;
                // 屏蔽那些不需要调整的属性输入框
                foreach (var box in CustomRowView.Children.Values) box.Visibility = Visibility.Hidden;
                var matches = Regex.Matches(rule, @"[A-Z|a-z]{1,}");
                foreach (Match match in matches)
                {
                    if (CustomRowView.Children.TryGetValue(match.Value, out var box))
                    {
                        box.Visibility = Visibility.Visible;
                        box.IsEnabled = true;
                        box.Focusable = true;
                    }
                }
                foreach (var (key, formula) in bonus)
                {
                    Bonus.Add(key, formula);
                    if (CustomRowView.Children.TryGetValue(key, out var box))
                    {
                        if (int.TryParse(formula, out int v))
                        {
                            box.Text = formula;
                            CustomRowView.Values[key] = v;
                            // 可见不可编辑
                            box.Visibility = Visibility.Visible;
                            box.IsEnabled = false;
                            box.Focusable = false;
                        }
                    }
                }
                // 判断是否有按照规则调整
                using var subhub = hub.CreateSubEnv(CustomRowView.Values);
                bool isValid = (bool)subhub.DoString(rule, isGlobal: true).First();
                Button_Confirm.IsEnabled = isValid;
                description.DataContext = isValid ? string.Empty : "Invalid";
                return isValid;
            }
            // 先刷新一遍数据的显示
            ValidCheck();
            // 更新显示的提示文字
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != nameof(Age)) return;
                ValidCheck();
            };
            // 
            checkButton.Click += (o, e) =>
            {
                ValidCheck();
            };
            var gestures = new InputGestureCollection();
            gestures.Add(new MouseGesture(MouseAction.LeftClick));
            gestures.Add(new KeyGesture(Key.Enter));
            gestures.Add(new KeyGesture(Key.Tab));
            this.AddCommandsBindings(new RoutedCommand("CatchMouse", this.GetType(), gestures), (o, e) =>
            {
                var valid = false;
                if (Keyboard.FocusedElement != inputField)
                    valid = ValidCheck();
                if (!valid) checkButton.Focus();
                else Button_Confirm.Focus();
            });
        }

        /// <summary>
        /// 将年龄对属性的影响施加到角色身上
        /// <para>当前年龄对属性的影响可在属性 <see cref="Bonus"/> 中查询</para>
        /// </summary>
        /// <param name="character"></param>
        /// <param name="calculator"></param>
        public void ApplyAgeBonus(Character character)
        {
            if (character == null) return;
            character.Age = Age;
            foreach (var kvp in Bonus)
            {
                if (!character.Initials.ContainsKey(kvp.Key)) continue;
                var delta = CharacteristicsRoller?.Invoke(kvp.Value, character.Initials) ?? 0;
                character.SetAdjustment(kvp.Key, delta);
            }
            foreach (var kvp in AdjustmentsEditor.Values)
            {
                if (!character.Initials.ContainsKey(kvp.Key) || kvp.Value == 0) continue;
                var old = character.GetAdjustment(kvp.Key);
                character.SetAdjustment(kvp.Key, old + kvp.Value);
            }
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
