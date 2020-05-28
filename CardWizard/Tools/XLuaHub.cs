using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using XLua;

namespace CardWizard.Tools
{
    public class XLuaHub : ScriptHub
    {
        private static LuaEnv Env { get; set; }

        private LuaTable global;

        private LuaTable Global
        {
            get
            {
                global ??= Env.Global;
                return global;
            }
            set
            {
                global = value;
            }
        }

        private XLuaHub Parent { get; set; }

        /// <summary>
        /// 构造 <see cref="XLua"/> 的脚本运行环境
        /// </summary>
        public XLuaHub()
        {
            Env ??= new LuaEnv();
            // 重载 print 方法
            var print = "print";
            var code = @$"
function {print}(...)
    local args = {{...}}
    local msg = ''
    for _, v in ipairs(args) do
        msg = msg..v..' '
    end
    CS.{typeof(Messenger).FullName}.{nameof(Messenger.Enqueue)}(msg)
end";
            DoString(code, global: true);
            Parent = null;
        }

        private XLuaHub(LuaTable table)
        {
            Global = table;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            Global.Dispose();
            if (Parent == null)
            {
                Env.FullGc();
                Env.Dispose();
            }
            isDisposed = true;
        }

        private static LuaTable GetSubTable(LuaEnv env)
        {
            LuaTable table = env.NewTable();
            using LuaTable meta = env.NewTable();
            meta.Set("__index", env.Global);
            table.SetMetaTable(meta);
            meta.Dispose();
            return table;
        }

        /// <summary>
        /// 构造一个子环境
        /// <para>为保证资源释放, 建议配合 <see cref="using"/> 关键字使用</para>
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public override ScriptHub CreateSubEnv(IDictionary variables = default)
        {
            var subhub = new XLuaHub(GetSubTable(Env))
            {
                Parent = this
            };
            if (variables != default)
            {
                foreach (var key in variables.Keys)
                {
                    subhub.Global.Set(key, variables[key]);
                }
            }
            return subhub;
        }

        /// <summary>
        /// 执行文本文件
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chunkName"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public override object[] DoString(string text, [CallerMemberName] string chunkName = "", bool global = false)
        {
            if (global)
            {
                return Env.DoString(text, chunkName, Global);
            }
            return Env.DoString(text, chunkName, GetSubTable(Env));
        }

        /// <summary>
        /// 执行二进制文本
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chunkName"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public override object[] DoString(byte[] source, [CallerMemberName] string chunkName = "", bool global = false)
        {
            if (global)
            {
                return Env.DoString(source, chunkName, Global);
            }
            return Env.DoString(source, chunkName, GetSubTable(Env));
        }

        /// <summary>
        /// 查询指定变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public override TValue Get<TKey, TValue>(TKey path)
        {
            return Global.Get<TKey, TValue>(path);
        }

        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public override void Set<TKey, TValue>(TKey path, TValue value)
        {
            Global.Set(path, value);
        }

        /// <summary>
        /// 在脚本运行环境中进行垃圾回收
        /// </summary>
        public override void EnvGC() => Env.GC();
    }
}
