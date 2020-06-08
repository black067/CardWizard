using CallOfCthulhu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        /// <summary>
        /// 初始化技能面板
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="skills"></param>
        public void InitializeSkills(MainManager manager, IEnumerable<Skill> skills, ValuesEditor valuesEditor)
        {
            Container.ClearAllChildren();
            if (skills == null || !skills.Any()) return;
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            // 缓存所有技能显示盒
            var boxes = new List<SkillBox>();
            void ResetHighlights(object sender, EventArgs e)
            {
                foreach (var s in boxes) { s.SetHighlight(false, false); }
            }
            // 资料更新时, 要刷新技能点的显示
            Manager.InfoUpdated += UpdatePointsSummary;
            Manager.SkillChanged += (o, e) =>
            {
                UpdatePointsSummary(o);
            };
            Manager.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(Character.Occupation)) UpdatePointsSummary(o as Character);
            };
            foreach (var item in skills)
            {
                var box = AddBox(Container);
                boxes.Add(box);
                box.BindToSkill(item, () => Manager.Current);
                Manager.SkillChanged += box.OnSkillChanged;
                Manager.InfoUpdated += box.UpdateValueFields;
                if (valuesEditor == null) continue;
                box.Label_Value.MouseDown += (o, e) =>
                {
                    var sender = box;
                    var source = sender.Source;
                    valuesEditor.Show(source.BaseValue, sender.ValueOccupation, sender.ValuePersonal, sender.ValueGrowth);
                    sender.SetHighlight(true, true);
                    // 如果技能的总值有范围限制, 设置提示
                    int basevalue = source.BaseValue, upper = source.Upper, lower = source.Lower;
                    valuesEditor.SetRangeTip(basevalue, lower, upper, Manager.Translator);
                    // 点击确认后, 要保存填写的值
                    valuesEditor.ConfirmCallback += (values) =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (valuesEditor.IsEdited(i, values[i]))
                                sender.TargetGetter().SetSkill(source, (Skill.Segment)i, values[i]);
                        }
                    };
                };
            }
            if (valuesEditor == null) return;
            // 编辑窗口打开的行为
            valuesEditor.PopupOpened += ResetHighlights;
            // 编辑窗口关闭时的行为
            valuesEditor.PopupClosed += ResetHighlights;
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
