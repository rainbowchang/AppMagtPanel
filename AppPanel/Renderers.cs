using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppPanel
{
    internal class Renderers
    {
        private const int GWL_STYLE = -16;      // 获取/设置窗口样式
        private const int GWL_EXSTYLE = -20;    // 获取/设置窗口扩展样式 ← 就是这个

        // ==================== 窗口样式常量 (GWL_STYLE) ====================
        private const uint WS_CAPTION = 0x00C00000;        // 标题栏
        private const uint WS_THICKFRAME = 0x00040000;     // 可调整大小的边框
        private const uint WS_SYSMENU = 0x00080000;        // 系统菜单（左上角图标）
        private const uint WS_BORDER = 0x00800000;         // 细边框
        private const uint WS_DLGFRAME = 0x00400000;       // 对话框边框（无标题栏）
        private const uint WS_POPUP = 0x80000000;          // 弹出式窗口
        private const uint WS_MINIMIZEBOX = 0x00020000;    // 最小化按钮
        private const uint WS_MAXIMIZEBOX = 0x00010000;    // 最大化按钮

        // 组合常量：标准窗口的完整边框和标题栏
        private const uint WS_BORDER_MASK = WS_CAPTION | WS_THICKFRAME | WS_SYSMENU;

        // ==================== 扩展样式常量 (GWL_EXSTYLE) ====================
        private const uint WS_EX_CLIENTEDGE = 0x00000200;    // 客户区凹陷边缘（3D效果）
        private const uint WS_EX_STATICEDGE = 0x00020000;    // 静态边缘（用于只读控件）
        private const uint WS_EX_WINDOWEDGE = 0x00000100;    // 窗口边缘（凸起效果）
        private const uint WS_EX_DLGMODALFRAME = 0x00000001; // 对话框边框
        private const uint WS_EX_COMPOSITED = 0x02000000;    // 双缓冲（阴影相关）
        private const uint WS_EX_LAYERED = 0x00080000;       // 分层窗口（阴影相关）
        private const uint WS_EX_TRANSPARENT = 0x00000020;   // 透明窗口
        private const uint WS_EX_TOPMOST = 0x00000008;       // 置顶窗口
        private const uint WS_EX_TOOLWINDOW = 0x00000080;    // 工具窗口（窄标题栏）
        private const uint WS_EX_APPWINDOW = 0x00040000;     // 任务栏显示

        private const int DWMWA_NCRENDERING_POLICY = 2;      // 非客户区渲染策略
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33; // 窗口圆角偏好（Win11）

        private const int DWMNCRP_DISABLED = 1;               // 禁用非客户区渲染
        private const int DWMNCRP_ENABLED = 2;                // 启用非客户区渲染

        // ==================== SetWindowPos 常量 ====================
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int DWMWCP_DEFAULT = 0;      // 默认（由系统决定）
        private const int DWMWCP_DONOTROUND = 1;    // 不圆角（方角）
        private const int DWMWCP_ROUND = 2;         // 圆角
        private const int DWMWCP_ROUNDSMALL = 3;    // 小圆角

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// 保留边框但移除阴影
        /// </summary>
        public static bool RemoveShadowOnly(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                // 1. 通过 DWM 禁用非客户区渲染（这会影响阴影）
                int shadowPolicy = DWMNCRP_DISABLED;
                int result = DwmSetWindowAttribute(
                    hWnd,
                    DWMWA_NCRENDERING_POLICY,
                    ref shadowPolicy,
                    sizeof(int)
                );

                // 2. 可选：修改扩展样式，进一步抑制阴影
                int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                // WS_EX_COMPOSITED 和 WS_EX_LAYERED 与阴影相关
                const uint WS_EX_COMPOSITED = 0x02000000;
                const uint WS_EX_LAYERED = 0x00080000;
                int newExStyle = exStyle & ~(int)(WS_EX_COMPOSITED | WS_EX_LAYERED);
                SetWindowLong(hWnd, GWL_EXSTYLE, newExStyle);

                // 3. 刷新窗口使更改生效
                RefreshWindow(hWnd);

                return result == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除阴影失败: {ex.Message}");
                return false;
            }
        }

        private static void RefreshWindow(IntPtr hWnd)
        {
            const uint SWP_FRAMECHANGED = 0x0020;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER);
        }

        public static bool SetWindowCornerPreference(IntPtr hWnd, bool round)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                int preference = round ? DWMWCP_ROUND : DWMWCP_DONOTROUND;
                int result = DwmSetWindowAttribute(
                    hWnd,
                    DWMWA_WINDOW_CORNER_PREFERENCE,
                    ref preference,
                    sizeof(int)
                );

                return result == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置窗口圆角失败: {ex.Message}");
                return false;
            }
        }
    }
}
