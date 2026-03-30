using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AppPanel
{
    public partial class Form1 : Form
    {
        private int panelId;
        private List<ProgramConfig> programConfigs;

        int MaxCellWidth = 128;
        int MaxCellHeight = 128;
        int itemsPerRow = 6; // 每行显示6个按钮
        int maxRowCount = 3; // 最多显示3行，多了就滚动
        bool quadSplit = false;

        public Form1(string[] args)
        {
            InitializeComponent();

            // 解析命令行参数，默认为-1（居中）
            panelId = -1;
            if (args.Length > 0 && int.TryParse(args[0], out int id))
            {
                panelId = id;
            }

            // 初始化程序配置列表
            programConfigs = new List<ProgramConfig>();

            // 在Load事件中设置窗口大小和位置，加载配置文件，创建按钮
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // 设置窗口大小和位置
            SetWindowSizeAndPosition();

            // 创建关闭按钮
            CreateCloseButton();

            // 加载配置文件
            LoadConfig();

            // 创建按钮
            CreateButtons();
        }

        private void CreateCloseButton()
        {
            // 只有panelId是-1或1的时候才显示关闭按钮
            if (panelId == -1 || panelId == 1)
            {
                Button closeButton = new Button();
                closeButton.Text = "×";
                closeButton.Font = new Font("Arial", 12, FontStyle.Bold);
                closeButton.Width = 30;
                closeButton.Height = 30;
                closeButton.Location = new Point(this.Width - 40, 10);
                closeButton.BackColor = Color.Transparent;
                closeButton.FlatStyle = FlatStyle.Flat;
                closeButton.FlatAppearance.BorderSize = 0;
                closeButton.ForeColor = Color.Gray;

                // 按钮点击事件：关闭所有实例
                closeButton.Click += btnCloseAll_Click;

                // 添加到窗体
                this.Controls.Add(closeButton);

                // 确保按钮在最顶层
                closeButton.BringToFront();

                // 监听窗口大小变化，调整按钮位置
                this.SizeChanged += (sender, e) =>
                {
                    closeButton.Location = new Point(this.Width - 40, 10);
                };
            }
        }

        private void SetWindowSizeAndPosition()
        {
            // 获取主显示器的大小
            int screenWidth = SystemInformation.PrimaryMonitorSize.Width;
            int screenHeight = SystemInformation.PrimaryMonitorSize.Height;
            int windowWidth = screenWidth / 2;
            int windowHeight = screenHeight / 2;

            this.Width = windowWidth;
            this.Height = windowHeight;

            switch (panelId)
            {
                case 0: // 左上角
                    this.Left = 0;
                    this.Top = 0;
                    break;
                case 1: // 右上角
                    this.Left = screenWidth / 2;
                    this.Top = 0;
                    break;
                case 2: // 左下角
                    this.Left = 0;
                    this.Top = screenHeight / 2;
                    break;
                case 3: // 右下角
                    this.Left = screenWidth / 2;
                    this.Top = screenHeight / 2;
                    break;
                default: // 全屏
                    this.Left = 0;
                    this.Top = 0;
                    this.Width = screenWidth;
                    this.Height = screenHeight;
                    break;
            }
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppConfig.xml");

            if (File.Exists(configPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(configPath);
                    XElement? quadSplitElement = doc.Descendants("quadsplit").FirstOrDefault();
                    if (quadSplitElement != null && !string.IsNullOrEmpty(quadSplitElement.Value) && bool.TryParse(quadSplitElement.Value, out bool value1))
                    {
                        quadSplit = value1;
                    }
                    XElement? maxCellWidthElement = doc.Descendants("maxCellWidth").FirstOrDefault();
                    if (maxCellWidthElement != null && !string.IsNullOrEmpty(maxCellWidthElement.Value) && int.TryParse(maxCellWidthElement.Value, out int value2))
                    {
                        MaxCellWidth = value2;
                    }
                    XElement? maxCellHeightElement = doc.Descendants("maxCellHeight").FirstOrDefault();
                    if (maxCellHeightElement != null && !string.IsNullOrEmpty(maxCellHeightElement.Value) && int.TryParse(maxCellHeightElement.Value, out int value3))
                    {
                       MaxCellHeight = value3;
                    }

                    if (quadSplit)
                    {
                        XElement? quadElements = doc.Descendants("quad").FirstOrDefault();
                        if (quadElements != null )
                        {
                            var itemsPerRowElement = quadElements.Element("itemsPerRow");
                            if (itemsPerRowElement != null && !string.IsNullOrEmpty(itemsPerRowElement.Value) && int.TryParse(itemsPerRowElement.Value, out int value4))
                            {
                                itemsPerRow = value4;
                            }

                            var maxRowCountElement = quadElements.Element("maxRowCount");
                            if (maxRowCountElement != null && !string.IsNullOrEmpty(maxRowCountElement.Value) && int.TryParse(maxRowCountElement.Value, out int value5))
                            {
                                maxRowCount = value5;
                            }
                        }
                    } else
                    {
                        XElement? singleElements = doc.Descendants("single").FirstOrDefault();
                        if (singleElements != null)
                        {
                            var itemsPerRowElement = singleElements.Element("itemsPerRow");
                            if (itemsPerRowElement != null && !string.IsNullOrEmpty(itemsPerRowElement.Value) && int.TryParse(itemsPerRowElement.Value, out int value4))
                            {
                                itemsPerRow = value4;
                            }
                            var maxRowCountElement = singleElements.Element("maxRowCount");
                            if (maxRowCountElement != null && !string.IsNullOrEmpty(maxRowCountElement.Value) && int.TryParse(maxRowCountElement.Value, out int value5))
                            {
                                maxRowCount = value5;
                            }
                        }
                    }

                    var programElements = doc.Descendants("program");
                    foreach (var programElement in programElements)
                    {
                        ProgramConfig config = new ProgramConfig();

                        // 读取path
                        var pathElement = programElement.Element("path");
                        if (pathElement != null)
                        {
                            config.Path = pathElement.Value;
                        }

                        // 读取params
                        var paramsElement = programElement.Element("params");
                        if (paramsElement != null)
                        {
                            var paramElements = paramsElement.Elements("param");
                            foreach (var paramElement in paramElements)
                            {
                                config.Params.Add(paramElement.Value);
                            }
                        }

                        // 读取icon
                        var iconElement = programElement.Element("icon");
                        if (iconElement != null)
                        {
                            config.Icon = iconElement.Value;
                        }

                        // 读取name
                        var nameElement = programElement.Element("name");
                        if (nameElement != null)
                        {
                            config.Name = nameElement.Value;
                        }

                        // 读取multi
                        var multiElement = programElement.Element("multi");
                        if (multiElement != null && bool.TryParse(multiElement.Value, out bool multi))
                        {
                            config.Multi = multi;
                        }

                        // 只添加有path的配置
                        if (!string.IsNullOrEmpty(config.Path))
                        {
                            programConfigs.Add(config);
                        }
                    }
                }
                catch { }
            }
        }


        private void CreateButtons()
        {

            // 使用TableLayoutPanel来布局按钮，实现居中效果
            int cellcount = programConfigs.Count;
            int rowCount = (cellcount + itemsPerRow - 1) / itemsPerRow;

            Panel tableLayoutPanel = new Panel();
            tableLayoutPanel.Dock = DockStyle.None;
            tableLayoutPanel.Height = (rowCount <= 3 ? rowCount * MaxCellHeight : maxRowCount * MaxCellHeight) + 50;  //this.Height * 8 / 10;
            tableLayoutPanel.Width = cellcount <= itemsPerRow ? MaxCellWidth * cellcount : MaxCellWidth * itemsPerRow; //this.Width * 8 / 10;
            tableLayoutPanel.Top = this.Height / 2 - tableLayoutPanel.Height / 2;
            tableLayoutPanel.Left = this.Width / 2 - tableLayoutPanel.Width / 2;
            tableLayoutPanel.Padding = new Padding(20);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.AutoScroll = true;
            //tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            this.Controls.Add(tableLayoutPanel);

            int buttonSize = 74;
            int padding = 10;

            int cellwidth = MaxCellWidth; //tableLayoutPanel.Width / itemsPerRow;
            //// 计算行数
            //int rowCount = (programConfigs.Count + itemsPerRow - 1) / itemsPerRow;

            //// 设置列数
            //tableLayoutPanel.ColumnCount = itemsPerRow;
            //for (int i = 0; i < itemsPerRow; i++)
            //{
            //    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / itemsPerRow));
            //}

            //// 设置行数
            //tableLayoutPanel.RowCount = rowCount;
            //for (int i = 0; i < rowCount; i++)
            //{
            //    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            //}

            int currentRow = 0;
            int currentColumn = 0;
            int index = 0;
            foreach (var config in programConfigs)
            {
                // 创建面板来容纳按钮和标签
                Panel panel = new Panel();
                panel.Width = buttonSize + padding * 2;
                panel.Height = buttonSize + 30 + padding; // 30是标签的高度
                panel.Margin = new Padding(10);

                // 创建按钮
                Button button = new Button();
                button.Width = buttonSize;
                button.Height = buttonSize;
                button.Margin = new Padding(padding);
                button.Left = padding;
                button.Top = padding;

                // 设置按钮图标
                if (!string.IsNullOrEmpty(config.Icon) && File.Exists(config.Icon))
                {
                    try
                    {
                        button.Image = Image.FromFile(config.Icon);
                    }
                    catch { }
                }
                else if (File.Exists(config.Path))
                {
                    // 从可执行文件中提取图标
                    Icon? icon = IconExtractor2.GetIconForFile(config.Path);
                    if (icon != null)
                    {
                        // 缩放图标到buttonSize - padding x buttonSize - padding
                        Image image = new Bitmap(icon.ToBitmap(), buttonSize - padding, buttonSize - padding);
                        button.Image = image;
                    }
                }

                // 设置按钮点击事件
                button.Click += (sender, e) => OnButtonClick(config);

                // 创建标签
                Label label = new Label();
                label.Text = GetProgramName(config);
                label.Width = buttonSize + padding * 2;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Margin = new Padding(padding, 0, padding, padding);
                label.Left = 0;
                label.Top = buttonSize + padding;

                panel.Controls.Add(button);
                panel.Controls.Add(label);


                {
                    int col = index % itemsPerRow;
                    int row = index / itemsPerRow;
                    panel.Top = 120 * row + 60;
                    panel.Left = cellwidth * col;
                    index++;
                }

                // 添加到表格布局
                tableLayoutPanel.Controls.Add(panel);//, currentColumn, currentRow);

                // 更新行列索引
                currentColumn++;
                if (currentColumn >= itemsPerRow)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        private string GetProgramName(ProgramConfig config)
        {
            if (!string.IsNullOrEmpty(config.Name))
            {
                return config.Name;
            }
            else if (!string.IsNullOrEmpty(config.Path))
            {
                // 从路径中提取文件名（不含扩展名）
                string fileName = Path.GetFileNameWithoutExtension(config.Path) ?? string.Empty;
                return fileName;
            }
            else
            {
                return string.Empty;
            }
        }

        private void OnButtonClick(ProgramConfig config)
        {
            if (!File.Exists(config.Path))
            {
                return;
            }

            if (!config.Multi)
            {
                // 检查是否已有实例在运行
                string processName = Path.GetFileNameWithoutExtension(config.Path);
                Process[] processes = Process.GetProcessesByName(processName);

                foreach (Process process in processes)
                {
                    try
                    {
                        // 激活窗口并置于顶层
                        if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                        {
                            Renderers.RemoveShadowOnly(process.MainWindowHandle);
                            Renderers.SetWindowCornerPreference(process.MainWindowHandle, false);
                            // 移动窗口到当前AppPanel窗口的位置
                            MoveWindowToCurrentPanel(process.MainWindowHandle);

                            // 激活窗口
                            SetWindowPos(process.MainWindowHandle, (int)WindowPosition.Topmost, 0, 0, 0, 0, (uint)(WindowPositionFlags.NoMove | WindowPositionFlags.NoSize));
                            SetWindowPos(process.MainWindowHandle, (int)WindowPosition.NotTopmost, 0, 0, 0, 0, (uint)(WindowPositionFlags.NoMove | WindowPositionFlags.NoSize));
                            ShowWindow(process.MainWindowHandle, (int)ShowWindowCommands.Restore);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            // 启动新实例
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = config.Path;

            if (config.Params.Count > 0)
            {
                startInfo.Arguments = string.Join(" ", config.Params);
            }

            try
            {
                Process? process = Process.Start(startInfo);
                if (process != null)
                {
                    // 等待窗口创建
                    process.WaitForInputIdle(2000);
                    System.Threading.Thread.Sleep(2000);
                    process.Refresh();
                    // 尝试多次找到窗口
                    for (int i = 0; i < 5; i++)
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            Renderers.RemoveShadowOnly(process.MainWindowHandle);
                            Renderers.SetWindowCornerPreference(process.MainWindowHandle, false);
                            // 移动窗口到当前AppPanel窗口的位置
                            MoveWindowToCurrentPanel(process.MainWindowHandle);
                            break;
                        }
                        System.Threading.Thread.Sleep(2000);
                        process.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MoveWindowToCurrentPanel(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                // 获取当前AppPanel窗口的位置和大小
                int x = this.Left;
                int y = this.Top;
                int width = this.Width;
                int height = this.Height;

                // 移动窗口到当前AppPanel窗口的位置
                MoveWindow(hWnd, x, y, width, height, true);
            }
        }

        // 定义用于窗口操作的常量和方法
        private enum WindowPosition
        {
            Topmost = -1,
            NotTopmost = -2,
            Top = 0,
            Bottom = 1
        }

        private enum WindowPositionFlags
        {
            NoSize = 0x0001,
            NoMove = 0x0002,
            NoZOrder = 0x0004,
            NoRedraw = 0x0008,
            NoActivate = 0x0010,
            FrameChanged = 0x0020,
            ShowWindow = 0x0040,
            HideWindow = 0x0080,
            NoCopyBits = 0x0100,
            NoOwnerZOrder = 0x0200,
            NoSendChanging = 0x0400
        }

        private enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private void btnCloseAll_Click(object? sender, EventArgs e)
        {
            // 确认对话框
            DialogResult result = MessageBox.Show(
                "确定要关闭程序的所有实例吗？",
                "确认关闭",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CloseAllInstances();
            }
        }

        private void CloseAllInstances()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            int currentProcessId = Process.GetCurrentProcess().Id;

            // 获取所有同名进程
            Process[] processes = Process.GetProcessesByName(currentProcessName);

            // 首先尝试友好关闭其他实例
            foreach (Process process in processes)
            {
                if (process.Id != currentProcessId)
                {
                    try
                    {
                        // 尝试发送关闭消息到主窗口
                        if (!process.CloseMainWindow())
                        {
                            // 如果主窗口关闭失败，等待后强制关闭
                            if (!process.WaitForExit(2000))
                            {
                                process.Kill();
                            }
                        }
                        else
                        {
                            // 给友好关闭一些时间
                            process.WaitForExit(2000);
                        }
                    }
                    catch
                    {
                        // 忽略错误，继续处理下一个
                    }
                }
            }

            // 最后关闭当前程序
            Application.Exit();
        }
    }
}
