using MySqlConnector;
using System;
using System.Collections.ObjectModel;
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

        // Создание задачи для выбранного сотрудника
        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            int workerId = Convert.ToInt32((sender as Button).CommandParameter);
            // Заглушка – позже замените на открытие окна создания задачи
            MessageBox.Show($"Создать задачу для сотрудника ID: {workerId}");
        }

        // Редактирование сотрудника по кнопке в строке таблицы
        private void EditWorkerRow_Click(object sender, RoutedEventArgs e)
        {
            int workerId = Convert.ToInt32((sender as Button).CommandParameter);
            // Заглушка – можно открыть EditWorkerWindow (когда будет готово)
            MessageBox.Show($"Редактировать сотрудника ID: {workerId}");

            // Пример реального вызова (если окно существует):
            // var editWindow = new EditWorkerWindow(workerId);
            // editWindow.ShowDialog();
            // LoadWorkers();
        }

        // Экспорт в Excel
        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт в Excel (функция в разработке)");
            // Здесь будет вызов вашего экспорта
        }

        public class WorkerModel
        {
            public int WrkId { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public Brush StatusColor =>
                Status == "Работает" ? Brushes.Green :
                Status == "Уволен" ? Brushes.Red :
                Status == "В отпуске" ? Brushes.Orange : Brushes.Gray;
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
                                WrkId = reader.GetInt32("WrkID"),
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
            // placeholder скрытие/показ можно оставить
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

        private async void DeleteWorker_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as Button).CommandParameter);
            if (MessageBox.Show("Удалить сотрудника?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand("DELETE FROM Workers WHERE WrkID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    string logMsg = $"Удалён сотрудник с WrkID = {id}";
                    await Logger.LogAsync(Session.UserID, "Удаление сотрудника", "Управление персоналом", logMsg);

                }
                LoadWorkers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private string GetValue(object val) =>
            val == null || string.IsNullOrWhiteSpace(val.ToString()) ? "Пусто" : val.ToString();
    }
}