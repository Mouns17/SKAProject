using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SKAProject.Pages
{
    public partial class WorkersPage : Page
    {
        ObservableCollection<WorkerModel> allWorkers = new ObservableCollection<WorkerModel>();

        public static List<string> DepartmentsAll = new List<string>();
        public static List<string> PositionsAll = new List<string>();
        public static List<string> StatusesAll = new List<string> { "Работает", "Уволен", "В отпуске" };

        public WorkersPage()
        {
            InitializeComponent();
            LoadWorkers();
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            DepartmentFilterComboBox.SelectionChanged += FilterChanged;
            PositionFilterComboBox.SelectionChanged += FilterChanged;
            StatusFilterComboBox.SelectionChanged += FilterChanged;
        }

        // ========== МОДЕЛЬ ==========
        public class WorkerModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            private bool _isEditing;
            private string _editDepartment;
            private string _editPosition;
            private string _editStatus;

            public int WrkID { get; set; }
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value) return;
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }

            public bool IsEditing
            {
                get => _isEditing;
                set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
            }

            public string EditDepartment
            {
                get => _editDepartment ?? Department;
                set { _editDepartment = value; OnPropertyChanged(nameof(EditDepartment)); }
            }
            public string EditPosition
            {
                get => _editPosition ?? Position;
                set { _editPosition = value; OnPropertyChanged(nameof(EditPosition)); }
            }
            public string EditStatus
            {
                get => _editStatus ?? Status;
                set { _editStatus = value; OnPropertyChanged(nameof(EditStatus)); }
            }

            public Brush StatusColor =>
                Status == "Работает" ? Brushes.Green :
                Status == "Уволен" ? Brushes.Red :
                Status == "В отпуске" ? Brushes.Orange : Brushes.Gray;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ========== ЗАГРУЗКА ДАННЫХ ==========
        private async void LoadWorkers()
        {
            try
            {
                allWorkers.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT w.WrkID, u.UserID,
                               u.FirstName, u.LastName, u.MiddleName,
                               u.Phone, u.Email, w.Status,
                               COALESCE(d.DepName, 'Не указан') AS DepName,
                               COALESCE(p.PosName, 'Не указана') AS PosName
                        FROM Workers w
                        JOIN Users u ON w.UserID = u.UserID
                        LEFT JOIN departments d ON w.DepID = d.DepID
                        LEFT JOIN positions p ON w.PosID = p.PosID";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            allWorkers.Add(new WorkerModel
                            {
                                WrkID = reader.GetInt32("WrkID"),
                                UserId = reader.GetInt32("UserID"),
                                FullName = $"{reader["LastName"]} {reader["FirstName"]} {reader["MiddleName"]}",
                                Department = reader["DepName"].ToString(),
                                Position = reader["PosName"].ToString(),
                                Phone = GetValue(reader["Phone"]),
                                Email = GetValue(reader["Email"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }

                    await LoadDepartmentsAndPositions();
                }

                WorkersItemsControl.ItemsSource = allWorkers;
                LoadFilters();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
            }
        }

        private async Task LoadDepartmentsAndPositions()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    DepartmentsAll.Clear();
                    PositionsAll.Clear();

                    using (var cmd = new MySqlCommand("SELECT DepName FROM departments ORDER BY DepName", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            DepartmentsAll.Add(reader.GetString(0));
                    }

                    using (var cmd = new MySqlCommand("SELECT PosName FROM positions ORDER BY PosName", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            PositionsAll.Add(reader.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message);
            }
        }

        private void LoadFilters()
        {
            DepartmentFilterComboBox.Items.Clear();
            DepartmentFilterComboBox.Items.Add("Все отделы");
            foreach (var dep in allWorkers.Select(w => w.Department).Distinct().OrderBy(d => d))
                DepartmentFilterComboBox.Items.Add(dep);
            DepartmentFilterComboBox.SelectedIndex = 0;

            PositionFilterComboBox.Items.Clear();
            PositionFilterComboBox.Items.Add("Все должности");
            foreach (var pos in allWorkers.Select(w => w.Position).Distinct().OrderBy(p => p))
                PositionFilterComboBox.Items.Add(pos);
            PositionFilterComboBox.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            string search = SearchTextBox.Text?.ToLower() ?? "";
            string dep = DepartmentFilterComboBox.SelectedItem?.ToString() ?? "Все отделы";
            string pos = PositionFilterComboBox.SelectedItem?.ToString() ?? "Все должности";
            string status = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все статусы";

            var filtered = allWorkers.Where(w =>
                (w.FullName.ToLower().Contains(search) ||
                 w.Email.ToLower().Contains(search) ||
                 w.Phone.ToLower().Contains(search)) &&
                (dep == "Все отделы" || w.Department == dep) &&
                (pos == "Все должности" || w.Position == pos) &&
                (status == "Все статусы" || w.Status == status)
            ).ToList();

            WorkersItemsControl.ItemsSource = filtered;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void FilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        // ========== РЕДАКТИРОВАНИЕ ==========
        private void EditWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = allWorkers.FirstOrDefault(w => w.IsSelected);
            if (selected == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            StartEditingWorker(selected);
        }

        private void StartEditingWorker(WorkerModel worker)
        {
            foreach (var w in allWorkers) w.IsEditing = false;
            worker.EditDepartment = worker.Department;
            worker.EditPosition = worker.Position;
            worker.EditStatus = worker.Status;
            worker.IsEditing = true;
        }

        private async void SaveWorker_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int workerId = Convert.ToInt32(button.CommandParameter);
            var worker = allWorkers.FirstOrDefault(w => w.WrkID == workerId);
            if (worker == null) return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        int? depId = await GetOrCreateDepartment(conn, tx, worker.EditDepartment);
                        int? posId = await GetOrCreatePosition(conn, tx, worker.EditPosition);

                        string update = "UPDATE workers SET DepID=@did, PosID=@pid, Status=@st WHERE WrkID=@wid";
                        using (var cmd = new MySqlCommand(update, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@did", (object)depId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@pid", (object)posId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@st", worker.EditStatus);
                            cmd.Parameters.AddWithValue("@wid", worker.WrkID);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tx.Commit();
                    }
                }

                worker.Department = worker.EditDepartment;
                worker.Position = worker.EditPosition;
                worker.Status = worker.EditStatus;
                worker.IsEditing = false;

                await Logger.LogAsync(Session.UserID, "Редактирование сотрудника", "Управление персоналом",
                    $"Обновлён сотрудник ID {worker.WrkID}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void CancelEditing_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int workerId = Convert.ToInt32(button.CommandParameter);
            var worker = allWorkers.FirstOrDefault(w => w.WrkID == workerId);
            if (worker != null) worker.IsEditing = false;
        }

        private async Task<int?> GetOrCreateDepartment(MySqlConnection conn, MySqlTransaction tx, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            using (var cmd = new MySqlCommand("SELECT DepID FROM departments WHERE DepName=@n", conn, tx))
            {
                cmd.Parameters.AddWithValue("@n", name);
                var res = await cmd.ExecuteScalarAsync();
                if (res != null) return Convert.ToInt32(res);
            }
            using (var cmd = new MySqlCommand("INSERT INTO departments (DepName) VALUES (@n); SELECT LAST_INSERT_ID();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@n", name);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        private async Task<int?> GetOrCreatePosition(MySqlConnection conn, MySqlTransaction tx, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            using (var cmd = new MySqlCommand("SELECT PosID FROM positions WHERE PosName=@n", conn, tx))
            {
                cmd.Parameters.AddWithValue("@n", name);
                var res = await cmd.ExecuteScalarAsync();
                if (res != null) return Convert.ToInt32(res);
            }
            using (var cmd = new MySqlCommand("INSERT INTO positions (PosName) VALUES (@n); SELECT LAST_INSERT_ID();", conn, tx))
            {
                cmd.Parameters.AddWithValue("@n", name);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        // ========== ОСТАЛЬНЫЕ ОБРАБОТЧИКИ ==========
        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddWorkerWindow();
            win.ShowDialog();
            LoadWorkers();
        }

        private void CreateTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = allWorkers.Where(w => w.IsSelected).Select(w => w.WrkID).ToList();
            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Выберите сотрудников, для которых создаётся задача.",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var taskWindow = new CreateTaskWindow(selectedIds);
            taskWindow.Owner = Window.GetWindow(this);
            taskWindow.ShowDialog();
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = allWorkers.Where(w => w.IsSelected).Select(w => w.WrkID).ToList();
            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Выберите сотрудников для удаления.");
                return;
            }

            string message = selectedIds.Count == 1
                ? "Удалить выбранного сотрудника?"
                : $"Удалить {selectedIds.Count} выбранных сотрудников?";

            if (MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        string deleteQuery = $"DELETE FROM workers WHERE WrkID IN ({string.Join(",", selectedIds)})";
                        using (var cmd = new MySqlCommand(deleteQuery, conn, tx))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var id in selectedIds)
                        {
                            await Logger.LogAsync(Session.UserID, "Удаление сотрудника", "Управление персоналом",
                                $"Удалён сотрудник с WrkID = {id}");
                        }

                        tx.Commit();
                    }
                }

                foreach (var w in allWorkers) w.IsSelected = false;
                LoadWorkers();
                MessageBox.Show("Выбранные сотрудники удалены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт в Excel (функция в разработке)");
        }

        private string GetValue(object val) =>
            val == null || string.IsNullOrWhiteSpace(val.ToString()) ? "Пусто" : val.ToString();
    }
}