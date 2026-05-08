using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class InputDialog : Window
    {
        public string Result { get; private set; }

        public InputDialog(string prompt, string defaultValue = "")
        {
            InitializeComponent();
            TxtPrompt.Text = prompt;
            TxtValue.Text = defaultValue;
            TxtValue.Focus();
            TxtValue.SelectAll();
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = TxtValue.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}