using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class EmployeeRequestsPage : Page
    {
        private ObservableCollection<RequestEntry> _requests = new ObservableCollection<RequestEntry>();

        public EmployeeRequestsPage()
        {
            InitializeComponent();
            LoadRequests();
        }

        public class RequestEntry : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int RowNumber { get; set; }
            public DateTime RequestDate { get; set; }
            public string RequestType { get; set; }
            public string EmployeeFullName { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public bool CanApprove => Status == "На рассмотрении";
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadRequests()
        {
            try
            {
                _requests.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT r.Id, r.RequestDate, r.RequestType, r.Reason, r.Status,
                               CONCAT(u.LastName, ' ', u.FirstName) AS EmployeeFullName
                        FROM employeerequests r
                        JOIN users u ON r.EmployeeUserID = u.UserID
                        ORDER BY r.RequestDate DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int row = 1;
                        while (await reader.ReadAsync())
                        {
                            string status = reader.GetString(reader.GetOrdinal("Status"));
                            _requests.Add(new RequestEntry
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                RowNumber = row++,
                                RequestDate = reader.GetDateTime(reader.GetOrdinal("RequestDate")),
                                RequestType = reader.GetString(reader.GetOrdinal("RequestType")),
                                EmployeeFullName = reader.GetString(reader.GetOrdinal("EmployeeFullName")),
                                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? "" : reader.GetString(reader.GetOrdinal("Reason")),
                                Status = status
                            });
                        }
                    }
                }
                RequestsItemsControl.ItemsSource = _requests;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var typeDialog = new InputDialog("Введите тип заявки (Отпуск / Отгул / Командировка):");
            if (typeDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(typeDialog.Result)) return;
            var reasonDialog = new InputDialog("Введите причину:");
            if (reasonDialog.ShowDialog() != true) return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    string insert = @"INSERT INTO employeerequests (RequestDate, RequestType, Reason, Status, EmployeeUserID)
                                     VALUES (NOW(), @type, @reason, 'На рассмотрении', @uid)";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@type", typeDialog.Result.Trim());
                        cmd.Parameters.AddWithValue("@reason", reasonDialog.Result.Trim());
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        cmd.ExecuteNonQuery();
                    }
                }
                Logger.LogAsync(Session.UserID, "Создание заявки", "Заявки", $"Создана заявка на {typeDialog.Result}").Wait();
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания заявки: " + ex.Message);
            }
        }

        private async void ApproveRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            await UpdateRequestStatus(id, "Утверждена");
        }

        private async void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            await UpdateRequestStatus(id, "Отклонена");
        }

        private async Task UpdateRequestStatus(int id, string status)
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string upd = "UPDATE employeerequests SET Status = @status WHERE Id = @id";
                    using (var cmd = new MySqlCommand(upd, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Изменение статуса заявки", "Заявки", $"Заявка ID={id} переведена в статус «{status}»");
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private async void DeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string del = "DELETE FROM employeerequests WHERE Id = @id";
                    using (var cmd = new MySqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Удаление заявки", "Заявки", $"Удалена заявка ID={id}");
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}