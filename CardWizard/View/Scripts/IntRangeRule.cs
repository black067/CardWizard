using System;
using System.Globalization;
using System.Windows.Controls;

namespace CardWizard.View
{
    /// <summary>
    /// 整数的范围限定
    /// </summary>
    public class IntRangeRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public IntRangeRule() { }

        /// <summary>
        /// 构造一个范围限定规则
        /// </summary>
        /// <param name="min">下限</param>
        /// <param name="max">上限</param>
        public IntRangeRule(int min, int max) { Min = min; Max = max; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int v = 0;
            if (value == null) return new ValidationResult(false, $"value is null");
            try
            {
                if (((string)value).Length > 0)
                    v = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }

            if ((v < Min) || (v > Max))
            {
                return new ValidationResult(false, $"Please enter an age in the range: {Min}-{Max}.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
