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

        public List<UIElement> HideOnCapturePic { get; set; }

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
            var cache = new Dictionary<UIElement, Action>();
            foreach (var item in HideOnCapturePic)
            {
                var origin = item.Visibility;
                cache.Add(item, () => item.Visibility = origin);
                item.Visibility = Visibility.Hidden;
            }

            var bg = MainGrid.Background;
            // 开始截屏
            MainGrid.Process(source =>
            {
                var c = (Color)ColorConverter.ConvertFromString(config.PrintSettings_BackgroundColor);
                source.Background = new SolidColorBrush(c);
                var originH = source.Height;
                cache.Add(source, () => source.Height = originH);
                source.Height = source.ActualWidth * config.PrintSettings_H_W_Scale;
                // 等待控件属性的变化刷新
                source.UpdateLayout();
                // 期望的 DPI
                var destDpi = config.PrintSettings_Dpi;
                // 查询得出当前的 真实尺寸 与 DPI
                var actualSize = UIExtension.GetActualSize(source);
                var actualDpi = UIExtension.GetDpi(source);
                int fileWidth = (int)(actualSize.Width * destDpi / actualDpi);
                int fileHeight = (int)(actualSize.Height * destDpi / actualDpi);
                var fileName = $"{fileNameWithoutExtension}{config.FileExtensionForCardPic}";
                var dest = Path.Combine(paths.PathSave, fileName);
                UIExtension.SaveAsPng(source, dest, fileWidth, fileHeight, destDpi, destDpi);
                Messenger.EnqueueFormat(translator.Translate("Message.Character.SavedPic", "Saved at {0}"), dest.Replace("\\", "/"));
            });
            // 将被更改的元素还原
            MainGrid.Background = bg;
            foreach (var restore in cache.Values)
            {
                restore();
            }
        }
    }
}
