using CallOfCthulhu;
using CardWizard.Data;
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
    /// SkillsViewer.xaml 的交互逻辑
    /// </summary>
    public partial class SkillsViewer : UserControl
    {
        public SkillsViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 缓存主管理器
        /// </summary>
        MainManager Manager { get; set; }

        private List<SkillBox> SkillBoxes { get; set; }

        /// <summary>
        /// 初始化技能面板
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="skills"></param>
        public void InitializeSkills(MainManager manager, IEnumerable<Skill> skills)
        {
            Container.ClearAllChildren();
            if (skills == null || !skills.Any()) return;
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            SkillBoxes = new List<SkillBox>();
            foreach (var item in skills)
            {
                var box = AddBox(Container);
                SkillBoxes.Add(box);
                box.BindToSkill(item, () => Manager.Current);
                Manager.SkillChanged += box.OnSkillChanged;
                Manager.InfoUpdated += box.UpdateValueFields;
                box.Label_Value.MouseDown += (o, e) =>
                {
                    PopupValueEditor.IsOpen = false;
                    PopupValueEditor.IsOpen = true;
                    var sender = box;
                    sender.Highlight(true);
                    sender.Editing = true;
                    SkillValuesEditor.InitFields(sender.ValueOccupation, sender.ValuePersonal, sender.ValueGrowth);
                    SkillValuesEditor.ConfirmCallback += (values) =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (SkillValuesEditor.IsEdited(i, values[i]))
                                sender.TargetGetter().SetSkill(sender.Source, (Skill.Segment)i, values[i]);
                        }
                    };
                    Keyboard.Focus(SkillValuesEditor.FieldA);
                };
            }
            // 编辑窗口打开的行为
            PopupValueEditor.Opened += (o, e) =>
            {
                SkillBoxes.ForEach(s => { s.Editing = false; s.Highlight(false); });
            };
            // 编辑窗口关闭时的行为
            PopupValueEditor.Closed += (o, e) =>
            {
                SkillBoxes.ForEach(s => { s.Editing = false; s.Highlight(false); });
            };
            // 取消编辑时必须执行的动作
            SkillValuesEditor.CancelCallbackStatic += () =>
            {
                PopupValueEditor.IsOpen = false;
            };
            // 按下确认按钮时的行为
            SkillValuesEditor.ButtonConfirm.Click += (o, e) =>
            {
                PopupValueEditor.IsOpen = false;
                SkillValuesEditor.Confirm();
            };
            // 资料更新时, 要刷新技能点的显示
            Manager.InfoUpdated += UpdatePointsSummary;
            Manager.CharacteristicChanged += (o, e) =>
            {
                UpdatePointsSummary(o);
            };
            Manager.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(Character.Occupation)) UpdatePointsSummary(o as Character);
            };
        }

        /// <summary>
        /// 刷新剩余点数的显示
        /// </summary>
        /// <param name="c"></param>
        private void UpdatePointsSummary(Character c)
        {
            int personalPoints = 0, ppConsumed = 0, ppSum = 0, occupationPoints = 0, opConsumed = 0, opSum = 0;
            if (c.Occupation != null && Manager.DataBus.TryGetOccupation(c.Occupation, out var occupation))
            {
                var formula = occupation.PointFormula;
                occupationPoints = Manager.CalcCharacteristic(formula, c.GetCharacteristicTotal(k => formula.Contains(k)));
                Label_OccupationPoints.AddOrSetToolTip(occupation.PointFormula, (Style)App.Current.FindResource("XToolTip"));
            }
            opConsumed = (from s in c.Skills select s.OccupationPoints).Sum();
            opSum = occupationPoints - opConsumed;
            Label_OccupationPoints.Content = opSum;
            if (opSum < 0) Label_OccupationPoints.DataContext = "invalid";
            else Label_OccupationPoints.DataContext = string.Empty;

            var pformula = "INT * 2";
            personalPoints = Manager.CalcCharacteristic(pformula, c.GetCharacteristicTotal(k => k == "INT"));
            ppConsumed = (from s in c.Skills select s.PersonalPoints).Sum();
            ppSum = personalPoints - ppConsumed;
            Label_PersonalPoints.Content = ppSum;
            if (ppSum < 0) Label_PersonalPoints.DataContext = "invalid";
            else Label_PersonalPoints.DataContext = string.Empty;
        }

        private static SkillBox AddBox(Panel panel)
        {
            var box = new SkillBox();
            box.BeginInit();
            box.Name = $"Box_{panel.Children.Count}";
            box.Margin = new Thickness(1, 2, 1, 2);
            box.FontSize = 10;
            box.EndInit();
            panel.RegisterName(box.Name, box);
            panel.Children.Add(box);
            return box;
        }
    }
}
