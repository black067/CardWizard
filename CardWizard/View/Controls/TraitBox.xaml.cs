using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using YamlDotNet.Core.Tokens;

namespace CardWizard.View
{
    /// <summary>
    /// TraitBox.xaml 的交互逻辑
    /// </summary>
    public partial class TraitBox : UserControl
    {
        private readonly TextBox[] texts;
/*
        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.RegisterAttached("Title",
                                                  typeof(string),
                                                  typeof(TraitBox),
                                                  new PropertyMetadata("属性\nProp"),
                                                  new ValidateValueCallback(o => o != null));

        public static void SetTitle(UIElement element, string value)
        {
            if (element is TraitBox box)
            {
                box.Block_Key.Text = value;
            }
            element?.SetValue(TitleProperty, value);
        }

        public static string GetTitle(UIElement element)
        {
            return element?.GetValue(TitleProperty)?.ToString();
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => GetTitle(this);
            set => SetTitle(this, value);
        }
*/
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
        /// 结束输入时触发的事件
        /// </summary>
        public event Action<Trait.Segment, string, int> InputFieldEndEdit;

        /// <summary>
        /// Constructor
        /// </summary>
        public TraitBox()
        {
            InitializeComponent();
            texts = new TextBox[] { Text_Initial, Text_Adjustment, Text_Growth };
            MouseEnter += TraitBox_ShowEditBoxes;
            MouseLeave += TraitBox_HideEditBoxes;
            EditGrid.Visibility = Visibility.Hidden;
            foreach (var box in texts)
            {
                box.GotFocus += InputField_GotFocus;
                box.LostFocus += InputField_LostFocus;
            }
            UpdateValueView();
        }

        private void InputField_GotFocus(object sender, RoutedEventArgs _)
        {
            IsEditing = true;
            TraitBox_ShowEditBoxes(sender, null);
        }

        private void InputField_LostFocus(object sender, RoutedEventArgs _)
        {
            IsEditing = false;
            TraitBox_HideEditBoxes(sender, null);
            UpdateValueView();
            if (sender is TextBox box && texts.Contains(box))
            {
                if (box.Equals(Text_Initial))
                    InputFieldEndEdit?.Invoke(Trait.Segment.INITIAL, Key, ValueInitial);
                else if (box.Equals(Text_Adjustment))
                    InputFieldEndEdit?.Invoke(Trait.Segment.ADJUSTMENT, Key, ValueAdjustment);
                else
                    InputFieldEndEdit?.Invoke(Trait.Segment.GROWTH, Key, ValueGrowth);
            }
        }

        private void TraitBox_ShowEditBoxes(object sender, EventArgs _)
        {
            EditGrid.Visibility = Visibility.Visible;
            Label_Value.Visibility = Visibility.Hidden;
        }

        private void TraitBox_HideEditBoxes(object sender, EventArgs _)
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
        public void UpdateValueView()
        {
            int value = ValueInitial + ValueAdjustment + ValueGrowth;
            Label_Value.Content = value;
            Label_ValueHalf.Content = (int)(value / 2);
            Label_ValueOneFifth.Content = (int)(value / 5);
        }

        /// <summary>
        /// 绑定到属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="targetGetter"></param>
        public Action<Character, TraitChangedEventArgs> BindToTrait(string key, Action<Trait.Segment, string, int> onEndEdit)
        {
            Key = key;
            Block_Key.SetValue(TagProperty, $"{key}.TraitBox");
            InputFieldEndEdit += onEndEdit;
            void ITraitChanged(Character c, TraitChangedEventArgs e)
            {
                if (!e.Key.EqualsIgnoreCase(Key)) { return; }

                ValueInitial = c.GetTraitInitial(Key);
                ValueAdjustment = c.GetTraitAdjustment(Key);
                ValueGrowth = c.GetTraitGrowth(Key);
                UpdateValueView();
            }
            return ITraitChanged;
        }
    }
}
