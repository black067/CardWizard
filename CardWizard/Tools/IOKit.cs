using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CardWizard.Tools
{
    /// <summary>
    /// 与系统相关的工具
    /// </summary>
    public static class IOKit
    {
        /// <summary>
        /// 为文件添加后缀使其不重名
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="suffix">后缀名, 不带 '.'</param>
        /// <returns></returns>
        public static string GetUniqueFileName(string file, string suffix)
        {
            var format = $"{file}.{{0}}.{suffix}";
            var newName = string.Format(format, 0);
            for (int i = 1; File.Exists(newName); i++)
            {
                newName = string.Format(format, i);
            }
            return newName;
        }

        /// <summary>
        /// 备份文件夹
        /// </summary>
        private const string NAME_BackupFolder = "Backup";

        /// <summary>
        /// 备份指定的文件
        /// </summary>
        /// <param name="file">文件的名称</param>
        public static void Backup(string file)
        {
            var info = new FileInfo(file);
            var path = info.Directory.FullName;
            var folder = Path.Combine(path, NAME_BackupFolder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var file_bak = Path.Combine(folder, info.Name);
            var newName = GetUniqueFileName(file_bak, NAME_BackupFolder.ToLower());
            info.CopyTo(newName);
        }

        /// <summary>
        /// 回滚到最近版本
        /// </summary>
        /// <param name="file"></param>
        /// <returns>表示回滚成功与否</returns>
        public static bool Rollback(string file)
        {
            var info = new FileInfo(file);
            var name = info.Name;
            var path = info.Directory.FullName;
            var suffix = $".{NAME_BackupFolder.ToLower()}";
            var folder = Path.Combine(path, NAME_BackupFolder);
            if (!Directory.Exists(folder))//不存在备份文件夹
            {
                return false;
            }
            var files = new DirectoryInfo(folder).GetFiles();
            var histories = (from f in files where f.Name.StartsWith(name) && f.Name.EndsWith(suffix) select f).ToList();
            if (histories.Count < 1)//一个备份都没有
            {
                return false;
            }
            histories.Sort((f_l, f_r) => -f_l.CreationTime.CompareTo(f_r.CreationTime));
            var file_latest = histories.First();
            var file_crashed = GetUniqueFileName(file, "crashed");
            info.MoveTo(file_crashed);
            file_latest.MoveTo(file);
            return true;
        }

        /// <summary>
        /// 查询目录下的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursiveLimit"></param>
        /// <returns></returns>
        public static List<FileInfo> GetAllFiles(string path, int recursiveLimit = 0)
        {
            static List<FileInfo> localGetAllFiles(string path, int i, int max, List<FileInfo> empty)
            {
                if (max > 0 && i > max) return empty;
                var files = new List<FileInfo>();
                // 是目录
                if (Directory.Exists(path))
                {
                    var subfiles = Directory.GetFiles(path);
                    files.AddRange(from f in subfiles select new FileInfo(f));
                    var subpaths = Directory.GetDirectories(path);
                    foreach (var subpath in subpaths)
                    {
                        files.AddRange(localGetAllFiles(subpath, i + 1, max, empty));
                    }
                }
                // 是文件
                else if (File.Exists(path))
                {
                    files.Add(new FileInfo(path));
                }
                return files;
            }
            return localGetAllFiles(path, 0, recursiveLimit, Array.Empty<FileInfo>().ToList());
        }


        /// <summary>
        /// 压缩文件时的缓冲区大小
        /// </summary>
        public static int BufferSizeForZipping { get; set; } = 4096;

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filePaths"></param>
        public static Thread Zip(string dest, string comments = "", params string[] filesToZip)
        {
            var dir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
            void localZip()
            {
                using var source = new ZipOutputStream(File.Create(dest));
                source.SetLevel(6);
                if (!string.IsNullOrWhiteSpace(comments))
                {
                    source.SetComment(comments);
                }
                var filesFinal = new List<FileInfo>();
                foreach (var item in filesToZip.ToList())
                {

                    FileAttributes attr = File.GetAttributes(item);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {

                    }
                }
                foreach (var item in filesToZip)
                {
                    if (!File.Exists(item)) continue;
                    try
                    {
                        using var file = File.OpenRead(item);
                        byte[] buffer = new byte[BufferSizeForZipping];
                        var entry = new ZipEntry(Path.GetFileName(item)) { DateTime = DateTime.Now };
                        source.PutNextEntry(entry);
                        for (int sourceBytes = 1; sourceBytes > 0;)
                        {
                            sourceBytes = file.Read(buffer, 0, buffer.Length);
                            source.Write(buffer, 0, sourceBytes);
                        }
                        source.CloseEntry();
                    }
                    catch
                    {
                        source.CloseEntry();
                        throw;
                    }
                }
            }
            var thread = new Thread(localZip);
            thread.Start();
            return thread;
        }

        /// <summary>
        /// 解压指定的文件到目标路径下, 目标路径默认是与压缩包同名的文件夹
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destDir"></param>
        public static Thread Extract(string sourceFile, string destDir = "")
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException($"未能找到文件 {sourceFile}");
            }
            // 默认解压到同名文件夹内
            if (string.IsNullOrWhiteSpace(destDir))
            {
                var folder = Path.GetFileNameWithoutExtension(sourceFile);
                var info = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), folder));
                destDir = info.FullName;
            }
            void localExtract()
            {
                using var source = new ZipInputStream(File.OpenRead(sourceFile));
                ZipEntry entry;
                while ((entry = source.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory) continue;

                    var iDestDir = Path.Combine(destDir, Path.GetDirectoryName(entry.Name));
                    if (!string.IsNullOrWhiteSpace(iDestDir))
                    {
                        Directory.CreateDirectory(iDestDir);
                    }

                    var iDestFile = Path.Combine(destDir, Path.GetFileName(entry.Name));
                    if (string.IsNullOrWhiteSpace(iDestFile))
                    {
                        continue;
                    }
                    // 创建文件用于写入
                    using var writer = File.Create(iDestFile);
                    int size = BufferSizeForZipping;
                    var buffer = new byte[size];
                    while (true)
                    {
                        size = source.Read(buffer, 0, buffer.Length);
                        if (size > 0) writer.Write(buffer, 0, size);
                        else break;
                    }
                }
            }
            var thread = new Thread(localExtract);
            thread.Start();
            return thread;
        }
    }
}
