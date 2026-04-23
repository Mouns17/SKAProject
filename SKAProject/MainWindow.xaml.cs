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
    public partial class MainWindow : Window
    {
        private RegOrgWindow _regOrgWindow;
        public static Frame MainFrameStatic { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            MainFrameStatic = MainFrame; 
            MainFrame.Navigate(new HomePage());
        }

        private void SearchBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox2.Text.ToLower();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnRollup(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMinimize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
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
            new SettingsWindow().Show();
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
            if (_regOrgWindow == null)
            {
                _regOrgWindow = new RegOrgWindow();
                _regOrgWindow.Closed += (s, args) => _regOrgWindow = null;
                _regOrgWindow.Show();
            }
            else
            {
                _regOrgWindow.Activate(); 
            }
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

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }
    }
}
