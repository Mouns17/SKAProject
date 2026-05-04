using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class MyTasks : Page
    {
        private ObservableCollection<TaskModel> _allTasks = new ObservableCollection<TaskModel>();

        public MyTasks()
        {
            InitializeComponent();
            LoadTasks();
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        public class TaskModel : INotifyPropertyChanged
        {
            public int TaskId { get; set; }
            public string CreatedByName { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? Deadline { get; set; }
            public string Priority { get; set; }
            public string Title { get; set; }
            public string Status { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadTasks()
        {
            try
            {
                _allTasks.Clear();
                int userId = Session.UserID;

                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT t.TaskID, t.Title, t.CreatedAt, t.Deadline,
                               t.Priority, t.Status,
                               CONCAT(u.LastName, ' ', u.FirstName) AS CreatedBy
                        FROM tasks t
                        JOIN users u ON t.CreatedBy = u.UserID
                        WHERE t.AssignedTo = @uid
                        ORDER BY t.CreatedAt DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int taskId = reader.GetInt32("TaskID");
                                string createdBy = reader.GetString("CreatedBy");
                                DateTime createdAt = reader.GetDateTime("CreatedAt");

                                // Правильное чтение Nullable‑поля
                                DateTime? deadline = null;
                                int deadlineIndex = reader.GetOrdinal("Deadline");
                                if (!reader.IsDBNull(deadlineIndex))
                                    deadline = reader.GetDateTime(deadlineIndex);

                                string priority = reader.GetString("Priority");
                                string title = reader.GetString("Title");
                                string status = reader.GetString("Status");

                                _allTasks.Add(new TaskModel
                                {
                                    TaskId = taskId,
                                    CreatedByName = createdBy,
                                    CreatedAt = createdAt,
                                    Deadline = deadline,
                                    Priority = priority,
                                    Title = title,
                                    Status = status
                                });
                            }
                        }
                    }
                }

                TasksItemsControl.ItemsSource = _allTasks;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки задач: " + ex.Message);
            }
        }

        private void UpdateStatistics()
        {
            int total = _allTasks.Count;
            int completed = _allTasks.Count(t => t.Status == "Завершена");
            int pending = total - completed;

            TxtTotalTasks.Text = total.ToString();
            TxtCompletedTasks.Text = completed.ToString();
            TxtPendingTasks.Text = pending.ToString();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            string search = SearchTextBox.Text?.ToLower() ?? "";
            var filtered = _allTasks.Where(t =>
                t.TaskId.ToString().Contains(search) ||
                t.Priority.ToLower().Contains(search) ||
                t.Status.ToLower().Contains(search) ||
                t.Title.ToLower().Contains(search)).ToList();

            TasksItemsControl.ItemsSource = filtered;
        }

        private void ReportCreate_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new SendReportWindow(Session.UserID);
            reportWindow.Owner = Window.GetWindow(this);
            reportWindow.ShowDialog();
        }
    }
}