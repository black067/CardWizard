using System;
using System.Collections.Generic;
using System.Text;
using XLua;

namespace CardWizard.Tools
{
    public class XLuaHub : ScriptHub
    {
        private LuaEnv Env { get; set; }

        /// <summary>
        /// 构造 <see cref="XLua"/> 的脚本运行环境
        /// </summary>
        public XLuaHub()
        {
            Env = new LuaEnv();
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
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            Env.FullGc();
            Env.Dispose();
            Env = null;
            isDisposed = true;
        }

        /// <summary>
        /// 执行文本文件
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chunkName"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public override object[] DoString(string text, string chunkName = "CHUNK", bool global = false)
        {
            if (global)
            {
                return Env.DoString(text, chunkName, Env.Global);
            }
            using LuaTable table = Env.NewTable();
            using LuaTable meta = Env.NewTable();
            meta.Set("__index", Env.Global);
            table.SetMetaTable(meta);
            meta.Dispose();
            return Env.DoString(text, chunkName, table);
        }

        /// <summary>
        /// 执行二进制文本
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chunkName"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public override object[] DoString(byte[] source, string chunkName = "CHUNK", bool global = false)
        {
            if (global)
            {
                return Env.DoString(source, chunkName, Env.Global);
            }
            using LuaTable table = Env.NewTable();
            using LuaTable meta = Env.NewTable();
            meta.Set("__index", Env.Global);
            table.SetMetaTable(meta);
            return Env.DoString(source, chunkName, table);
        }

        /// <summary>
        /// 查询指定变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public override T Get<T>(string path)
        {
            return Env.Global.Get<T>(path);
        }

        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public override void Set<T>(string path, T value)
        {
            Env.Global.Set(path, value);
        }

        /// <summary>
        /// 在脚本运行环境中进行垃圾回收
        /// </summary>
        public override void EnvGC() => Env.GC();
    }
}
