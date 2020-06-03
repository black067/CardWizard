using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        }

        public List<Weapon> Weapons { get; set; }

        public void InitializeBox(MainManager manager, IEnumerable<Weapon> weapons)
        {
            if (manager == null || weapons == null || !weapons.Any()) return;
            Weapons = weapons.ToList();
            MainGrid.ItemsSource = Weapons;
            Character getter() => manager.Current;
            foreach (var item in FormulaCalculator.FormulaCalculators)
            {
                item.Calculator = manager.CalcCharacteristic;
                item.CharacterGetter = getter;
            }
        }
    }

    /// <summary>
    /// 转换器: 将公式转换为值
    /// </summary>
    public class FormulaCalculator : IValueConverter
    {
        internal static List<FormulaCalculator> FormulaCalculators { get; set; } = new List<FormulaCalculator>();

        public FormulaCalculator() { FormulaCalculators.Add(this); }

        public CalculateCharacteristic Calculator { get; set; }

        public Func<Character> CharacterGetter { get; set; }

        public object Convert(object raw, Type targetType, object parameter, CultureInfo culture)
        {
            if (raw == null || Calculator == null || CharacterGetter == null) return DependencyProperty.UnsetValue;

            int value = Calculator(raw as string, CharacterGetter().GetCharacteristicTotal());
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
