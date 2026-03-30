using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppPanel
{
    internal class IconExtractor2
    {
        public static Icon? GetIconForFile(string filePath)
        {
            using Icon? largeIcon = Icon.ExtractIcon(filePath, 0, 64);
            if (largeIcon != null) return (Icon)largeIcon.Clone();
            using Icon? smallIcon = Icon.ExtractIcon(filePath, 0, 32);
            if (smallIcon != null) return (Icon)smallIcon.Clone();
            else return null;
        }
    }
}
