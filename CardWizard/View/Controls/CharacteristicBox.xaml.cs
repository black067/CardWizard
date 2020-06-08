using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardWizard.View
{
    /// <summary>
    /// CharacteristicBox.xaml 的交互逻辑
    /// </summary>
    public partial class CharacteristicBox : UserControl
    {
        #region Dependency
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.RegisterAttached(nameof(LabelWidth), typeof(GridLength), typeof(CharacteristicBox));

        public static readonly DependencyProperty ValueFontSizeProperty = DependencyProperty.RegisterAttached(nameof(ValueFontSize), typeof(double), typeof(CharacteristicBox));

        public GridLength LabelWidth { get; set; }

        public double ValueFontSize { get; set; }
        #endregion
        /// <summary>
        /// 与之绑定的属性名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Getter: 取得当前编辑的角色
        /// </summary>
        public Func<Character> TargetGetter { get; set; }

        /// <summary>
        /// 属性初始值
        /// </summary>
        public int ValueInitial
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Key)) return 0;
                return TargetGetter?.Invoke()?.GetInitial(Key) ?? 0;
            }
        }

        /// <summary>
        /// 属性调整值
        /// </summary>
        public int ValueAdjustment
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Key)) return 0;
                return TargetGetter?.Invoke()?.GetAdjustment(Key) ?? 0;
            }
        }

        /// <summary>
        /// 属性成长值
        /// </summary>
        public int ValueGrowth
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Key)) return 0;
                return TargetGetter?.Invoke()?.GetGrowth(Key) ?? 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CharacteristicBox()
        {
            InitializeComponent();
            UpdateValueLabels();
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
        public void BindToCharacteristicByTag(Func<Character> getter, MouseButtonEventHandler onClick)
        {
            var tag = Tag?.ToString();
            Key = tag;
            TargetGetter = getter;
            Block_Key.SetValue(TagProperty, $"{Key}.Block");
            Label_Value.MouseDown += (o, e)=>
            {
                onClick(this, e);
            };
        }

        /// <summary>
        /// 当目标的属性数值改变时, 判断是否与自己的 <see cref="Key"/> 相同, 是则刷新
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        public void OnCharacteristicChanged(Character c, CharacteristicChangedEventArgs e)
        {
            if (!e.Key.EqualsIgnoreCase(Key)) { return; }
            UpdateValueLabels();
        }

        /// <summary>
        /// 当目标属性变更时, 强制刷新
        /// </summary>
        /// <param name="c"></param>
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
