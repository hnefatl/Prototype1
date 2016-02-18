using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Data.Models;
using System.Windows.Media.Animation;

namespace Client.TimetableDisplay
{
    public partial class TimetableTile
        : UserControl
    {
        public TimeSlot Time { get; protected set; }
        public Room Room { get; protected set; }
        public Booking Booking { get; protected set; }


        public SolidColorBrush Brush { get; protected set; }

        public TimetableTile(Booking Booking, TimeSlot Time, Room Room, User CurrentUser)
        {
            DataContext = this;

            this.Booking = Booking;
            this.Time = Time;
            this.Room = Room;

            BrightnessCurve = DefaultBrightnessCurve;

            Brush = SystemColors.WindowBrush;
            if (Booking != null)
                Brush = new SolidColorBrush(Booking.Subject.Colour);
            Background = Brush;

            InitializeComponent();

            MouseEnter += TimetableTile_MouseEnter;
            MouseLeave += TimetableTile_MouseLeave;

            if (Booking != null && CurrentUser != null && (CurrentUser is Student) && Booking.Students.Any(s => s.Id == CurrentUser.Id))
            {
                Storyboard PulseEffect = (Storyboard)Resources["PulseEffect"];
                PulseEffect.Begin(Outer);
            }
        }

        protected void TimetableTile_MouseEnter(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(ScaleLuminosity(Brush.Color));
        }
        protected void TimetableTile_MouseLeave(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(Brush.Color);
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

            // Calculate the new brightness using the provided colour curve. The default is x^3
            float NewBrightness = BrightnessCurve(YUV[0] / 255f);

            YUV[0] = Clamp((int)Math.Round(NewBrightness * 255f));
            byte[] ScaledRGB = YUVToRGB(YUV);

            return new Color() { A = c.A, R = ScaledRGB[0], G = ScaledRGB[1], B = ScaledRGB[2] };
        }
        private byte[] RGBToYUV(byte[] RGB)
        {
            if (RGB.Length != 3)
                throw new ArgumentException("Invalid number of bytes provided.");
            // Calculations from https://msdn.microsoft.com/en-us/library/aa917087.aspx?f=255&MSPPError=-2147217396
            int R = RGB[0];
            int G = RGB[1];
            int B = RGB[2];

            int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
            int U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
            int V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

            return new byte[] { Clamp(Y), Clamp(U), Clamp(V) };
        }
        private byte[] YUVToRGB(byte[] YUV)
        {
            if (YUV.Length != 3)
                throw new ArgumentException("Invalid number of bytes provided.");
            // Calculations from https://msdn.microsoft.com/en-us/library/aa917087.aspx?f=255&MSPPError=-2147217396
            int C = YUV[0] - 16;
            int D = YUV[1] - 128;
            int E = YUV[2] - 128;

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
