using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MultiBank.Extention
{
    public class Logger
    {
        static string Path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\Log";

        public static void Write(string Message)
        {
            StreamWriter sw = null;
            try
            {
                string FileName = Path.TrimEnd('\\') + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                sw.WriteLine(Message);
                sw.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
        }

        public static void A_Write(string log)
        {
            StreamWriter sw = null;
            try
            {
                string FileName = Path.TrimEnd('\\') + "\\" + "AUM_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                sw.WriteLine(log);
                sw.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
        }


        public static void Write(string Title, string Message)
        {
            StreamWriter sw = null;
            try
            {
                string FileName = Path.TrimEnd('\\') + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                sw.WriteLine(Title);
                sw.WriteLine(Message);
                sw.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
        }

        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="MethodName">方法名称</param>
        /// <param name="Title">标题+参数</param>
        /// <param name="Message">内容</param>
        public static void Write(string MethodName, string Title, string Message)
        {
            StreamWriter sw = null;
            try
            {
                string FileName = Path.TrimEnd('\\') + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                sw.WriteLine(MethodName + " → " + Title);
                sw.WriteLine(Message);
                sw.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
        }

        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="MethodName">方法名称</param>
        /// <param name="Title">标题+参数</param>
        /// <param name="Message">内容</param>
        /// <param name="LogType">日志类型</param>
        public static void Write(string MethodName, string Title, string Message, string LogType)
        {
            StreamWriter sw = null;
            try
            {
                string FileName = Path.TrimEnd('\\') + "\\" + LogType + "-" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                sw.WriteLine(MethodName + " → " + Title);
                sw.WriteLine(Message);
                sw.WriteLine();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                sw.Close();
                sw.Dispose();
            }
        }
    }
}