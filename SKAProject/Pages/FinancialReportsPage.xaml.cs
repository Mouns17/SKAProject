using ClosedXML.Excel;
using Microsoft.Win32;
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
    public partial class FinancialReportsPage : Page
    {
        private ObservableCollection<PayrollEntry> _reportData = new ObservableCollection<PayrollEntry>();

        public FinancialReportsPage()
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
            _reportData.Clear();
            PanelSummary.Visibility = Visibility.Collapsed;
            BtnExport.Visibility = Visibility.Collapsed;

            int month = CmbMonth.SelectedIndex + 1;

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
                            _reportData.Add(new PayrollEntry
                            {
                                RowNumber = row++,
                                EmployeeFullName = $"{reader["LastName"]} {reader["FirstName"]}",
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Bonus = reader.GetDecimal(reader.GetOrdinal("Bonus"))
                            });
                        }
                    }
                }

                ReportItemsControl.ItemsSource = _reportData;

                if (_reportData.Count > 0)
                {
                    decimal totalPayroll = _reportData.Sum(x => x.Salary + x.Bonus);
                    decimal avgSalary = totalPayroll / _reportData.Count;
                    TxtTotalPayroll.Text = $"{totalPayroll:N2} руб.";
                    TxtAvgSalary.Text = $"{avgSalary:N2} руб.";
                    TxtEmployeeCount.Text = _reportData.Count.ToString();
                    PanelSummary.Visibility = Visibility.Visible;
                    BtnExport.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Нет данных для выбранного периода. Проверьте, назначены ли оклады сотрудникам.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при формировании отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Финансовый отчёт");
                ws.Cell(1, 1).Value = "№";
                ws.Cell(1, 2).Value = "Сотрудник";
                ws.Cell(1, 3).Value = "Оклад (руб.)";
                ws.Cell(1, 4).Value = "Надбавка (руб.)";
                ws.Cell(1, 5).Value = "НДФЛ (руб.)";
                ws.Cell(1, 6).Value = "К выдаче (руб.)";

                int row = 2;
                foreach (var item in _reportData)
                {
                    ws.Cell(row, 1).Value = item.RowNumber;
                    ws.Cell(row, 2).Value = item.EmployeeFullName;
                    ws.Cell(row, 3).Value = item.Salary;
                    ws.Cell(row, 4).Value = item.Bonus;
                    ws.Cell(row, 5).Value = item.Tax;
                    ws.Cell(row, 6).Value = item.NetPay;
                    row++;
                }
                ws.Columns().AdjustToContents();

                var dlg = new SaveFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx", FileName = "Финансовый_отчёт.xlsx" };
                if (dlg.ShowDialog() == true)
                {
                    workbook.SaveAs(dlg.FileName);
                    MessageBox.Show("Экспорт выполнен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}