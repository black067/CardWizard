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
        /// 缓存的控件模板
        /// </summary>
        private readonly Dictionary<string, UIElement> templates = new Dictionary<string, UIElement>();
        private Character current;
        private const string TMP_CARDSLISTITEM = "CardsListItem";

        /// <summary>
        /// 配置表
        /// </summary>
        public Config Config { get; set; }

        /// <summary>
        /// 本地化文本
        /// </summary>
        public Config.Localization Translator { get => Config.Translator; }

        /// <summary>
        /// 路径管理器
        /// </summary>
        public Config.PathsDatabase PathosDatabase { get => Config.Paths; }

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
        /// 角色的特点值被改变时, 触发的事件
        /// </summary>
        public event Action<object, Character.TraitChangedEventArgs> TraitChanged;

        /// <summary>
        /// 当前载入的所有角色
        /// </summary>
        public Dictionary<Character, ListBoxItem> CharacterCardPairs { get; set; } = new Dictionary<Character, ListBoxItem>();

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
                }
                current = value;
                if (current != null)
                {
                    current.TraitChanged += CurrentTraitChanged;
                }
            }
        }

        /// <summary>
        /// 创建主窗口管理器
        /// </summary>
        /// <param name="window"></param>
        public MainManager(MainWindow window)
        {
            Window = window;
            // 设置 Logger
            Messenger.OnEnqueue += Messenger_OnEnqueue;
            // 保存界面中的预设
            if (window.List_Cards.Items[0] is ListBoxItem templateListBoxItem)
            {
                templates.Add(TMP_CARDSLISTITEM, templateListBoxItem);
                templateListBoxItem.Visibility = Visibility.Hidden;
                window.List_Cards.Items.Remove(templateListBoxItem);
            }
#if DEBUG
            YamlKit.SaveFile(Resources.FileConfig, new Config());
            //if (Directory.Exists("./Save")) Directory.Delete("./Save", true);
#endif
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
            CreateDirectories(PathosDatabase);
            // 创建数据总线
            DataBus = new DataBus();
            // 新建一个随机数工具
            Die = new Die();
            // 创建 Lua 环境
            InitLuaHub();
            // 如果存在存储了计算公式的脚本文件, 执行
            LuaHub.DoFile(PathosDatabase.ScriptFormula, global: true);
            // 创建必要的文件夹
            Directory.CreateDirectory(PathosDatabase.PathSave);
            // 载入角色数据
            var files = Directory.GetFiles(PathosDatabase.PathSave);
            foreach (var item in files)
            {
                if (!item.EndsWith(Config.FileExtensionForCard)) 
                    continue;
                var c = YamlKit.LoadFile<Character>(item);
                if (c == default(Character)) continue;
                AddToCharacters(c);
            }
            if (CharacterCardPairs.Count != 0)
            {
                Current = CharacterCardPairs.Keys.First();
                Window.List_Cards.SelectedItem = CharacterCardPairs[Current];
            }
            else
            {
                Current = Character.Create(Config.BaseModelDict, (p, f) => CalcForInt(FormatScript(p, f)));
                AddToCharacters(Current);
            }
            #region 设置界面的交互逻辑
            // 给按钮绑定事件
            Window.Button_Create.Click += Button_Create_Click;
            Window.Button_Save.Click += Button_Save_Click;
            Window.Button_Debug.Click += Button_Debug_Click;
            // 角色基本信息的控制
            Window.InfoPanel.InitializeBinding(this);
            // 重生成角色属性的按钮
            Window.Button_Regenerate.Click += GetHandlerForReGenerateTraits(Current);
            // 角色属性的显示
            Window.BaseTraits_Headers.InitAsHeaders(from d in Config.DataModels where !d.Derived select d.Name, Translator);
            Window.DerivedTraits_Headers.InitAsHeaders(from d in Config.DataModels where d.Derived select d.Name, Translator);
            // 显示属性的说明
            var descriptions = from d in Config.DataModels select (d.Name, d.Description);
            Window.BaseTraits_Headers.InitToolTips(descriptions);
            Window.DerivedTraits_Headers.InitToolTips(descriptions);
            // 在更新角色信息后, 要刷新以下控件的显示状态
            Window.BaseTraits_Values.BindTraits(this, false);
            Window.DerivedTraits_Values.BindTraits(this, true);
            // 角色总属性的显示
            InfoUpdated += UpdateSumOfBaseTraits;
            // 角色伤害奖励的控制
            InfoUpdated += UpdateDamageBonus;
            Window.Button_Roll_DMGBonus.Click += (o, e) =>
            {
                var message = Translator.TryTranslate("Message.RollADMGBonus", out string sentence) ? sentence : "{1}";
                var formula = Window.Value_DamageBonus.Content.ToString();
                var result = CalcForInt(FormatScript(null, formula));
                Messenger.EnqueueFormat(message, Window.Value_DamageBonus.Content, result);
            };
            Window.Image_Avatar.Process(image =>
            {
                InfoUpdating += c => AvatarUpdate(c, image);
                image.MouseDown += Image_Avatar_Click;
            });
            #endregion
            // 刷新界面
            OnInfoUpdate(Current);
            // 对界面进行本地化
            Localize(Window, Translator);
            // 如果存在启动脚本文件, 执行
            LuaHub.DoFile(PathosDatabase.ScriptStartup, global: true);
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
                Title = Translator.TryTranslate("Dialog.Import.Avatar.Title", out var text) ? text : "选择调查员的照片",
            };
            openFiledialog.ShowDialog(Window);
            var result = openFiledialog.FileName;
            if (!File.Exists(result)) return;
            var ext = new FileInfo(result).Extension;
            var dest = $"./Save/{Current.Name}{ext}";
            // 如果文件已经存在, 要弹出确认窗口
            if (File.Exists(dest))
            {
                var message = Translator.TryTranslate("Dialog.Import.Avatar.Confirmation", out var msg) ? msg : "是否确定用\n{0}\n覆盖现有文件?";
                message = string.Format(message, result);
                var confirmDialog = new DialogWindow(message)
                {
                    Owner = Window,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                confirmDialog.ShowDialog();
                if (confirmDialog.Result)
                {
                    File.Copy(result, dest, true);
                    AvatarUpdate(Current, Window.Image_Avatar);
                }
            }
            else
            {
                File.Copy(result, dest);
                AvatarUpdate(Current, Window.Image_Avatar);
            }
        }

        /// <summary>
        /// Debug 按钮点击时触发的事件
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void Button_Debug_Click(object o, RoutedEventArgs e)
        {
            if (File.Exists(PathosDatabase.ScriptDebug))
            {
                try
                {
                    LuaHub.DoFile(PathosDatabase.ScriptDebug, global: false);
                }
                catch (Exception exception)
                {
                    Messenger.EnqueueFormat("{0}\n{1}", exception.Message, exception.StackTrace);
                    System.Diagnostics.Process.Start("explorer.exe", PathosDatabase.ScriptDebug.Replace("/", "\\"));
                }
            }
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
            var path = $"{PathosDatabase.PathSave}/{c.Name}.png";
            var rawImage = !File.Exists(path) ? Resources.Image_Avatar_Empty : System.Drawing.Image.FromFile(path);
            double width = image.Width, height = image.Height;
            var bitmapImage = rawImage
                                //.ZoomIn(width, height)
                                .ToBitmapImage();
            image.Source = bitmapImage;
        }

        /// <summary>
        /// 当 <see cref="Messenger"/> 收到消息时, 将其打印到 <see cref="Window"/> 的 Logger 控件中
        /// </summary>
        /// <param name="obj"></param>
        private void Messenger_OnEnqueue(string obj)
        {
            if (!string.IsNullOrEmpty(obj))
            {
                if (!string.IsNullOrEmpty(Window.Logger.Text)) Window.Logger.AppendText("\n");
                Window.Logger.AppendText(obj.Trim());
                Window.Logger.ScrollToEnd();
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
            Window.Closing += (o, e) =>
            {
                LuaHub.Dispose();
            };
            GCDispatcher.Tick += (o, e) =>
            {
                LuaHub.GC();
            };
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="pathos"></param>
        private static void CreateDirectories(Config.PathsDatabase pathos)
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
        private static void Localize(ContentControl container,
                                     Config.Localization transdict,
                                     Dictionary<string, ContentControl> histories = null)
        {
            if (histories == null)
            {
                histories = new Dictionary<string, ContentControl>();
            }
            if (histories.ContainsKey(container.Name)) { return; }
            else { histories.Add(container.Name, container); }
            var fields = container.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            object fitem;
            foreach (var field in fields)
            {
                fitem = field.GetValue(container);
                if (fitem is FrameworkElement element)
                {
                    var path = element.Tag?.ToString() ?? string.Empty;
                    var hasText = transdict.TryTranslate(path, out var text);
                    if (hasText)
                    {
                        if (element is Image image)
                        {
                            image.ToolTip = text;
                        }
                        if (element is ContentControl childContainer)
                        {
                            childContainer.Content = text;
                        }
                    }
                    if (element is ContentControl control)
                    {
                        Localize(control, transdict, histories);
                    }
                }
            }
        }

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
        /// 将指定的角色添加到 <see cref="CharacterCardPairs"/> 中
        /// </summary>
        /// <param name="character"></param>
        public void AddToCharacters(Character character)
        {
            if (CharacterCardPairs.ContainsKey(character)) { return; }
            if (string.IsNullOrWhiteSpace(character.Name))
            {

                var defaultName = Translator.TryTranslate("Name.Default", out var v) ? v : "Adam";
                character.Name = $"{defaultName}#{CharacterCardPairs.Count}";
            }
            templates.TryGetValue(TMP_CARDSLISTITEM, out var rawTemplate);
            var template = rawTemplate as ListBoxItem;
            Window.List_Cards.AddItem(template, $"Card_{character.GetHashCode()}").Process(item =>
            {
                OnNameChanged(this, new PropertyChangedEventArgs(nameof(Character.Name)), character, item);
                item.GotFocus += (o, e) =>
                {
                    var message = Translator.TryTranslate("Message.Character.Switched", out var v) ? v : "Switch to {0}";
                    Messenger.EnqueueFormat(message, character.Name);
                    Current = character;
                    OnInfoUpdate(Current);
                };
                character.PropertyChanged += (object o1, PropertyChangedEventArgs e1) => OnNameChanged(o1, e1, character, item);
                item.Unloaded += (o, e) => character.PropertyChanged -= (object o1, PropertyChangedEventArgs e1) => OnNameChanged(o1, e1, character, item);
                CharacterCardPairs.Add(character, item);
            });
        }

        /// <summary>
        /// 当角色的名称改变时, 更改侧边按钮的内容, 使其显示当前角色的名称
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        /// <param name="character"></param>
        /// <param name="item"></param>
        private void OnNameChanged(object o, PropertyChangedEventArgs e, Character character, ListBoxItem item)
        {
            if (e.PropertyName == nameof(Character.Name))
            {
                if (string.IsNullOrWhiteSpace(character.Name))
                {
                    int index = Window.List_Cards.Items.IndexOf(item);
                    var defaultName = Translator.TryTranslate("Name.Default", out var v) ? v : "Adam";
                    item.Content = $"{defaultName}#{index}";
                }
                else { item.Content = character.Name; }
            }
        }

        /// <summary>
        /// 移除指定的角色
        /// </summary>
        /// <param name="character"></param>
        public void RemoveFromCharacters(Character character)
        {
            if (CharacterCardPairs.TryGetValue(character, out var item))
            {
                CharacterCardPairs.Remove(character);
                Window.List_Cards.Items.Remove(item);
                Window.List_Cards.UnregisterName(item.Name);
            }
        }

        /// <summary>
        /// 刷新角色的基础属性统计标签
        /// </summary>
        /// <param name="c"></param>
        public void UpdateSumOfBaseTraits(Character c)
        {
            Window.Value_Sum_Base_Traits.Content = SumTraits(c, d => !d.Derived);
        }

        /// <summary>
        /// 刷新角色的基础属性统计标签
        /// </summary>
        /// <param name="c"></param>
        public void UpdateDamageBonus(Character c)
        {
            var scrtipt = string.Format("return DamageBonus({0}, {1})", c.GetTrait("STR"), c.GetTrait("SIZ"));
            Window.Value_DamageBonus.Content = LuaHub.DoString(scrtipt).First().ToString();
        }

        /// <summary>
        /// 当角色属性发生改变时执行的事件
        /// </summary>
        /// <param name="character"></param>
        /// <param name="args"></param>
        private void CurrentTraitChanged(object character, Character.TraitChangedEventArgs args)
        {
            TraitChanged?.Invoke(character, args);
        }

        /// <summary>
        /// 新建按钮点击时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="_"></param>
        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            Current = Character.Create(Config.BaseModelDict, null);
            AddToCharacters(Current);
            Window.List_Cards.SelectedItem = CharacterCardPairs[Current];
            GetHandlerForReGenerateTraits(Current).Invoke(this, new RoutedEventArgs());
            //InfoUpdate(Current);
        }

        /// <summary>
        /// 用递归的方式对字典的键进行翻译
        /// </summary>
        /// <param name="target"></param>
        private void TranslateDict(Dictionary<string, object> target)
        {
            var keys = target.Keys.ToArray();
            foreach (var key in keys)
            {
                var value = target[key];
                if (value is Dictionary<string, object> child)
                {
                    TranslateDict(child);
                }
                target.Remove(key);
                var newKey = Translator.TryTranslate(key, out var msg) ? msg : key;
                target[newKey] = value;
            }
        }

        /// <summary>
        /// 保存按钮点击时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string dest = Path.Combine(PathosDatabase.PathSave, Current.Name + Config.FileExtensionForCard);
            var source = YamlKit.SaveFile(dest, Current);
            if (Config.SaveTranslationDoc)
            {
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var graph = deserializer.Deserialize<Dictionary<string, object>>(source);
                TranslateDict(graph);
                YamlKit.SaveFile(Path.Combine(PathosDatabase.PathSave, Current.Name + Config.FileExtensionForCardDoc), graph);
            }
            var message = Translator.TryTranslate("Message.Character.Saved", out var v) ? v : "Saved at {0}";
            Messenger.EnqueueFormat(message, dest.Replace("\\", "/"));
        }

        /// <summary>
        /// 取得一个事件: 打开批量随机属性的面板
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private RoutedEventHandler GetHandlerForReGenerateTraits(Character character)
        {
            void regenerateprops(object o, RoutedEventArgs e)
            {
                var childWindow = new BatchGenerationWindow(Config.DataModels, CalcForInt, Translator)
                {
                    Owner = Window,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                childWindow.ShowDialog();
                if ((bool)childWindow.DialogResult)
                {
                    var selection = childWindow.Selection;
                    character.Growths?.Clear();
                    character.Senescence?.Clear();
                    foreach (var kvp in selection)
                    {
                        if (Config.BaseModelDict.ContainsKey(kvp.Key))
                            character.Traits[kvp.Key] = kvp.Value;
                    }
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
        /// <param name="formula"></param>
        /// <returns></returns>
        public static string FormatScript(Dictionary<string, int> properties, string formula)
        {
            var segments = formula.Split(";");
            var seg = segments[0];
            // 找出所有 xDy
            var matches = Regex.Matches(seg, @"[0-9]{1,}D[0-9]{1,}");
            foreach (Match m in matches)
            {
                var segmentsJ = m.Value.Split('D');
                seg = seg.Replace(m.Value, $"{nameof(Roll)}({segmentsJ[0]}, {segmentsJ[1]})");
            }
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    seg = seg.Replace(kvp.Key, kvp.Value.ToString());
                }
            }
            seg = $"return {seg}";
            try
            {
                return seg;
            }
            catch (Exception e)
            {
                Messenger.Enqueue(e);
                return "return 0";
            }
        }

        /// <summary>
        /// 计算一段脚本并返回 <see cref="int"/>
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public int CalcForInt(string script)
        {
            var r = LuaHub.DoString(script, "CALC");
            return Convert.ToInt32(r[0]);
        }

        /// <summary>
        /// 取得角色的总属性点数
        /// </summary>
        /// <param name="character"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public int SumTraits(Character character, Func<DataModel, bool> selector)
        {
            return (from prop in Config.DataModels
                    where selector(prop)
                    select character.GetTrait(prop.Name)).Sum();
        }

        /// <summary>
        /// 导出当前的数据总线
        /// </summary>
        public void ExportDataBus()
        {
            YamlKit.SaveFile(PathosDatabase.FileWeapons, DataBus.Weapons.Values);
            YamlKit.SaveFile(PathosDatabase.FileOccupations, DataBus.Occupations.Values);
            YamlKit.SaveFile(PathosDatabase.FileSkills, DataBus.Skills.Values);
        }
    }
}
