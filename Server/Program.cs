using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Data.Entity;

using NetCore;
using NetCore.Server;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;
using Data.Models;
using Shared;

namespace Server
{
    class Program
    {
        static Listener Listener { get; set; }

        static void Main(string[] args)
        {
            #region Initialise Data
            using (DataRepository Repo = new DataRepository())
            {
                Repo.Rooms.Add(new Room() { RoomName = "D6", SpecialSeats = 5, StandardSeats = 10, SpecialSeatsType = "Computer" });
                Repo.Rooms.Add(new Room() { RoomName = "D12", SpecialSeats = 20, StandardSeats = 5, SpecialSeatsType = "Workbench" });
                Repo.Rooms.Add(new Room() { RoomName = "Library", SpecialSeats = 20, StandardSeats = 30, SpecialSeatsType = "Computer" });
                Repo.Rooms.Add(new Room() { RoomName = "Sports Hall", SpecialSeats = 0, StandardSeats = 100, SpecialSeatsType = "" });

                Repo.Periods.Add(new TimeSlot() { Name = "Period 1", Start = new TimeSpan(12, 0, 0), End = new TimeSpan(13, 0, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 2", Start = new TimeSpan(13, 0, 0), End = new TimeSpan(14, 0, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 3", Start = new TimeSpan(14, 0, 0), End = new TimeSpan(15, 0, 0) });

                const string LogonName = DataRepository.Home ? "Keith" : "09135"; // For testing on home/school computers
                Repo.Students.Add(new Student() { FirstName = "Keith", LastName = "Collister", Form = "WT", Year = 13, LogonName = LogonName });
                Repo.Students.Add(new Student() { FirstName = "Max", LastName = "Norman", Form = "WT", Year = 13, LogonName = "Max" });
                Repo.Students.Add(new Student() { FirstName = "Dan", LastName = "Wrenn", Form = "MB", Year = 13, LogonName = "Dan" });

                Repo.Subjects.Add(new Subject() { SubjectName = "Maths", Colour = Colors.Red });
                Repo.Subjects.Add(new Subject() { SubjectName = "Physics", Colour = Colors.Orange });
                Repo.Subjects.Add(new Subject() { SubjectName = "Computing", Colour = Colors.Blue });

                Repo.Departments.Add(new Department() { Name = "Maths" });
                Repo.Departments.Add(new Department() { Name = "Science" });
                Repo.Departments.Add(new Department() { Name = "Computing/IT" });

                Repo.SaveChanges();

                Repo.Classes.Add(new Class() { ClassName = "Computing", Students = Repo.Students.ToList() });
                Repo.Classes.Add(new Class() { ClassName = "Maths", Students = Repo.Students.Where(s => s.Form == "13WT").ToList() });

                Repo.Teachers.Add(new Teacher() { Title = "Mrs", LogonName = "mb", FirstName = "Mary", LastName = "Bogdiukiewicz", Department = Repo.Departments.ToList().Where(d => d.Name.Contains("Computing")).Single() });
                Repo.Teachers.Add(new Teacher() { Title = "Mr", LogonName = "jk", FirstName = "J....", LastName = "Kenny", Department = Repo.Departments.Where(d => d.Name == "Science").Single() });
                Repo.Teachers.Add(new Teacher() { Title = "Mrs", LogonName = "rb", FirstName = "R....", LastName = "Britton", Department = Repo.Departments.Where(d => d.Name == "Maths").Single() });

                Repo.SaveChanges();

                Repo.Bookings.Add(new Booking()
                {
                    Rooms = Repo.Rooms.ToList().Where(r => r.RoomName[0] == 'D').ToList(),
                    Students = Repo.Students.ToList().Where(s => s.FullForm == "13WT").ToList(),
                    BookingType = BookingType.Single,
                    Date = DateTime.Now.Date,
                    Subject = Repo.Subjects.Where(s => s.SubjectName == "Maths").Single(),
                    Teacher = Repo.Teachers.Where(t => t.LastName == "Britton").Single(),
                    TimeSlot = Repo.Periods.Where(p => p.Name == "Period 1").Single(),
                });
                Repo.Bookings.Add(new Booking()
                {
                    Rooms = Repo.Rooms.Where(r => r.RoomName == "Library").ToList(),
                    Students = Repo.Students.ToList().Where(s => s.FullForm == "13MB").ToList(),
                    BookingType = BookingType.Weekly,
                    Date = DateTime.Now.Date,
                    Subject = Repo.Subjects.Where(s => s.SubjectName == "Computing").Single(),
                    Teacher = Repo.Teachers.Where(t => t.LastName == "Bogdiukiewicz").Single(),
                    TimeSlot = Repo.Periods.Where(p => p.Name == "Period 2").Single(),
                });

                Repo.SaveChanges();
            }
            #endregion

            Print("Initialised data", ConsoleColor.Gray);

            Listener = new Listener("127.0.0.1", 34652);
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
            Listener.Dispose();
        }

        static void ClientConnected(Listener Sender, Client c)
        {
            Print(c.ToString() + " connected", ConsoleColor.Green);
            DataSnapshot Frame = DataRepository.TakeSnapshot();
            c.Send(new InitialiseMessage(Frame));
            c.Send(new UserInformationMessage(Frame.Teachers.Union<User>(Frame.Students).Where(u => u.LogonName == c.Username).SingleOrDefault()));
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
            else if (Message is DataMessage)
            {
                DataMessage Data = (DataMessage)Message;
                using (DataRepository Repo = new DataRepository())
                    Data.Item.Expand(Repo);

                if (Data.Item is Booking)
                {
                    EditDataEntry((Booking)Data.Item, Data.Delete);
                    Output = "Booking received from " + c.ToString();
                }
            }

            if (Output != null)
                Print(Output, ConsoleColor.Gray);
        }
        static void EditDataEntry<T>(T Entry, bool Delete) where T : DataModel
        {
            using (DataRepository Repo = new DataRepository())
            {
                if (!Entry.Expand(Repo))
                    return;

                DbSet<T> Set = Repo.Set<T>();

                if (Delete)
                    Set.Remove(Set.Where(e => e.Id == Entry.Id).Single());
                else
                    Set.Add(Entry);

                Repo.SaveChanges();

                Listener.Send(new DataMessage(Entry, Delete));
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
