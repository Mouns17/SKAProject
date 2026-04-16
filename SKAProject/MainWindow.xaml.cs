using SKAProject.Pages;
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
        public static Frame MainFrameStatic { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            MainFrameStatic = MainFrame;  // сохраняем ссылку
            MainFrame.Navigate(new HomePage());
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnRollup(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.HomePage());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProfilePage());
        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            
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

        private void BtnRegOrg(object sender, RoutedEventArgs e)
        {
            new RegOrgWindow().Show();
        }

        private void Workers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.WorkersPage());
        }

        private void MyOrg_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.MyOrganizationPage());
        }

        private void TasksOrg_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.OrgTasksPage());
        }

        private void MyTasks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.MyTasks());
        }

        private void Otcheti_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ReportsPage());
        }
    }
}
