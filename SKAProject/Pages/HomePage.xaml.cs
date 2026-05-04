using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            LoadData();
        }

        public class RecentLogEntry
        {
            public DateTime CreatedAt { get; set; }
            public string CreatedAtStr => CreatedAt.ToString("dd.MM.yyyy HH:mm");
            public string UserName { get; set; }
            public string Action { get; set; }
            public string Description { get; set; }
        }

        private async void LoadData()
        {
            await LoadSummaryCounts();
            await LoadRecentLogs();
            SetWelcomeMessage();
        }

        private void SetWelcomeMessage()
        {
            // Получаем имя текущего пользователя (если есть)
            string firstName = "";
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT FirstName FROM users WHERE UserID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", Session.UserID);
                    firstName = cmd.ExecuteScalar()?.ToString() ?? "";
                }
            }
            catch { }
            TxtWelcomeUser.Text = $"Рады вас видеть, {firstName}!";
        }

        private async Task LoadSummaryCounts()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    // Всего сотрудников (работающих)
                    string empQuery = "SELECT COUNT(*) FROM workers WHERE Status IN ('Работает', 'В отпуске')";
                    using (var cmd = new MySqlCommand(empQuery, conn))
                    {
                        long count = (long)(await cmd.ExecuteScalarAsync());
                        TxtTotalEmployees.Text = count.ToString();
                    }

                    // Мои активные задачи (Новая + В работе)
                    string taskQuery = @"SELECT COUNT(*) FROM tasks 
                                         WHERE AssignedTo = @uid AND Status IN ('Новая','В работе')";
                    using (var cmd = new MySqlCommand(taskQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        long count = (long)(await cmd.ExecuteScalarAsync());
                        TxtMyTasks.Text = count.ToString();
                    }

                    // Отчётов на проверке (адресованных мне)
                    string reportQuery = @"SELECT COUNT(*) FROM reports 
                                           WHERE RecipientID = @uid AND Status = 'На проверке'";
                    using (var cmd = new MySqlCommand(reportQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        long count = (long)(await cmd.ExecuteScalarAsync());
                        TxtPendingReports.Text = count.ToString();
                    }

                    // Новых задач за неделю (созданных кем-либо для меня)
                    string recentQuery = @"SELECT COUNT(*) FROM tasks 
                                           WHERE AssignedTo = @uid AND CreatedAt >= DATE_SUB(NOW(), INTERVAL 7 DAY)";
                    using (var cmd = new MySqlCommand(recentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        long count = (long)(await cmd.ExecuteScalarAsync());
                        TxtRecentTasks.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сводки: " + ex.Message);
            }
        }

        private async Task LoadRecentLogs()
        {
            try
            {
                var logs = new ObservableCollection<RecentLogEntry>();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT l.CreatedAt, 
                               CONCAT(u.LastName, ' ', u.FirstName) AS UserName,
                               l.Action, l.Description
                        FROM logs l
                        LEFT JOIN users u ON l.UserID = u.UserID
                        ORDER BY l.CreatedAt DESC
                        LIMIT 15";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var log = new RecentLogEntry();

                            // Поле CreatedAt всегда должно быть, получаем напрямую
                            log.CreatedAt = reader.GetDateTime("CreatedAt");

                            // Получаем индексы для Nullable полей
                            int userIdx = reader.GetOrdinal("UserName");
                            int descIdx = reader.GetOrdinal("Description");
                            int actionIdx = reader.GetOrdinal("Action");

                            // Безопасное чтение
                            log.UserName = reader.IsDBNull(userIdx) ? "Система" : reader.GetString(userIdx);
                            log.Description = reader.IsDBNull(descIdx) ? "" : reader.GetString(descIdx);
                            log.Action = reader.GetString(actionIdx); // Action не должно быть NULL, но можно тоже проверить

                            logs.Add(log);
                        }
                    }
                }
                RecentLogsItemsControl.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки логов: " + ex.Message);
            }
        }
    }
}