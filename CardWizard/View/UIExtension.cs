using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;

namespace CardWizard.View
{
    /// <summary>
    /// 拓展 UI 类
    /// </summary>
    public static class UIExtension
    {
        /// <summary>
        /// 取得控件的备份
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(this T graph)
        {
            var doc = XamlWriter.Save(graph);
            return (T)XamlReader.Parse(doc);
        }

        /// <summary>
        /// 转换 UI 元素为文本, 可以用 <see cref="Parse{T}(string)"/> 将文本转换为 UI 元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static string ToXaml<T>(this T graph)
        {
            return XamlWriter.Save(graph);
        }

        /// <summary>
        /// 将文本转换为 UI 元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xamlText"></param>
        /// <returns></returns>
        public static T Parse<T>(string xamlText)
        {
            return (T)XamlReader.Parse(xamlText);
        }

        /// <summary>
        /// 向指定的列表框中添加子项目
        /// </summary>
        /// <param name="box"></param>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T AddItem<T>(this ListBox box, T template, string name) where T : UIElement, new()
        {
            T item = template != null ? UIExtension.DeepCopy(template) : new T();
            if (box == null) throw new NullReferenceException();
            box.Items.Add(item);
            box.RegisterName(name, item);
            item.Visibility = Visibility.Visible;
            return item;
        }

        /// <summary>
        /// 遍历每个子控件(<see cref="UIElement"/>)
        /// <para><paramref name="dosth"/> : 返回值表示是否继续循环</para>
        /// <para><paramref name="loopLimit"/> : 表示最大循环次数(实际循环次数必定小于这个值)</para>
        /// <para><paramref name="reverse"/> : 是否逆序循环</para>
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="dosth"></param>
        /// <param name="loopLimit"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static T ForeachChild<T>(this T panel, Func<UIElement, int, bool> dosth,
                                        int loopLimit = int.MaxValue, bool reverse = false) where T : Panel
        {
            if (panel == null) throw new NullReferenceException();
            if (reverse)
            {
                for (int i = Math.Min(panel.Children.Count, loopLimit) - 1; i >= 0; i--)
                {
                    var @continue = dosth.Invoke(panel.Children[i], i);
                    if (!@continue) break;
                }
                return panel;
            }
            for (int i = 0, len = Math.Min(panel.Children.Count, loopLimit); i < len; i++)
            {
                var @continue = dosth.Invoke(panel.Children[i], i);
                if (!@continue) break;
            }
            return panel;
        }

        /// <summary>
        /// 遍历每个子控件(<see cref="UIElement"/>)
        /// <para><paramref name="dosth"/> : 返回值表示是否继续循环</para>
        /// <para><paramref name="loopLimit"/> : 表示最大循环次数(实际循环次数必定小于等于这个值)</para>
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="dosth"></param>
        /// <param name="loopLimit"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static T ForeachChild<T>(this T panel, Action<UIElement, int> dosth,
                                        int loopLimit = int.MaxValue, bool reverse = false) where T : Panel
        {
            if (panel == null) throw new NullReferenceException();
            if (reverse)
            {
                for (int i = Math.Min(panel.Children.Count, loopLimit) - 1; i >= 0; i--)
                {
                    dosth.Invoke(panel.Children[i], i);
                }
                return panel;
            }
            for (int i = 0, len = Math.Min(panel.Children.Count, loopLimit); i < len; i++)
            {
                dosth.Invoke(panel.Children[i], i);
            }
            return panel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static System.Drawing.Size GetActualSize(FrameworkElement element)
        {
            if (element == null) return new System.Drawing.Size(0, 0);
            var pxWidth = (int)element.ActualWidth;
            var pxHeight = (int)element.ActualHeight;
            return new System.Drawing.Size(pxWidth, pxHeight);
        }

        /// <summary>
        /// 取得控件的 DPI
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static double GetDpi(Visual visual)
        {
            PresentationSource source = PresentationSource.FromVisual(visual);
            return 96 * (source?.CompositionTarget.TransformToDevice.M11 ?? 1);
        }

        /// <summary>
        /// 将控件保存为 png
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="filePath"></param>
        /// <param name="pxWidth"></param>
        /// <param name="pxHeight"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        public static void CapturePng(Visual visual, string filePath, int pxWidth, int pxHeight, double dpiX, double dpiY)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(pxWidth, pxHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(visual);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using Stream fileStream = File.Create(filePath);
            pngImage.Save(fileStream);
        }

        /// <summary>
        /// 载入图片资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<BitmapImage> LoadBitmapImage(string path)
        {
            var buffer = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            using MemoryStream stream = new MemoryStream(buffer);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        /// <summary>
        /// 取得 <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(this Image image)
        {
            if (image == null) throw new NullReferenceException();
            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        /// <summary>
        /// 将图片缩小到指定的长宽
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image ZoomIn(this Image bitmap, double width, double height)
        {
            if (bitmap == null) throw new NullReferenceException();
            if (height <= 0)
            {
                height = width / (1.0 * bitmap.Width / bitmap.Height);
            }
            if (width <= 0)
            {
                width = height * 1.0 * bitmap.Width / bitmap.Height;
            }
            return bitmap.GetThumbnailImage((int)width, (int)height, () => false, IntPtr.Zero);
        }

        /// <summary>
        /// 将图像转换为 WindowsIcon
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Icon ToIcon(this Bitmap bitmap)
        {
            if (bitmap == null) throw new NullReferenceException();
            using MemoryStream bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream, ImageFormat.Png);

            using var iconStream = new MemoryStream();
            using var writer = new BinaryWriter(iconStream);
            // 写图标头部
            // 0-1保留
            writer.Write((short)0);
            // 2-3文件类型。1=图标, 2=光标
            writer.Write((short)1);
            // 4-5图像数量（图标可以包含多个图像）
            writer.Write((short)1);
            // 6图标宽度 7图标高度
            writer.Write((byte)bitmap.Width);
            writer.Write((byte)bitmap.Height);
            // 8颜色数 (若像素位深 >= 8，填 0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示)
            writer.Write((byte)0);
            //9保留。必须为0
            writer.Write((byte)0);
            // 10-11调色板
            writer.Write((short)0);
            // 12-13位深
            writer.Write((short)32);
            // 14-17位图数据大小
            writer.Write((int)bitmapStream.Length);
            // 18-21位图数据起始字节
            writer.Write(22);
            // 写入图像数据
            writer.Write(bitmapStream.ToArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            return new Icon(iconStream);
        }
    }
}
