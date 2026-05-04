using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SKAProject.Pages
{
    public partial class ReportsPage : Page
    {
        private ObservableCollection<ReportModel> _allReports = new ObservableCollection<ReportModel>();

        public ReportsPage()
        {
            InitializeComponent();
            LoadReports();
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        public class ReportModel : INotifyPropertyChanged
        {
            public int ReportID { get; set; }
            public string AuthorName { get; set; }
            public string RecipientName { get; set; }
            public DateTime? ReportDate { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string FilePath { get; set; }

            public bool HasFile => !string.IsNullOrEmpty(FilePath);
            public SolidColorBrush StatusColor
            {
                get
                {
                    switch (Status)
                    {
                        case "Принято": return Brushes.Green;
                        case "Отклонено": return Brushes.Red;
                        case "На проверке": return Brushes.Orange;
                        default: return Brushes.Gray;
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadReports()
        {
            try
            {
                _allReports.Clear();
                int userId = Session.UserID;

                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT r.ReportID, r.ReportDate, r.Description, r.FilePath, r.Status,
                               CONCAT(a.LastName, ' ', a.FirstName) AS AuthorName,
                               CONCAT(rc.LastName, ' ', rc.FirstName) AS RecipientName
                        FROM reports r
                        JOIN users a ON r.AuthorID = a.UserID
                        JOIN users rc ON r.RecipientID = rc.UserID
                        WHERE r.RecipientID = @uid
                        ORDER BY r.ReportDate DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int reportId = reader.GetInt32("ReportID");
                                string authorName = reader.GetString("AuthorName");
                                string recipientName = reader.GetString("RecipientName");

                                DateTime? reportDate = null;
                                int rdIdx = reader.GetOrdinal("ReportDate");
                                if (!reader.IsDBNull(rdIdx))
                                    reportDate = reader.GetDateTime(rdIdx);

                                string description = "";
                                int descIdx = reader.GetOrdinal("Description");
                                if (!reader.IsDBNull(descIdx))
                                    description = reader.GetString(descIdx);

                                string filePath = null;
                                int fpIdx = reader.GetOrdinal("FilePath");
                                if (!reader.IsDBNull(fpIdx))
                                    filePath = reader.GetString(fpIdx);

                                string status = reader.GetString("Status");

                                _allReports.Add(new ReportModel
                                {
                                    ReportID = reportId,
                                    AuthorName = authorName,
                                    RecipientName = recipientName,
                                    ReportDate = reportDate,
                                    Description = description,
                                    FilePath = filePath,
                                    Status = status
                                });
                            }
                        }
                    }
                }

                ReportsItemsControl.ItemsSource = _allReports;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отчётов: " + ex.Message);
            }
        }

        private void UpdateStatistics()
        {
            int total = _allReports.Count;
            int accepted = _allReports.Count(r => r.Status == "Принято");
            int pending = _allReports.Count(r => r.Status == "На проверке");
            int rejected = _allReports.Count(r => r.Status == "Отклонено");

            TxtTotalReports.Text = total.ToString();
            TxtAcceptedReports.Text = accepted.ToString();
            TxtPendingReports.Text = pending.ToString();
            TxtRejectedReports.Text = rejected.ToString();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            string search = SearchTextBox.Text?.ToLower() ?? "";
            var filtered = _allReports.Where(r =>
                r.ReportID.ToString().Contains(search) ||
                r.AuthorName.ToLower().Contains(search) ||
                r.Description.ToLower().Contains(search) ||
                r.Status.ToLower().Contains(search)
            ).ToList();
            ReportsItemsControl.ItemsSource = filtered;
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            string filePath = button.CommandParameter.ToString();
            if (File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось открыть файл: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Файл не найден.", "Ошибка");
            }
        }

        private async void AcceptReport_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int reportId = Convert.ToInt32(button.CommandParameter);
            await ChangeReportStatusAsync(reportId, "Принято");
        }

        private async void RejectReport_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int reportId = Convert.ToInt32(button.CommandParameter);
            await ChangeReportStatusAsync(reportId, "Отклонено");
        }

        private async Task ChangeReportStatusAsync(int reportId, string newStatus)
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

                await Logger.LogAsync(Session.UserID, "Изменение статуса отчёта", "Отчёты",
                    $"Отчёт ID={reportId} переведён в статус «{newStatus}»");

                LoadReports();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при изменении статуса: " + ex.Message);
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Отчеты");
                ws.Cell(1, 1).Value = "№"; ws.Cell(1, 2).Value = "От кого"; ws.Cell(1, 3).Value = "Кому";
                ws.Cell(1, 4).Value = "Дата"; ws.Cell(1, 5).Value = "Описание"; ws.Cell(1, 6).Value = "Статус";

                int row = 2;
                foreach (var r in _allReports)
                {
                    ws.Cell(row, 1).Value = r.ReportID;
                    ws.Cell(row, 2).Value = r.AuthorName;
                    ws.Cell(row, 3).Value = r.RecipientName;
                    ws.Cell(row, 4).Value = r.ReportDate?.ToShortDateString() ?? "";
                    ws.Cell(row, 5).Value = r.Description;
                    ws.Cell(row, 6).Value = r.Status;
                    row++;
                }
                ws.Columns().AdjustToContents();

                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx", FileName = "Отчеты.xlsx" };
                if (dlg.ShowDialog() == true)
                {
                    workbook.SaveAs(dlg.FileName);
                    MessageBox.Show("Экспорт выполнен!");
                }
            }
        }
    }
}