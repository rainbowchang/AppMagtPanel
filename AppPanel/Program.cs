using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;

namespace AppPanel
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 解析命令行参数，检查是否是子进程
            if (args.Length > 0 && int.TryParse(args[0], out int id) && (id >= 1 && id <= 3))
            {
                // 子进程直接启动窗口
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1(args));
                return;
            }

            // 主进程，检查配置文件
            bool quadSplit = false;
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppConfig.xml");
            
            if (File.Exists(configPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(configPath);
                    XElement? quadSplitElement = doc.Descendants("quadsplit").FirstOrDefault();
                    if (quadSplitElement != null && !string.IsNullOrEmpty(quadSplitElement.Value) && bool.TryParse(quadSplitElement.Value, out bool value))
                    {
                        quadSplit = value;
                    }
                }
                catch { }
            }

            // 如果需要四分割，先启动子进程
            if (quadSplit)
            {
                Process currentProcess = Process.GetCurrentProcess();
                if (currentProcess.MainModule != null)
                {
                    string exePath = currentProcess.MainModule.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // 启动子进程1（右上角）
                        Process.Start(exePath, "1");
                        
                        // 启动子进程2（左下角）
                        Process.Start(exePath, "2");
                        
                        // 启动子进程3（右下角）
                        Process.Start(exePath, "3");
                    }
                }
            }

            // 启动主窗口
            ApplicationConfiguration.Initialize();
            // 当quadsplit为true时，主窗口使用参数0
            string[] mainArgs = quadSplit ? new string[] { "0" } : args;
            Form1 mainForm = new Form1(mainArgs);
            Application.Run(mainForm);
        }
    }
}