using CallOfCthulhu;
using CardWizard.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CardWizard.View
{
    /// <summary>
    /// ValuesEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ValuesEditor : UserControl
    {
        private Popup editorPopup;

        public Popup EditorPopup
        {
            get => editorPopup;
            set
            {
                if (value != editorPopup)
                {
                    if (editorPopup != null)
                    {
                        editorPopup.Opened -= OnPopupOpened;
                        editorPopup.Closed -= OnPopupClosed;
                    }
                    editorPopup = value ?? throw new ArgumentNullException(nameof(value));
                    editorPopup.Opened += OnPopupOpened;
                    editorPopup.Closed += OnPopupClosed;
                }
            }
        }

        public ValuesEditor()
        {
            InitializeComponent();
            var boxes = new List<TextBox>();
            foreach (UIElement item in wrapPanel.Children)
            {
                if (item is TextBox box)
                {
                    boxes.Add(box);
                    box.LostFocus += Box_LostFocus;
                }
            }
            boxes.Sort((left, right) => int.Parse(left.Tag as string).CompareTo(int.Parse(right.Tag as string)));
            FieldBoxes = boxes.ToArray();
            OldValues = new int[FieldBoxes.Length + 1];
            ButtonConfirm.Click += ButtonConfirm_Click;
            ButtonCancel.Click += ButtonCancel_Click;
            ButtonClear.Click += ButtonClear_Click;

            UIExtension.AddCommandsBindings(this, new RoutedCommand("enterpress", typeof(ValuesEditor)), (o, e) =>
            {
                Confirm();
            }, new KeyGesture(Key.Enter));
            UIExtension.AddCommandsBindings(this, new RoutedCommand("escapepress", typeof(ValuesEditor)), (o, e) =>
            {
                Cancel();
            }, new KeyGesture(Key.Escape));
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            FieldB.Text = "0";
            FieldC.Text = "0";
            LabelSum.Content = GetFields().Sum() + OldValues.Last();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
            if (EditorPopup != null) EditorPopup.IsOpen = false;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        private void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            LabelSum.Content = GetFields().Sum() + OldValues.Last();
        }

        /// <summary>
        /// 气泡浮现时执行的事件
        /// </summary>
        public event EventHandler PopupOpened;

        private void OnPopupOpened(object o, EventArgs e) => PopupOpened?.Invoke(o, e);

        /// <summary>
        /// 气泡关闭时执行的事件
        /// </summary>
        public event EventHandler PopupClosed;
        private void OnPopupClosed(object o, EventArgs e) => PopupClosed?.Invoke(o, e);

        /// <summary>
        /// 确认时的回调, 执行完后会被清空
        /// </summary>
        public event Action<int[]> ConfirmClick;

        /// <summary>
        /// 每次取消时的回调, 执行完后会被清空
        /// </summary>
        public event Action CancelClick;

        /// <summary>
        /// 取消时的回调, 不会被清空
        /// </summary>
        public event Action CancelClickStatic;

        public void Confirm()
        {
            ConfirmClick?.Invoke(GetFields());
            if (EditorPopup != null) EditorPopup.IsOpen = false;
        }

        public void Cancel()
        {
            CancelClickStatic?.Invoke();
            CancelClick?.Invoke();
            if (EditorPopup != null) EditorPopup.IsOpen = false;
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
        public void Show(int baseValue, params int[] values)
        {
            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (i < 0 || i >= FieldBoxes.Length) continue;
                FieldBoxes[i].Text = values[i].ToString();
                OldValues[i] = values[i];
            }
            OldValues[OldValues.Length - 1] = baseValue;
            LabelSum.Content = OldValues.Sum();
            CancelClick = null;
            ConfirmClick = null;
            EditorPopup.IsOpen = false;
            EditorPopup.IsOpen = true;
        }

        /// <summary>
        /// 设置范围的显示
        /// </summary>
        /// <param name="basevalue"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="translator"></param>
        public void SetRangeTip(int basevalue, int lower, int upper, Translator translator)
        {
            if (translator == null) return;
            string message;
            if (upper != lower)
            {
                message = translator.Translate("ValuesEditor.ToolTip.BaseAndRange", "基础值: {0}, 合理范围: {1} ~ {2}");
            }
            else
            {
                message = translator.Translate("ValuesEditor.ToolTip.BaseOnly", "基础值: {0}");
            }
            message = string.Format(message, basevalue, lower, upper);
            LabelMoreInfo.AddOrSetToolTip(message, (Style)App.Current.FindResource("XToolTip"));
        }

        /// <summary>
        /// 设置每个输入框标签的 Tag
        /// </summary>
        /// <param name="tags"></param>
        public void SetTags(params string[] tags)
        {
            var labels = new Label[] { LabelA, LabelB, LabelC };
            for (int i = 0, length = labels.Length, max = tags.Length; i < length && i < max; i++)
            {

                labels[i].Tag = tags[i];
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
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsEdited(int index, int value) => index >= 0 && index < FieldBoxes.Length && OldValues[index] != value;
    }
}
