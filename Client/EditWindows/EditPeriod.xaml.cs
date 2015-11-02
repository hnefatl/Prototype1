﻿using System;
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
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditPeriod
        : Window, INotifyPropertyChanged
    {
        protected string _RoomName;
        public string RoomName
        {
            get
            {
                return _RoomName;
            }
            set
            {
                _RoomName = value;
                OnPropertyChanged("RoomName");
            }
        }

        protected string _Start;
        public string Start
        {
            get
            {
                return _Start;
            }
            set
            {
                _Start = value;
                OnPropertyChanged("Start");
            }
        }

        protected string _End;
        public string End
        {
            get
            {
                return _End;
            }
            set
            {
                _End = value;
                OnPropertyChanged("End");
            }
        }

        public int PeriodId { get; set; }

        public EditPeriod(TimeSlot Existing)
        {
            InitializeComponent();

            if (Existing == null)
            {
                RoomName = string.Empty;
                Start = string.Empty;
                End = string.Empty;
                PeriodId = 0;
            }
            else
            {
                RoomName = Existing.Name;
                Start = Convert.ToString(Existing.Start.Hours).PadLeft(2, '0') + ":" + Convert.ToString(Existing.Start.Minutes).PadLeft(2, '0');
                End = Convert.ToString(Existing.End.Hours).PadLeft(2, '0') + ":" + Convert.ToString(Existing.End.Minutes).PadLeft(2, '0');
                PeriodId = Existing.Id;
            }
        }

        public TimeSlot GetPeriod()
        {
            TimeSlot New = new TimeSlot();
            try
            {
                New.Name = RoomName;
                New.Start = TimeSpan.Parse(Start);
                New.End = TimeSpan.Parse(End);
                New.Id = PeriodId;
            }
            catch
            {
                return null;
            }
            return New;
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string Error = null;

            TimeSpan Out;
            if (string.IsNullOrWhiteSpace(RoomName))
                Error = "You must enter a room name";
            else if (string.IsNullOrWhiteSpace(Start) || !TimeSpan.TryParse(Start, out Out))
                Error = "Invalid start time format.";
            else if (string.IsNullOrWhiteSpace(End) || !TimeSpan.TryParse(End, out Out))
                Error = "Invalid end time format";

            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error", MessageBoxButton.OK);
            else
            {
                DialogResult = true;
                Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}