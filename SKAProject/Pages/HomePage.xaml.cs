using MySqlConnector;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SKAProject.Pages
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            await LoadSummaryCounts();
            await LoadTaskProgress();
            await LoadOrganizationInfo();
            LoadWelcomeInfo();
        }

        private void LoadWelcomeInfo()
        {
            string firstName = "";
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT FirstName, Role FROM users WHERE UserID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", Session.UserID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            firstName = reader["FirstName"]?.ToString() ?? "";
                            TxtRole.Text = reader["Role"]?.ToString() ?? "Пользователь";
                        }
                    }
                }
            }
            catch { }

            TxtWelcomeUser.Text = string.IsNullOrWhiteSpace(firstName)
                ? "Добро пожаловать!"
                : $"Привет, {firstName}!";
            TxtCurrentDate.Text = DateTime.Now.ToString("dddd, d MMMM yyyy");
        }

        private async Task LoadSummaryCounts()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    // Всего сотрудников
                    var empCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM workers WHERE Status IN ('Работает', 'В отпуске')", conn);
                    TxtTotalEmployees.Text = ((long)await empCmd.ExecuteScalarAsync()).ToString();

                    // Мои активные задачи
                    var taskCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND Status IN ('Новая','В работе')", conn);
                    taskCmd.Parameters.AddWithValue("@uid", Session.UserID);
                    TxtMyTasks.Text = ((long)await taskCmd.ExecuteScalarAsync()).ToString();

                    // Отчётов на проверке
                    var repCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM reports WHERE RecipientID = @uid AND Status = 'На проверке'", conn);
                    repCmd.Parameters.AddWithValue("@uid", Session.UserID);
                    TxtPendingReports.Text = ((long)await repCmd.ExecuteScalarAsync()).ToString();

                    // Новых задач за неделю
                    var recentCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND CreatedAt >= DATE_SUB(NOW(), INTERVAL 7 DAY)", conn);
                    recentCmd.Parameters.AddWithValue("@uid", Session.UserID);
                    TxtRecentTasks.Text = ((long)await recentCmd.ExecuteScalarAsync()).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сводки: " + ex.Message);
            }
        }

        private async Task LoadTaskProgress()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    var totalCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid", conn);
                    totalCmd.Parameters.AddWithValue("@uid", Session.UserID);
                    long total = (long)await totalCmd.ExecuteScalarAsync();

                    var doneCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND Status = 'Завершена'", conn);
                    doneCmd.Parameters.AddWithValue("@uid", Session.UserID);
                    long completed = (long)await doneCmd.ExecuteScalarAsync();

                    int percent = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0;
                    TxtTotalTasks.Text = total.ToString();
                    TxtCompletedTasks.Text = completed.ToString();
                    TaskProgressBar.Value = percent;
                    TxtProgressPercent.Text = $"{percent}%";
                }
            }
            catch (Exception ex)
            {
                // не критично
            }
        }

        private async Task LoadOrganizationInfo()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand(
                        "SELECT Address, Phone, Email FROM company_info WHERE CompanyID = 1", conn);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            TxtOrgName.Text = reader.IsDBNull(reader.GetOrdinal("Address")) ? "Не указано" : reader.GetString(reader.GetOrdinal("Address"));
                            TxtOrgPhone.Text = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "—" : reader.GetString(reader.GetOrdinal("Phone"));
                            TxtOrgEmail.Text = reader.IsDBNull(reader.GetOrdinal("Email")) ? "—" : reader.GetString(reader.GetOrdinal("Email"));
                        }
                    }
                }
            }
            catch { }
        }

        private void OpenMyTasks(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new MyTasks());
        private void OpenSendReport(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new SendReportPage());
        private void OpenProfile(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new ProfilePage());
    }
}