using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

        public WorkersPage()
        {
            InitializeComponent();
            LoadWorkers();
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            DepartmentFilterComboBox.SelectionChanged += FilterChanged;
            PositionFilterComboBox.SelectionChanged += FilterChanged;
            StatusFilterComboBox.SelectionChanged += FilterChanged;
        }

        private void EditWorkerRow_Click(object sender, RoutedEventArgs e)
        {
            int workerId = Convert.ToInt32((sender as Button).CommandParameter);
            MessageBox.Show($"Редактировать сотрудника ID: {workerId}");

        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт в Excel (функция в разработке)");
        }

        public class WorkerModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            private bool _isEditing;
            private string _editDepartment;
            private string _editPosition;
            private string _editStatus;

            public string UserId { get; set; }
            public int WrkID { get; set; }
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

            // Эти свойства будут привязаны к ComboBox'ам
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

        private async void LoadWorkers()
        {
            try
            {
                allWorkers.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT w.WrkID,
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
                                FullName = $"{reader["LastName"]} {reader["FirstName"]} {reader["MiddleName"]}",
                                Department = reader["DepName"].ToString(),
                                Position = reader["PosName"].ToString(),
                                Phone = GetValue(reader["Phone"]),
                                Email = GetValue(reader["Email"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
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

        private void LoadFilters()
        {
            // Отделы — из данных сотрудников (уникальные)
            DepartmentFilterComboBox.Items.Clear();
            DepartmentFilterComboBox.Items.Add("Все отделы");
            foreach (var dep in allWorkers.Select(w => w.Department).Distinct().OrderBy(d => d))
                DepartmentFilterComboBox.Items.Add(dep);
            DepartmentFilterComboBox.SelectedIndex = 0;

            // Должности
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddWorkerWindow();
            win.ShowDialog();
            LoadWorkers();
        }

        private void EditWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактирование сотрудника (заглушка)");
        }

        private string GetValue(object val) =>
            val == null || string.IsNullOrWhiteSpace(val.ToString()) ? "Пусто" : val.ToString();

        private void CreateTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            int workerId = Convert.ToInt32((sender as Button).CommandParameter);
            MessageBox.Show($"Создать задачу для сотрудника ID: {workerId}");
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ID всех отмеченных сотрудников
            var selectedIds = allWorkers
                .Where(w => w.IsSelected)
                .Select(w => w.WrkID)
                .ToList();

            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Выберите сотрудников для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        // Удаляем записи из workers
                        string deleteQuery = $"DELETE FROM workers WHERE WrkID IN ({string.Join(",", selectedIds)})";
                        using (var cmd = new MySqlCommand(deleteQuery, conn, tx))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Логируем удаление каждого сотрудника
                        foreach (var id in selectedIds)
                        {
                            await Logger.LogAsync(Session.UserID, "Удаление сотрудника", "Управление персоналом", $"Удалён сотрудник с WrkID = {id}");
                        }

                        tx.Commit();
                    }
                }

                // Снимаем выделение и перезагружаем список
                foreach (var w in allWorkers)
                    w.IsSelected = false;
                LoadWorkers();

                MessageBox.Show("Выбранные сотрудники удалены.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}