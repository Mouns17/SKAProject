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
    /// Логика взаимодействия для PhoneLogginWindow.xaml
    /// </summary>
    public partial class PhoneLogginWindow : Window
    {
        public PhoneLogginWindow()
        {
            InitializeComponent();
        }

        private void OpenEmailAutorization_Click(object sender, RoutedEventArgs e)
        {
            EmailLogginWindow bonecaambalabu = new EmailLogginWindow();
            bonecaambalabu.Show();
            this.Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            LogginWindow labuba = new LogginWindow();
            labuba.Show();
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
