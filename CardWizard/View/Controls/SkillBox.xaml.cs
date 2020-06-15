using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardWizard.View
{
    /// <summary>
    /// SkillBox.xaml 的交互逻辑
    /// </summary>
    public partial class SkillBox : UserControl
    {
        public const string ContextForInvalid = "invalid";
        public const string ContextForOccupationed = "occupationed";

        private Skill source;

        private bool isChild;

        public SkillBox()
        {
            InitializeComponent();
            UpdateValueLabels();
            MouseEnter += SkillBox_MouseEnter;
            MouseLeave += SkillBox_MouseLeave;
        }

        private void SkillBox_MouseEnter(object sender, MouseEventArgs e)
        {
            SetHighlight(true, Editing);
        }

        private void SkillBox_MouseLeave(object sender, MouseEventArgs e)
        {
            SetHighlight(Editing || false, Editing);
        }

        public void SetHighlight(bool higtlight, bool editing)
        {
            Editing = editing;
            Background = higtlight ? (SolidColorBrush)FindResource("HoverdBrush") : null;
        }

        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool Editing { get; set; }

        public bool IsChild { get => isChild; set => isChild = value; }

        /// <summary>
        /// 绑定的技能
        /// </summary>
        public Skill Source
        {
            get
            {
                if (IsChild)
                {
                    return Combo_Selector.SelectedItem as Skill;
                }
                return source;
            }

            set
            {
                if (IsChild)
                {
                    Combo_Selector.SelectedItem = value;
                }
                source = value;
            }
        }


        /// <summary>
        /// 获取当前角色的方法
        /// </summary>
        public Func<Character> TargetGetter { get; set; }

        /// <summary>
        /// 数值计算器
        /// </summary>
        public CalculateCharacteristic Calculator { get; set; }

        /// <summary>
        /// 被绑定到的角色身上的指定技能
        /// </summary>
        private Skill TargetSkill
        {
            get
            {
                if (TargetGetter == null || Source == null) return default;
                if (!TargetGetter().TryGetSkill(Source.ID, out var result))
                {
                    result = Source.Clone() as Skill;
                    TargetGetter().Skills.Add(result);
                }
                return result;
            }
        }

        /// <summary>
        /// 技能的职业点数
        /// </summary>
        public int ValueOccupation
        {
            get
            {
                if (TargetGetter == null || Source == null) return 0;
                return TargetGetter().TryGetSkill(Source.ID, out var skill) ? skill.OccupationPoints : 0;
            }
        }

        /// <summary>
        /// 技能的个人点数
        /// </summary>
        public int ValuePersonal
        {
            get
            {
                if (TargetGetter == null || Source == null) return 0;
                return TargetGetter().TryGetSkill(Source.ID, out var skill) ? skill.PersonalPoints : 0;
            }
        }

        /// <summary>
        /// 技能成长值
        /// </summary>
        public int ValueGrowth
        {
            get
            {
                if (TargetGetter == null || Source == null) return 0;
                return TargetGetter().TryGetSkill(Source.ID, out var skill) ? skill.GrowthPoints : 0;
            }
        }

        /// <summary>
        /// 技能的基本成功率
        /// </summary>
        public int BaseValue
        {
            get
            {
                if (TargetGetter == null || Source == null || Calculator == null) return 0;
                return Calculator(Source.BaseValue, TargetGetter().GetCharacteristicTotal());
            }
        }

        /// <summary>
        /// 绑定到特定的技能
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="source"></param>
        /// <param name="targetGetter"></param>
        public void BindToSkill(Func<Character> targetGetter, CalculateCharacteristic calculator, Skill source, params Skill[] sources)
        {
            TargetGetter = targetGetter ?? throw new ArgumentNullException(nameof(targetGetter));
            Calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            if (sources.Length > 0)
            {
                isChild = true;
                Grid_Selector.Visibility = Visibility.Visible;
                Block_Key.Visibility = Visibility.Hidden;
                GrowthMark.Visibility = Visibility.Visible;
                Block_Category.Tag = $"{nameof(Skill)}.{nameof(Skill.Categories)}.{sources[0].Category}";
                Block_Category.Text = sources[0].Category.ToString();
                Combo_Selector.ItemsSource = sources;
                Source = source;
                this.AddOrSetToolTip(source.ToStringFormat(), (Style)App.Current.FindResource("XToolTip"), MainManager.SynchronizeOpacity);
                Combo_Selector.SelectionChanged += Combo_Selector_SelectionChanged;
            }
            else
            {
                Source = source ?? throw new ArgumentNullException(nameof(source));
                isChild = false;
                Grid_Selector.Visibility = Visibility.Hidden;
                Block_Key.Visibility = Visibility.Visible;
                GrowthMark.Visibility = source.Growable ? Visibility.Visible : Visibility.Hidden;
                var baseValueText = Source.BaseValue ?? "0";
                Block_Key.Text = $"{Source.Name ?? "Skill"} ({baseValueText:D2}%)";
                this.AddOrSetToolTip(Source.ToStringFormat(), (Style)App.Current.FindResource("XToolTip"), MainManager.SynchronizeOpacity);
            }
        }

        private void Combo_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Skill newSklll = e.AddedItems.Count > 0 ? e.AddedItems[0] as Skill : null, oldSkill = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as Skill : null;
            if (newSklll != null)
            {
                if (TargetGetter().TryGetSkill(newSklll.ID, out var tskill)) { }
                else
                {
                    tskill = newSklll.Clone() as Skill;
                    TargetGetter().Skills.Add(tskill);
                }
                this.AddOrSetToolTip(newSklll.ToStringFormat(), (Style)App.Current.FindResource("XToolTip"), MainManager.SynchronizeOpacity);
            }
            else
            {
                this.AddOrSetToolTip(string.Empty);
                Block_Key.DataContext = string.Empty;
                Combo_Selector.DataContext = string.Empty;
                Block_Category.DataContext = string.Empty;
            }
            UpdateValueLabels();
        }

        /// <summary>
        /// 技能数值改变时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnSkillChanged(Character sender, SkillChangedEventArgs args)
        {
            if (args == null || !args.IsSameSkill(Source)) { return; }
            UpdateValueLabels();
        }

        /// <summary>
        /// 刷新数字的显示
        /// </summary>
        /// <param name="c"></param>
        public void UpdateValueFields(Character c) => UpdateValueLabels();

        private void GrowthMark_Checked(object sender, RoutedEventArgs e)
        {
            var targetSkill = TargetSkill;
            if (targetSkill != null) targetSkill.Grown = true;
        }

        private void GrowthMark_Unchecked(object sender, RoutedEventArgs e)
        {
            var targetSkill = TargetSkill;
            if (targetSkill != null) targetSkill.Grown = false;
        }

        /// <summary>
        /// 设置显示的值
        /// </summary>
        /// <param name="value"></param>
        private void UpdateValueLabels()
        {
            bool grown;
            if (TargetGetter == null || Source == null)
            {
                grown = false;
            }
            else if (TargetGetter().TryGetSkill(Source.ID, out var skill))
            {
                grown = skill.Grown;
            }
            else
            {
                grown = false;
                Combo_Selector.SelectedIndex = -1;
            }
            GrowthMark.IsChecked = grown;
            var baseValue = BaseValue;
            int value = baseValue + ValueOccupation + ValuePersonal + ValueGrowth;
            if (value == baseValue)
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

        private void ResetSelector(object sender, RoutedEventArgs e)
        {
            Combo_Selector.SelectedIndex = -1;
            UpdateValueLabels();
        }
    }

    /// <summary>
    /// 委托: 可绑定到 <see cref="SkillBox.InputFieldEndEdit"/> 结束编辑的事件
    /// </summary>
    /// <param name="target"></param>
    /// <param name="segment"></param>
    /// <param name="skillName"></param>
    /// <param name="value"></param>
    public delegate void SkillEndEditedEventHandler(SkillBox sender, Character target, Skill.Segment segment, int value);
}
