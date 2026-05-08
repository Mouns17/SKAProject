using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SKAProject.Pages
{
    public partial class EmployeeReportsPage : Page
    {
        private ObservableCollection<ReportEntry> _reports = new ObservableCollection<ReportEntry>();

        public EmployeeReportsPage()
        {
            InitializeComponent();
            LoadReports();
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        public class ReportEntry : INotifyPropertyChanged
        {
            public int ReportID { get; set; }
            public string AuthorName { get; set; }
            public DateTime? ReportDate { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string FilePath { get; set; }

            public bool HasFile => !string.IsNullOrEmpty(FilePath);
            public bool CanDecide => Status == "На проверке";

            public SolidColorBrush StatusColor
            {
                get
                {
                    switch (Status)
                    {
                        case "Принято":
                            return new SolidColorBrush(Color.FromRgb(16, 185, 129));
                        case "Отклонено":
                            return new SolidColorBrush(Color.FromRgb(239, 68, 68));
                        case "На проверке":
                            return new SolidColorBrush(Color.FromRgb(245, 158, 11));
                        default:
                            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadReports()
        {
            try
            {
                _reports.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT r.ReportID, r.ReportDate, r.Description, r.FilePath, r.Status,
                               CONCAT(u.LastName, ' ', u.FirstName) AS AuthorName
                        FROM reports r
                        JOIN users u ON r.AuthorID = u.UserID
                        ORDER BY r.ReportDate DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _reports.Add(new ReportEntry
                            {
                                ReportID = reader.GetInt32("ReportID"),
                                AuthorName = reader.GetString("AuthorName"),
                                ReportDate = reader.IsDBNull(reader.GetOrdinal("ReportDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ReportDate")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                                FilePath = reader.IsDBNull(reader.GetOrdinal("FilePath")) ? null : reader.GetString(reader.GetOrdinal("FilePath")),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                }
                ReportsItemsControl.ItemsSource = _reports;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отчётов: " + ex.Message);
            }
        }

        private void UpdateStatistics()
        {
            int total = _reports.Count;
            int accepted = _reports.Count(r => r.Status == "Принято");
            int pending = _reports.Count(r => r.Status == "На проверке");
            int rejected = _reports.Count(r => r.Status == "Отклонено");
            TxtTotalReports.Text = total.ToString();
            TxtAcceptedReports.Text = accepted.ToString();
            TxtPendingReports.Text = pending.ToString();
            TxtRejectedReports.Text = rejected.ToString();
        }

        private async void ChangeStatus(int reportId, string newStatus)
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string update = "UPDATE reports SET Status = @status WHERE ReportID = @id";
                    using (var cmd = new MySqlCommand(update, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", reportId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Изменение статуса отчёта", "Отчёты", $"Отчёт ID={reportId} переведён в «{newStatus}»");
                LoadReports();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения статуса: " + ex.Message);
            }
        }

        private void AcceptReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter != null)
                ChangeStatus(Convert.ToInt32(btn.CommandParameter), "Принято");
        }

        private void RejectReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter != null)
                ChangeStatus(Convert.ToInt32(btn.CommandParameter), "Отклонено");
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter != null)
            {
                string filePath = btn.CommandParameter.ToString();
                if (File.Exists(filePath))
                {
                    try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show("Не удалось открыть файл: " + ex.Message); }
                }
                else MessageBox.Show("Файл не найден.");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            string search = SearchTextBox.Text?.ToLower() ?? "";
            var filtered = _reports.Where(r =>
                r.ReportID.ToString().Contains(search) ||
                r.AuthorName.ToLower().Contains(search) ||
                r.Description.ToLower().Contains(search) ||
                r.Status.ToLower().Contains(search)).ToList();
            ReportsItemsControl.ItemsSource = filtered;
        }
    }
}