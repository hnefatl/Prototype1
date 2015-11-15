using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

using Data.Models;

namespace Server
{
    public class MailHelper
    {
        public const string SenderAddress = "hnefatl@gmail.com";
        public const string SenderPassword = "9j*E1D~y1235810";

        public const string SMTPServer = "smtp.gmail.com";
        public const int SMTPPort = 587;

        public static bool Send(Booking Booking, bool Edited)
        {
            try
            {
                using (SmtpClient Client = new SmtpClient(SMTPServer, SMTPPort))
                {
                    Client.EnableSsl = true;
                    Client.UseDefaultCredentials = false;
                    Client.Credentials = new NetworkCredential(SenderAddress, SenderPassword);
                    Client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    MailMessage Message = new MailMessage();
                    Message.From = new MailAddress(SenderAddress);
                    Message.To.Add(new MailAddress(Booking.Teacher.Email));
                    Message.Subject = Edited ? "One of your bookings has been edited" : "You've made a new booking";
                    Message.Body = (Edited ? "One of your bookings has been edited." : "You've made a new booking.") + "\r\n\r\n" +
                        "Date: " + Booking.Date.ToShortDateString() + "\r\n" +
                        "Period: " + Booking.TimeSlot + "\r\n" +
                        "Rooms: " + Booking.Rooms.Aggregate("", (a, r) => { return a + r.RoomName + ", "; }).TrimEnd(',', ' ') + "\r\n" +
                        "Recurrence: " + Booking.BookingType + "\r\n" +
                        "Subject: " + Booking.Subject.SubjectName + "\r\n" +
                        "Students: " + Booking.Students.Count + "\r\n";

                    Client.Send(Message);
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
