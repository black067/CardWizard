using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using System.ComponentModel;
using System.Linq;

namespace CardWizard.Tools
{
    /// <summary>
    /// 数据载入工具
    /// </summary>
    public static class YamlKit
    {
        public static T Parse<T>(string text)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize<T>(text);
        }

        /// <summary>
        /// 在读取失败时的操作
        /// </summary>
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
            catch (Exception e)
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
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 将对象序列化
        /// <para>带有 <see cref="DescriptionAttribute"/> 标签的成员会生成注释</para>
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static string Serialize(object graph)
        {
            var serializer = new SerializerBuilder()
                                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                                .Build();
            var yaml = serializer.Serialize(graph);
            return yaml;
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
                if (!File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                else if (!overwrite)
                {
                    IOKit.Backup(path);
                }
                var source = Serialize(item);
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

    /// <summary>
    /// 注释查询器
    /// </summary>
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException("innerTypeDescriptor");
            }

            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor
                .GetProperties(type, container)
                .Select(d => new CommentsPropertyDescriptor(d));
        }

        /// <summary>
        /// 注释的解释器
        /// </summary>
        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                this.baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type { get { return baseDescriptor.Type; } }

            public Type TypeOverride
            {
                get { return baseDescriptor.TypeOverride; }
                set { baseDescriptor.TypeOverride = value; }
            }

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get { return baseDescriptor.ScalarStyle; }
                set { baseDescriptor.ScalarStyle = value; }
            }

            public bool CanWrite { get { return baseDescriptor.CanWrite; } }

            public void Write(object target, object value)
            {
                baseDescriptor.Write(target, value);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                var description = baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(baseDescriptor.Read(target), description.Description)
                    : baseDescriptor.Read(target);
            }
        }
    }

    public sealed class CommentsObjectDescriptor : IObjectDescriptor
    {
        private readonly IObjectDescriptor innerDescriptor;

        public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
        {
            this.innerDescriptor = innerDescriptor;
            this.Comment = comment;
        }

        public string Comment { get; private set; }

        public object Value { get { return innerDescriptor.Value; } }
        public Type Type { get { return innerDescriptor.Type; } }
        public Type StaticType { get { return innerDescriptor.StaticType; } }
        public ScalarStyle ScalarStyle { get { return innerDescriptor.ScalarStyle; } }
    }

    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            var commentsDescriptor = value as CommentsObjectDescriptor;
            if (commentsDescriptor != null && commentsDescriptor.Comment != null)
            {
                context.Emit(new Comment(commentsDescriptor.Comment, false));
            }

            return base.EnterMapping(key, value, context);
        }
    }
}
