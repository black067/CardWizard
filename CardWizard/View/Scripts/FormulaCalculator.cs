using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CardWizard.View
{
    /// <summary>
    /// 转换器: 将公式转换为值
    /// </summary>
    public class FormulaCalculator : IValueConverter
    {
        /// <summary>
        /// 所有公式计算器
        /// </summary>
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
