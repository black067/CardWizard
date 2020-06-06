using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardWizard.View
{
    /// <summary>
    /// BatchInputsPopup.xaml 的交互逻辑
    /// </summary>
    public partial class ValuesEditor : UserControl
    {
        public ValuesEditor()
        {
            InitializeComponent();
            var boxes = new List<TextBox>();
            foreach (UIElement item in wrapPanel.Children)
            {
                if (item is TextBox box)
                {
                    boxes.Add(box);
                }
            }
            boxes.Sort((left, right) => -int.Parse(left.Tag as string).CompareTo(int.Parse(right.Tag as string)));
            FieldBoxes = boxes.ToArray();
            OldValues = new int[FieldBoxes.Length];


            ButtonCancel.Click+= (o, e) =>
            {
                Cancel();
            };
            MouseEnter += ValuesEditor_MouseEnter;
            MouseLeave += ValuesEditor_MouseLeave;
        }

        private void ValuesEditor_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseIn = false;
        }

        private void ValuesEditor_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseIn = true;
        }

        /// <summary>
        /// 鼠标是否在控件内
        /// </summary>
        public bool MouseIn { get; set; }

        /// <summary>
        /// 确认时的回调
        /// </summary>
        public event Action<int[]> ConfirmCallback;

        /// <summary>
        /// 取消时的回调
        /// </summary>
        public event Action CancelCallback;

        public event Action CancelCallbackStatic;

        public void Confirm()
        {
            ConfirmCallback?.Invoke(GetFields());
            ConfirmCallback = null;
        }

        public void Cancel()
        {
            CancelCallbackStatic?.Invoke();
            CancelCallback?.Invoke();
            CancelCallback = null;
            ConfirmCallback = null;
        }
        

        private TextBox[] FieldBoxes { get; set; }

        /// <summary>
        /// 初始值, 用于判断指定的输入框是否被编辑
        /// </summary>
        private int[] OldValues { get; set; }

        /// <summary>
        /// 初始化每个输入框
        /// </summary>
        /// <param name="values"></param>
        public void InitFields(int baseValue, params int[] values)
        {
            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (i < 0 || i >= FieldBoxes.Length) continue;
                FieldBoxes[i].Text = values[i].ToString();
                OldValues[i] = values[i];
            }
        }

        /// <summary>
        /// 查询指定输入框的值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetField(int id)
        {
            if (id < 0 || id >= FieldBoxes.Length) return 0;
            return int.TryParse(FieldBoxes[id].Text, out int r) ? r : 0;
        }

        /// <summary>
        /// 查询所有输入框的值
        /// </summary>
        /// <returns></returns>
        public int[] GetFields()
        {
            var values = new int[FieldBoxes.Length];
            for (int i = values.Length - 1; i >= 0; i--)
            {
                values[i] = GetField(i);
            }
            return values;
        }

        /// <summary>
        /// 判断指定的输入框是否被编辑
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsEdited(int id, int value) => id >= 0 && id < OldValues.Length && OldValues[id] != value;
    }
}
