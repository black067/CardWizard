using CardWizard.Data;
using CardWizard.Tools;
using CardWizard.View;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AppResources = CardWizard.Properties.Resources;

namespace CardWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CommandCreate.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandSave.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandCapture.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            CommandConfirm.InputGestures.Add(new KeyGesture(Key.Enter));
            MouseDown += MainWindow_MouseDown;

            var fileConfig = AppResources.FileConfig;
            // 如果项目路径下存在文件"DEBUG", 就执行以下操作
            if (File.Exists("DEBUG"))
            {
                YamlKit.SaveFile(fileConfig, new Config());
            }
            // 读取配置表
            var config = YamlKit.LoadFile<Config>(fileConfig).Process();
            // 关闭窗口的时候保存配置表
            Closing += (o, e) =>
            {
                YamlKit.SaveFile(fileConfig, config);
            };
            // 将动态链接库的目录添加到 PATH
            AddEnvironmentPaths(config.Paths.PathLibs);
            // UI 逻辑处理
            _ = new MainManager(this, config);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (Keyboard.FocusedElement is TextBox)
        }

        /// <summary>
        /// 将目录添加到环境变量中
        /// </summary>
        /// <param name="paths"></param>
        static void AddEnvironmentPaths(params string[] paths)
        {
            var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };

            string newPath = string.Join(Path.PathSeparator.ToString(), path.Concat(paths));

            Environment.SetEnvironmentVariable("PATH", newPath);
        }
        /// <summary>
        /// 新建
        /// </summary>
        public static RoutedCommand CommandCreate { get; set; } = new RoutedCommand("Create", typeof(Window));
        /// <summary>
        /// 指令: 新建 触发时执行的动作
        /// </summary>
        public event RoutedEventHandler CommandCreateGestured;
        private void CommandCreate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandCreateGestured?.Invoke(sender, e);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public static RoutedCommand CommandSave { get; set; } = new RoutedCommand("Save", typeof(Window));
        /// <summary>
        /// 指令: 保存 触发时执行的动作
        /// </summary>
        public event RoutedEventHandler CommandSaveGestured;
        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandSaveGestured?.Invoke(sender, e);
        }

        /// <summary>
        /// 生成图像文档
        /// </summary>
        public static RoutedCommand CommandCapture { get; set; } = new RoutedCommand("Capture", typeof(Window));
        /// <summary>
        /// 指令: 生成图像文档 触发时执行的动作
        /// </summary>
        public event RoutedEventHandler CommandCaptureGestured;
        private void CommandCapture_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandCaptureGestured?.Invoke(sender, e);
        }

        /// <summary>
        /// 切换提示浮窗的显示与关闭
        /// </summary>
        public static RoutedCommand CommandSwitchToolTip { get; set; } = new RoutedCommand("SwitchToolTip", typeof(Window));
        /// <summary>
        /// 指令: 切换提示浮窗的显示与关闭 触发时执行的动作
        /// </summary>
        public event RoutedEventHandler CommandSwitchToolTipGestured;
        private void CommandShowToolTip_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandSwitchToolTipGestured?.Invoke(sender, e);
        }

        /// <summary>
        /// 按下 Enter
        /// </summary>
        public static RoutedCommand CommandConfirm { get; set; } = new RoutedCommand("Confirm", typeof(Window));
        /// <summary>
        /// 指令: 按下 Enter 触发时执行的动作
        /// </summary>
        public event RoutedEventHandler CommandConfirmGestured;
        private void CommandConfirm_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandConfirmGestured?.Invoke(sender, e);
        }
    }
}
