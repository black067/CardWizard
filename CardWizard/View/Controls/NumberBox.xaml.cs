﻿using System;
using System.Collections.Generic;
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
    // 用 decimal 做数值的类型
    using DATATYPE = System.Int32;

    /// <summary>
    /// NumberBox.xaml 的交互逻辑
    /// </summary>
    public partial class NumberBox : UserControl
    {
        /// <summary>
        /// 值的属性引用
        /// </summary>
        public static DependencyProperty NumberProperty = DependencyProperty.RegisterAttached(nameof(Number),
                                                                                              typeof(DATATYPE),
                                                                                              typeof(NumberBox));

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetNumber(UIElement element, object value)
        {
            element?.SetValue(NumberProperty, value);
        }

        /// <summary>
        /// 查询值
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static DATATYPE GetNumber(UIElement element)
        {
            return DATATYPE.TryParse(element?.GetValue(NumberProperty)?.ToString(), out var r) ? r : 0;
        }

        /// <summary>
        /// 数字值
        /// </summary>
        public DATATYPE Number { get => GetNumber(this); set => SetNumber(this, value); }

        public NumberBox()
        {
            InitializeComponent();
            DecTen.Click += DecTen_Click;
            DecOne.Click += DecOne_Click;
            IncOne.Click += IncOne_Click;
            IncTen.Click += IncTen_Click;
            InputField.SetBinding(TextBox.TextProperty, new Binding(nameof(Number)) { Source = this });
            InputField.SetBinding(InputScopeProperty, new Binding(nameof(InputScope)) { Source = this });
        }

        private void IncTen_Click(object sender, RoutedEventArgs e)
        {
            Number += 10;
        }

        private void IncOne_Click(object sender, RoutedEventArgs e)
        {
            Number += 1;
        }

        private void DecOne_Click(object sender, RoutedEventArgs e)
        {
            Number -= 1;
        }

        private void DecTen_Click(object sender, RoutedEventArgs e)
        {
            Number -= 10;
        }
    }
}
