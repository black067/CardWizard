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

        /// <summary>
        /// 将内容保存为图片
        /// </summary>
        /// <param name="config"></param>
        /// <param name="fileNameWithoutExtension"></param>
        public void CapturePng(Config config, string fileNameWithoutExtension)
        {
            if (config == null) return;
            var paths = config.Paths;
            var translator = config.Translator;

            // 开始截屏前先缓存并更改部分元素的状态
            var recovery = new Dictionary<UIElement, Action<UIElement>>();
            if (HideOnCapturePic != null && HideOnCapturePic.Count != 0)
            {
                foreach (var item in HideOnCapturePic)
                {
                    var origin = item.Visibility;
                    recovery.Add(item, i => i.Visibility = origin);
                    item.Visibility = Visibility.Hidden;
                }
            }
            // 截屏的步骤如下
            void Process(Panel source)
            {
                // 缓存原来的 背景
                var bg = source.GetValue(Panel.BackgroundProperty);
                recovery.Add(source, i => i.SetValue(Panel.BackgroundProperty, bg));
                // 从配置从读取打印时的背景颜色, 作为打印时的背景
                var c = (Color)ColorConverter.ConvertFromString(config.PrintSettings_BackgroundColor);
                source.Background = new SolidColorBrush(c);
                // 缓存原来的 高度
                var originH = source.Height;
                recovery[source] += i => i.SetValue(FrameworkElement.HeightProperty, originH);
                // 将高度设置为配置中指定的高度
                source.Height = source.ActualWidth * config.PrintSettings_H_W_Scale;
                // 等待控件属性的改变生效
                source.UpdateLayout();
                // 期望的 DPI
                var destDpi = config.PrintSettings_Dpi;
                // 查询得出当前的 真实尺寸 与 DPI
                var actualSize = UIExtension.GetActualSize(source);
                var actualDpi = UIExtension.GetDpi(source);
                // 根据 期望的 DPI / 真实的DPI / 真实的尺寸 计算出文件的宽高
                int fileWidth = (int)(actualSize.Width * destDpi / actualDpi);
                int fileHeight = (int)(actualSize.Height * destDpi / actualDpi);
                var fileName = $"{fileNameWithoutExtension}{config.FileExtensionForCardPic}";
                var dest = Path.Combine(paths.PathSave, fileName);
                // 存为图像
                UIExtension.SaveAsPng(source, dest, fileWidth, fileHeight, destDpi, destDpi);
                // 输出日志
                Messenger.EnqueueFormat(translator.Translate("Message.Character.SavedPic", "Saved at {0}"), dest.Replace("\\", "/"));
            };
            // 执行
            Process(MainGrid);
            // 将被更改的元素还原
            foreach (var kvp in recovery)
            {
                kvp.Value.Invoke(kvp.Key);
            }
        }
    }
}
