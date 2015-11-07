using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    public abstract class EditWindow<T>
        : Window, INotifyPropertyChanged where T : DataModel
    {
        public EditWindow()
        {
            PropertyChanged = delegate { };
        }

        public abstract T GetItem();

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
