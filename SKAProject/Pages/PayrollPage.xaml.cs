using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class PayrollPage : Page
    {
        private ObservableCollection<PayrollEntry> _payroll = new ObservableCollection<PayrollEntry>();

        public PayrollPage()
        {
            InitializeComponent();
            // Заполняем ComboBox месяцами
            for (int i = 1; i <= 12; i++)
                CmbMonth.Items.Add(new ComboBoxItem { Content = new DateTime(2020, i, 1).ToString("MMMM", CultureInfo.CurrentCulture) });
            CmbMonth.SelectedIndex = DateTime.Now.Month - 1;
        }

        public class PayrollEntry : INotifyPropertyChanged
        {
            public int RowNumber { get; set; }
            public string EmployeeFullName { get; set; }
            public decimal Salary { get; set; }
            public decimal Bonus { get; set; }
            public decimal Tax => Math.Round((Salary + Bonus) * 0.13m, 2);
            public decimal NetPay => Math.Round((Salary + Bonus) - Tax, 2);
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            _payroll.Clear();
            int month = CmbMonth.SelectedIndex + 1; // 1‑‑12
            // В реальном проекте нужно учитывать год и фактически отработанное время.
            // Здесь просто демонстрируем расчёт по данным из таблицы salary.

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT u.LastName, u.FirstName, s.Salary, s.Bonus
                        FROM salary s
                        JOIN users u ON s.EmployeeUserID = u.UserID
                        WHERE u.UserID IN (SELECT UserID FROM workers WHERE Status IN ('Работает','В отпуске'))
                        ORDER BY u.LastName, u.FirstName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int row = 1;
                        while (await reader.ReadAsync())
                        {
                            _payroll.Add(new PayrollEntry
                            {
                                RowNumber = row++,
                                EmployeeFullName = $"{reader["LastName"]} {reader["FirstName"]}",
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Bonus = reader.GetDecimal(reader.GetOrdinal("Bonus"))
                            });
                        }
                    }
                }
                PayrollItemsControl.ItemsSource = _payroll;
                if (_payroll.Count == 0)
                    MessageBox.Show("Нет данных для расчёта. Проверьте, назначены ли оклады сотрудникам.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при расчёте: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}