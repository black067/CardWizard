using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections;
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
using System.Windows.Shapes;

namespace CardWizard.View
{
    /// <summary>
    /// OccupationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OccupationWindow : Window
    {
        /// <summary>
        /// 翻译器
        /// </summary>
        private Translator Translator { get; set; }

        /// <summary>
        /// 当前选中的职业
        /// </summary>
        public Occupation Selection { get; set; }

        /// <summary>
        /// 当前的焦点控件
        /// </summary>
        private TextBlock FocusPoint { get; set; }

        /// <summary>
        /// 被选中的 TextBlock 的 tag
        /// </summary>
        private string tagForSelected { get; set; }

        public OccupationWindow(IEnumerable<Occupation> datas = null, Translator translator = null)
        {
            InitializeComponent();
            Button_Cancel.Click += Button_Cancel_Click;
            Button_Confirm.Click += Button_Confirm_Click;
            tagForSelected = (string)FindResource("TagForSelected");
            Translator = translator;
            bool hasTranslator = Translator != null;
            var getters = Translator?.ToKeywordsMap();
            if (datas != null)
            {
                MainPanel.ClearAllChildren();
                int titleFontSize = 18;
                var style = (Style)FindResource("OBlock");
                foreach (Setter item in style.Setters)
                {
                    if (item.Property.Equals("FontSize")) { titleFontSize = Convert.ToInt32(item.Value) + 4; }
                }
                foreach (var item in datas)
                {
                    var inlines = ConvertOccupation(item, titleFontSize);
                    if (hasTranslator)
                    {
                        foreach (var inline in inlines)
                        {
                            if (inline is Run run)
                            {
                                run.Text = Translator.MapKeywords(run.Text, getters);
                            }
                        }
                    }
                    var block = UIExtension.AddBlock(MainPanel, item.Name, style, inlines);
                    block.BeginInit();
                    block.MouseDown += Block_MouseClick;
                    block.DataContext = item;
                    block.EndInit();
                }
            }
        }

        private void Block_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock block && block.DataContext is Occupation item)
            {
                Selection = item;
                block.Tag = tagForSelected;
                if (FocusPoint != null && FocusPoint != block)
                {
                    FocusPoint.Tag = string.Empty;
                }
                FocusPoint = block;
            }
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 将职业转化为字符元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<TextElement> ConvertOccupation(Occupation item, int titleFontSize = 14)
        {
            var titleStyle = $"# {{ FontSize: {titleFontSize}, FontWeight: Bold }}";
            var context = @$"
{item.Name} {titleStyle}
{item.Description}
{{{nameof(Occupation.CreditRatingRange)}}} {titleStyle}
{item.CreditRatingRange}
{{{nameof(Occupation.Skills)}}} {titleStyle}
{item.Skills.Select(str => str.SplitRemoveEmpty('#')[0]).CombineToString(", ", null)}
{{{nameof(Occupation)}.{nameof(Occupation.PointFormula)}}} {titleStyle}
{item.PointFormula}
";
            var inlines = UIExtension.ResolveTextElements(context.Trim());
            return inlines;
        }
    }
}
