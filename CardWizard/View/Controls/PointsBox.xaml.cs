using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
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
using Segment = CallOfCthulhu.Characteristic.Segment;

namespace CardWizard.View
{
    /// <summary>
    /// PointsBox.xaml 的交互逻辑
    /// </summary>
    public partial class PointsBox : UserControl
    {
        public PointsBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 绑定的属性名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 目标获取函数
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

        public void UpdateValueLabels()
        {
            int i = ValueInitial, a = ValueAdjustment, g = ValueGrowth;
            int value = i + a + g;
            if (value == 0)
            {
                Label_MaxValue.Content = string.Empty;
                Text_CurrentValue.Text = string.Empty;
            }
            else
            {
                if (Key != "MOV")
                {
                    Label_MaxValue.Content = i;
                    if (value >= i)
                    {
                        value = i;
                    }
                }
                else
                {
                    var a_ = value - 8;
                    Value_Misc.Content = a_ > 0 ? $"+{a_}" : a_.ToString();
                }
                Text_CurrentValue.Text = value.ToString();
                if (Key == "HP")
                {
                    Value_Misc.Content = (i / 2).ToString();
                }
            }
        }

        /// <summary>
        /// 与属性值绑定(作为点数型)
        /// </summary>
        public void BindToCharacteristicByTag(Func<Character> getter, Translator translator)
        {
            TargetGetter = getter ?? throw new ArgumentNullException(nameof(getter));
            Key = Tag?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(Key))
            {
                Visibility = Visibility.Hidden;
                return;
            }
            Block_Key.Tag = $"{Key}.Block";
            int maximum = TargetGetter().GetInitial(Key);
            Text_CurrentValue.Text = TargetGetter().GetTotal(Key).ToString();

            string[] status;
            switch (Key)
            {
                case "HP":
                    Label_MaxValue.Content = maximum;
                    Label_Misc.Tag = "HP.MajorWound";
                    Value_Misc.Content = (maximum / 2).ToString();
                    status = new string[] { "HP.Status.Healthy", "HP.Status.MajorWound", "HP.Status.Dying" };
                    break;
                case "SAN":
                    Label_MaxValue.Content = maximum;
                    Label_Misc.Content = string.Empty;
                    Value_Misc.Content = string.Empty;
                    status = new string[] { "SAN.Status.Sober", "SAN.Status.Insanity.Temporary", "SAN.Status.Insanity.Indefinite", "SAN.Status.Insanity.Permanent" };
                    break;
                case "MP":
                    Label_MaxValue.Content = maximum;
                    Label_Misc.Tag = "MP.RestoreRate";
                    Value_Misc.Content = "1 / H";
                    status = Array.Empty<string>();
                    break;
                case "MOV":
                    Mark_MaxValue.Visibility = Visibility.Hidden;
                    Label_MaxValue.Content = string.Empty;
                    Label_Misc.Tag = "Adjustment";
                    Value_Misc.Content = TargetGetter().GetAdjustment(Key).ToString();
                    status = Array.Empty<string>();
                    Text_CurrentValue.IsReadOnly = true;
                    Text_CurrentValue.Focusable = false;
                    Text_CurrentValue.Cursor = Cursors.Arrow;
                    break;
                default:
                    Label_MaxValue.Content = string.Empty;
                    Label_Misc.Content = string.Empty;
                    Value_Misc.Content = string.Empty;
                    status = Array.Empty<string>();
                    break;
            }
            if (status.Length == 0)
            {
                Label_Status.Visibility = Visibility.Hidden;
                Combo_Status.Visibility = Visibility.Hidden;
            }
            else
            {
                var statusItems = new List<string>();
                statusItems.AddRange(from s in status select translator.Translate(s, s));
                Combo_Status.ItemsSource = statusItems;
                var idx = statusItems.IndexOf(TargetGetter().Backstory.TryGet($"{Key}.Status", out string r) ? r : "");
                Combo_Status.SelectedIndex = idx >= 0 ? idx : 0;
                Combo_Status.SelectionChanged += Combo_Status_SelectionChanged;
            }
            Text_CurrentValue.LostFocus += Text_CurrentValue_LostFocus;
        }

        private void Combo_Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            TargetGetter().Backstory[$"{Key}.Status"] = combo.SelectedItem.ToString();
        }

        private void Text_CurrentValue_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (int.TryParse(box.Text, out var value))
                TargetGetter().SetCharacteristic(Key, Segment.ADJUSTMENT, value - ValueInitial);
            else
                box.Text = TargetGetter().GetAdjustment(Key).ToString();
        }

        /// <summary>
        /// 当目标的属性数值改变时, 判断是否与自己的 <see cref="Key"/> 相同, 是则刷新
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        public void OnCharacteristicChanged(Character c, CharacteristicChangedEventArgs e)
        {
            if (e == null || !e.Key.EqualsIgnoreCase(Key)) { return; }
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
}
