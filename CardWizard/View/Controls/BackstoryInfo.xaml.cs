using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private string InvalidMark { get; set; }

        private Label AgeBonusMark { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BackstoryInfo()
        {
            InitializeComponent();
            InvalidMark = (string)Application.Current.FindResource("AgeBonusMark");
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
            // 角色职业
            Button_Occupation.Click += Button_Occupation_Click;
            // 角色年龄的显示
            BindTextBox(Text_Age, nameof(Character.Age), Manager, new IntRangeRule(1, 99));
            AgeBonusMark = Manager.Window.Label_Validity;
            manager.PropertyChanged += CurrentAgeChanged;
            manager.InfoUpdated += c => IsAgeValid(AgeBonusMark, c);
            // 角色现居地点的显示
            BindTextBox(Text_Address, nameof(Character.Address), Manager);
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

        private void Button_Occupation_Click(object sender, RoutedEventArgs e)
        {
            var window = new OccupationWindow(Manager.Config.OccupationModels, Manager.Translator)
            {
                Owner = Manager.Window,
            };
            MainManager.Localize(window, Manager.Translator);
            if ((bool)window.ShowDialog() && window.Selection is Occupation occupation)
            {
                Manager.Current.Occupation = occupation.Name;
                Button_Occupation.Content = occupation.Name;
                if (Button_Occupation.ToolTip is ToolTip tip)
                {
                    var tiptext = occupation.ToString();
                    tiptext = Translator.MapKeywords(tiptext, Manager.Translator.ToKeywordsMap());
                    tip.Content = tiptext;
                }
            }
        }

        /// <summary>
        /// 检查角色的年龄是否满足最小年龄的要求
        /// </summary>
        /// <param name="check"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        static int IsAgeValid(Label check, Character c)
        {
            int minAge = 1;
            if (c.Age < minAge)
            {
                check.Visibility = Visibility.Visible;
            }
            else
            {
                check.Visibility = Visibility.Hidden;
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
            // 改变的值不是年龄, 直接退出
            if (!e.PropertyName.EqualsIgnoreCase(nameof(Character.Age))) return;

            //var edu = Manager.Current.GetTraitInitial("EDU");
            //var script = $"return AgeBonus({edu}, {Manager.Current.Age}, {minAge})";
            //var table = (XLua.LuaTable)Manager.LuaHub.DoString(script).First();
            //var bonus = new Dictionary<string, int>();
            //foreach (var key in table.GetKeys<string>())
            //{
            //    bonus[key] = Convert.ToInt32(table.Get<object>(key));
            //}
            //if (bonus.TryGetValue("EDU", out int eduGrowth))
            //{
            //    Manager.Current.SetTraitGrowth("EDU", eduGrowth);
            //}
            //if (bonus.TryGetValue("Adjustment", out int adjustment))
            //{
            //    Manager.Current.SetTraitAdjustment("EDU", adjustment);
            //}
        }

        /// <summary>
        /// 绑定输入框到管理器
        /// </summary>
        /// <param name="box"></param>
        /// <param name="path"></param>
        /// <param name="manager"></param>
        private static void BindTextBox(TextBox box, string path, MainManager manager, params ValidationRule[] rules)
        {
            manager.InfoUpdating += c =>
            {
                var binding = new Binding(path)
                {
                    Source = c,
                };
                foreach (var item in rules)
                {
                    binding.ValidationRules.Add(item);
                }
                box.SetBinding(TextBox.TextProperty, binding);
            };
            UIExtension.OnClickSelectAll(box);
        }

    }
}
