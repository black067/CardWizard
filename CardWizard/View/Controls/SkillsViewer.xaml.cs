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
        /// <summary>
        /// 缓存主管理器
        /// </summary>
        MainManager Manager { get; set; }

        public SkillsViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化技能面板
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="skills"></param>
        public void InitializeSkills(MainManager manager, IEnumerable<Skill> skills)
        {
            Container.ClearAllChildren();
            if (manager == null) throw new NullReferenceException("manager is null");
            if (skills == null || !skills.Any()) return;
            Manager = manager;
            foreach (var item in skills)
            {
                var box = AddBox(Container, ConvertSkill(item));
                var iChanged = box.BindToSkill(item, () => Manager.Current, Manager.OnCharacteristicEdited);
                Manager.CharacteristicChanged += iChanged;
                Manager.InfoUpdated += MainManager.GetHandlerForCharacteristicBox(iChanged, box.Key);
            }
            Manager.InfoUpdated += o =>
            {
                UpdatePointsView(o);
            };
            Manager.CharacteristicChanged += (o, e) =>
            {
                UpdatePointsView(o);
            };
            Manager.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(Character.Occupation)) UpdatePointsView(o as Character);
            };
        }

        private void UpdatePointsView(Character c)
        {
            int personalPoints = 0, occupationPoints = 0;
            if (c.Occupation != null && Manager.DataBus.TryGetOccupation(c.Occupation, out var occupation))
            {
                var formula = occupation.PointFormula;
                occupationPoints = Manager.CalcCharacteristic(formula, c.GetCharacteristicTotal(k => formula.Contains(k)));
                //Label_OccupationPoints.AddOrSetToolTip(occupation.PointFormula, (Style)App.Current.FindResource("XToolTip"));
            }
            Label_OccupationPoints.Content = occupationPoints;

            var pformula = "INT * 2";
            personalPoints = Manager.CalcCharacteristic(pformula, c.GetCharacteristicTotal(k => k == "INT"));
            Label_PersonalPoints.Content = personalPoints;
        }

        private static IEnumerable<TextElement> ConvertSkill(Skill item)
        {
            var style = $"# {{ FontSize: 12, FontStyle: Italic }}";
            var context = $"{item.Name}({item.BaseValue:00}%) {style}";
            var inlines = UIExtension.ResolveTextElements(context.Trim());
            return inlines;
        }

        private static CharacteristicBox AddBox(Panel panel, IEnumerable<TextElement> inlines)
        {
            var box = new CharacteristicBox();
            box.BeginInit();
            box.Name = $"Box_{panel.Children.Count}";
            box.Margin = new Thickness(1, 2, 1, 2);
            box.Block_Key.Inlines.Clear();
            box.Block_Key.Inlines.AddRange(inlines);
            box.Block_Key.TextAlignment = TextAlignment.Left;
            box.EndInit();
            panel.RegisterName(box.Name, box);
            panel.Children.Add(box);
            return box;
        }
    }
}
