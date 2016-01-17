using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Shared
{
    public static class Helpers
    {
        public static Color IntToColour(int i)
        {
            byte[] Bytes = BitConverter.GetBytes(i);
            return Color.FromArgb(Bytes[3], Bytes[2], Bytes[1], Bytes[0]);
        }
        public static int ColorToInt(Color c)
        {
            return BitConverter.ToInt32(new byte[] { c.B, c.G, c.R, c.A }, 0);
        }
    }
}
