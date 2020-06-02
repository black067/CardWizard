using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CardWizard.View
{
    /// <summary>
    /// WeaponBox.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponBox : UserControl
    {
        public WeaponBox()
        {
            InitializeComponent();
            RowTemplate = new Dictionary<string, object>();
            foreach (var item in MainGrid.Columns)
            {
                if (item.Header is FrameworkElement e && e.Tag is string key)
                    RowTemplate.Add(key, string.Empty);
            }
        }

        private Dictionary<string, object> RowTemplate = new Dictionary<string, object>();

        public void InitializeBox(IEnumerable<Weapon> weapons)
        {
            var source = new List<Dictionary<string, object>>();
            foreach (var item in weapons)
            {
                var dict = new Dictionary<string, object>(RowTemplate);
                dict["Weapon"] = item.Name;
                dict["Weapon.Normal"] = item.Hitrate;
                dict["Weapon.Hard"] = "";
                dict["Weapon.ExtremeHard"] = "";
                dict["Weapon.Weapon"] = item.Damage;
                dict["Weapon.Range"] = item.BaseRange;
                dict["Weapon.AttacksPerRound"] = item.AttacksPerRound;
                dict["Weapon.Bullets"] = item.Bullets;
                dict["Weapon.Resistance"] = item.Resistance;
                source.Add(dict);
            }

            MainGrid.ItemsSource = source;
        }
    }


}
