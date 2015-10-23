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
using System.Text.RegularExpressions;

namespace Client.Admin
{
    public partial class EditWindow
        : Window
    {
        protected Dictionary<string, EditItem> Items { get; set; }

        private EditWindow(Dictionary<string, EditItem> Items)
        {
            InitializeComponent();

            this.Items = Items;
        }
        private void UpdateUI()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)UpdateUI);
            else
            {
                Grid_Controls.RowDefinitions.Clear();
                int x = 0;
                foreach (KeyValuePair<string, EditItem> i in Items)
                {
                    Grid_Controls.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                    TextBlock Label = new TextBlock();
                    Label.Text = i.Value.Label + ":";
                    Grid_Controls.Children.Add(Label);
                    Label.SetValue(Grid.RowProperty, x);
                    Label.SetValue(Grid.ColumnProperty, 1);

                    Control Control = i.Value.GetControl();
                    Grid_Controls.Children.Add(Control);
                    Control.SetValue(Grid.RowProperty, x);
                    Control.SetValue(Grid.ColumnProperty, 1);

                    x++;
                }
            }
        }

        private Dictionary<string, EditItem> Load()
        {

        }

        public Dictionary<string, object> Show(Dictionary<string, EditItem> Items)
        {
            EditWindow Wnd = new EditWindow(Items);
            Wnd.UpdateUI();

            bool? Result = Wnd.ShowDialog();
            if (Result.HasValue && Result.Value)
                return Wnd.Load().Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Value));
            else
                return null;
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class EditItem
    {
        public object Value { get; set; }
        public Type ValueType { get { return Value.GetType(); } }

        public string Label { get; set; }

        public EditItem(object DefaultValue, string Label)
        {
            Value = DefaultValue;
            this.Label = Label;
        }

        public Control GetControl()
        {
            if (ValueType == typeof(string))
                return new TextBox() { Text = Value as string };
            else if (ValueType.IsEnum)
                return new ComboBox() { ItemsSource = Enum.GetNames(ValueType), SelectedItem = Enum.GetName(ValueType, Value) };
            else if (ValueType == typeof(bool))
                return new CheckBox() { IsChecked = (bool)Value };
            else
                throw new NotSupportedException("Invalid type");
        }
        public void LoadFromControl(Control c)
        {
            if (ValueType == typeof(string) && c is TextBox)
                Value = (c as TextBox).Text;
            else if (ValueType.IsEnum && c is ComboBox)
                Value = Enum.Parse(ValueType, (string)(c as ComboBox).SelectedItem);
            else if (ValueType == typeof(bool) && c is CheckBox)
                Value = (c as CheckBox).IsChecked;
            else
                throw new NotSupportedException("Invalid type");
        }
    }
}
