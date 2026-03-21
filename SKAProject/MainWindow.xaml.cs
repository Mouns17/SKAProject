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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Retake_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Attendance_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Students_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Teachers_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Notify_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Theme_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
