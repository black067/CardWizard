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

namespace CardWizard.View
{
    /// <summary>
    /// SkillBox.xaml 的交互逻辑
    /// </summary>
    public partial class SkillBox : UserControl
    {
        public SkillBox()
        {
            InitializeComponent();
            UpdateValueLabels();
            MouseEnter += SkillBox_MouseEnter;
            MouseLeave += SkillBox_MouseLeave;
        }

        private void SkillBox_MouseEnter(object sender, MouseEventArgs e)
        {
            Highlight(true);
        }

        private void SkillBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Highlight(Editing || false);
        }

        public void Highlight(bool active)
        {
            Background = active ? (SolidColorBrush)FindResource("HoverdBrush") : null;
        }

        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool Editing { get; set; }

        /// <summary>
        /// 绑定的主管理器
        /// </summary>
        private MainManager Manager { get; set; }

        /// <summary>
        /// 绑定的技能
        /// </summary>
        public Skill Source { get; set; }

        /// <summary>
        /// 获取当前角色的方法
        /// </summary>
        public Func<Character> TargetGetter { get; set; }

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
        /// 属性初始值
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
        /// 属性调整值
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
        /// 属性成长值
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
        /// 绑定到特定的技能
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="source"></param>
        /// <param name="targetGetter"></param>
        public void BindToSkill(Skill source, Func<Character> targetGetter)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            TargetGetter = targetGetter ?? throw new ArgumentNullException(nameof(targetGetter));
            GrowthMark.Visibility = source.Growable ? Visibility.Visible : Visibility.Hidden;
            Block_Key.Text = $"{source.Name} ({source.BaseValue}%)";
        }

        /// <summary>
        /// 技能数值改变时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnSkillChanged(Character sender, SkillChangedEventArgs args)
        {
            if (!args.IsSameSkill(Source)) { return; }
            UpdateValueLabels();
        }

        /// <summary>
        /// 刷新数字的显示
        /// </summary>
        /// <param name="c"></param>
        public void UpdateValueFields(Character c)
        {
            OnSkillChanged(c, new SkillChangedEventArgs(Source.ID));
        }

        private void GrowthMark_Checked(object sender, RoutedEventArgs e)
        {
            var targetSkill = TargetSkill;
            targetSkill.Grown = true;
        }

        private void GrowthMark_Unchecked(object sender, RoutedEventArgs e)
        {
            var targetSkill = TargetSkill;
            targetSkill.Grown = false;
        }

        /// <summary>
        /// 设置显示的值
        /// </summary>
        /// <param name="value"></param>
        private void UpdateValueLabels()
        {
            var baseValue = Source == null ? 0 : Source.BaseValue;
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
