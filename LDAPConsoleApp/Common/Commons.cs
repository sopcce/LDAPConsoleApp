using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LDAPConsoleApp.Common
{
    public static class Commons
    {
        /// <summary>
        /// 字符串空判断扩展
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
        /// <summary>
        /// 字符串空判断扩展
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
        /// <summary>
        /// 字符串格式化扩展
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            if (!format.IsNullOrEmpty())
            {
                return string.Format(format, args);
            }
            return format;
        }
        /// <summary>
        /// 数组不为空并且长度大于某值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsNotNullAndGreaterThan<T>(this List<T> list, int num)
        {
            return list != null && list.Count() > num;
        }
        /// <summary>
        /// ToInt32 扩展方法 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defV"></param>
        /// <returns></returns>
        public static int ToInt32(this object obj, int defV = 0)
        {
            int temp = 0;
            try
            {
                temp = Convert.ToInt32(obj);
            }
            catch
            {
                temp = defV;
            }
            return temp;
        }
        /// <summary>
        /// ToString 扩展方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defV"></param>
        /// <returns></returns>
        public static string ToStrings(this object obj, string defV = "")
        {
            string temp = "";
            try
            {
                temp = Convert.ToString(obj);
            }
            catch
            {
                temp = defV;
            }
            return temp;
        }
        /// <summary>
        /// object convert bool
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool ToBool(this object val)
        {
            if (val != null)
            {
                switch (val.ToString().ToLower())
                {
                    case "1":
                        return true;
                    case "是":
                        return true;
                    case "yes":
                        return true;
                    case "on":
                        return true;
                    case "true":
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
        /// <summary>
        /// ToDateTime 扩展方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defV"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object obj, string defV = "1970-01-01")
        {
            DateTime temp;
            try
            {
                if (obj == null)
                {
                    obj = defV;
                }
                temp = Convert.ToDateTime(obj);
                if (temp <= DateTime.Parse(defV))
                {
                    return temp = DateTime.Parse(defV);
                }
            }
            catch
            {
                temp = DateTime.Parse(defV);
            }
            return temp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defV"></param>
        /// <returns></returns>
        public static DateTime? ToDateTimeOrNull(this object obj)
        {
            DateTime? temp;
            try
            {
                if (obj == null || Convert.ToDateTime(obj) <= new DateTime(1970, 1, 1))
                {
                    temp = null;
                }
                else
                {
                    temp = Convert.ToDateTime(obj);
                }
            }
            catch
            {
                temp = null;
            }
            return temp;
        }
        /// <summary>
        /// ToDecimal 扩展方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defV"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object obj, decimal defV = 0M)
        {
            decimal tem = 0M;
            try
            {
                tem = Convert.ToDecimal(obj);
            }
            catch
            {
                tem = defV;
            }
            return tem;
        }
        /// <summary>
        /// 获取config value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigSettingValue(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        /// <summary>
        /// 替换特殊字符，防SQL注入
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSQLChar(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            str = str.Replace("'", "");
            str = str.Replace(";", "");
            str = str.Replace(",", "");
            str = str.Replace("?", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("@", "");
            str = str.Replace("=", "");
            str = str.Replace("+", "");
            str = str.Replace("*", "");
            str = str.Replace("&", "");
            str = str.Replace("#", "");
            str = str.Replace("%", "");
            str = str.Replace("$", "");

            //删除与数据库相关的词
            str = Regex.Replace(str, "select", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "insert", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "delete from", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "count", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "drop table", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "asc", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "mid", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "char", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "xp_cmdshell", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "exec master", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net localgroup administrators", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "and", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net user", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "or", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "-", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "delete", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "drop", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "script", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "update", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "and", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "chr", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "master", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "declare", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "mid", "", RegexOptions.IgnoreCase);

            return str;
        }
    }
}
