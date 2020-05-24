using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using CardWizard.Data;
using CardWizard.View;
using CardWizard.Tools;

namespace CardWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 处理逻辑与数据
        /// </summary>
        public MainManager manager;

        public MainWindow()
        {
            InitializeComponent();
            CommandCreate.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandSave.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandCapture.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            // UI 逻辑处理
            manager = new MainManager(this);
        }

        /// <summary>
        /// 新建
        /// </summary>
        public static RoutedCommand CommandCreate = new RoutedCommand("Create", typeof(MainWindow));
        /// <summary>
        /// 保存
        /// </summary>
        public static RoutedCommand CommandSave = new RoutedCommand("Save", typeof(MainWindow));
        /// <summary>
        /// 生成图像文档
        /// </summary>
        public static RoutedCommand CommandCapture = new RoutedCommand("Capture", typeof(MainWindow));

        /// <summary>
        /// 指令: 保存 触发时执行的动作
        /// </summary>
        public event CommandExcution CommandSaveGestured;
        /// <summary>
        /// 指令: 新建 触发时执行的动作
        /// </summary>
        public event CommandExcution CommandCreateGestured;
        /// <summary>
        /// 指令: 生成图像文档 触发时执行的动作
        /// </summary>
        public event CommandExcution CommandCaptureGestured;



        /// <summary>
        /// 绑定快捷键时使用的委托类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CommandExcution(object sender, ExecutedRoutedEventArgs e);


        private void CommandCreate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandCreateGestured?.Invoke(sender, e);
        }
        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandSaveGestured?.Invoke(sender, e);
        }
        private void CommandCapture_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandCaptureGestured?.Invoke(sender, e);
        }
    }
}
