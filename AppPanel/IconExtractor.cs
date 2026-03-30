using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AppPanel
{
    [System.Obsolete("Use IconExtractor2 instead")]
    public class IconExtractor
    {
        [DllImport("Shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon? ExtractIcon(string filePath, int iconIndex = -3)
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int count = ExtractIconEx(filePath, iconIndex, largeIcons, smallIcons, 1);

            if (count > 0 && largeIcons[0] != IntPtr.Zero)
            {
                Icon icon = Icon.FromHandle(largeIcons[0]);
                // 创建图标副本，因为我们需要销毁原始句柄
                Icon iconCopy = (Icon)icon.Clone();
                DestroyIcon(largeIcons[0]);
                return iconCopy;
            }

            return null;
        }

        public static Icon? GetIconForFile(string filePath)
        {
            return ExtractIcon(filePath);
        }
    }
}