using System;
using System.Windows;
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    // Provides a generic base class for all the edit windows, reduces their code
    public abstract class EditWindow<T>
        : Window, INotifyPropertyChanged where T : DataModel
    {
        public EditWindow()
        {
            PropertyChanged = delegate { };
        }

        // Gets the item that's been constructed by the EditWindow
        public abstract T GetItem();

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
