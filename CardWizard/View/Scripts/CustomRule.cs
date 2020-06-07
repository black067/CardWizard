using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CardWizard.View
{
    /// <summary>
    /// 可以自定规则的绑定值合法性判断器
    /// <para>可用于 <see cref="Binding.ValidationRules"/></para>
    /// </summary>
    public class CustomRule : ValidationRule
    {
        public Func<object, bool> ValidationChecker;

        /// <summary>
        /// 构造一个自定规则的判断器
        /// </summary>
        /// <param name="checker"></param>
        public CustomRule(Func<object, bool> checker) => ValidationChecker = checker;

        /// <summary>
        /// 判断是否符合要求
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                bool isValid = ValidationChecker?.Invoke(value) ?? true;
                if (!isValid) return new ValidationResult(false, "invalid value");
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"{e.Message}\n{e.StackTrace}");
            }
            return ValidationResult.ValidResult;
        }
    }
}
