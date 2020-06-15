using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CardWizard.Tools
{
    /// <summary>
    /// 字符串的扩展方法
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 对比两个字符串, 不区分大小写
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string self, string other)
            => self.Equals(other, StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// 尝试将字符串转换为指定的枚举型变量, 不区分大小写
        /// </summary>
        /// <typeparam name="T">要转化的枚举类型</typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T ParseTo<T>(this string self) where T : struct, Enum
        {
            if (Enum.TryParse(self, true, out T rst))
                return rst;
            return default;
        }

        /// <summary>
        /// 将字符串转化为枚举值, 与另一个枚举值进行对比, 判断是否一致, 不区分大小写
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other">要比较的枚举值</param>
        /// <returns></returns>
        public static bool EqualsTo<T>(this string self, T other) where T : struct, Enum
        {
            if (Enum.TryParse(self, true, out T parseRst))
                return other.Equals(parseRst);
            return false;
        }

        /// <summary>
        /// 分隔字符串并移除空项
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        public static string[] SplitRemoveEmpty(this string text, params char[] separators)
            => text?.Split(separators, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        /// <summary>
        /// 分隔字符串并移除空项
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separators"></param>
        /// <returns></returns>
        public static string[] SplitRemoveEmpty(this string text, params string[] separators)
            => text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// 返回字符串被指定的前缀, 后缀包围的结果
        /// </summary>
        /// <param name="text"></param>
        /// <param name="prefix">前缀</param>
        /// <param name="extension">后缀</param>
        /// <returns></returns>
        public static string SiegeBy(this string text, string prefix, string extension)
            => $"{prefix}{text}{extension}";

        /// <summary>
        /// 为字符串添加前缀
        /// </summary>
        /// <param name="text"></param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static string PrefixBy(this string text, string prefix)
            => text.SiegeBy(prefix, string.Empty);

        /// <summary>
        /// 按照给定的转换方式, 将集合转换成列表字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">要转化的集合</param>
        /// <param name="separator">分隔符</param>
        /// <param name="converter">将集合内元素转化为字符串的方法</param>
        /// <returns></returns>
        public static string CombineToString<T>(this IEnumerable<T> self, string separator, Func<T, string> converter)
        {
            List<string> after = new List<string>();
            bool hasConverter = converter != null;
            foreach (var item in self)
            {
                if (item == null) continue;
                after.Add(hasConverter ? converter.Invoke(item) : item.ToString());
            }
            return string.Join(separator, self);
        }

        /// <summary>
        /// 中文字符在编码后的前缀
        /// </summary>
        public static string PrefixCHS { get; } = "OX";

        /// <summary>
        /// 将字符串中的所有中文字符转换为 16 进制编码, 比如 "文字" => "OX8765OX575b"
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string EncodeHex(this string self)
        {
            return Regex.Replace(self, @"([\u4e00-\u9faf])",
                chs =>
                {
                    var encoded = Encoding.Unicode.GetBytes(chs.Value);
                    var hexed = BitConverter.ToString(encoded).ToLower().Replace("-", string.Empty);
                    return hexed.PrefixBy(PrefixCHS);
                });
        }

        /// <summary>
        /// 将字符串中的所有 16 进制编码字符转换为 Unitcode 编码的字符, 比如 "OX8765OX575b" => "文字";
        /// 转换的方式: 
        /// 找出所有以 "OX" 开头, 后面跟着 4 个字符的子字符串, 且这 4 个字符都在 0-9 或 a-f 的范围内, 然后将其用 Unicode 解码
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string DecodeHex(this string self)
        {
            byte[] bytes = new byte[2];
            //pattern 的含义: 找出所有以 "OX" 开头, 后面跟着 4 个字符, 且这 4 个字符都在 0-9 或 a-f 的范围内
            return Regex.Replace(self, @"OX([a-f]|[0-9]){4}",
                hexed =>
                {
                    bytes[0] = Convert.ToByte(hexed.Value.Substring(2, 2), 16);
                    bytes[1] = Convert.ToByte(hexed.Value.Substring(4, 2), 16);
                    return Encoding.Unicode.GetString(bytes);
                });
        }

        /// <summary>
        /// 将字符串中的全角标点字符转换为半角标点字符
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToDBC(this string self)
        {
            char[] c = self.ToCharArray();
            for (int i = 0, len = c.Length; i < len; i++)
            {
                var cI = (int)c[i];
                switch (cI)
                {
                    //对‘’单独处理
                    case 8216:
                    case 8217:
                        c[i] = (char)39;
                        break;
                    //对“”单独处理
                    case 8220:
                    case 8221:
                        c[i] = (char)34;
                        break;
                    //对全角空格单独处理
                    case 12288:
                        c[i] = (char)32;
                        break;
                    //对、单独处理
                    case 12289:
                        c[i] = (char)92;
                        break;
                    //对。单独处理
                    case 12290:
                        c[i] = (char)46;
                        break;
                    //对【单独处理
                    case 12304:
                        c[i] = (char)91;
                        break;
                    //对】单独处理
                    case 12305:
                        c[i] = (char)93;
                        break;
                    default:
                        if (c[i] >= 65281 && c[i] <= 65374) c[i] = (char)(c[i] - 65248);
                        break;
                }
            }
            return new string(c);
        }
    }
}
