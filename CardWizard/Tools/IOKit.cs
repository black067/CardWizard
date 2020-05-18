using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
