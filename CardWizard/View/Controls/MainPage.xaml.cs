using CallOfCthulhu;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using CardWizard.Tools;
using CardWizard.Data;

namespace CardWizard.View
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl
    {
        /// <summary>
        /// 构造新窗口
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置伤害奖励的显示
        /// </summary>
        /// <param name="damageBonus"></param>
        /// <param name="build"></param>
        public void SetDamageBonus(object damageBonus, int build)
        {
            Value_DamageBonus.Content = damageBonus;
            Value_Build.Content = build;
        }

        public List<UIElement> HideOnCapturePic { get; set; } = new List<UIElement>();

    }
}
