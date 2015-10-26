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

        protected EditWindow(string Title, Dictionary<string, EditItem> Items)
        {
            InitializeComponent();

            this.Title = Title;

            this.Items = Items;
        }
        protected void UpdateUI()
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
                    Label.Margin = new Thickness(5);
                    Label.Text = i.Value.Label + ":";
                    Grid_Controls.Children.Add(Label);
                    Label.SetValue(Grid.RowProperty, x);
                    Label.SetValue(Grid.ColumnProperty, 0);

                    Control Control = i.Value.Control;
                    Control.Margin = new Thickness(5);
                    Grid_Controls.Children.Add(Control);
                    Control.SetValue(Grid.RowProperty, x);
                    Control.SetValue(Grid.ColumnProperty, 1);

                    x++;
                }
            }
        }


        public static Dictionary<string, object> Show(string Title, Dictionary<string, EditItem> Items)
        {
            EditWindow Wnd = new EditWindow(Title, Items);
            Wnd.UpdateUI();

            bool? Result = Wnd.ShowDialog();
            if (Result.HasValue && Result.Value)
                return Wnd.Items.ToDictionary(p => p.Key, p => p.Value.Value);
            else
                return null;
        }

        protected void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        protected void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> Errors = new Dictionary<string, string>();
            foreach (KeyValuePair<string, EditItem> i in Items)
            {
                string Error = i.Value.Validate();
                if (!string.IsNullOrWhiteSpace(Error))
                    Errors.Add(i.Value.Label, Error);
            }

            if (Errors.Count > 0)
            {
                string Message = Errors.Count + " validation errors:" + Environment.NewLine;
                foreach (KeyValuePair<string, string> i in Errors)
                    Message += i.Key + ": " + i.Value + Environment.NewLine;

                MessageBox.Show(Message, "Validation Errors", MessageBoxButton.OK);
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }

    public class EditItem
    {
        public object Value
        {
            get
            {
                if (ValueType == typeof(string) && Control is TextBox)
                    return (Control as TextBox).Text;
                else if (ValueType == typeof(int) && Control is TextBox)
                    return (Control as TextBox).Text;
                else if (ValueType.IsEnum && Control is ComboBox)
                    return Enum.Parse(ValueType, (string)(Control as ComboBox).SelectedItem);
                else if (ValueType == typeof(bool) && Control is CheckBox)
                    return (Control as CheckBox).IsChecked;
                else
                    throw new NotSupportedException("Invalid type");
            }
            protected set
            {
                if (ValueType == typeof(string))
                    (Control as TextBox).Text = value as string;
                else if (ValueType == typeof(int))
                    (Control as TextBox).Text = Convert.ToString((int)value);
                else if (ValueType.IsEnum)
                    (Control as ComboBox).SelectedItem = Enum.GetName(ValueType, value);
                else if (ValueType == typeof(bool))
                    (Control as CheckBox).IsChecked = (bool)value;
                else
                    throw new NotSupportedException("Invalid type");
            }
        }
        protected readonly Type _ValueType;
        public Type ValueType { get { return _ValueType; } }

        public string Label { get; set; }

        public Func<object, string> Validator { get; set; }

        public Control Control { get; protected set; }

        public EditItem(string Label, object DefaultValue)
            : this(Label, DefaultValue, null)
        {
        }
        public EditItem(string Label, object DefaultValue, Func<object, string> Validator)
        {
            _ValueType = DefaultValue.GetType();
            this.Label = Label;
            this.Validator = Validator;

            if (ValueType == typeof(string))
                Control = new TextBox() { Text = DefaultValue as string };
            else if (ValueType == typeof(int))
                Control = new TextBox() { Text = Convert.ToString((int)DefaultValue) };
            else if (ValueType.IsEnum)
                Control = new ComboBox() { ItemsSource = Enum.GetNames(ValueType), SelectedItem = Enum.GetName(ValueType, DefaultValue) };
            else if (ValueType == typeof(bool))
                Control = new CheckBox() { IsChecked = (bool)DefaultValue };
            else
                throw new NotSupportedException("Invalid type");

            Value = DefaultValue;
        }

        public string Validate()
        {
            if (Validator != null)
                return Validator(Value);
            else
                return null;
        }

        public static readonly Func<object, string> NonNegativeIntegerValidator = (o) =>
        { int Out; return (o is string && int.TryParse(o as string, out Out) && Out >= 0) ? "" : "Must be a number greater than or equal to 0"; };
    }
}
