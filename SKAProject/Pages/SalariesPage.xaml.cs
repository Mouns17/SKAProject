using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class SalariesPage : Page
    {
        private ObservableCollection<SalaryEntry> _salaries = new ObservableCollection<SalaryEntry>();

        public SalariesPage()
        {
            InitializeComponent();
            LoadSalaries();
        }

        public class SalaryEntry : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int RowNumber { get; set; }
            public string EmployeeFullName { get; set; }
            public decimal Salary { get; set; }
            public decimal Bonus { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadSalaries()
        {
            try
            {
                _salaries.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT s.Id, s.Salary, s.Bonus,
                               CONCAT(u.LastName, ' ', u.FirstName) AS EmployeeFullName
                        FROM salary s
                        JOIN users u ON s.EmployeeUserID = u.UserID
                        ORDER BY u.LastName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int row = 1;
                        while (await reader.ReadAsync())
                        {
                            _salaries.Add(new SalaryEntry
                            {
                                Id = reader.GetInt32("Id"),
                                RowNumber = row++,
                                EmployeeFullName = reader.GetString("EmployeeFullName"),
                                Salary = reader.GetDecimal("Salary"),
                                Bonus = reader.GetDecimal("Bonus")
                            });
                        }
                    }
                }
                SalariesItemsControl.ItemsSource = _salaries;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки окладов: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var empDialog = new InputDialog("Введите UserID сотрудника:");
            if (empDialog.ShowDialog() != true || !int.TryParse(empDialog.Result, out int userId)) return;
            var salDialog = new InputDialog("Введите оклад (руб.):", "0");
            if (salDialog.ShowDialog() != true || !decimal.TryParse(salDialog.Result, out decimal salary)) return;
            var bonDialog = new InputDialog("Введите надбавку (руб.):", "0");
            if (bonDialog.ShowDialog() != true || !decimal.TryParse(bonDialog.Result, out decimal bonus)) return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    string insert = @"INSERT INTO salary (EmployeeUserID, Salary, Bonus, EffectiveFrom)
                                     VALUES (@uid, @sal, @bon, NOW())";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.Parameters.AddWithValue("@sal", salary);
                        cmd.Parameters.AddWithValue("@bon", bonus);
                        cmd.ExecuteNonQuery();
                    }
                }
                Logger.LogAsync(Session.UserID, "Назначение оклада", "Расчёты",
                    $"Сотрудник UserID={userId} получил оклад {salary} руб.").Wait();
                LoadSalaries();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления оклада: " + ex.Message);
            }
        }

        private async void DeleteSalary_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string del = "DELETE FROM salary WHERE Id = @id";
                    using (var cmd = new MySqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Удаление оклада", "Расчёты", $"Удалён оклад ID={id}");
                LoadSalaries();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}