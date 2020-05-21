using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace CardWizard.Tools
{
    /// <summary>
    /// 数据载入工具
    /// </summary>
    public static class YamlKit
    {
        /// <summary>
        /// Yaml 文件后缀名 .yaml
        /// </summary>
        public const string EXTENSION_A = ".yaml";

        /// <summary>
        /// Yaml 文件后缀名 .yml
        /// </summary>
        public const string EXTENSION_B = ".yml";

        public static T Parse<T>(string text)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize<T>(text);
        }

        public enum ParseFail
        {
            Throw,
            Ignore,
            Print,
        }

        public static bool TryParse<T>(string text, out T item, ParseFail behavior = ParseFail.Ignore)
        {
            try
            {
                item = Parse<T>(text);
                return true;
            }
            catch(Exception e)
            {
                switch (behavior)
                {
                    case ParseFail.Throw:
                        throw;
                    case ParseFail.Print:
                        Messenger.Enqueue(e);
                        break;
                    case ParseFail.Ignore:
                    default:
                        break;
                }
            }
            item = default;
            return false;
        }

        /// <summary>
        /// 从文件中反序列化数据获得类的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T LoadFile<T>(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    var item = Create<T>();
                    SaveFile(path, item);
                    return item;
                }
                var texts = File.ReadAllText(path);
                Deserializer deserializer = new Deserializer();
                return deserializer.Deserialize<T>(texts);
            }
            catch (Exception e)
            {
                Messenger.Enqueue(e);
                return Create<T>();
            }
        }

        private static T Create<T>()
        {
            var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor == null) { return default; }
            var item = (T)ctor.Invoke(Array.Empty<object>());
            return item;
        }

        /// <summary>
        /// 将对象序列化并保存到文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="item"></param>
        public static string SaveFile<T>(string path, T item, bool overwrite = true)
        {
            try
            {
                ISerializer serializer = new SerializerBuilder().Build();
                if (!File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                else if (!overwrite)
                {
                    IOKit.Backup(path);
                }
                var source = serializer.Serialize(item);
                File.WriteAllText(path, source, Encoding.UTF8);
                return source;
            }
            catch (Exception e)
            {
                try { IOKit.Rollback(path); }
                catch (Exception) { }
                Messenger.EnqueueFormat("{0}\n{1}", e.Message, e.StackTrace);
                return string.Empty;
            }
        }

        /// <summary>
        /// 取得一个对象的拷贝, 被标记为 <see cref="YamlIgnoreAttribute"/> 的成员不会被复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T Clone<T>(T item)
        {
            var text = new Serializer().Serialize(item);
            return new Deserializer().Deserialize<T>(text);
        }
    }
}
