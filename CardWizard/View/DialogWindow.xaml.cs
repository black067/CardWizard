using System;
using System.Collections.Generic;
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
    /// DialogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow(string message, string title = "", string confirm = "确定", string cancel = "取消")
        {
            InitializeComponent();
            Title = title;
            Message.Content = message;
            if (!string.IsNullOrWhiteSpace(confirm))
                Button_Confirm.Content = confirm;

            if (!string.IsNullOrWhiteSpace(cancel))
            {
                Button_Cancel.Content = cancel;
            }
            else
            {
                Button_Cancel.Visibility = Visibility.Hidden;
                Button_Confirm.SetValue(Grid.ColumnProperty, 1);
            }
            Button_Confirm.Click += Button_Confirm_Click;
            Button_Cancel.Click += Button_Cancel_Click;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        public bool Result;
    }
}
