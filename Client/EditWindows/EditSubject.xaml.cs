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

namespace Client.EditWindows
{
    public partial class EditSubject
        : EditWindow<Subject>
    {
        protected string _SubjectName;
        public string SubjectName
        {
            get { return _SubjectName; }
            set { _SubjectName = value; OnPropertyChanged("SubjectName"); }
        }

        public SolidColorBrush Colour
        {
            get
            {
                return new SolidColorBrush(new Color()
                {
                    R = Convert.ToByte(ColourRed),
                    G = Convert.ToByte(ColourGreen),
                    B = Convert.ToByte(ColourBlue)
                });
            }
            set
            {
                ColourRed = Convert.ToString(value.Color.R);
                ColourGreen = Convert.ToString(value.Color.G);
                ColourBlue = Convert.ToString(value.Color.B);
            }
        }

        protected string _ColourRed;
        public string ColourRed
        {
            get { return _ColourRed; }
            set { _ColourRed = value; OnPropertyChanged("ColourRed"); }
        }

        protected string _ColourGreen;
        public string ColourGreen
        {
            get { return _ColourGreen; }
            set { _ColourGreen = value; OnPropertyChanged("ColourGreen"); }
        }

        protected string _ColourBlue;
        public string ColourBlue
        {
            get { return _ColourBlue; }
            set { _ColourBlue = value; OnPropertyChanged("ColourBlue"); }
        }

        protected int SubjectId { get; set; }
        protected List<Booking> Bookings { get; set; }

        public EditSubject(Subject Existing)
        {
            InitializeComponent();

            if (Existing == null)
            {
                SubjectId = 0;
                SubjectName = string.Empty;
                Colour = Brushes.Black;
                Bookings = new List<Booking>();
            }
            else
            {
                SubjectId = Existing.Id;
                SubjectName = Existing.SubjectName;
                Colour = new SolidColorBrush(Existing.Colour);
                Bookings = Existing.Bookings;
            }
        }

        public override Subject GetItem()
        {
            return new Subject() { Id = SubjectId, SubjectName = SubjectName, Colour = Colour.Color, Bookings = Bookings };
        }
    }
}
