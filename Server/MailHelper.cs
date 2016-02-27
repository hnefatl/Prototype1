using System.Linq;
using System.Net;
using System.Net.Mail;

using Data.Models;

namespace Server
{
    public class MailHelper
    {
        // Email Address to send from
        public const string SenderAddress = "hnefatl@gmail.com";
        // Password of above email address
        public const string SenderPassword = "9j*E1D~y1235810";
        // Mail server to use
        public const string SMTPServer = "smtp.gmail.com";
        // Port on mail server
        public const int SMTPPort = 587;

        // Tries to send an email to the Teacher specified by the given Booking,
        // containing information on the booking itself, and whether it was edited/created
        public static bool Send(Booking Booking, bool Edited)
        {
            try
            {
                // Make a new Email socket
                using (SmtpClient Client = new SmtpClient(SMTPServer, SMTPPort))
                {
                    // Secure, specific credentials, over network delivery
                    Client.EnableSsl = true;
                    Client.UseDefaultCredentials = false;
                    Client.Credentials = new NetworkCredential(SenderAddress, SenderPassword);
                    Client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Create the message and fill out the fields
                    MailMessage Message = new MailMessage();
                    Message.From = new MailAddress(SenderAddress);
                    Message.To.Add(new MailAddress(Booking.Teacher.Email));

                    // Pick an appropriate subject based on what happened to the booking
                    Message.Subject = Edited ? "One of your bookings has been edited" : "You've made a new booking";
                    // Fill out the body using information from the Booking object
                    Message.Body = (Edited ? "One of your bookings has been edited." : "You've made a new booking.") + "\r\n\r\n" +
                        "Date: " + Booking.Date.ToShortDateString() + "\r\n" +
                        "Period: " + Booking.TimeSlot + "\r\n" +
                        "Rooms: " + Booking.Rooms.Aggregate("", (a, r) => { return a + r.RoomName + ", "; }).TrimEnd(',', ' ') + "\r\n" +
                        "Recurrence: " + Booking.BookingType + "\r\n" +
                        "Subject: " + Booking.Subject.SubjectName + "\r\n" +
                        "Students: " + Booking.Students.Count + "\r\n";

                    // Send off the message
                    Client.SendAsync(Message, null);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
