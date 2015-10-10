using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCore;
using NetCore.Server;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;
using Data.Models;
using System.Windows.Media;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initialise Data
            using (DataRepository Repo = new DataRepository())
            {
                Repo.Rooms.Add(new Room() { RoomName = "D6", SpecialSeats = 5, StandardSeats = 10, SpecialSeatsType = "Computer" });
                Repo.Rooms.Add(new Room() { RoomName = "D12", SpecialSeats = 20, StandardSeats = 5, SpecialSeatsType = "Workbench" });

                Repo.Periods.Add(new TimeSlot() { Name = "Period 1", Start = new TimeSpan(12, 0, 0), End = new TimeSpan(13, 0, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 2", Start = new TimeSpan(13, 0, 0), End = new TimeSpan(14, 0, 0) });

                Repo.Students.Add(new Student() { FirstName = "Keith", LastName = "Collister", Form = "WT", Year = 13 });
                Repo.Students.Add(new Student() { FirstName = "Max", LastName = "Norman", Form = "WT", Year = 13 });
                Repo.Students.Add(new Student() { FirstName = "Dan", LastName = "Wrenn", Form = "MB", Year = 13 });

                Repo.Subjects.Add(new Subject() { SubjectName = "Maths", Colour = Colors.Red });

                Repo.Departments.Add(new Department() { Name = "Maths" });

                Repo.SaveChanges();

                Repo.Teachers.Add(new Teacher() { Title = "Mr", FirstName = "Bob", LastName = "Doherty", Department = Repo.Departments.Where(d => d.Name == "Maths").Single() });

                Repo.SaveChanges();

                Repo.Bookings.Add(new Booking()
                {
                    Rooms = Repo.Rooms.ToList().Where(r => r.RoomName[0] == 'D').ToList(),
                    Students = Repo.Students.ToList().Where(s => s.FullForm == "13WT").ToList(),
                    BookingType = BookingType.Single,
                    Date = DateTime.Now.Date,
                    Subject = Repo.Subjects.Where(s => s.SubjectName == "Maths").Single(),
                    Teacher = Repo.Teachers.Where(t => t.FirstName == "Bob").Single(),
                    TimeSlot = Repo.Periods.Where(p => p.Name == "Period 1").Single(),
                });

                Repo.SaveChanges();
            }
            #endregion

            Print("Initialised data", ConsoleColor.Gray);

            using (Listener Listener = new Listener("127.0.0.1", 34652))
            {
                try
                {
                    Listener.ClientConnect += ClientConnected;
                    Listener.ClientDisconnect += ClientDisconnect;
                    Listener.ClientMessageReceived += ClientMessageReceived;
                    Listener.Start(false);
                    Print("Listener started...", ConsoleColor.Green);

                    Console.ReadKey(true);

                    Listener.Stop();
                    Print("Listener stopped...", ConsoleColor.Red);
                    Listener.ClientConnect -= ClientConnected;
                    Listener.ClientDisconnect -= ClientDisconnect;
                    Listener.ClientMessageReceived -= ClientMessageReceived;
                }
                catch (Exception e)
                {
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: " + e.ToString());
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.ReadKey(true);
                }
            }
        }

        static void ClientConnected(Listener Sender, Client c)
        {
            Print(c.ToString() + " connected", ConsoleColor.Green);
            c.Send(new InitialiseMessage(DataRepository.TakeSnapshot()));
        }
        static void ClientDisconnect(Listener Sender, Client c, DisconnectMessage Message)
        {
            Print(c.ToString() + " disconnected. Reason: " + Message.Reason.ToString(), ConsoleColor.DarkGreen);
        }
        static void ClientMessageReceived(Listener Sender, Client c, Message Message)
        {
            string Output = null;
            if (Message is TestMessage)
                Output = "Message received from " + c.ToString();
            else if (Message is NewBookingMessage)

                if (Output != null)
                {
                    Print(Output, ConsoleColor.Gray);
                }
        }

        static void Print(string Text, ConsoleColor Colour)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = Colour;
                Console.WriteLine(Text);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
