using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using CardWizard.Data;
using CardWizard.Tools;
using CardWizard.Properties;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using CallOfCthulhu;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;

namespace CardWizard.View
{
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
        /// 骰子
        /// </summary>
        public Die Die { get; set; }

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
        /// 角色的属性值被改变时, 触发的事件
        /// </summary>
        public event TraitChangedEventHandler TraitChanged;

        /// <summary>
        /// 对事件的包装
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        private void OnTraitChanged(Character c, TraitChangedEventArgs e) => TraitChanged?.Invoke(c, e);

        /// <summary>
        /// 已载入的所有角色
        /// </summary>
        public ObservableCollection<Character> Characters { get; set; }

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
                        value.TraitChanged += OnTraitChanged;
                    }
                }
                current = value;
            }
        }

        /// <summary>
        /// 创建主窗口管理器
        /// </summary>
        /// <param name="window"></param>
        public MainManager(MainWindow window)
        {
            Window = window ?? throw new NullReferenceException();
            // 设置 Logger
            Messenger.OnEnqueue += Messenger_OnEnqueue;
            // 如果项目路径下存在文件"DEBUG", 就执行以下下操作
            if (File.Exists("DEBUG"))
            {
                YamlKit.SaveFile(Resources.FileConfig, new Config());
            }
            // 读取配置表
            Config = YamlKit.LoadFile<Config>(Resources.FileConfig).Process();
            // 设置定期 Garbage Collect
            GCDispatcher = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, Config.GCInterval),
                Tag = $"{nameof(GC)}.{nameof(GC.Collect)}",
            };
            GCDispatcher.Tick += (o, e) =>
            {
                GC.Collect();
            };
            GCDispatcher.Start();
            // 根据 Config.PathosDatabase 创建目录
            CreateDirectories(Paths);
            // 创建数据总线
            DataBus = new DataBus();
            // 新建一个随机数工具
            Die = new Die();
            // 创建 Lua 环境
            InitLuaHub();
            // 如果存在存储了计算公式的脚本文件, 执行
            LuaHub.DoFile(Paths.ScriptFormula, isGlobal: true);
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
            }
            if (Characters.Count != 0)
            {
                Current = Characters.First();
            }
            else
            {
                Current = Character.Create(Config.BaseModelDict, CalcTrait);
                AddToCharacters(Current);
            }
            #region 设置界面的交互逻辑
            // 给按钮绑定事件
            Window.Button_Create.Click += Button_Create_Click;
            Window.Button_Save.Click += Button_Save_Click;
            Window.Button_Debug.Click += Button_Debug_Click;
            Window.Button_SavePic.Click += Button_SavePic_Click;
            InitAsCardList(Window.List_Cards);
            // 角色基本信息面板的初始化
            Window.Backstory.InitializeBinding(this);
            // 属性重生成按钮
            Window.Button_Regenerate.Click += GetHandlerForReGenerateTraits(() => Current);
            HideOnCapturePic.Add(Window.Button_Regenerate);
            // 角色总属性的显示
            void UpdateSumOfBaseTraits(Character c)
            {
                if (c == null) return;
                Window.Value_Sum_Base_Traits.Content = SumTraits(c, d => !d.Derived);
            }
            InfoUpdated += UpdateSumOfBaseTraits;

            // 角色属性数值面板初始化
            var traitBoxes = from UIElement c in Window.TraitsGrid.Children where c is TraitBox select c as TraitBox;
            foreach (var box in traitBoxes)
            {
                var iTraitChanged = box.BindToTraitByTag(OnTraitBoxEndEdit);
                TraitChanged += iTraitChanged;
                iTraitChanged.Invoke(Current, new TraitChangedEventArgs(box.Key));
            }
            // 角色伤害奖励的控制
            InfoUpdated += UpdateDamageBonus;
            // 绑定事件: 点击骰一次按钮时触发
            Window.Button_Roll_DMGBonus.Click += (o, e) =>
            {
                var message = Translator.Translate("Message.RollADMGBonus", "{1}");
                var formula = Window.Value_DamageBonus.Content.ToString();
                var result = CalcTrait(null, formula);
                Messenger.EnqueueFormat(message, Window.Value_DamageBonus.Content, result);
            };
            HideOnCapturePic.Add(Window.Button_Roll_DMGBonus);
            // 头像绑定事件: 点击时可以选择图片导入
            Window.Image_Avatar.Process(image =>
            {
                InfoUpdating += c => AvatarUpdate(c, image);
                image.MouseDown += Image_Avatar_Click;
            });
            #endregion
            // 刷新界面
            OnInfoUpdate(Current);
            // 如果存在启动脚本文件, 执行
            LuaHub.DoFile(Paths.ScriptStartup, isGlobal: true);
            // 对界面进行本地化
            Localize(Window, Translator);
            // 垃圾回收
            GC.Collect();
        }

        #region Button Click Handler
        /// <summary>
        /// 新建按钮点击时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new GenerationWindow(Config, CalcTrait)
            {
                Owner = Window,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            childWindow.ShowDialog();
            if (childWindow.DialogResult ?? false)
            {
                Current = Character.Create(Config.BaseModelDict);
                foreach (var kvp in childWindow.Selection)
                {
                    Current.SetTraitInitial(kvp.Key, kvp.Value);
                }
                RecalcTraitsInitial(Current, t => t.Derived);
                AddToCharacters(Current);
            }
        }

        /// <summary>
        /// 保存按钮点击时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string dest = Path.Combine(Paths.PathSave, Current.Name + Config.FileExtensionForCard);
            var source = YamlKit.SaveFile(dest, Current);
            if (Config.SaveTranslationDoc)
            {
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var dict = deserializer.Deserialize<Dictionary<string, object>>(source);
                Translator.TranslateKeys(dict);
                YamlKit.SaveFile(Path.Combine(Paths.PathSave, Current.Name + Config.FileExtensionForCardDoc), dict);
            }
            var message = Translator.Translate("Message.Character.Saved", "Saved at {0}");
            Messenger.EnqueueFormat(message, dest.Replace("\\", "/"));
        }

        /// <summary>
        /// 在生成调查员图像文档时需要隐藏的控件
        /// </summary>
        public List<UIElement> HideOnCapturePic { get; set; } = new List<UIElement>();

        /// <summary>
        /// 生成图片时的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_SavePic_Click(object sender, RoutedEventArgs e)
        {
            // 开始截屏前先缓存并更改部分元素的状态
            var cacheVisibility = new Dictionary<UIElement, Visibility>();
            foreach (var item in HideOnCapturePic)
            {
                cacheVisibility.Add(item, item.Visibility);
                item.Visibility = Visibility.Hidden;
            }
            var bg = Window.InvestigatorView.Background;
            // 开始截屏
            Window.InvestigatorView.Process(source =>
            {
                var c = (Color)ColorConverter.ConvertFromString(Config.PrintSettings_BackgroundColor);
                source.Background = new SolidColorBrush(c);
                // 等待控件属性的变化刷新
                source.UpdateLayout();
                // 期望的 DPI
                var tDpi = Config.PrintSettings_Dpi;
                // 查询得出当前的 真实尺寸 与 DPI
                var aSize = UIExtension.GetActualSize(source);
                var aDpi = UIExtension.GetDpi(source);
                var tWidth = (tDpi / aDpi) * aSize.Width;
                var tHeight = (tDpi / aDpi) * aSize.Height;
                var dest = Path.Combine(Paths.PathSave, Current.Name + Config.FileExtensionForCardPic);
                UIExtension.CapturePng(source, dest, (int)tWidth, (int)tHeight, tDpi, tDpi);
                Messenger.EnqueueFormat(Translator.Translate("Message.Character.SavedPic", "Saved at {0}"), dest.Replace("\\", "/"));
            });
            // 将被更改的元素还原
            Window.InvestigatorView.Background = bg;
            foreach (var kvp in cacheVisibility)
            {
                kvp.Key.Visibility = kvp.Value;
            }
        }

        /// <summary>
        /// Debug 按钮点击时触发的事件
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void Button_Debug_Click(object o, RoutedEventArgs e)
        {
            if (File.Exists(Paths.ScriptDebug))
            {
                try
                {
                    LuaHub.DoFile(Paths.ScriptDebug, isGlobal: false);
                }
#pragma warning disable CA1031 // 不捕获常规异常类型
                catch (Exception exception)
#pragma warning restore CA1031 // 不捕获常规异常类型
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
        private void Image_Avatar_Click(object o, System.Windows.Input.MouseButtonEventArgs e)
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

            var ext = new FileInfo(result).Extension;
            var dest = $"./Save/{Current.Name}{ext}";
            // 如果文件已经存在, 要弹出确认窗口
            if (File.Exists(dest))
            {
                var message = Translator.Translate("Dialog.Import.Avatar.Confirmation", "是否确定用\n{0}\n覆盖现有文件?");
                message = string.Format(message, result);
                var confirmDialog = new DialogWindow(message)
                {
                    Owner = Window,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                confirmDialog.ShowDialog();
                if (!confirmDialog.Result)
                    return;
            }
            File.Copy(result, dest, true);
            AvatarUpdate(Current, Window.Image_Avatar);
        }
        #endregion

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
            var rawImage = !File.Exists(path) ? Resources.Image_Avatar_Empty : System.Drawing.Image.FromFile(path);
            image.Source = rawImage.ToBitmapImage();
        }

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
        private void InitLuaHub()
        {
            LuaHub = new XLuaHub();
            // 传入窗口与管理器的实例
            LuaHub.Set(nameof(MainManager), this);
            LuaHub.Set(nameof(Data.DataBus), DataBus);
            LuaHub.Set(nameof(Data.Config), Config);
            LuaHub.Set<Func<int, int, int>>(nameof(Roll), Roll);
            GCDispatcher.Tick += (o, e) => LuaHub.EnvGC();
            Window.Closed += (o, e) => LuaHub.Dispose();
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="pathos"></param>
        private static void CreateDirectories(Paths pathos)
        {
            var folders = new string[] {
                pathos.PathSave, pathos.PathData,
                pathos.PathScripts, pathos.PathTextures };
            foreach (var item in folders)
            {
                Directory.CreateDirectory(item);
            }
        }

        /// <summary>
        /// 本地化控件及其子控件的文本
        /// </summary>
        private static void Localize(ContentControl container, Translator transdict,
                                     HashSet<ContentControl> histories = null)
        {
            if (histories == null) { histories = new HashSet<ContentControl>(); }
            if (histories.Contains(container)) { return; }
            else { histories.Add(container); }
            var subElements = container.SelectAllSubElement();
            var elements = from e in subElements where e is FrameworkElement select e as FrameworkElement;
            foreach (var element in elements)
            {
                var path = element.Tag?.ToString() ?? string.Empty;
                if (transdict.TryTranslate(path, out var text))
                {
                    if (element is Image image)
                    {
                        image.ToolTip = text;
                    }
                    if (element is ContentControl childContainer && !(element is TraitBox))
                    {
                        childContainer.Content = text;
                    }
                    if (element is TextBlock block)
                    {
                        block.Inlines.Clear();
                        var segments = text.Split("#", 2, StringSplitOptions.RemoveEmptyEntries);
                        var run = new Run(segments[0].Trim());
                        if (segments.Length > 1)
                        {
                            var styleDict = UIExtension.ResolveTextElementProperties(segments[1]);
                            foreach (var kvp in styleDict)
                            {
                                run.SetValue(kvp.Key, kvp.Value);
                            }
                        }
                        block.Inlines.Add(run);
                    }
                }
                if (transdict.TryTranslate($"{path}.ToolTip", out text))
                {
                    element.ToolTip = text;
                }
                if (element is ContentControl control)
                {
                    Localize(control, transdict, histories);
                }
            }
        }

        /// <summary>
        /// 初始化卡牌显示列表
        /// </summary>
        /// <param name="box"></param>
        private void InitAsCardList(ListBox box)
        {
            var binding = new Binding() { Source = Characters };
            box.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            box.SelectedIndex = 0;
            box.SelectionChanged += (o, e) =>
            {
                if (e.Source is ListBox b && b.SelectedItem is Character c)
                {
                    Current = c;
                    OnInfoUpdate(Current);
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
            Characters.Add(character);
        }

        /// <summary>
        /// 属性编辑框结束编辑时触发的事件: 用新的值设置角色的属性值
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="traitName"></param>
        /// <param name="value"></param>
        private void OnTraitBoxEndEdit(Trait.Segment segment, string traitName, int value)
        {
            switch (segment)
            {
                case Trait.Segment.INITIAL:
                    Current.SetTraitInitial(traitName, value);
                    break;
                case Trait.Segment.ADJUSTMENT:
                    Current.SetTraitAdjustment(traitName, value);
                    break;
                default:
                    Current.SetTraitGrowth(traitName, value);
                    break;
            }
        }

        /// <summary>
        /// 取得角色的总属性点数
        /// </summary>
        /// <param name="character"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public int SumTraits(Character character, Func<Trait, bool> selector) => (from prop in Config.DataModels
                                                                                  where selector(prop)
                                                                                  select character.GetTrait(prop.Name)).Sum();

        /// <summary>
        /// 刷新角色的基础属性统计标签
        /// </summary>
        /// <param name="c"></param>
        public void UpdateDamageBonus(Character c)
        {
            if (c == null) return;
            var scrtipt = string.Format("return DamageBonus({0}, {1})", c.GetTrait("STR"), c.GetTrait("SIZ"));
            Window.Value_DamageBonus.Content = LuaHub.DoString(scrtipt).First().ToString();
        }

        /// <summary>
        /// 取得一个事件: 打开批量随机属性的面板
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public RoutedEventHandler GetHandlerForReGenerateTraits(Func<Character> getter)
        {
            void regenerateprops(object o, RoutedEventArgs e)
            {
                var character = getter?.Invoke();
                if (character == null) return;
                var childWindow = new GenerationWindow(Config, CalcTrait)
                {
                    Owner = Window,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                childWindow.ShowDialog();
                if ((bool)childWindow.DialogResult)
                {
                    var selection = childWindow.Selection;
                    foreach (var kvp in selection)
                    {
                        if (Config.BaseModelDict.ContainsKey(kvp.Key))
                            character.SetTraitInitial(kvp.Key, kvp.Value);
                    }
                    RecalcTraitsInitial(character, t => t.Derived);
                    OnInfoUpdate(character);
                }
            }
            return regenerateprops;
        }

        /// <summary>
        /// 骰点
        /// </summary>
        /// <param name="count"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        private int Roll(int count, int upper)
        {
            int sum = 0;
            for (int i = count; i > 0; i--)
            {
                sum += Die.Range(1, upper + 1);
            }
            return sum;
        }

        /// <summary>
        /// 根据掷骰公式产生一个随机数
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        private string FormatScript(Dictionary<string, int> properties, string formula)
        {
            var segments = formula?.Split(";") ?? new string[] { string.Empty };
            var seg = segments[0];
            // 找出所有 xDy
            var matches = Regex.Matches(seg, @"[0-9]{1,}D[0-9]{1,}");
            foreach (Match m in matches)
            {
                var segmentsJ = m.Value.Split('D');
                seg = seg.Replace(m.Value, $"{nameof(Roll)}({segmentsJ[0]}, {segmentsJ[1]})");
            }
            if (properties == null) properties = new Dictionary<string, int>();
            foreach (var kvp in Config.BaseModelDict)
            {
                seg = seg.Replace($"${kvp.Key}", (properties.TryGetValue(kvp.Key, out int v) ? v : 0).ToString());
            }

            seg = Regex.Replace(seg, @"\$C\b", $"{nameof(MainManager)}.{nameof(Current)}",
                                RegexOptions.Multiline);
            return $"return {seg}";
        }

        /// <summary>
        /// 计算一段脚本并返回 <see cref="int"/>
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public int CalcForInt(string script)
        {
            var r = LuaHub.DoString(script, "CALC").FirstOrDefault() ?? 0;
            return Convert.ToInt32(r);
        }

        /// <summary>
        /// 根据公式与角色属性值的数组计算属性值
        /// </summary>
        /// <param name="traits"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        public int CalcTrait(Dictionary<string, int> traits, string formula)
        {
            return CalcForInt(FormatScript(traits, formula));
        }

        /// <summary>
        /// 重新计算角色属性的初始值
        /// </summary>
        /// <param name="character"></param>
        /// <param name="filter"></param>
        public void RecalcTraitsInitial(Character character, Func<Trait, bool> filter = null)
        {
            if (character == null) return;
            if (filter == null) filter = m => true;
            foreach (var model in Config.DataModels.Where(filter))
            {
                var value = CalcTrait(character.Traits, model.Formula);
                character.SetTraitInitial(model.Name, value);
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
    }
}
