using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Client.TimetableDisplay
{
    public class Checkable<T>
        : INotifyPropertyChanged
    {
        protected bool _Checked;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                OnPropertyChanged("Checked");
            }
        }

        protected T _Value;
        public T Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                OnPropertyChanged("Value");
            }
        }

        public Checkable()
            : this(default(T), false)
        {
        }
        public Checkable(T Value)
            : this(Value, false)
        {
        }
        public Checkable(T Value, bool Checked)
        {
            this.Value = Value;
            this.Checked = Checked;
            PropertyChanged = delegate { };
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
