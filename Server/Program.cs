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
                Repo.Departments.Add(new Department() { Name = "Maths" });
                Repo.Departments.Add(new Department() { Name = "Science" });
                Repo.Departments.Add(new Department() { Name = "Computing/IT" });
                Repo.Departments.Add(new Department() { Name = "History" });

                Repo.SaveChanges();
                
                List<string> ComputerNames = GenerateComputerNames();
                Repo.Rooms.Add(new Room() { RoomName = "D6", SpecialSeats = 5, StandardSeats = 10, SpecialSeatType = "Computer", Department = Repo.Departments.Single(d => d.Name == "Computing/IT"), ComputerNames = ComputerNames });
                Repo.Rooms.Add(new Room() { RoomName = "D12", SpecialSeats = 20, StandardSeats = 5, SpecialSeatType = "Workbench", Department = Repo.Departments.Single(d => d.Name == "Computing/IT"), ComputerNames = ComputerNames });
                Repo.Rooms.Add(new Room() { RoomName = "Library", SpecialSeats = 20, StandardSeats = 30, SpecialSeatType = "Computer", Department = Repo.Departments.Single(d => d.Name == "Science") });
                Repo.Rooms.Add(new Room() { RoomName = "Sports Hall", SpecialSeats = 0, StandardSeats = 100, SpecialSeatType = "", Department = Repo.Departments.Single(d => d.Name == "Maths") });

                //Repo.Periods.Add(new TimeSlot() { Name = "Period 1", Start = new TimeSpan(8, 50, 0), End = new TimeSpan(9, 50, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 1", Start = new TimeSpan(15, 0, 0), End = new TimeSpan(23, 0, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 2", Start = new TimeSpan(9, 50, 0), End = new TimeSpan(10, 50, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 3", Start = new TimeSpan(11, 10, 0), End = new TimeSpan(12, 10, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 4", Start = new TimeSpan(12, 10, 0), End = new TimeSpan(13, 10, 0) });
                Repo.Periods.Add(new TimeSlot() { Name = "Period 5", Start = new TimeSpan(14, 0, 0), End = new TimeSpan(15, 0, 0) });

                string LogonName = DataRepository.Home ? "Keith" : "09135"; // For testing on home versus school computers
                Repo.Students.Add(new Student() { FirstName = "Keith", LastName = "Collister", Form = "WT", Year = 13, LogonName = LogonName, Access = AccessMode.Admin });
                Repo.Students.Add(new Student() { FirstName = "Max", LastName = "Norman", Form = "WT", Year = 13, LogonName = "Max" });
                Repo.Students.Add(new Student() { FirstName = "Dan", LastName = "Wrenn", Form = "MB", Year = 13, LogonName = "Dan" });
                Repo.Students.Add(new Student() { FirstName = "Peter", LastName = "Champion", Form = "WT", Year = 13, LogonName = "Peter" });
                Repo.Students.Add(new Student() { FirstName = "Euan", LastName = "Rossie", Form = "WT", Year = 13, LogonName = "09185" });
                Repo.Students.Add(new Student() { FirstName = "Mia", LogonName = "Mia", LastName = "West", Form = "MB", Year = 13 });
                Repo.Students.Add(new Student() { FirstName = "Matthew", LogonName = "Matthew", LastName = "Pilkington", Form = "WT", Year = 13 });
                Repo.Students.Add(new Student() { FirstName = "Kaleb", LogonName = "Kaleb", LastName = "Poole", Form = "BR", Year = 11 });
                Repo.Students.Add(new Student() { FirstName = "Sam", LogonName = "Sam", LastName = "Kitto", Form = "BR", Year = 11 });
                Repo.Students.Add(new Student() { FirstName = "Isobel", LogonName = "Isobel", LastName = "Stephens", Form = "MI", Year = 11 });

                Repo.Subjects.Add(new Subject() { SubjectName = "Maths", Colour = Colors.Red });
                Repo.Subjects.Add(new Subject() { SubjectName = "Physics", Colour = Colors.Orange });
                Repo.Subjects.Add(new Subject() { SubjectName = "Computing", Colour = Colors.Blue });
                Repo.Subjects.Add(new Subject() { SubjectName = "History", Colour = Colors.Green });

                Repo.SaveChanges();

                Repo.Teachers.Add(new Teacher() { Title = "Mrs", LogonName = "mb", FirstName = "Mary", LastName = "Bogdiukiewicz", Department = Repo.Departments.ToList().Where(d => d.Name.Contains("Computing")).Single(), Email = "hnefatl@gmail.com" });
                Repo.Teachers.Add(new Teacher() { Title = "Mr", LogonName = "pc", FirstName = "Patrick", LastName = "Count", Department = Repo.Departments.ToList().Where(d => d.Name.Contains("Computing")).Single(), Email = "hnefatl@gmail.com" });
                Repo.Teachers.Add(new Teacher() { Title = "Mr", LogonName = "jk", FirstName = "James", LastName = "Kenny", Department = Repo.Departments.Where(d => d.Name == "Science").Single(), Email = "hnefatl@gmail.com" });
                Repo.Teachers.Add(new Teacher() { Title = "Mrs", LogonName = "rb", FirstName = "Rosemary", LastName = "Britton", Department = Repo.Departments.Where(d => d.Name == "Maths").Single(), Email = "hnefatl@gmail.com" });
                Repo.Teachers.Add(new Teacher() { Title = "Mrs", LogonName = "ed", FirstName = "Emma", LastName = "Denny", Department = Repo.Departments.ToList().Where(d => d.Name.Contains("History")).Single(), Email = "hnefatl@gmail.com" });

                Repo.SaveChanges();

                Repo.Classes.Add(new Class() { ClassName = "Computing Yr13", Students = Repo.Students.Where(s => s.FirstName == "Keith" || s.FirstName == "Max" || s.FirstName == "Dan" || s.FirstName == "Peter").ToList(), Owner = Repo.Teachers.Where(t => t.LogonName == "mb").Single() });
                Repo.Classes.Add(new Class() { ClassName = "Maths Yr13", Students = Repo.Students.Where(s => s.Form == "WT").ToList(), Owner = Repo.Teachers.Where(t => t.LogonName == "rb").Single() });
                Repo.Classes.Add(new Class() { ClassName = "History Yr11", Students = Repo.Students.Where(s => s.Year == 11).ToList(), Owner = Repo.Teachers.Where(t => t.LogonName == "rb").Single() });

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

            bool Closing = false;
            Listener = new Listener(34652);
            try
            {
                Listener.ClientConnect += ClientConnected;
                Listener.ClientDisconnect += ClientDisconnect;
                Listener.ClientMessageReceived += ClientMessageReceived;
                Listener.Start(false);
                Print("Listener started...", ConsoleColor.Green);

                Console.ReadKey(true);

                Closing = true;
                Listener.Stop();
                Print("Listener stopped...", ConsoleColor.Red);
                Listener.ClientConnect -= ClientConnected;
                Listener.ClientDisconnect -= ClientDisconnect;
                Listener.ClientMessageReceived -= ClientMessageReceived;
            }
            catch (Exception e)
            {
                if (!Closing)
                {
                    Print("Error: " + e.ToString(), ConsoleColor.Red);
                    Console.ReadKey(true);
                }
            }
            try
            {
                Listener.Dispose();
            }
            catch { }
        }

        static void ClientConnected(Listener Sender, Client c)
        {
            Print(c.ToString() + " connected. Connected clients: " + Sender.Clients.Count, ConsoleColor.Green);
            DataSnapshot Frame = DataRepository.TakeSnapshot();
            c.Send(new InitialiseMessage(Frame));
            c.Send(new UserInformationMessage(Frame.Users.Where(u => u.LogonName == c.Username).SingleOrDefault(), Frame.Rooms.Where(r => r.ComputerNames.Contains(c.ComputerName)).FirstOrDefault()));
        }
        static void ClientDisconnect(Listener Sender, Client c, DisconnectMessage Message)
        {
            Print(c.ToString() + " disconnected. Reason: " + Message.Reason.ToString() + ". Connected clients: " + Sender.Clients.Count, ConsoleColor.DarkGreen);
        }
        static void ClientMessageReceived(Listener Sender, Client c, Message Message)
        {
            string Output = null;
            if (Message is TestMessage)
                Output = "Message received from " + c.ToString();
            else if (Message is DataMessage)
            {
                DataMessage Data = (DataMessage)Message;

                if (Data.Item is Booking)
                {
                    using (DataRepository Repo = new DataRepository())
                        Data.Item.Expand(Repo);

                    bool Edited = EditDataEntry((Booking)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Booking received from " + c.ToString();
                    if (MailHelper.Send((Booking)Data.Item, Edited))
                        Output += "    Email sent to teacher.";
                    else
                        Output += "    Email failed to send.";
                }
                else if (Data.Item is Class)
                {
                    bool Edited = EditDataEntry((Class)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Class received from " + c.ToString();
                }
                else if (Data.Item is Department)
                {
                    bool Edited = EditDataEntry((Department)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Department received from " + c.ToString();
                }
                else if (Data.Item is Room)
                {
                    bool Edited = EditDataEntry((Room)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Room received from " + c.ToString();
                }
                else if (Data.Item is Subject)
                {
                    bool Edited = EditDataEntry((Subject)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Subject received from " + c.ToString();
                }
                else if (Data.Item is User)
                {
                    bool Edited = EditDataEntry((User)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " User received from " + c.ToString();
                }
                else if (Data.Item is TimeSlot)
                {
                    bool Edited = EditDataEntry((TimeSlot)Data.Item, Data.Delete);
                    Output = (Data.Delete ? "Delete" : Edited ? "Edit" : "Add") + " Period received from " + c.ToString();
                }
            }

            if (Output != null)
                Print(Output, ConsoleColor.Gray);
        }
        static bool EditDataEntry<T>(T Entry, bool Delete) where T : DataModel
        {
            bool Edited = false;
            using (DataRepository Repo = new DataRepository())
            {
                Repo.SetProxies(true);
                DbSet<T> Set = Repo.Set<T>();

                if (!Delete)
                    Entry.Expand(Repo);

                if (Delete)
                    Set.Remove(Set.Single(e => e.Id == Entry.Id));
                else
                {
                    // Check for conflicts if necessary
                    if (!Entry.Conflicts(Set.ToList().Cast<DataModel>().ToList()))
                    {
                        if (Set.Any(m => m.Id == Entry.Id)) // Updating existing item
                        {
                            Set.ToList().Single(m => m.Id == Entry.Id).Update(Entry);
                            Edited = true;
                        }
                        else
                        {
                            Set.Add(Entry);
                            Edited = false;
                        }
                    }
                }

                Repo.SaveChanges();

                Listener.Send(new DataMessage(Entry, Delete));
            }
            return Edited;
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



        static List<string> GenerateComputerNames()
        {
            List<string> Results = new List<string>();

            for (int x = 6; x <= 12; x += 6)
                for (int y = 0; y <= 40; y++)
                    Results.Add("D" + x + "D" + y);
            Results.Add("KEITH-PC");

            return Results;
        }
    }
}
