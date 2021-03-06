﻿using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CardWizard.Tools
{
    public class CodeBuilder
    {
        /// <summary>
        /// 注释的格式化文本
        /// </summary>
        const string SummaryFormat = "<summary>\r\n {0}\r\n </summary>";

        static readonly Type[] TypeRefWhitenames = new Type[]
        {
            typeof(string), typeof(char),
            typeof(byte), typeof(int), typeof(double),
            typeof(bool), typeof(float), typeof(long), typeof(short),
            typeof(ulong), typeof(uint), typeof(ushort), typeof(decimal),
        };

        /// <summary>
        /// 类的命名空间
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// 引用的命名空间
        /// </summary>
        public List<string> NamespaceImported { get; set; } = new List<string>();

        /// <summary>
        /// 类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类的全名
        /// </summary>
        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Namespace))
                {
                    return Name;
                }
                return $"{Namespace}.{Name}";
            }
        }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// 类代码文件的保存路径
        /// </summary>
        public string DestPath { get; set; } = ".";

        /// <summary>
        /// 生成的类
        /// </summary>
        public CodeTypeDeclaration Declaration { get; set; }

        public CodeBuilder(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 取得格式化的注释文本
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static string Format(string s) => string.Format(SummaryFormat, s);

        /// <summary>
        /// 生成编译单元
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public CodeCompileUnit GenerateUnit(IEnumerable<MemberDefinition> members)
        {
            // 编译的基本单元
            CodeCompileUnit unit = new CodeCompileUnit();
            // 声明命名空间
            CodeNamespace @namespace = new CodeNamespace(Namespace);
            // 引用的命名空间
            foreach (var item in NamespaceImported)
            {
                @namespace.Imports.Add(new CodeNamespaceImport(item));
            }
            unit.Namespaces.Add(@namespace);
            // 声明类型
            CodeTypeDeclaration @class = new CodeTypeDeclaration(Name)
            {
                // 类型的属性 public class
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                IsPartial = true,
            };
            Declaration = @class;
            @class.Comments.Add(new CodeCommentStatement(Format($"{Comment} - 生成的代码"), true));
            // 添加到命名空间
            @namespace.Types.Add(@class);

            // new() 方法
            var constructorEmpty = new CodeConstructor() { Attributes = MemberAttributes.Public, };
            @class.Members.Add(constructorEmpty);
            // 继承 INotifyPropertyChanged 接口
            var name_event = nameof(INotifyPropertyChanged.PropertyChanged);
            @class.BaseTypes.Add(new CodeTypeReference(typeof(INotifyPropertyChanged)));
            @class.Members.Add(new CodeMemberEvent
            {
                Name = name_event,
                Type = new CodeTypeReference(typeof(PropertyChangedEventHandler)),
                Attributes = MemberAttributes.Public | MemberAttributes.New,
            });
            @class.Members.Add(new CodeMemberMethod() { Name = name_event.PrefixBy("On"), }.Process(m =>
            {
                var name_variable = "name";
                var param = new CodeParameterDeclarationExpression(typeof(string), name_variable);
                // OptionalAttribute
                var optional = new CodeAttributeDeclaration(new CodeTypeReference(typeof(OptionalAttribute)));
                param.CustomAttributes.Add(optional);
                // DefaultParameterValueAttribute
                var defaultValue = new CodeAttributeDeclaration(new CodeTypeReference(typeof(DefaultParameterValueAttribute)));
                defaultValue.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression("")));
                param.CustomAttributes.Add(defaultValue);
                // CallerMemberNameAttribute
                param.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(CallerMemberNameAttribute))));
                m.Parameters.Add(param);
                m.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeEventReferenceExpression(new CodeThisReferenceExpression(), name_event),
                        CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                    new CodeExpressionStatement(
                        new CodeDelegateInvokeExpression(
                            new CodeEventReferenceExpression(
                                new CodeThisReferenceExpression(), name_event),
                            new CodeThisReferenceExpression(),
                            new CodeObjectCreateExpression(
                                typeof(PropertyChangedEventArgs),
                                new CodeVariableReferenceExpression(name_variable))))));
            }).AssignAs(out var notifyMethod));
            // 通过字典创建实例
            var constructor_arg_name = "values";
            var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public, };
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(Dictionary<string, object>)), constructor_arg_name));
            @class.Members.Add(constructor);
            // 生成属性与字段
            CodeMemberField field; CodeMemberProperty prop; string name; Type typeRef;
            // 过滤重复的
            var membersDistinct = (from m in members group m by m.Name into defgroup select defgroup.First());

            foreach (var definition in membersDistinct)
            {
                name = definition.Name;
                if (string.IsNullOrWhiteSpace(name)) continue;

                typeRef = definition.TypeRef is Type typeItem ? typeItem : typeof(string);
                field = new CodeMemberField
                {
                    Attributes = MemberAttributes.Private,
                    Name = name.ToLower().PrefixBy("_"),
                    Type = new CodeTypeReference(typeRef),
                };
                if (definition.DefaultValue != null && TypeRefWhitenames.Contains(typeRef))
                    field.InitExpression = new CodePrimitiveExpression(definition.DefaultValue);
                else if (typeRef.IsSubclassOf(typeof(Enum)))
                    field.InitExpression = new CodeSnippetExpression($"{typeRef.FullName.Replace('+', '.')}.{definition.DefaultValue}");
                else if (definition.DefaultValue is string snippet)
                    field.InitExpression = new CodeSnippetExpression(snippet);
                else if (typeRef.GetConstructor(Type.EmptyTypes) != null)
                    field.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeRef));
                else
                    field.InitExpression = new CodeDefaultValueExpression(new CodeTypeReference(typeRef));
                prop = new CodeMemberProperty
                {
                    Attributes = MemberAttributes.Final | MemberAttributes.Public,
                    Name = name,
                    Type = new CodeTypeReference(typeRef),
                };
                prop.Comments.Add(new CodeCommentStatement(Format(definition.Comment), true));

                prop.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), field.Name)));
                prop.SetStatements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), field.Name),
                    new CodePropertySetValueReferenceExpression()));
                prop.SetStatements.Add(new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), notifyMethod.Name)));
                var valueGetter = new CodeIndexerExpression(
                                    new CodeVariableReferenceExpression(constructor_arg_name),
                                    new CodePrimitiveExpression(prop.Name));
                var condition = new CodeConditionStatement(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression(constructor_arg_name),
                        nameof(Dictionary<string, object>.ContainsKey),
                        new CodePrimitiveExpression(prop.Name)),
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), field.Name),
                        new CodeCastExpression(typeRef, valueGetter)));
                constructor.Statements.Add(condition);
                @class.Members.Add(field);
                @class.Members.Add(prop);
            }
            return unit;
        }

        /// <summary>
        /// 生成代码
        /// <para>如果输出路径中存在同名文件, 且不覆盖 (<paramref name="overwrite"/> == <see cref="false"/>), 则为同名文件会移动到 Backup 文件夹下</para>
        /// </summary>
        /// <param name="unit">代码单元</param>
        /// <param name="fileName">文件名称(无后缀)</param>
        /// <param name="overwrite">是否覆盖同名文件</param>
        /// <returns>生成源代码的完整路径</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0063:使用简单的 \"using\" 语句", Justification = "<挂起>")]
        public static string GenerateCode(CodeCompileUnit unit, string directory, string fileName, bool overwrite = false)
        {
            using CSharpCodeProvider provider = new CSharpCodeProvider();
            string file = provider.FileExtension[0] == '.' ? $"{fileName}{provider.FileExtension}" : $"{fileName}.{provider.FileExtension}";
            var file_full = Path.Combine(directory, file);

            CodeGeneratorOptions options = new CodeGeneratorOptions()
            {
                BlankLinesBetweenMembers = false,
                BracingStyle = "C",
            };

            if (!overwrite && File.Exists(file_full))//如果存在同名文件, 且 overwrite == false, 则备份同名文件
            {
                IOKit.Backup(file_full);
            }
            Directory.CreateDirectory(directory);
            using FileStream fileStream = File.Open(file_full, FileMode.Create, FileAccess.Write);
            using StreamWriter streamWriter = new StreamWriter(fileStream);
            IndentedTextWriter textWriter = new IndentedTextWriter(streamWriter, "    ");
            try
            {
                provider.GenerateCodeFromCompileUnit(unit, textWriter, options);
                textWriter.Close();
            }
            catch (Exception e)
            {
                IOKit.Rollback(file_full);//写入失败则回滚到上一个版本
                Messenger.Enqueue(e);
            }
            finally
            {
                textWriter.Close();
            }
            return file_full;
        }

        /// <summary>
        /// 读取 .cs 文件, 生成 <see cref="Assembly"/>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Assembly GenerateAssembly(string file)
        {
            using CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters options = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true,
            };
            options.ReferencedAssemblies.AddRange(
                (from a in AppDomain.CurrentDomain.GetAssemblies() where !a.IsDynamic select a.Location).ToArray());
            var result = provider.CompileAssemblyFromFile(options, file);
            //var result = provider.CompileAssemblyFromDom(options, unit);
            if (result.Errors.HasErrors)
            {
                foreach (CompilerError error in result.Errors)
                {
                    Messenger.EnqueueFormat("@{0}: {1}", error.Line, error.ErrorText);
                }
                return null;
            }
            return result.CompiledAssembly;
        }

        /// <summary>
        /// 检查并取得字符串的类型;
        /// <para>支持以下缩写
        ///  <see cref="int"/>, <see cref="float"/>, <see cref="double"/>, <see cref="bool"/>,
        ///  <see cref="string"/>, <see cref="char"/>, <see cref="byte"/>, <see cref="short"/>, 
        ///  <see cref="object"/>
        /// </para>
        /// <para>以及 <see cref="List{T}"/> where T : 上述所有类型 </para>
        /// <para>找不到对应类型的字符串会用反射查询, 仍找不到时会返回 typeof(<see cref="string"/>) </para>
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type ResolveType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) typeName = "object";
            var lowerName = typeName.ToLower();
            switch (lowerName)
            {
                case "bool":
                case "boolean":
                    return typeof(bool);
                case "byte":
                    return typeof(byte);
                case "int16":
                case "short":
                    return typeof(short);
                case "int32":
                case "int":
                    return typeof(int);
                case "double":
                    return typeof(double);
                case "float":
                case "single":
                    return typeof(float);
                case "string":
                    return typeof(string);
                case "char":
                    return typeof(char);
                case "object":
                    return typeof(object);
                default:
                    break;
            }
            if (typeName.StartsWith("List<"))
            {
                var innerTypeName = typeName.Replace("List<", string.Empty).Replace(">", string.Empty).Trim();
                var innerType = ResolveType(innerTypeName);//递归取得泛型类型
                var rstType = typeof(List<>).MakeGenericType(innerType);
                return rstType;
            }
            //消耗较大, 最后再查询
            var type_tryGet = ResolveTypeByFullName(typeName);
            if (type_tryGet != null) { return type_tryGet; }

            return typeof(string);
        }

        /// <summary>
        /// 根据字符串取得类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type ResolveTypeByFullName(string typeName)
        {
            Type type = null;
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            int assemblyArrayLength = assemblyArray.Length;
            for (int i = 0; i < assemblyArrayLength; ++i)
            {
                type = assemblyArray[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            for (int i = 0; i < assemblyArrayLength; ++i)
            {
                Type[] typeArray = assemblyArray[i].GetTypes();
                int typeArrayLength = typeArray.Length;
                for (int j = 0; j < typeArrayLength; ++j)
                {
                    if (typeArray[j].Name.Equals(typeName))
                    {
                        return typeArray[j];
                    }
                }
            }
            return type;
        }
    }

    /// <summary>
    /// 属性的定义
    /// </summary>
    public class MemberDefinition
    {
        public MemberDefinition(string name, object defaultValue, string comment = null) : this(name, defaultValue, null, comment) { }

        public MemberDefinition(string name, Type typeRef, string comment = null) : this(name, null, typeRef, comment) { }

        public MemberDefinition(string name, object defaultValue, Type typeRef, string comment)
        {
            Name = name;
            DefaultValue = defaultValue;
            TypeRef = typeRef ?? defaultValue?.GetType() ?? typeof(object);
            Comment = comment ?? string.Empty;
        }

        public MemberDefinition(FieldInfo info, object target = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Name = info.Name;
            TypeRef = info.FieldType;
            if (target != null)
            {
                DefaultValue = info.GetValue(target);
            }
            var descrAttr = info.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
            if (descrAttr is DescriptionAttribute description)
            {
                Comment = description.Description;
            }
        }

        public MemberDefinition(PropertyInfo info, object target = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Name = info.Name;
            TypeRef = info.PropertyType;
            if (target != null)
            {
                DefaultValue = info.GetValue(target);
            }
            var descrAttr = info.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
            if (descrAttr is DescriptionAttribute description)
            {
                Comment = description.Description;
            }
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Type TypeRef { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
    }
}
