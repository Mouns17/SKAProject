using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для EmailLogginWindow.xaml
    /// </summary>
    public partial class EmailLogginWindow : Window
    {
        public EmailLogginWindow()
        {
            InitializeComponent();
        }

        private void SendCode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            PhoneLogginWindow shpionirogolubiro = new PhoneLogginWindow();
            shpionirogolubiro.Show();
            this.Close();
        }
    }
}
