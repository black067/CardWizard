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
        /// <para>只有在调用了方法: <see cref="InitAsDatas(Dictionary{string, int}, bool)"/> 之后, 才会被赋值</para>
        /// </summary>
        public Dictionary<string, int> Values { get; set; }

        private Dictionary<string, TextBox> Children { get; set; }

        public TraitsViewItem()
        {
            InitializeComponent();
        }

        private MainManager Manager { get; set; }

        /// <summary>
        /// 作为一行数据进行初始化
        /// </summary>
        /// <param name="datas"></param>
        public void InitAsDatas(Dictionary<string, int> datas, bool enableEdit = true)
        {
            Values = datas;
            var values = datas.Values.ToArray();
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
        /// 作为角色的属性面板进行初始化
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="derived"></param>
        public void BindTraits(MainManager manager, bool derived)
        {
            Manager = manager;
            var models = new Dictionary<string, DataModel>(from kvp in Manager.Config.BaseModelDict
                                                           where derived == kvp.Value.Derived
                                                           select kvp);
            var keys = models.Keys.ToArray();
            Children = new Dictionary<string, TextBox>();
            for (int i = 0, len = Math.Min(MainGrid.Children.Count, models.Count); i < len; i++)
            {
                if (MainGrid.Children[i] is TextBox box)
                {
                    // 用 box 的 DataContext 保存其指向的角色特点名称
                    box.DataContext = keys[i];
                    box.LostFocus += (o, e) => EndEditTraitBox(box);
                    Manager.InfoUpdating += c =>
                    {
                        var key = box.DataContext.ToString();
                        box.Text = c.GetTraitText(key);
                    };
                    box.ToolTip = $"{keys[i]} = {models[keys[i]].Formula}";
                    Children.Add(keys[i], box);
                }
            }
            Manager.TraitChanged += C_TraitChanged;
            CleanExcessColumns(keys.Length);
        }

        private void C_TraitChanged(object arg1, Character.TraitChangedEventArgs arg2)
        {
            if (Children.ContainsKey(arg2.Key))
            {
                Children[arg2.Key].Text = Manager.Current.GetTraitText(arg2.Key);
            }
        }

        /// <summary>
        /// 结束编辑时触发的事件
        /// </summary>
        /// <param name="box"></param>
        private void EndEditTraitBox(TextBox box)
        {
            // box 的 DataContext 保存了其指向的角色特点名称
            var key = box.DataContext.ToString();
            (_, int growth, int senescence) = Character.SplitTraitText(box.Text);
            Manager.Current.SetTraitGrowth(key, growth);
            Manager.Current.SetTraitSenescence(key, senescence);

            box.Text = Manager.Current.GetTraitText(key);
            // 编辑完毕后, 还要更新角色的属性总计与伤害奖励的数据
            Manager.UpdateSumOfBaseTraits(Manager.Current);
            Manager.UpdateDamageBonus(Manager.Current);
        }

        /// <summary>
        /// 作为表头初始化
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="localization"></param>
        public void InitAsHeaders(IEnumerable<string> headers, Config.Localization localization)
        {
            var loopLimit = headers.Count();
            void dosth(UIElement e, int i)
            {
                var item = headers.ElementAt(i);
                if (e is FrameworkElement element)
                {
                    element.Tag = item;
                    element.Cursor = System.Windows.Input.Cursors.Arrow;
                }
                if (e is TextBox box)
                {
                    if (localization.TryTranslate(item, out var text))
                    {
                        box.Text = text;
                        box.ToolTip = item;
                    }
                    else
                    {
                        box.Text = item;
                    }
                    box.SetValue(TextBox.IsReadOnlyProperty, true);
                }
            }
            MainGrid.ForeachChild(dosth, loopLimit: loopLimit);
            CleanExcessColumns(loopLimit);
        }

        /// <summary>
        /// 添加提示信息
        /// </summary>
        /// <param name="tooltips"></param>
        public void InitToolTips(IEnumerable<(string k, string v)> pairs)
        {
            var tooltips = new Dictionary<string, string>(from tp in pairs select new KeyValuePair<string, string>(tp.k, tp.v));
            void dosth(UIElement e, int i)
            {
                if (e is FrameworkElement ctr && tooltips.TryGetValue(ctr.Tag?.ToString() ?? string.Empty, out var value))
                {
                    ctr.ToolTip = value;
                }
            }
            MainGrid.ForeachChild(dosth, loopLimit: tooltips.Count);
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
