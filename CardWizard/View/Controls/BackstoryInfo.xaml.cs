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
using CallOfCthulhu;
using CardWizard.Tools;

namespace CardWizard.View
{
    /// <summary>
    /// BackstoryInfo.xaml 的交互逻辑
    /// </summary>
    public partial class BackstoryInfo : UserControl
    {

        private MainManager Manager { get; set; }

        private string ValidText;

        private string InvalidText;

        /// <summary>
        /// Constructor
        /// </summary>
        public BackstoryInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化角色信息交互面板
        /// </summary>
        /// <param name="manager"></param>
        public void InitializeBinding(MainManager manager)
        {
            if (manager == null) throw new NullReferenceException();
            Manager = manager;
            var translator = Manager.Translator;
            // 角色名称的控制
            BindTextBox(Text_Name, nameof(Character.Name), Manager);
            // 角色时代背景的控制
            Manager.InfoUpdating += c =>
            {
                var binding = new Binding(nameof(Character.Era)) { Source = c };
                Combo_Era.SetBinding(ComboBox.SelectedValueProperty, binding);
            };
            Combo_Era.ItemsSource = translator.labelForEras;
            // 角色年龄的显示
            BindTextBox(Text_Age, nameof(Character.Age), Manager);
            ValidText = translator.Translate("Valid", "✔");
            InvalidText = translator.Translate("Invalid", "❌");
            // 角色出生地的控制
            BindTextBox(Text_Homeland, nameof(Character.Homeland), Manager);
            // 角色学历的控制
            BindTextBox(Text_Education, nameof(Character.Education), Manager);
            // 角色性别的控制
            var gender_male = translator.Translate("Gender.Male", "Male");
            var gender_female = translator.Translate("Gender.Female", "Female");
            var gender_others = translator.Translate("Gender.Others", "Others");
            Combo_Gender.ItemsSource = new string[] { gender_male, gender_female, gender_others };
            Manager.InfoUpdating += c =>
            {
                var binding = new Binding(nameof(Character.Gender)) { Source = c };
                Combo_Gender.SetBinding(ComboBox.SelectedValueProperty, binding);
            };
        }

        /// <summary>
        /// 检查角色的年龄是否满足最小年龄的要求
        /// </summary>
        /// <param name="check"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        int IsAgeValid(TextBox check, Character c)
        {
            var minAge = Manager.CalcForInt($"return GetMinAge({c.GetTraitInitial("EDU")})");
            if (c.Age < minAge)
            {
                //check.Content = InvalidText;
                check.BorderBrush = (SolidColorBrush)Application.Current.FindResource("InvalidForeground");
            }
            else
            {
                //check.Content = ValidText;
                check.Foreground = (SolidColorBrush)Application.Current.FindResource("ValidForeground");
            }
            return minAge;
        }

        /// <summary>
        /// 当角色年龄被编辑时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentAgeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 改变的值不是年龄, 直接中断
            if (!e.PropertyName.EqualsIgnoreCase(nameof(Character.Age))) return;
            // 判断年龄是否符合规则
            var minAge = IsAgeValid(Text_Age, Manager.Current);
            var edu = Manager.Current.GetTraitInitial("EDU");
            var script = $"return AgeBonus({edu}, {Manager.Current.Age}, {minAge})";
            var table = (XLua.LuaTable)Manager.LuaHub.DoString(script).First();
            var bonus = new Dictionary<string, int>();
            foreach (var key in table.GetKeys<string>())
            {
                bonus[key] = Convert.ToInt32(table.Get<object>(key));
            }
            if (bonus.TryGetValue("EDU", out int eduGrowth))
            {
                Manager.Current.SetTraitGrowth("EDU", eduGrowth);
            }
            if (bonus.TryGetValue("Adjustment", out int adjustment))
            {
                Manager.Current.SetTraitAdjustment("EDU", adjustment);
            }
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
