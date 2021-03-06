﻿namespace CardWizard.View
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
        public List<CharacteristicBox> CharacteristicBoxes { get; set; }

        public CharacteristicViewer()
        {
            InitializeComponent();
            CharacteristicBoxes = (from UIElement e in firstPanel.Children where e is CharacteristicBox select e as CharacteristicBox).ToList();
        }
    }
}
