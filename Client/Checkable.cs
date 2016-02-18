using System;
using System.ComponentModel;

namespace Client
{
    public class Checkable<T>
        : INotifyPropertyChanged
    {
        // Whether the object is selected
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

        // The object that can be selected
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
            : this(Activator.CreateInstance<T>(), false)
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
