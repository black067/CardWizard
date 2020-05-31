using CallOfCthulhu;
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
    public partial class CustomRowView : UserControl
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
        /// 新建此控件
        /// </summary>
        public CustomRowView()
        {
            InitializeComponent();
            Children = new Dictionary<string, TextBox>();
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
                var key = keys[i];
                var value = values[i];
                var item = MainGrid.Children[i];
                if (item is FrameworkElement element)
                {
                    element.ToolTip = key;
                }
                if (item is ContentControl ctr)
                {
                    ctr.Content = value;
                }
                else if (item is TextBox box)
                {
                    box.Text = value.ToString();
                    Children.Add(key, box);
                    if (enableEdit)
                    {
                        box.TextChanged += (o, e) =>
                        {
                            if (int.TryParse(box.Text, out int v))
                            {
                                datas[key] = v;
                            }
                        };
                        UIExtension.OnClickSelectAll(box);
                    }
                }
                if (enableEdit)
                {
                    item.IsEnabled = true;
                    item.Focusable = true;
                }
                else
                {
                    item.IsEnabled = false;
                }
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
            bool hasTranslator = translator != null;
            void dosth(UIElement e, int i)
            {
                var item = headers.ElementAt(i);
                if (e is FrameworkElement element)
                {
                    element.Tag = item;
                    element.Cursor = System.Windows.Input.Cursors.Arrow;
                    if (hasTranslator && translator.TryTranslate($"{item}.ToolTip", out var value))
                    {
                        element.ToolTip = value;
                    }
                }
                if (e is TextBox box)
                {
                    box.Text = hasTranslator ? translator.Translate(item, item) : item;
                    box.SetValue(TextBox.IsReadOnlyProperty, true);
                    Children.Add(item, box);
                }
            }
            MainGrid.ForeachChild(dosth, loopLimit: loopLimit);
            CleanExcessColumns(loopLimit);
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
