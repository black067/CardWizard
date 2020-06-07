namespace CardWizard.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Segment = CallOfCthulhu.Characteristic.Segment;

    /// <summary>
    /// CharacteristicViewer.xaml 的交互逻辑
    /// </summary>
    public partial class CharacteristicViewer : UserControl
    {
        /// <summary>
        /// 受管理的属性显示器
        /// </summary>
        public List<CharacteristicBox> Boxes { get; set; }

        public CharacteristicViewer()
        {
            InitializeComponent();
            var boxes = (from UIElement e in firstPanel.Children where e is CharacteristicBox select e as CharacteristicBox).ToList();
            boxes.AddRange(from UIElement e in secondPanel.Children where e is CharacteristicBox select e as CharacteristicBox);
            Boxes = boxes;
            XValueEditor.EditorPopup = XValueEditorPopup;
            XValueEditor.SetTags("Initial", "Adjustment", "Growth");

        }

        public void OpenEditor(object sender, EventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            var box = sender as CharacteristicBox;
            var target = box.TargetGetter();
            XValueEditor.Show(0, box.ValueInitial, box.ValueAdjustment, box.ValueGrowth);
            XValueEditor.ConfirmCallback += (values) =>
            {
                for (int i = 0; i < 3; i++)
                {
                    if (XValueEditor.IsEdited(i, values[i]))
                    {
                        target.SetCharacteristic(box.Key, (Segment)i, values[i]);
                    }
                }
            };
        }
    }
}
