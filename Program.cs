using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HIK_DEMON
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => {
                Debug.WriteLine("UI线程异常: " + e.Exception.Message);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                Debug.WriteLine("未处理异常: " + e.ExceptionObject);
            };
            Application.Run(new Form1());
        }
    }
}
