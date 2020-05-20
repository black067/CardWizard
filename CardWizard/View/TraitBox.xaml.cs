using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// TraitBox.xaml 的交互逻辑
    /// </summary>
    public partial class TraitBox : UserControl
    {
        private bool isEditing;

        /// <summary>
        /// 与之绑定的属性名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 初始值
        /// </summary>
        public int ValueInitial { get; set; }

        /// <summary>
        /// 调整值
        /// </summary>
        public int ValueAdjustment { get; set; }

        /// <summary>
        /// 成长值
        /// </summary>
        public int ValueGrowth { get; set; }

        /// <summary>
        /// 总值
        /// </summary>
        public int Value { get => ValueInitial + ValueAdjustment + ValueGrowth; }

        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        private bool IsEditing
        {
            get => isEditing; 
            set
            {
                isEditing = value;
                if (!isEditing)
                    TraitBox_MouseLeave(this, null);
            }
        }

        public TraitBox()
        {
            InitializeComponent();
            EditGrid.Visibility = Visibility.Hidden;
            Text_Initial.GotFocus += InputField_GotFocus;
            Text_Initial.LostFocus += InputField_LostFocus;
            Text_Adjustment.GotFocus += InputField_GotFocus;
            Text_Adjustment.LostFocus += InputField_LostFocus;
            Text_Growth.GotFocus += InputField_GotFocus;
            Text_Growth.LostFocus += InputField_LostFocus;
            MouseEnter += TraitBox_MouseEnter;
            MouseLeave += TraitBox_MouseLeave;
        }

        private void InputField_GotFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = true;
            if (sender is TextBox box)
            {
                var tag = box.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) return;
            }
        }

        private void InputField_LostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false;
            if (sender is TextBox box)
            {
                var tag = box.Tag?.ToString();
                if (string.IsNullOrEmpty(tag) || !int.TryParse(box.Text, out var value)) return;
                if (tag.EqualsIgnoreCase("Initial"))
                {
                    ValueInitial = value;
                }
                else if (tag.EqualsIgnoreCase("Adjustment"))
                {
                    ValueAdjustment = value;
                }
                else
                {
                    ValueGrowth = value;
                }
                SetValueView(Value);
            }
        }

        private void TraitBox_MouseEnter(object sender, MouseEventArgs e)
        {
            EditGrid.Visibility = Visibility.Visible;
            Label_Value.Visibility = Visibility.Hidden;
        }

        private void TraitBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEditing)
            {
                EditGrid.Visibility = Visibility.Hidden;
                Label_Value.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 设置显示的值
        /// </summary>
        /// <param name="value"></param>
        public void SetValueView(int value)
        {
            int half = (int)(value / 2), oneFifth = (int)(value / 5);
            Label_Value.Content = value;
            Label_ValueHalf.Content = half;
            Label_ValueOneFifth.Content = oneFifth;
        }

        /// <summary>
        /// 绑定到属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="targetGetter"></param>
        public Action<Character, TraitChangedEventArgs> BindToTrait(string key)
        {
            Key = key;
            Block_Key.Tag = $"TraitBox.{key}";
            void TraitChanged(Character c, TraitChangedEventArgs e)
            {
                if (!e.Key.EqualsIgnoreCase(Key)) { return; }
                var value = c.GetTrait(Key);
                SetValueView(value);
            }
            return TraitChanged;
        }
    }
}
