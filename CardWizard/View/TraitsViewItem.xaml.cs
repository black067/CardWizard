using CardWizard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CardWizard.View
{
    /// <summary>
    /// ListItemCustom.xaml 的交互逻辑
    /// </summary>
    public partial class TraitsViewItem : UserControl
    {
        /// <summary>
        /// 值的集合
        /// <para>只有在调用了方法: <see cref="InitAsDatas(Dictionary{string, int}, bool)"/> 之后, 值才有意义</para>
        /// </summary>
        public Dictionary<string, int> Values { get; private set; }

        /// <summary>
        /// 此控件管理的所有 <see cref="TextBox"/>
        /// </summary>
        public Dictionary<string, TextBox> Children { get; private set; }

        /// <summary>
        /// 数据提示的显示格式
        /// </summary>
        private const string FORMAT_TOOLTIP = "{0} = {1}\n{2}\n{3}";

        private string tooltipForTrait;

        /// <summary>
        /// 新建此控件
        /// </summary>
        public TraitsViewItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 作为一行数据进行初始化
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="enableEdit">是否允许编辑</param>
        public void InitAsDatas(Dictionary<string, int> datas, bool enableEdit = true)
        {
            Values = datas;
            var values = datas?.Values.ToArray() ?? Array.Empty<int>();
            var keys = datas.Keys.ToArray();
            for (int i = 0, len = Math.Min(MainGrid.Children.Count, values.Length); i < len; i++)
            {
                var item = MainGrid.Children[i];
                if (item is FrameworkElement element)
                {
                    element.ToolTip = keys[i];
                }
                if (item is ContentControl ctr)
                {
                    ctr.Content = values[i];
                }
                else if (item is TextBox box)
                {
                    box.Text = values[i].ToString();
                }
                if (!enableEdit) { item.IsEnabled = false; }
            }
            CleanExcessColumns(values.Length);
        }

        /// <summary>
        /// 作为表头初始化
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="translator"></param>
        public void InitAsHeaders(IEnumerable<string> headers, Translator translator)
        {
            var loopLimit = headers.Count();
            void dosth(UIElement e, int i)
            {
                var item = headers.ElementAt(i);
                if (e is FrameworkElement element)
                {
                    element.Tag = item;
                    element.Cursor = System.Windows.Input.Cursors.Arrow;
                    if (translator.TryTranslate($"{item}.Description", out var value))
                    {
                        element.ToolTip = value;
                    }
                }
                if (e is TextBox box)
                {
                    box.Text = translator.Translate(item, item);
                    box.SetValue(TextBox.IsReadOnlyProperty, true);
                }
            }
            MainGrid.ForeachChild(dosth, loopLimit: loopLimit);
            CleanExcessColumns(loopLimit);
        }

        /// <summary>
        /// 作为属性面板显示时, 需要保存对 <see cref="MainManager"/> 的引用
        /// </summary>
        private MainManager Manager { get; set; }

        /// <summary>
        /// 刷新 <see cref="TextBox"/> 控件的 <see cref="TextBox.Text"/>
        /// </summary>
        /// <param name="box"></param>
        /// <param name="key"></param>
        /// <param name="character"></param>
        private void UpdateBoxText(TextBox box, string key, Character character)
        {
            box.Text = character.GetTrait(key).ToString();
            if (Manager != null)
                box.ToolTip = string.Format(FORMAT_TOOLTIP, key, Manager.Current.GetTraitText(key), tooltipForTrait, Manager.Config.BaseModelDict[key].Formula);
        }

        /// <summary>
        /// 作为角色的属性面板进行初始化
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="derived">是否绑定派生属性</param>
        public void BindTraits(MainManager manager, bool derived)
        {
            Manager = manager ?? throw new NullReferenceException();
            Children = new Dictionary<string, TextBox>();
            var models = new Dictionary<string, DataModel>(from kvp in Manager.Config.BaseModelDict
                                                           where derived == kvp.Value.Derived
                                                           select kvp);
            tooltipForTrait = Manager.Translator.Translate("Trait.ToolTip", "= Initial + Growth + Adjustment");
            var keys = models.Keys.ToArray();
            for (int i = 0, len = Math.Min(MainGrid.Children.Count, models.Count); i < len; i++)
            {
                if (MainGrid.Children[i] is TextBox box)
                {
                    // 用 box 的 DataContext 保存其指向的角色特点名称
                    box.DataContext = keys[i];
                    box.GotFocus += (o, e) =>
                    {
                        var key = box.DataContext.ToString();
                        box.Text = Manager.Current.GetTraitText(key);
                    };
                    box.LostFocus += (o, e) => EndEditTraitBox(box);
                    Manager.InfoUpdating += c =>
                    {
                        var key = box.DataContext.ToString();
                        box.Text = c.GetTraitText(key);
                    };
                    UpdateBoxText(box, keys[i], Manager.Current);
                    Children.Add(keys[i], box);
                }
            }
            Manager.TraitChanged += C_TraitChanged;
            CleanExcessColumns(keys.Length);
        }

        private void C_TraitChanged(Character character, Character.TraitChangedEventArgs e)
        {
            var eKey = e.Key;
            if (Children.ContainsKey(eKey))
            {
                UpdateBoxText(Children[eKey], eKey, character);
            }
            // 如果当前修改的是基础特点值, 检查是否需要更新派生特点值
            if (Manager.Config.BaseModelDict.TryGetValue(eKey, out var basemodel) && !basemodel.Derived)
            {
                var models = from m in Manager.Config.DataModels where m.Derived && m.Formula.Contains(basemodel.Name, StringComparison.OrdinalIgnoreCase) select m;
                foreach (var m in models)
                {
                    var cKey = m.Name;
                    if (!Children.ContainsKey(cKey)) { continue; }
                    var @base = Manager.CalcTrait(character.Traits, m.Formula);
                    var growth = Manager.CalcTrait(character.Growths, m.Formula);
                    var adjustment = Manager.CalcTrait(character.Adjustment, m.Formula);
                    character.SetTrait(cKey, @base, growth, adjustment);
                    UpdateBoxText(Children[cKey], cKey, character);
                }
            }
        }

        /// <summary>
        /// 结束编辑时触发的事件
        /// </summary>
        /// <param name="box"></param>
        private void EndEditTraitBox(TextBox box)
        {
            // box 的 DataContext 保存了特点名称
            var key = box.DataContext.ToString();
            // 基础特点值才可以被修改
            (_, int growth, int adjustment) = Character.SplitTraitText(box.Text);
            Manager.Current.SetTraitGrowth(key, growth);
            Manager.Current.SetTraitAdjustment(key, adjustment);
            UpdateBoxText(box, key, Manager.Current);
            // 编辑完毕后, 还要更新角色的属性总计与伤害奖励的数据
            Manager.UpdateSumOfBaseTraits(Manager.Current);
            Manager.UpdateDamageBonus(Manager.Current);
        }

        /// <summary>
        /// 清理多余的列
        /// </summary>
        /// <param name="actualCount"></param>
        public void CleanExcessColumns(int actualCount)
        {
            int childrenCount = MainGrid.Children.Count;
            var actualWidth = MainGrid.ColumnDefinitions[0].Width;
            foreach (var item in MainGrid.ColumnDefinitions)
            {
                item.Width = actualWidth;
            }
            for (int i = childrenCount - actualCount; i > 0; i--)
            {
                var j = actualCount + i - 1;
                var child = MainGrid.Children[j];
                MainGrid.Children.RemoveAt(j);
                MainGrid.ColumnDefinitions.RemoveAt(j);
                var name = child.GetValue(NameProperty)?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                    MainGrid.UnregisterName(name);
            }
        }
    }
}
