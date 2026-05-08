using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            LoadWelcomeInfo();
            LoadNotifications();
        }

        private void LoadWelcomeInfo()
        {
            string firstName = "";
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT FirstName, Role FROM users WHERE UserID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", Session.UserID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            firstName = reader["FirstName"]?.ToString() ?? "";
                            string role = reader["Role"]?.ToString() ?? "Пользователь";
                            TxtRole.Text = role;
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

                    string empQuery = "SELECT COUNT(*) FROM workers WHERE Status IN ('Работает', 'В отпуске')";
                    using (var cmd = new MySqlCommand(empQuery, conn))
                        TxtTotalEmployees.Text = ((long)await cmd.ExecuteScalarAsync()).ToString();

                    string taskQuery = "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND Status IN ('Новая','В работе')";
                    using (var cmd = new MySqlCommand(taskQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        TxtMyTasks.Text = ((long)await cmd.ExecuteScalarAsync()).ToString();
                    }

                    string reportQuery = "SELECT COUNT(*) FROM reports WHERE RecipientID = @uid AND Status = 'На проверке'";
                    using (var cmd = new MySqlCommand(reportQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        TxtPendingReports.Text = ((long)await cmd.ExecuteScalarAsync()).ToString();
                    }

                    string recentQuery = "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND CreatedAt >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
                    using (var cmd = new MySqlCommand(recentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        TxtRecentTasks.Text = ((long)await cmd.ExecuteScalarAsync()).ToString();
                    }
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

                    // Сначала получаем общее количество задач
                    string totalQuery = "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid";
                    long totalCount = 0;
                    using (var cmd = new MySqlCommand(totalQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        totalCount = (long)await cmd.ExecuteScalarAsync();
                    }

                    // Затем получаем количество завершённых задач
                    string completedQuery = "SELECT COUNT(*) FROM tasks WHERE AssignedTo = @uid AND Status = 'Завершена'";
                    long completed = 0;
                    using (var cmd = new MySqlCommand(completedQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        completed = (long)await cmd.ExecuteScalarAsync();
                    }

                    // Вычисляем процент
                    int percent = 0;
                    if (totalCount > 0)
                        percent = (int)Math.Round((double)completed / totalCount * 100);

                    TaskProgressBar.Value = percent;
                    TxtTotalTasks.Text = totalCount.ToString();
                    TxtCompletedTasks.Text = completed.ToString();
                    TxtProgressPercent.Text = $"{percent}%";
                }
            }
            catch (Exception ex)
            {
                // При ошибке просто оставляем значения по умолчанию (0)
            }
        }

        private void LoadNotifications()
        {
            // Пример учебных уведомлений – позже можно загружать из БД
            var notifications = new ObservableCollection<dynamic>
            {
                new { Icon = "📋", Message = "Вам назначена новая задача «Подготовить отчёт»", TimeAgo = "5 минут назад" },
                new { Icon = "✅", Message = "Ваш отчёт «Март 2026» принят руководителем", TimeAgo = "1 час назад" },
                new { Icon = "📄", Message = "Заявка на отпуск утверждена", TimeAgo = "3 часа назад" }
            };
            NotificationsItemsControl.ItemsSource = notifications;
        }

        // Быстрые действия
        private void OpenMyTasks(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new MyTasks());
        private void OpenSendReport(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new SendReportPage());
        private void OpenProfile(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new ProfilePage());
    }
}