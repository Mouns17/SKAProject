using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            MainFrame.Navigate(new Pages.HomePage());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.HomePage());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProfilePage());
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AccountPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.SettingsPage());
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ExportPage());
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.LogsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
