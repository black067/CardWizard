﻿namespace CardWizard.View
{
    using CallOfCthulhu;
    using CardWizard.Data;
    using CardWizard.Properties;
    using CardWizard.Tools;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    /// <summary>
    /// <see cref="MainWindow"/> 的控制器
    /// </summary>
    public sealed class MainManager
    {
        /// <summary>
        /// 管理的窗口主体
        /// </summary>
        public MainWindow Window { get; private set; }

        /// <summary>
        /// 调查员文档的正面
        /// </summary>
        public MainPage IMainPage { get => Window.MainPage; }

        /// <summary>
        /// 调查员文档的背面
        /// </summary>
        public BackstoryPage IBackstoryPage { get => Window.BackstoryPage; }

        public List<UIElement> HideOnCapturing { get; set; } = new List<UIElement>();

        /// <summary>
        /// 配置表
        /// </summary>
        public Config Config { get; set; }

        /// <summary>
        /// 本地化文本
        /// </summary>
        public Translator Translator { get => Config.Translator; }

        /// <summary>
        /// 路径管理器
        /// </summary>
        public Paths Paths { get => Config.Paths; }

        /// <summary>
        /// 垃圾回收的时钟
        /// </summary>
        private DispatcherTimer GCDispatcher { get; set; }

        /// <summary>
        /// 数据总线
        /// </summary>
        public DataBus DataBus { get; set; }

        /// <summary>
        /// lua 执行工具
        /// </summary>
        public ScriptHub LuaHub { get; set; }

        /// <summary>
        /// 刷新角色卡牌信息的事件
        /// </summary>
        public event Action<Character> InfoUpdating;

        /// <summary>
        /// 刷新角色卡牌信息时执行完毕的事件
        /// </summary>
        public event Action<Character> InfoUpdated;

        /// <summary>
        /// 刷新与此角色相关的控件的值
        /// </summary>
        /// <param name="character"></param>
        private void OnInfoUpdate(Character character)
        {
            InfoUpdating?.Invoke(character);
            InfoUpdated?.Invoke(character);
        }

        /// <summary>
        /// 角色的背景信息被改变时, 触发的事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 用于绑定到角色: 角色的背景信息被改变时, 触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) => PropertyChanged?.Invoke(sender, args);

        /// <summary>
        /// 角色的属性值被改变时, 触发的事件
        /// </summary>
        public event CharacteristicChangedEventHandler CharacteristicChanged;

        /// <summary>
        /// 用于绑定到角色: 角色的属性值被改变时, 触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCharacteristicChanged(Character sender, CharacteristicChangedEventArgs args) => CharacteristicChanged?.Invoke(sender, args);

        /// <summary>
        /// 当前角色技能数值变动时出发的事件
        /// </summary>
        public event SkillChangedEventHandler SkillChanged;

        /// <summary>
        /// 当前角色技能数值变动时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSkillChanged(Character sender, SkillChangedEventArgs args) => SkillChanged?.Invoke(sender, args);

        /// <summary>
        /// 已载入的所有角色
        /// </summary>
        public ObservableCollection<Character> Characters { get; set; }

        /// <summary>
        /// 随机数生成器
        /// </summary>
        public static Random Die { get; set; } = new Random(Guid.NewGuid().GetHashCode() % 100);

        /// <summary>
        /// 骰点
        /// </summary>
        /// <param name="count"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        private static int Roll(int count, int upper)
        {
            int sum = 0;
            for (int i = count; i > 0; i--)
            {
                sum += Die.Next(1, upper + 1);
            }
            return sum;
        }

        /// <summary>
        /// 是否显示提示信息
        /// </summary>
        public static double ToolTipOpacity { get; set; }

        /// <summary>
        /// 角色与其档案文件
        /// </summary>
        private Dictionary<Character, string> CharacterFiles { get; set; } = new Dictionary<Character, string>();

        private Character current;

        /// <summary>
        /// 当前查看/编辑的角色
        /// </summary>
        public Character Current
        {
            get => current;
            set
            {
                if (current != value)
                {
                    current?.ClearObservers();
                    if (value != null)
                    {
                        value.CharacteristicChanged += OnCharacteristicChanged;
                        value.SkillChanged += OnSkillChanged;
                        value.PropertyChanged += OnPropertyChanged;
                    }
                }
                current = value;
            }
        }

        /// <summary>
        /// 方法: 取得当前查看/编辑的角色
        /// </summary>
        /// <returns></returns>
        public Character CurrentGetter() => current;

        /// <summary>
        /// 创建主窗口管理器
        /// </summary>
        /// <param name="window"></param>
        public MainManager(MainWindow window, Config config)
        {
            Window = window ?? throw new ArgumentNullException(nameof(window));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            // 如果消息管理器中已经有消息, 将其打印
            if (!string.IsNullOrWhiteSpace(Messenger.Peek))
            {
                Messenger_OnEnqueue(Messenger.DequeueAll());
            }
            // 设置 Logger
            Messenger.OnEnqueue += Messenger_OnEnqueue;
            // 设置定期 Garbage Collect
            GCDispatcher = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, Config.GCInterval),
                Tag = $"{nameof(GC)}.{nameof(GC.Collect)}",
            };
            GCDispatcher.Start();
            // 根据 Config.PathosDatabase 创建目录
            Paths.CreateDirectories();
            // 初始化数据总线
            DataBus = new DataBus();
            DataBus.LoadFrom(Paths.PathData);
            if (DataBus.Weapons.Count == 0)
            {
                DataBus.GenerateDefaultWeapons();
                YamlKit.SaveFile(Paths.FileWeapons, DataBus.Weapons.Values);
            }
            if (DataBus.Occupations.Count == 0)
            {
                DataBus.GenerateDefaultOccupations();
                YamlKit.SaveFile(Paths.FileOccupations, DataBus.Occupations.Values);
            }
            // 创建 Lua 环境
            InitLuaHub();
            // 如果存在存储了计算公式的脚本文件, 执行
            LuaHub.DoFile(Paths.ScriptFormula, "FORMULA", isGlobal: true);
            // 创建必要的文件夹
            Directory.CreateDirectory(Paths.PathSave);
            // 载入角色数据
            Characters = new ObservableCollection<Character>();
            var files = Directory.GetFiles(Paths.PathSave);
            foreach (var item in files)
            {
                if (!item.EndsWith(Config.FileExtensionForCard, StringComparison.OrdinalIgnoreCase))
                    continue;
                var c = YamlKit.LoadFile<Character>(item);
                if (c == default(Character)) continue;
                AddToCharacters(c);
                CharacterFiles.Add(c, item);
            }
            if (Characters.Count != 0)
            {
                Current = Characters.First();
            }
            else
            {
                GetHandlerForReGenerateCharacteristics(() =>
                {
                    Current = Character.Create(Config.BaseModelDict);
                    return Current;
                }, null).Invoke(null, null);
            }
            #region 设置界面的交互逻辑
            // 绑定各种事件
            Window.CommandCreateGestured += GetHandlerForReGenerateCharacteristics(() => Character.Create(Config.BaseModelDict), Window);
            Window.CommandSaveGestured += DoSave;
            //Window.Button_Debug.Click += DoDebug;
            Window.CommandCaptureGestured += DoCapturePicture;
            Window.CommandSwitchToolTipGestured += DoSwitchToolTip;
            Window.CommandConfirmGestured += DoMoveFocus;
            Window.MouseWheel += DoCatchMouseWheel;
            // ToolTip 的显示
            ToolTipOpacity = Config.ToolTipAvailable ? Config.ToolTipOpacity : 0;
            Window.MenuItemSwitchToolTip.IsChecked = Config.ToolTipAvailable;
            Window.MenuItemDebug.Click += (o, _) =>
            {
                try
                {
                    LuaHub.DoFile(Paths.ScriptDebug);
                }
                catch (Exception e)
                {
                    Messenger.Enqueue(e);
                }
            };
            // 可查看的角色卡列表初始化
            InitializeCardList(Window.List_Cards);
            // 角色文档初始化
            InitializeMainPage(IMainPage);
            InitializeBackstoryPage(Window.BackstoryPage);
            // 初始化所有公式计算器
            foreach (var item in FormulaCalculator.FormulaCalculators)
            {
                item.Calculator = CalcCharacteristic;
                item.CharacterGetter = CurrentGetter;
            }
            #endregion
            // 刷新界面
            OnInfoUpdate(Current);
            // 如果存在启动脚本文件, 执行
            LuaHub.DoFile(Paths.ScriptStartup, "STARTUP", isGlobal: true);
            // 对界面进行本地化
            Localize(Window, Translator);
            // 垃圾回收
            GC.Collect();
        }


        #region Button Click Handler

        /// <summary>
        /// 按下回车时
        /// <para>如果当前的键盘焦点是 <see cref="TextBox"/> 且 <see cref="TextBox.MaxLines"/> 为 1, 则将焦点移向下一个元素</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoMoveFocus(object sender, RoutedEventArgs e)
        {
            var element = Keyboard.FocusedElement;
            if (element == null) return;
            if (element is TextBox textBox && textBox.MaxLines < 2)
            {
                //Window.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void DoCatchMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox box)
            {
                e.Handled = true;
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = "0";
                    return;
                }
                if (int.TryParse(box.Text, out int value))
                {
                    value += e.Delta > 0 ? 1 : -1;
                    box.Text = value.ToString();
                }
            }
        }

        /// <summary>
        /// 保存按钮点击时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoSave(object sender, RoutedEventArgs e)
        {
            ClearTextBoxFocus();

            if (!CharacterFiles.TryGetValue(Current, out string dest))
            {
                dest = Path.Combine(Paths.PathSave, Current.Name + Config.FileExtensionForCard);
                if (File.Exists(dest))
                {
                    var d = new SaveFileDialog()
                    {
                        CheckFileExists = true,
                        DefaultExt = Config.FileExtensionForCard,
                        AddExtension = true,
                        Title = "Save As..."
                    };
                    d.ShowDialog(Window);
                    if (!string.IsNullOrWhiteSpace(d.FileName))
                    {
                        dest = d.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            foreach (var skill in Current.Skills.ToArray())
            {
                if (skill.GetPointsTotal() == 0) Current.Skills.Remove(skill);
            }
            YamlKit.SaveFile(dest, Current);
            var message = Translator.Translate("Message.Character.Saved", "Saved at {0}");
            Messenger.EnqueueFormat(message, dest.Replace("\\", "/"));
            CharacterFiles[Current] = dest;
        }

        /// <summary>
        /// 清除当前在 TextBox 上的焦点
        /// </summary>
        public static void ClearTextBoxFocus()
        {
            if (Keyboard.FocusedElement is TextBox textBox)
            {
                // Move to a parent that can take focus
                FrameworkElement parent = (FrameworkElement)textBox.Parent;
                while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
                {
                    parent = (FrameworkElement)parent.Parent;
                }

                DependencyObject scope = FocusManager.GetFocusScope(textBox);
                FocusManager.SetFocusedElement(scope, parent as IInputElement);
            }
        }

        /// <summary>
        /// 生成图片时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoCapturePicture(object sender, RoutedEventArgs e)
        {
            if (Window.TabItem_Front.IsSelected)
                IMainPage.CapturePng(Config, Current.Name + ".Main", HideOnCapturing);
            else
                IBackstoryPage.CapturePng(Config, Current.Name + ".Back", HideOnCapturing);
        }

        /// <summary>
        /// 切换提示信息的显示与隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoSwitchToolTip(object sender, RoutedEventArgs e)
        {
            Config.ToolTipAvailable = !Config.ToolTipAvailable;
            if (Config.ToolTipAvailable)
            {
                ToolTipOpacity = Config.ToolTipOpacity;
            }
            else
            {
                ToolTipOpacity = 0;
            }
            Window.MenuItemSwitchToolTip.IsChecked = Config.ToolTipAvailable;
        }

        /// <summary>
        /// Debug 按钮点击时触发的事件
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DoDebug(object o, RoutedEventArgs e)
        {
            if (File.Exists(Paths.ScriptDebug))
            {
                try
                {
                    LuaHub.DoFile(Paths.ScriptDebug, isGlobal: false);
                }
                catch (Exception exception)
                {
                    Messenger.EnqueueFormat("{0}\n{1}", exception.Message, exception.StackTrace);
                    System.Diagnostics.Process.Start("explorer.exe", Paths.ScriptDebug.Replace("/", "\\"));
                }
            }
        }

        /// <summary>
        /// 玩家点击头像时触发的事件 (导入新的头像)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DoImportAvatar(object o, System.Windows.Input.MouseButtonEventArgs e)
        {
            var openFiledialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                CheckFileExists = true,
                Multiselect = false,
                DefaultExt = ".png",
                Title = Translator.Translate("Dialog.Import.Avatar.Title", "选择调查员的照片"),
            };
            openFiledialog.ShowDialog(Window);
            var result = openFiledialog.FileName;
            if (string.IsNullOrWhiteSpace(result) || !File.Exists(result)) return;
            ImportAvatar(result);
        }

        private void OnAvatarImageDrop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((System.Array)data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                if (file.EndsWith(".png"))
                {
                    ImportAvatar(file);
                }
                else
                {
                    var fmt = Translator.Translate("Message.Avatar.FileNotSupported", "不支持此文件格式: {0}, 目前只支持 {1} 图片.");
                    Messenger.EnqueueFormat(fmt, Path.GetExtension(file), "png");
                }
            }
        }

        private void ImportAvatar(string source)
        {
            var ext = new FileInfo(source).Extension;
            var dest = $"{Paths.PathSave}/{Current.Name}{ext}";
            // 如果文件已经存在, 要弹出确认窗口
            if (File.Exists(dest))
            {
                var message = Translator.Translate("Dialog.Overwrite.Confirmation", "是否确定用\n{0}\n覆盖现有文件?");
                message = string.Format(message, source);
                var confirmDialog = new DialogWindow(message);
                confirmDialog.ShowDialog(Window);
                if (!(bool)confirmDialog.DialogResult)
                    return;
            }
            File.Copy(source, dest, true);
            AvatarUpdate(Current, IMainPage.Image_Avatar);
        }

        /// <summary>
        /// 更新角色头像的显示
        /// </summary>
        /// <param name="c"></param>
        /// <param name="image"></param>
        private async void AvatarUpdate(Character c, Image image)
        {
            if (image.Source is BitmapImage b)
            {
                await b.StreamSource.DisposeAsync();
                b.StreamSource.Close();
            }

            var path = $"{Paths.PathSave}/{c.Name}.png";
            using var rawImage = !File.Exists(path) ? Resources.Image_Avatar_Empty : System.Drawing.Image.FromFile(path);
            image.Source = rawImage.ToBitmapImage();
        }
        #endregion

        #region Initialize Document Pages
        /// <summary>
        /// 角色主页面的初始化
        /// </summary>
        /// <param name="page"></param>
        private void InitializeMainPage(MainPage page)
        {
            if (page is null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.MouseWheel += DoCatchMouseWheel;
            // 初始化数值编辑器
            Window.XValueEditor.EditorPopup = Window.XValueEditorPopup;
            Window.XValueEditor.SetTags("Initial", "Adjustment", "Growth");
            void OpenEditor(object sender, EventArgs e)
            {
                if (sender == null) throw new ArgumentNullException(nameof(sender));
                var box = sender as CharacteristicBox;
                var target = box.TargetGetter();
                Window.XValueEditor.Show(0, box.ValueInitial, box.ValueAdjustment, box.ValueGrowth);
                Window.XValueEditor.ConfirmClick += (values) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (Window.XValueEditor.IsEdited(i, values[i]))
                        {
                            target.SetCharacteristic(box.Key, (Characteristic.Segment)i, values[i]);
                        }
                    }
                };
            }
            // 初始化杂项信息显示面板
            page.Miscellaneous.InitializeControl(this);
            // 属性重生成按钮
            page.Button_Regenerate.Click += GetHandlerForReGenerateCharacteristics(CurrentGetter, Window);
            page.CharacteristicBoxes.CharacteristicBoxes.Add(page.Box_Dodge);
            foreach (var box in page.CharacteristicBoxes.CharacteristicBoxes)
            {
                box.BindToCharacteristicByTag(CurrentGetter, OpenEditor);
                CharacteristicChanged += box.OnCharacteristicChanged;
                InfoUpdated += box.UpdateValueFields;
            }
            var pointsboxes = (from UIElement e in page.PointsPanel.Children where e is PointsBox select e as PointsBox).ToList();
            foreach (var box in pointsboxes)
            {
                box.BindToCharacteristicByTag(CurrentGetter, Translator);
                CharacteristicChanged += box.OnCharacteristicChanged;
                InfoUpdated += box.UpdateValueFields;
            }

            // 角色伤害奖励的控制
            InfoUpdated += UpdateDamageBonus;
            // 监控角色的属性变化
            CharacteristicChanged += MainManager_CharacteristicChanged;
            // 初始化技能的显示
            Window.SkillValuesEditor.EditorPopup = Window.SValuesEditorPopup;
            page.SkillPanel.InitializeSkills(this, DataBus.Skills.Values, Window.SkillValuesEditor);
            // 绑定事件: 点击骰一次按钮时触发
            page.Button_Roll_DMGBonus.Click += (o, e) =>
            {
                var message = Translator.Translate("Message.RollADMGBonus", "{1}");
                var formula = page.Value_DamageBonus.Content.ToString();
                var result = CalcCharacteristic(formula, Current.Initials);
                Messenger.EnqueueFormat(message, page.Value_DamageBonus.Content, result);
            };
            // 头像绑定事件: 点击时可以选择图片导入
            page.Image_Avatar.Process(image =>
            {
                InfoUpdating += c => AvatarUpdate(c, image);
                image.MouseDown += DoImportAvatar;
                image.Drop += OnAvatarImageDrop;
            });
            // 武器面板初始化
            InfoUpdated += c =>
            {
                page.WeaponBox.Weapons = c.Weapons;
            };
            page.WeaponBox.InitializeDataGrid(DataBus.Weapons.Values);
        }

        /// <summary>
        /// 初始化角色卡的背面
        /// </summary>
        /// <param name="page"></param>
        private void InitializeBackstoryPage(BackstoryPage page)
        {
            if (page is null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.MouseWheel += DoCatchMouseWheel;
            var children = page.GetChildren();
            foreach (var child in children)
            {
                if (!(child is TextBox box)) continue;
                var tag = box.Tag?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (tag.StartsWith("Backstory"))
                {
                    tag = tag.Replace("Backstory.", string.Empty);
                    InfoUpdated += c =>
                    {
                        var b = new Binding(path: $"{nameof(Character.Backstory)}[{tag}]") { Source = c, };
                        box.SetBinding(TextBox.TextProperty, b);
                    };
                }
            }
            page.GearsCollectionChanged += (o, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    Current.GearAndPossessions.Add(e.NewItems[0] as string);
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var index = e.OldStartingIndex;
                    Current.GearAndPossessions.RemoveAt(index);
                }
            };
            // 打印时隐藏新道具的输入框与按钮
            HideOnCapturing.Add(page.Text_NewItem);
            HideOnCapturing.Add(page.Button_NewIitem);
        }
        #endregion

        /// <summary>
        /// 当 <see cref="Messenger"/> 收到消息时, 将其打印到 <see cref="Window"/> 的 Logger 控件中
        /// </summary>
        /// <param name="obj"></param>
        private void Messenger_OnEnqueue(string obj)
        {
            if (!string.IsNullOrEmpty(obj))
            {
                var control = Window.Logger;
                if (!string.IsNullOrEmpty(control.Text)) control.AppendText("\n");
                control.AppendText(obj.Trim());
                control.ScrollToEnd();
            }
            Messenger.Dequeue();
        }

        /// <summary>
        /// 初始化 <see cref="LuaHub"/>
        /// </summary>
        private ScriptHub InitLuaHub()
        {
            LuaHub = new XLuaHub();
            // 传入窗口与管理器的实例
            LuaHub.Set(nameof(MainManager), this);
            LuaHub.Set(nameof(Data.DataBus), DataBus);
            LuaHub.Set(nameof(Data.Config), Config);
            LuaHub.Set<Func<int, int, int>>(nameof(Roll), Roll);
            GCDispatcher.Tick += (o, e) => LuaHub.EnvGC();
            Window.Closed += (o, e) => LuaHub.Dispose();
            return LuaHub;
        }

        /// <summary>
        /// 在 ToolTip 浮现时, 同步其透明度
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        internal static void SynchronizeOpacity(object o, RoutedEventArgs e)
        {
            if (o is ToolTip tip) tip.Opacity = ToolTipOpacity;
        }

        /// <summary>
        /// 本地化控件及其子控件的文本
        /// </summary>
        public static void Localize(FrameworkElement container, Translator translator, HashSet<Visual> histories = null)
        {
            static void translate(FrameworkElement element, Translator translator)
            {
                if (translator == null) return;
                var path = element?.Tag?.ToString() ?? string.Empty;
                if (translator.TryTranslate(path, out var text))
                {
                    switch (element)
                    {
                        case MenuItem menuItem: menuItem.Header = text; break;
                        case HeaderedContentControl headered: headered.Header = text; break;
                        case Image image: image.ToolTip = text; break;
                        case Label label: label.Content = text; break;
                        case Button button: button.Content = text; break;
                        case Window window: window.Title = text; break;
                        case TextBox textBox: textBox.DataContext = text; break;
                        case TextBlock block:
                            var inlines = UIExtension.ResolveTextElements(text);
                            if (!inlines.Any()) break;
                            block.Inlines.Clear();
                            block.Inlines.AddRange(inlines);
                            break;
                    }
                }
                // 查询是否存在这个子控件的提示信息, 若有, 添加 ToolTip
                if (!string.IsNullOrWhiteSpace(path) && translator.TryTranslate($"{path}.ToolTip", out text) && !string.IsNullOrWhiteSpace(text))
                {
                    element.AddOrSetToolTip(text, (Style)Application.Current.FindResource("XToolTip"), SynchronizeOpacity);
                }
                // 否则不添加 ToolTip
                //else if (element.ToolTip != null)
                //{
                //    element.ToolTip = null;
                //}
            }
            if (translator == null) throw new ArgumentNullException(nameof(translator));
            // 创建历史记录, 避免重复操作元素
            if (histories == null) { histories = new HashSet<Visual>(); }
            if (histories.Contains(container)) { return; }
            else
            {
                translate(container, translator);
                histories.Add(container);
            }
            // 查询所有的子控件
            var items = container.GetChildren();
            // 遍历子控件, 尝试对其本地化
            foreach (var subItem in items)
            {
                if (histories.Contains(subItem)) continue;
                if (subItem is FrameworkElement element)
                {
                    translate(element, translator);
                    histories.Add(element);
                }
            }
        }

        /// <summary>
        /// 初始化卡牌显示列表
        /// </summary>
        /// <param name="listBox"></param>
        private void InitializeCardList(ListBox listBox)
        {
            var binding = new Binding() { Source = Characters };
            listBox.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            listBox.SelectedIndex = 0;
            listBox.SelectionChanged += (o, e) =>
            {
                if (e.Source is ListBox b && b.SelectedItem is Character c)
                {
                    Current = c;
                    OnInfoUpdate(Current);
                }
            };
            Characters.CollectionChanged += (o, e) =>
            {
                if (e.NewItems?.Count > 0)
                {
                    listBox.SelectedItem = e.NewItems[0];
                }
            };
        }

        /// <summary>
        /// 将指定的角色添加到 <see cref="Characters"/> 中
        /// </summary>
        /// <param name="character"></param>
        public void AddToCharacters(Character character)
        {
            if (character == null) { return; }
            if (string.IsNullOrWhiteSpace(character?.Name))
            {
                var defaultName = Translator.Translate("Name.Default", "Adam");
                character.Name = $"{defaultName} {Characters.Count}";
            }
            if (!Characters.Contains(character))
                Characters.Add(character);
        }

        /// <summary>
        /// 角色属性数值变动时触发的事件
        /// <para>基础属性变更时要连同派生属性一起变更</para>
        /// </summary>
        /// <param name="character"></param>
        /// <param name="args"></param>
        private void MainManager_CharacteristicChanged(Character character, CharacteristicChangedEventArgs args)
        {
            var key = args.Key;
            if (Config.BaseModelDict.TryGetValue(key, out var model))
            {
                if (model.Derived) return;
            }
            if (key.EqualsIgnoreCase("STR") || key.EqualsIgnoreCase("SIZ"))
                UpdateDamageBonus(character);
            var source = args.Segment switch
            {
                Characteristic.Segment.INITIAL => character.Initials,
                Characteristic.Segment.GROWTH => character.Growths,
                Characteristic.Segment.ADJUSTMENT => character.Adjustments,
                _ => null,
            };
            if (source == null) return;

            foreach (var m in Config.DataModels)
            {
                if (!m.Formula.Contains(key) || m.Name.EqualsIgnoreCase("MOV")) continue;
                var value = CalcCharacteristic(m.Formula, source);
                OnCharacteristicEdited(character, args.Segment, m.Name, value);
            }
        }

        /// <summary>
        /// 属性编辑框结束编辑时触发的事件: 用新的值设置角色的属性值
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="traitName"></param>
        /// <param name="value"></param>
        internal void OnCharacteristicEdited(Character character, Characteristic.Segment segment, string traitName, int value)
        {
            int i = character.GetInitial(traitName),
                a = character.GetAdjustment(traitName),
                g = character.GetGrowth(traitName);
            var (after, func) = segment switch
            {
                Characteristic.Segment.INITIAL => (value + a + g, (Action<string, int>)character.SetInitial),
                Characteristic.Segment.ADJUSTMENT => (value + i + g, character.SetAdjustment),
                Characteristic.Segment.GROWTH => (value + i + a, character.SetGrowth),
                _ => (0, null),
            };
            // 判断总值是否超过范围
            if (Config.BaseModelDict.TryGetValue(traitName, out var model))
            {
                if (model.Upper > 0 && after > model.Upper)
                {
                    value = model.Upper - (after - value);
                    var message = Translator.Translate("Message.Characteristic.Overflow", "{0} 的值不能超过 {1}");
                    Messenger.EnqueueFormat(message, model.Name, model.Upper);
                }
            }

            func?.Invoke(traitName, value);
        }

        /// <summary>
        /// 刷新角色的基础属性统计标签
        /// </summary>
        /// <param name="c"></param>
        public void UpdateDamageBonus(Character c)
        {
            if (c == null) return;
            var scrtipt = string.Format("return DamageBonus({0}, {1})", c.GetTotal("STR"), c.GetTotal("SIZ"));
            var results = LuaHub.DoString(scrtipt);
            string damageBonus = results[0] as string;
            int build = Convert.ToInt32(results[1]);
            IMainPage.Value_DamageBonus.Content = damageBonus;
            IMainPage.Value_Build.Content = build;
            c.DamageBonus = damageBonus;
            c.SetAdjustment("Build", build);
        }

        /// <summary>
        /// 取得一个事件: 打开批量随机属性的面板
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public RoutedEventHandler GetHandlerForReGenerateCharacteristics(Func<Character> getter, Window owner)
        {
            void regenerateprops(object o, EventArgs _)
            {
                var character = getter?.Invoke();
                if (character == null) return;
                var generator = new GenerationWindow(this, CalcCharacteristic)
                {
                    Owner = owner,
                    Age = character.Age,
                };
                if (owner == null)
                {
                    generator.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    generator.Title = Translator.Translate("GenerationWindow.Title.New", "新建调查员档案");
                }
                else
                {
                    generator.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                generator.ShowDialog();
                if ((bool)generator.DialogResult)
                {
                    // 设置角色的基础属性
                    var selection = generator.Selection;
                    foreach (var kvp in selection)
                    {
                        if (Config.BaseModelDict.ContainsKey(kvp.Key))
                            character.SetInitial(kvp.Key, kvp.Value);
                    }
                    // 将年龄的影响作用到角色上
                    generator.ApplyAgeBonus(character);
                    // 重算派生属性
                    RecalcInitials(character, t => t.Derived);
                    if (!Characters.Contains(character))
                        AddToCharacters(character);
                    OnInfoUpdate(character);
                }
            }
            return regenerateprops;
        }

        /// <summary>
        /// 计算一段脚本并返回 <see cref="int"/>
        /// </summary>
        /// <param name="script"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public int CalcForInt(string script, IDictionary variables)
        {
            using var subhub = LuaHub.CreateSubEnv(variables);
            var r = subhub.DoString(script, "CALC", true).FirstOrDefault() ?? 0;
            return Convert.ToInt32(r);
        }

        /// <summary>
        /// 根据公式与角色属性值的数组计算属性值
        /// <para>参见: <see cref="CalculateCharacteristic"/></para>
        /// </summary>
        /// <param name="traits"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        public int CalcCharacteristic(string formula, Dictionary<string, int> variables = null)
        {
            if (string.IsNullOrWhiteSpace(formula)) return 0;
            var segments = formula.Split(";");
            var seg = segments[0];
            // 找出所有 xDy
            var matches = Regex.Matches(seg, @"[0-9]{1,}D[0-9]{1,}");
            foreach (Match m in matches)
            {
                var segmentsJ = m.Value.Split('D');
                seg = seg.Replace(m.Value, $"{nameof(Roll)}({segmentsJ[0]}, {segmentsJ[1]})");
            }
            variables ??= new Dictionary<string, int>();
            return CalcForInt($"return {seg}", variables);
        }

        /// <summary>
        /// 重新计算角色属性的初始值
        /// </summary>
        /// <param name="character"></param>
        /// <param name="filter"></param>
        public void RecalcInitials(Character character, Func<Characteristic, bool> filter = null)
        {
            if (character == null) return;
            if (filter == null)
            {
                static bool filterDefault(Characteristic m) => true;
                filter = filterDefault;
            }
            var variables = new Dictionary<string, int>(character.Initials)
            {
                { "AGE", character.Age }
            };
            foreach (var model in Config.DataModels.Where(filter))
            {
                var value = CalcCharacteristic(model.Formula, variables);
                character.SetInitial(model.Name, value);
            }
        }

        /// <summary>
        /// 导出当前的数据总线
        /// </summary>
        public void ExportDataBus()
        {
            YamlKit.SaveFile(Paths.FileWeapons, DataBus.Weapons.Values);
            YamlKit.SaveFile(Paths.FileOccupations, DataBus.Occupations.Values);
            YamlKit.SaveFile(Paths.FileSkills, DataBus.Skills.Values);
        }

        /// <summary>
        /// 根据字符串生成技
        /// </summary>
        /// <param name="text"></param>
        public void GenerateDatas(string text)
        {
            var skills = new List<Skill>();
            var segments = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segline in segments)
            {
                if (string.IsNullOrWhiteSpace(segline)) continue;
                var s = new Skill();
                s.ID = skills.Count + 1;
                var isegments = segline.Split('–', 2);
                var first = isegments[0].Trim();
                s.Description = isegments[1].Trim();
                var match = Regex.Match(first, @"(?<=\()[^\)]+", RegexOptions.Multiline);
                if (match.Success)
                {
                    s.Name = first.Replace(match.Value, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Trim();
                    s.BaseValue = match.Value.Replace("%", string.Empty);
                }
                else
                {
                    s.Name = first;
                    s.BaseValue = "0";
                }
                skills.Add(s);
            }
            YamlKit.SaveFile(Paths.FileSkills, skills);
        }
    }
}
