using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CardWizard.View
{
    /// <summary>
    /// CharacteristicBox.xaml 的交互逻辑
    /// </summary>
    public partial class CharacteristicBox : UserControl
    {
        private readonly TextBox[] texts;

        private Func<Character> TargetGetter { get; set; }

        /// <summary>
        /// 与之绑定的属性名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 属性初始值
        /// </summary>
        public int ValueInitial
        {
            get
            {
                var text = Text_Initial.Text?.ToString();
                return int.TryParse(text, out int value) ? value : 0;
            }
            set
            {
                Text_Initial.Text = value.ToString();
            }
        }

        /// <summary>
        /// 属性调整值
        /// </summary>
        public int ValueAdjustment
        {
            get
            {
                var text = Text_Adjustment.Text?.ToString();
                return int.TryParse(text, out int value) ? value : 0;
            }
            set
            {
                Text_Adjustment.Text = value.ToString();
            }
        }

        /// <summary>
        /// 属性成长值
        /// </summary>
        public int ValueGrowth
        {
            get
            {
                var text = Text_Growth.Text?.ToString();
                return int.TryParse(text, out int value) ? value : 0;
            }
            set
            {
                Text_Growth.Text = value.ToString();
            }
        }

        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        private bool IsEditing { get; set; }

        /// <summary>
        /// 用户是否可以编辑
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 结束输入时触发的事件
        /// </summary>
        public event CharacteristicEndEditEventHandler InputFieldEndEdit;

        /// <summary>
        /// Constructor
        /// </summary>
        public CharacteristicBox()
        {
            InitializeComponent();
            MouseEnter += Box_ShowEditBoxes;
            MouseLeave += Box_HideEditBoxes;
            EditGrid.Visibility = Visibility.Hidden;
            texts = new TextBox[] { Text_Initial, Text_Adjustment, Text_Growth };
            foreach (var box in texts)
            {
                box.GotFocus += InputField_GotFocus;
                box.GotKeyboardFocus += InputField_GotFocus;
                box.LostFocus += InputField_LostFocus;
                box.LostKeyboardFocus += InputField_LostFocus;
            }
            UpdateValueLabels();
            IsReadOnly = false;
        }

        private void InputField_GotFocus(object sender, RoutedEventArgs _)
        {
            IsEditing = true;
            Box_ShowEditBoxes(sender, null);
        }

        private void InputField_LostFocus(object sender, RoutedEventArgs _)
        {
            IsEditing = false;
            Box_HideEditBoxes(sender, null);
            UpdateValueLabels();
            if (sender is TextBox box && texts.Contains(box))
            {
                var c = TargetGetter();
                if (c == null) return;
                if (box.Equals(Text_Initial))
                    InputFieldEndEdit?.Invoke(TargetGetter(), Characteristic.Segment.INITIAL, Key, ValueInitial);
                else if (box.Equals(Text_Adjustment))
                    InputFieldEndEdit?.Invoke(TargetGetter(), Characteristic.Segment.ADJUSTMENT, Key, ValueAdjustment);
                else
                    InputFieldEndEdit?.Invoke(TargetGetter(), Characteristic.Segment.GROWTH, Key, ValueGrowth);
            }
        }

        private void Box_ShowEditBoxes(object sender, EventArgs _)
        {
            if (IsReadOnly) { return; }
            EditGrid.Visibility = Visibility.Visible;
            Label_Value.Visibility = Visibility.Hidden;
        }

        private void Box_HideEditBoxes(object sender, EventArgs _)
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
        private void UpdateValueLabels()
        {
            int value = ValueInitial + ValueAdjustment + ValueGrowth;
            if (value == 0)
            {
                Label_Value.Content = string.Empty;
                Label_ValueHalf.Content = string.Empty;
                Label_ValueOneFifth.Content = string.Empty;
            }
            else
            {
                Label_Value.Content = value;
                Label_ValueHalf.Content = (int)(value / 2);
                Label_ValueOneFifth.Content = (int)(value / 5);
            }
        }

        /// <summary>
        /// 根据自身的 <see cref="CharacteristicBox.Tag"/>, 绑定到指定的属性
        /// </summary>
        /// <param name="onEndEdit"></param>
        /// <returns></returns>
        public void BindToCharacteristicByTag(Func<Character> getter, CharacteristicEndEditEventHandler onEndEdit)
        {
            var tag = Tag?.ToString();
            Key = tag;
            TargetGetter = getter;
            Block_Key.SetValue(TagProperty, $"{Key}.Block");
            if (onEndEdit != null) InputFieldEndEdit += onEndEdit;
        }

        public void OnCharacteristicChanged(Character c, CharacteristicChangedEventArgs e)
        {
            if (!e.Key.EqualsIgnoreCase(Key)) { return; }
            ValueInitial = c.GetInitial(Key);
            ValueAdjustment = c.GetAdjustment(Key);
            ValueGrowth = c.GetGrowth(Key);
            UpdateValueLabels();
        }

        public void UpdateValueFields(Character c)
        {
            OnCharacteristicChanged(c, new CharacteristicChangedEventArgs(Key));
        }
    }

    /// <summary>
    /// 委托: 可绑定到 <see cref="CharacteristicBox.InputFieldEndEdit"/> 结束编辑的事件
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="traitName"></param>
    /// <param name="value"></param>
    public delegate void CharacteristicEndEditEventHandler(Character characterGetter, Characteristic.Segment segment, string traitName, int value);
}
