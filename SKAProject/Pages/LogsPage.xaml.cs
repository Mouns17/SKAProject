using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class LogsPage : Page
    {
        private ObservableCollection<LogEntry> allLogs = new ObservableCollection<LogEntry>();

        public LogsPage()
        {
            InitializeComponent();
            LoadLogs();

            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            DepartmentFilterComboBox.SelectionChanged += Filter_SelectionChanged;
            PositionFilterComboBox.SelectionChanged += Filter_SelectionChanged;

            SearchPlaceholder.Visibility = Visibility.Visible;
        }

        public class LogEntry : INotifyPropertyChanged
        {
            public int LogId { get; set; }
            public int? UserId { get; set; }
            public string UserName { get; set; }
            public string Action { get; set; }
            public string EventType { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedAtStr => CreatedAt.ToString("dd.MM.yyyy HH:mm:ss");

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void LoadLogs()
        {
            try
            {
                allLogs.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                SELECT l.LogID, l.UserID, l.Action, l.EventType, l.Description, l.CreatedAt,
                       CONCAT(u.LastName, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS UserFullName,
                       u.Login
                FROM logs l
                LEFT JOIN users u ON l.UserID = u.UserID
                ORDER BY l.CreatedAt DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Явно получаем значения, избегая тернарных операторов над проблемными типами
                            int logId = reader.GetInt32(reader.GetOrdinal("LogID"));
                            int? userId = null;
                            if (!reader.IsDBNull(reader.GetOrdinal("UserID")))
                                userId = reader.GetInt32(reader.GetOrdinal("UserID"));

                            string userName;
                            if (reader.IsDBNull(reader.GetOrdinal("Login")))
                                userName = "Система";
                            else
                                userName = reader.GetString(reader.GetOrdinal("Login"));

                            string action = reader.GetString(reader.GetOrdinal("Action"));

                            string eventType;
                            if (reader.IsDBNull(reader.GetOrdinal("EventType")))
                                eventType = "";
                            else
                                eventType = reader.GetString(reader.GetOrdinal("EventType"));

                            string description;
                            if (reader.IsDBNull(reader.GetOrdinal("Description")))
                                description = "";
                            else
                                description = reader.GetString(reader.GetOrdinal("Description"));

                            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));

                            allLogs.Add(new LogEntry
                            {
                                LogId = logId,
                                UserId = userId,
                                UserName = userName,
                                Action = action,
                                EventType = eventType,
                                Description = description,
                                CreatedAt = createdAt
                            });
                        }
                    }
                }
                LogsItemsControl.ItemsSource = null;
                LogsItemsControl.ItemsSource = allLogs;

                LoadEventTypes();

                // Принудительно обновляем привязку
                await Dispatcher.InvokeAsync(() =>
                {
                    LogsItemsControl.ItemsSource = allLogs;
                });
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки логов: " + ex.Message);
            }

            await Dispatcher.InvokeAsync(() =>
            {
                LogsItemsControl.ItemsSource = allLogs;
            });
        }

        private void LoadEventTypes()
        {
            DepartmentFilterComboBox.Items.Clear();
            DepartmentFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все типы", IsSelected = true });

            var types = allLogs
                .Select(l => l.EventType)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .OrderBy(t => t);

            foreach (var t in types)
            {
                DepartmentFilterComboBox.Items.Add(new ComboBoxItem { Content = t, Tag = t });
            }
        }

        private void ApplyFilters()
        {
            string search = SearchTextBox.Text?.ToLower() ?? "";
            var filtered = allLogs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(l =>
                    (l.UserName != null && l.UserName.ToLower().Contains(search)) ||
                    (l.Action != null && l.Action.ToLower().Contains(search)) ||
                    (l.Description != null && l.Description.ToLower().Contains(search)));
            }

            if (DepartmentFilterComboBox.SelectedItem is ComboBoxItem typeItem && typeItem.Tag != null)
            {
                string type = typeItem.Tag.ToString();
                filtered = filtered.Where(l => l.EventType == type);
            }

            if (PositionFilterComboBox.SelectedItem is ComboBoxItem dateItem && dateItem.Tag != null)
            {
                string dateTag = dateItem.Tag.ToString();
                DateTime now = DateTime.Now;
                DateTime? fromDate = null;
                bool singleDay = false;

                switch (dateTag)
                {
                    case "today":
                        fromDate = now.Date;
                        singleDay = true;
                        break;
                    case "yesterday":
                        fromDate = now.Date.AddDays(-1);
                        singleDay = true;
                        break;
                    case "7days":
                        fromDate = now.Date.AddDays(-7);
                        break;
                    case "30days":
                        fromDate = now.Date.AddDays(-30);
                        break;
                }

                if (fromDate.HasValue)
                {
                    filtered = filtered.Where(l => l.CreatedAt >= fromDate.Value);
                    if (singleDay)
                        filtered = filtered.Where(l => l.CreatedAt < fromDate.Value.AddDays(1));
                }
            }

            LogsItemsControl.ItemsSource = new ObservableCollection<LogEntry>(filtered);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            ApplyFilters();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
    }
}