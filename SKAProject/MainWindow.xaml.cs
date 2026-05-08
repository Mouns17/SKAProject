using SKAProject.Pages;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThemeManager.LoadSavedTheme();
            ApplyRoleRestrictions();
            MainFrame.Navigate(new HomePage());
        }

        // ========== Управление окном ==========
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                MaximizeRestoreWindow_Click(sender, e);
            else
                this.DragMove();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeRestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MaxRestoreButton.Content = "\uE923";
                MaxRestoreButton.ToolTip = "Восстановить";
            }
            else
            {
                WindowState = WindowState.Normal;
                MaxRestoreButton.Content = "\uE922";
                MaxRestoreButton.ToolTip = "Развернуть";
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e) => ThemeManager.ApplyTheme("Dark");
        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e) => ThemeManager.ApplyTheme("Light");

        // ========== Навигация ==========
        private void NavigateToMainMenu(object sender, RoutedEventArgs e) => MainFrame.Navigate(new HomePage());
        private void NavigateToMyTasks(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MyTasks());
        private void NavigateToSendReport(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SendReportPage());
        private void NavigateToProfile(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
        private void NavigateToWorkers(object sender, RoutedEventArgs e) => MainFrame.Navigate(new WorkersPage());
        private void NavigateToStaffSchedule(object sender, RoutedEventArgs e) => MainFrame.Navigate(new StaffSchedulePage());
        private void NavigateToOrders(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrdersPage());
        private void NavigateToEmployeeRequests(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EmployeeRequestsPage());
        private void NavigateToEmployeeReports(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EmployeeReportsPage());
        private void NavigateToSalaries(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SalariesPage());
        private void NavigateToPayroll(object sender, RoutedEventArgs e) => MainFrame.Navigate(new PayrollPage());
        private void NavigateToFinancialReports(object sender, RoutedEventArgs e) => MainFrame.Navigate(new FinancialReportsPage());
        private void NavigateToOrganization(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MyOrganizationPage());
        private void NavigateToUsers(object sender, RoutedEventArgs e) => MainFrame.Navigate(new UsersPage());
        private void NavigateToLogs(object sender, RoutedEventArgs e) => MainFrame.Navigate(new LogsPage());

        // ========== Скрытие пунктов меню по ролям ==========
        private void ApplyRoleRestrictions()
        {
            string role = Session.Role ?? "User";

            bool isAdmin = role == "Owner" || role == "Admin";
            bool isHR = role == "HR";
            bool isAccountant = role == "Accountant";
            bool isDirector = role == "Director";
            bool isDeptHead = role == "DepartmentHead"; 
            bool isUser = role == "User";

            // Кадры
            CategoryHR.Visibility = (isAdmin || isHR || isDirector || isDeptHead) ? Visibility.Visible : Visibility.Collapsed;
            BtnEmployees.Visibility = (isAdmin || isHR || isDirector || isDeptHead) ? Visibility.Visible : Visibility.Collapsed;
            BtnStaffSchedule.Visibility = (isAdmin || isHR) ? Visibility.Visible : Visibility.Collapsed;
            BtnOrders.Visibility = (isAdmin || isHR) ? Visibility.Visible : Visibility.Collapsed;
            BtnEmployeeRequests.Visibility = (isAdmin || isHR || isDeptHead || isDirector) ? Visibility.Visible : Visibility.Collapsed;
            BtnEmployeeReports.Visibility = (isAdmin || isHR || isDeptHead || isDirector) ? Visibility.Visible : Visibility.Collapsed;

            // Расчёты
            CategoryPayroll.Visibility = (isAdmin || isAccountant) ? Visibility.Visible : Visibility.Collapsed;
            BtnSalaries.Visibility = (isAdmin || isAccountant) ? Visibility.Visible : Visibility.Collapsed;
            BtnPayroll.Visibility = (isAdmin || isAccountant) ? Visibility.Visible : Visibility.Collapsed;
            BtnFinancialReports.Visibility = (isAdmin || isAccountant || isDirector) ? Visibility.Visible : Visibility.Collapsed;

            // Администрирование
            CategoryAdmin.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnOrganization.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnLogs.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}