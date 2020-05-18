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
using CardWizard.Data;
using CardWizard.Tools;

namespace CardWizard.View
{
    /// <summary>
    /// InfomationView.xaml 的交互逻辑
    /// </summary>
    public partial class InfomationView : UserControl
    {
        private MainManager Manager { get; set; }

        private string ValidText;

        private string InvalidText;

        public InfomationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化角色信息交互面板
        /// </summary>
        /// <param name="manager"></param>
        public void InitializeBinding(MainManager manager)
        {
            Manager = manager;
            // 角色名称的控制
            BindTextBox(Text_Name, nameof(Character.Name), Manager);
            // 角色时代背景的控制
            Manager.InfoUpdating += c =>
            {
                var binding = new Binding(nameof(Character.Era)) { Source = c };
                Combo_Era.SetBinding(ComboBox.SelectedValueProperty, binding);
            };
            Combo_Era.ItemsSource = Manager.Translator.labelForEras;
            // 角色年龄的显示
            BindTextBox(Text_Age, nameof(Character.Age), Manager);
            ValidText = Manager.Translator.TryTranslate("Valid", out var t) ? t : "✔";
            InvalidText = Manager.Translator.TryTranslate("Invalid", out t) ? t : "✘";
            Label_Age_Check.Process(check =>
            {
                Manager.InfoUpdated += c =>
                {
                    var minAge = IsAgeValid(check, c);
                    var tooltip = Manager.Translator.TryTranslate("ToolTip.Age.Min", out var m) ? m : "Minimum Age is {0}";
                    tooltip = string.Format(tooltip, minAge);
                    check.ToolTip = tooltip;
                    c.PropertyChanged -= CurrentPropertyChanged;
                    c.PropertyChanged += CurrentPropertyChanged;
                };
            });
            Label_Age_Penalty.Process(label =>
            {
                label.Content = "";
                label.ToolTip = Manager.Translator.TryTranslate("ToolTip.Age.Penalty", out var msg) ? msg : "Your Age Penalty.";
            });
            // 角色出生地的控制
            BindTextBox(Text_Homeland, nameof(Character.Homeland), Manager);
            // 角色学历的控制
            BindTextBox(Text_Education, nameof(Character.Education), Manager);
            // 角色性别的控制
            InitializeGenderRadios(Manager, Radio_Gender_Male, Radio_Gender_Female, Radio_Gender_Other);
        }

        /// <summary>
        /// 检查角色的年龄是否满足最小年龄的要求
        /// </summary>
        /// <param name="check"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        int IsAgeValid(Label check, Character c)
        {
            var minAge = Manager.CalcForInt($"return GetMinAge({c.GetTraitBase("EDU")})");
            if (c.Age < minAge)
            {
                check.Content = InvalidText;
                check.Foreground = (SolidColorBrush)Application.Current.FindResource("InvalidForeground");
            }
            else
            {
                check.Content = ValidText;
                check.Foreground = (SolidColorBrush)Application.Current.FindResource("ValidForeground");
            }
            return minAge;
        }

        private void CurrentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.EqualsIgnoreCase(nameof(Character.Age)))
            {
                var minAge = IsAgeValid(Label_Age_Check, Manager.Current);
                var edu = Manager.Current.GetTraitBase("EDU");
                var script = $"return AgeBonus({edu}, {Manager.Current.Age}, {minAge})";
                var table = (XLua.LuaTable)Manager.LuaHub.DoString(script).First();
                var bonus = new Dictionary<string, int>();
                foreach (var key in table.GetKeys<string>())
                {
                    bonus[key] = Convert.ToInt32(table.Get<object>(key));
                }
                if (bonus.TryGetValue("EDU", out int eduGrowth)) { Manager.Current.SetTraitGrowth("EDU", eduGrowth); }
                if (bonus.TryGetValue("Senescence", out int senescence))
                {
                    Label_Age_Penalty.Content = senescence;
                }
            }
        }

        /// <summary>
        /// 初始化性别按钮组
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="radios"></param>
        private static void InitializeGenderRadios(MainManager manager, params RadioButton[] radios)
        {
            foreach (var item in radios)
            {
                item.Checked += (o, e) => manager.Current.Gender = item.Content.ToString();
            }
            manager.InfoUpdating += c =>
            {
                if (string.IsNullOrWhiteSpace(c.Gender))
                {
                    foreach (var item in radios)
                    {
                        item.IsChecked = false;
                    }
                }
                else
                {
                    foreach (var item in radios)
                    {
                        if (item.Content.ToString().EqualsIgnoreCase(c.Gender))
                        {
                            item.IsChecked = true;
                            break;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 绑定输入框到管理器
        /// </summary>
        /// <param name="box"></param>
        /// <param name="path"></param>
        /// <param name="manager"></param>
        private static void BindTextBox(TextBox box, string path, MainManager manager)
        {
            manager.InfoUpdating += c =>
            {
                var binding = new Binding(path)
                {
                    Source = c
                };
                box.SetBinding(TextBox.TextProperty, binding);
            };
            #region 单击时将内容全选
            box.PreviewMouseDown += Box_PreviewMouseDown;
            box.GotFocus += Box_GotFocus;
            box.LostFocus += Box_LostFocus;
            #endregion
        }

        #region 单击时将内容全选
        private static void Box_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var box = e.Source as TextBox;
            box.Focus();
            e.Handled = true;
        }
        private static void Box_GotFocus(object sender, RoutedEventArgs e)
        {
            var box = e.Source as TextBox;
            box.SelectAll();
            box.PreviewMouseDown -= Box_PreviewMouseDown;
        }
        private static void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = e.Source as TextBox;
            box.PreviewMouseDown += Box_PreviewMouseDown;
        }
        #endregion
    }
}
