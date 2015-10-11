using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Data.Models;

namespace Client.TimetableDisplay
{
    public class TimetableTile
        : StackPanel
    {
        public TimeSlot Time { get; set; }
        public Room Room { get; set; }
        public Booking Booking { get; set; }

        protected SolidColorBrush _Brush;
        public SolidColorBrush Brush
        {
            get
            {
                return _Brush;
            }
            set
            {
                _Brush = value;
                Background = new SolidColorBrush(Brush.Color);
            }
        }

        public TimetableTile()
        {
            MouseEnter += TimetableTile_MouseEnter;
            MouseLeave += TimetableTile_MouseLeave;

            BrightnessCurve = DefaultBrightnessCurve;

            Orientation = Orientation.Vertical;
        }

        protected void TimetableTile_MouseEnter(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(ScaleLuminosity(Brush.Color));
        }
        protected void TimetableTile_MouseLeave(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(Brush.Color); // Copy to prevent referencing
        }

        public float DefaultBrightnessCurve(float Y)
        {
            return (float)Math.Pow(Y, 3);
        }
        public Func<float, float> BrightnessCurve { get; set; }

        private Color ScaleLuminosity(Color c)
        {
            // To scale a colour without weird results (as RGB components have different perceived strengths to the human eye),
            // convert the RGB to YUV, which then lets you scale the Y value to adjust the "brightness".
            // Convert the scaled YUV back to RGB, and create a colour with the same values
            // We also keep the same Alpha as the original colour as it doesn't affect the "brightness" of the colour

            // To darken a colour, the scale should be less than 1.0, to brighten it should be above 1.0
            // If scale s is applied to a colour (r,g,b), then the resultant colour is (sr, sg, sb). To revert back to the
            // original colour, we must scale by 1/s, giving us (r,g,b) again.

            byte[] YUV = RGBToYUV(new byte[] { c.R, c.G, c.B });
            YUV[0] = Clamp((int)Math.Round(BrightnessCurve(YUV[0] / 255f) * 255f));
            byte[] ScaledRGB = YUVToRGB(YUV);

            return new Color() { A = c.A, R = ScaledRGB[0], G = ScaledRGB[1], B = ScaledRGB[2] };
        }
        private byte[] RGBToYUV(byte[] c)
        {
            // Calculations from https://msdn.microsoft.com/en-us/library/aa917087.aspx?f=255&MSPPError=-2147217396
            int R = c[0];
            int G = c[1];
            int B = c[2];

            int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
            int U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
            int V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

            return new byte[] { Clamp(Y), Clamp(U), Clamp(V) };
        }
        private byte[] YUVToRGB(byte[] c)
        {
            // Calculations from https://msdn.microsoft.com/en-us/library/aa917087.aspx?f=255&MSPPError=-2147217396
            int C = c[0] - 16;
            int D = c[1] - 128;
            int E = c[2] - 128;

            int R = (298 * C + 409 * E + 128) >> 8;
            int G = (298 * C - 100 * D - 208 * E + 128) >> 8;
            int B = (298 * C + 516 * D + 128) >> 8;

            return new byte[] { Clamp(R), Clamp(G), Clamp(B) };
        }
        private byte Clamp(int x)
        {
            if (x < byte.MinValue)
                return byte.MinValue;
            else if (x > byte.MaxValue)
                return byte.MaxValue;
            else
                return (byte)x;
        }
    }
}
