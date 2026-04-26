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
            ApplyRoleAccess();
            ThemeManager.LoadSavedTheme();
        }

        private void ApplyRoleAccess()
        {
            if (Session.Role != "User")
                return;

            BtnWorkers.Visibility = Visibility.Collapsed;       
            BtnOrganization.Visibility = Visibility.Collapsed;   
            BtnReports.Visibility = Visibility.Collapsed;        
            BtnLogs.Visibility = Visibility.Collapsed;                                                           
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.LogsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void Workers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.WorkersPage());
        }

        private void MyOrg_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.MyOrganizationPage());
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

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeToggleButton.Content.ToString().Contains("Тёмная"))
            {
                ThemeManager.ApplyTheme("Dark");
                ThemeToggleButton.Content = "☀️ Светлая тема";
                MessageBox.Show("Переключили на тёмную");  // тест
            }
            else
            {
                ThemeManager.ApplyTheme("Light");
                ThemeToggleButton.Content = "🌙 Тёмная тема";
                MessageBox.Show("Переключили на светлую");
            }
        }
    }
}
