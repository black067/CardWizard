using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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

        private List<SkillBox> Boxes { get; set; }

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
            Boxes = new List<SkillBox>();
            void ResetHighlights(object sender, EventArgs e)
            {
                foreach (var s in Boxes) { s.SetHighlight(false, false); }
            }
            // 资料更新时, 要刷新技能点的显示
            Manager.InfoUpdated += c =>
            {
                UpdatePointsSummary(c);
                CheckValidationForSkills(c);
            };
            Manager.SkillChanged += (o, e) =>
            {
                UpdatePointsSummary(o);
                var boxes = from box in Boxes where box.Source?.ID == e.SkillID select box;
                CheckValidationForSkills(o);
            };
            Manager.PropertyChanged += OccupationUpdated;

            var groups = new Dictionary<Skill.Categories, Skill[]>(from item in skills
                                                                   group item by item.Category into igroup
                                                                   select KeyValuePair.Create(igroup.Key, igroup.ToArray()));
            var sorted = new Skill[skills.Count()];
            var copystart = 0;
            foreach (var item in groups.Values)
            {
                item.CopyTo(sorted, copystart);
                copystart += item.Length;
            }
            foreach (var item in sorted)
            {
                var box = CreateSkillBoxFor(Container);
                Boxes.Add(box);
                if (item.Category != Skill.Categories.Any && groups[item.Category].Length > 1)
                {
                    box.BindToSkill(Manager.CurrentGetter, Manager.CalcCharacteristic, item, sources: groups[item.Category]);
                    box.Combo_Selector.SelectionChanged += Combo_Selector_SelectionChanged;
                }
                else
                {
                    box.BindToSkill(Manager.CurrentGetter, Manager.CalcCharacteristic, item);
                }
                Manager.SkillChanged += box.OnSkillChanged;
                Manager.InfoUpdated += box.UpdateValueFields;
                if (valuesEditor == null) continue;
                void IOpenPointsEditor(object o, MouseButtonEventArgs e)
                {
                    var sender = box;
                    var source = sender.Source;
                    if (source == null) return;
                    int basevalue = sender.BaseValue, upper = source.Upper, lower = source.Lower;
                    valuesEditor.Show(basevalue, sender.ValueOccupation, sender.ValuePersonal, sender.ValueGrowth);
                    sender.SetHighlight(true, true);
                    // 如果技能的总值有范围限制, 设置提示
                    valuesEditor.SetRangeTip(basevalue, lower, upper, Manager.Translator);
                    // 点击确认后, 要保存填写的值
                    valuesEditor.ConfirmClick += (values) =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (valuesEditor.IsEdited(i, values[i]))
                                sender.TargetGetter().SetSkill(source, (Skill.Segment)i, values[i]);
                        }
                    };
                };
                box.Label_Value.MouseDown += IOpenPointsEditor;
            }
            if (valuesEditor == null) return;
            // 编辑窗口打开的行为
            valuesEditor.PopupOpened += ResetHighlights;
            // 编辑窗口关闭时的行为
            valuesEditor.PopupClosed += ResetHighlights;
        }

        private void Combo_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Skill newSklll = e.AddedItems.Count > 0 ? e.AddedItems[0] as Skill : null, oldSkill = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as Skill : null;
            if (newSklll != null)
            {
                var boxes = from box in Boxes where box.Source?.ID == newSklll.ID select box;
                CheckValidationForSkills(Manager.Current);
                var count = boxes.Count();
                if (count > 1)
                {
                    for (int i = count - 1; i >= 1; i--)
                    {
                        var item = boxes.ElementAt(i);
                        item.Combo_Selector.SelectedIndex = -1;
                    }
                }
            }
        }

        private void CheckValidationForSkills(Character target)
        {
            var ocpID = target.GetOccupationID();
            Skill[] validateSkills;
            if (ocpID != 0 && Manager.DataBus.TryGetOccupation(ocpID, out var occupation))
            {
                validateSkills = occupation.IsSkillValidate(from b in Boxes let s = b.Source where s != null select s);
            }
            else
            {
                validateSkills = Array.Empty<Skill>();
            }
            foreach (var item in Boxes)
            {
                var occupationValue = item.ValueOccupation;
                var sID = item.Source?.ID ?? 0;
                var sCategory = item.Source?.Category ?? Skill.Categories.Any;
                var isOccupationSkill = sID != 0 && validateSkills.Contains(item.Source);
                if (isOccupationSkill)
                {
                    item.Block_Key.DataContext = SkillBox.ContextForOccupationed;
                    item.Combo_Selector.DataContext = SkillBox.ContextForOccupationed;
                }
                else
                {
                    item.Block_Key.DataContext = string.Empty;
                    item.Combo_Selector.DataContext = string.Empty;
                }
                item.Label_Value.DataContext = (occupationValue == 0 || isOccupationSkill) ? string.Empty : SkillBox.ContextForInvalid;
            }
        }

        /// <summary>
        /// 角色职业发生改变时, 刷新职业点数的显示, 检查技能合法性
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OccupationUpdated(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Character.Occupation)) return;
            var target = o as Character;
            UpdatePointsSummary(target);
            CheckValidationForSkills(target);
        }

        /// <summary>
        /// 刷新剩余点数的显示
        /// </summary>
        /// <param name="c"></param>
        private void UpdatePointsSummary(Character c)
        {
            int personalPoints = 0, ppConsumed = 0, ppSum = 0, occupationPoints = 0, opConsumed = 0, opSum = 0;
            var ocpID = c.GetOccupationID();
            if (ocpID != 0 && Manager.DataBus.TryGetOccupation(ocpID, out var occupation))
            {
                var formula = occupation.PointFormula;
                occupationPoints = Manager.CalcCharacteristic(formula, c.GetCharacteristicTotal(k => formula.Contains(k)));
                Label_OccupationPoints.AddOrSetToolTip(occupation.PointFormula, (Style)App.Current.FindResource("XToolTip"));
            }
            opConsumed = (from s in c.Skills select s.OccupationPoints).Sum();
            opSum = occupationPoints - opConsumed;
            Label_OccupationPoints.Content = opSum;
            if (opSum < 0) Label_OccupationPoints.DataContext = SkillBox.ContextForInvalid;
            else Label_OccupationPoints.DataContext = string.Empty;

            var pformula = "INT * 2";
            personalPoints = Manager.CalcCharacteristic(pformula, c.GetCharacteristicTotal(k => k == "INT"));
            ppConsumed = (from s in c.Skills select s.PersonalPoints).Sum();
            ppSum = personalPoints - ppConsumed;
            Label_PersonalPoints.Content = ppSum;
            if (ppSum < 0) Label_PersonalPoints.DataContext = SkillBox.ContextForInvalid;
            else Label_PersonalPoints.DataContext = string.Empty;
        }

        private static SkillBox CreateSkillBoxFor(Panel panel)
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
