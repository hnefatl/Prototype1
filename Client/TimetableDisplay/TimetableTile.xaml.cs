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
    // Represents a single Tile on the timetable display
    public partial class TimetableTile
        : UserControl
    {
        // The time this tile represents
        public TimeSlot Time { get; protected set; }
        // The room this tile represents
        public Room Room { get; protected set; }

        // The Booking (or null) for the Booking in this slot
        public Booking Booking { get; protected set; }
        
        // The brush used to paint the background colour
        public SolidColorBrush Brush { get; protected set; }

        // Function object that determines how dark a tile gets when hovered over
        public Func<float, float> BrightnessCurve { get; set; }

        public TimetableTile(Booking Booking, TimeSlot Time, Room Room, User CurrentUser)
        {
            // Sets UI bindings to reference this object
            DataContext = this;

            this.Booking = Booking;
            this.Time = Time;
            this.Room = Room;

            // Set the default brightness function to the default
            BrightnessCurve = DefaultBrightnessCurve;

            // Default to the background window colour
            Brush = SystemColors.WindowBrush;
            if (Booking != null) // If there's actually a booking in this slot, use its colour
                Brush = new SolidColorBrush(Booking.Subject.Colour);
            Background = Brush;

            // Initialise the UI
            InitializeComponent();

            // Hook up mouse events
            MouseEnter += TimetableTile_MouseEnter;
            MouseLeave += TimetableTile_MouseLeave;

            // If there's a booking in this slot
            if (Booking != null && CurrentUser != null)
            {
                // If the current user is somehow involved in the booking (either 
                // as a student or teacher)
                if (((CurrentUser is Student) && Booking.Students.Any(s => s.Id == CurrentUser.Id)) ||
                    ((CurrentUser is Teacher) && Booking.Teacher == CurrentUser))
                {
                    // Do a simple animation on the tiles to draw attention
                    Storyboard PulseEffect = (Storyboard)Resources["PulseEffect"];
                    PulseEffect.Begin(Outer);
                }
            }
        }

        // When the mouse hovers over the tile
        protected void TimetableTile_MouseEnter(object sender, MouseEventArgs e)
        {
            // Change the background colour
            Background = new SolidColorBrush(ScaleLuminosity(Brush.Color));
        }
        // When the mouse stops hovering over the tile
        protected void TimetableTile_MouseLeave(object sender, MouseEventArgs e)
        {
            // Reset the background colour
            Background = new SolidColorBrush(Brush.Color);
        }

        // Initial brightness curve
        public float DefaultBrightnessCurve(float Y)
        {
            return (float)Math.Pow(Y, 3);
        }

        // Changes the brightness of the given colour using the brightness curve
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

            // Calculate the new brightness using the provided colour curve
            float NewBrightness = BrightnessCurve(YUV[0] / 255f);

            YUV[0] = Clamp((int)Math.Round(NewBrightness * 255f));
            byte[] ScaledRGB = YUVToRGB(YUV);

            return new Color() { A = c.A, R = ScaledRGB[0], G = ScaledRGB[1], B = ScaledRGB[2] };
        }

        // Converts an RGB array into a YUV array
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
        // Converts a YUV array into an RGB array
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
        // Limits the values taken by the input
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
