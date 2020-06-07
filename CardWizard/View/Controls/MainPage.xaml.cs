using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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

        public List<UIElement> HideOnCapturePic { get; set; } = new List<UIElement>();
    }
}
