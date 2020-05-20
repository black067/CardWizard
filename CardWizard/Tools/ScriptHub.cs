using System;
using System.IO;

namespace CardWizard.Tools
{
    /// <summary>
    /// Lua 脚本的基类
    /// </summary>
    public abstract class ScriptHub : IDisposable
    {
        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public abstract void Set<T>(string path, T value);

        /// <summary>
        /// 查询变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract T Get<T>(string path);

        /// <summary>
        /// 执行 Lua 字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chunkName"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public abstract object[] DoString(string text, string chunkName = "CHUNK", bool isGlobal = false);

        /// <summary>
        /// 执行 二进制 Lua 字符串 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chunkName"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public abstract object[] DoString(byte[] source, string chunkName = "CHUNK", bool isGlobal = false);

        /// <summary>
        /// 执行 Lua 脚本文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="chunkName"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public virtual object[] DoFile(string path, string chunkName = "CHUNK", bool isGlobal = false)
        {
            if (File.Exists(path))
            {
                return DoString(File.ReadAllText(path), chunkName, isGlobal);
            }
            return null;
        }

        /// <summary>
        /// 执行 二进制 Lua 脚本文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chunkName"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public virtual object[] DoFile(byte[] source, string chunkName = "CHUNK", bool isGlobal = false)
        {
            return DoString(source, chunkName, isGlobal);
        }

        /// <summary>
        /// 脚本运行环境的手动垃圾回收
        /// </summary>
        public abstract void EnvGC();

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 是否已释放
        /// </summary>
        protected bool isDisposed;

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Dispose(bool disposing);
    }
}
