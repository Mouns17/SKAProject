using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class OrdersPage : Page
    {
        private ObservableCollection<OrderEntry> _orders = new ObservableCollection<OrderEntry>();
        private ObservableCollection<EmployeeItem> _employees = new ObservableCollection<EmployeeItem>();

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            LoadEmployees();
        }

        public class OrderEntry : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int RowNumber { get; set; }
            public string OrderNumber { get; set; }
            public DateTime OrderDate { get; set; }
            public string OrderType { get; set; }
            public string EmployeeFullName { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class EmployeeItem
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
        }

        private async void LoadOrders()
        {
            try
            {
                _orders.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT o.Id, o.OrderNumber, o.OrderDate, o.OrderType,
                               CONCAT(u.LastName, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS EmployeeFullName
                        FROM orders o
                        JOIN users u ON o.EmployeeUserID = u.UserID
                        ORDER BY o.OrderDate DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int row = 1;
                        while (await reader.ReadAsync())
                        {
                            _orders.Add(new OrderEntry
                            {
                                Id = reader.GetInt32("Id"),
                                RowNumber = row++,
                                OrderNumber = reader.GetString("OrderNumber"),
                                OrderDate = reader.GetDateTime("OrderDate"),
                                OrderType = reader.GetString("OrderType"),
                                EmployeeFullName = reader.GetString("EmployeeFullName")
                            });
                        }
                    }
                }
                OrdersItemsControl.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки приказов: " + ex.Message);
            }
        }

        private async void LoadEmployees()
        {
            try
            {
                _employees.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT u.UserID, CONCAT(u.LastName,' ',u.FirstName,' ',COALESCE(u.MiddleName,'')) AS FullName
                                     FROM users u
                                     JOIN workers w ON u.UserID = w.UserID
                                     WHERE w.Status IN ('Работает','В отпуске')
                                     ORDER BY u.LastName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _employees.Add(new EmployeeItem
                            {
                                UserId = reader.GetInt32("UserID"),
                                FullName = reader.GetString("FullName")
                            });
                        }
                    }
                }
                CmbEmployee.ItemsSource = _employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var numDialog = new InputDialog("Введите номер приказа:");
            if (numDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(numDialog.Result)) return;
            var typeDialog = new InputDialog("Введите тип приказа (Приём / Увольнение / Отпуск):");
            if (typeDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(typeDialog.Result)) return;

            if (CmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника из списка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int userId = (int)CmbEmployee.SelectedValue;

            string orderNum = numDialog.Result.Trim();
            string orderType = typeDialog.Result.Trim();

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string insert = @"INSERT INTO orders (OrderNumber, OrderDate, OrderType, EmployeeUserID)
                                     VALUES (@num, NOW(), @type, @uid)";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@num", orderNum);
                        cmd.Parameters.AddWithValue("@type", orderType);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Создание приказа", "Кадры",
                    $"Приказ {orderNum} ({orderType}) для сотрудника {CmbEmployee.Text}");
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания приказа: " + ex.Message);
            }
        }

        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string del = "DELETE FROM orders WHERE Id = @id";
                    using (var cmd = new MySqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Удаление приказа", "Кадры", $"Удалён приказ ID={id}");
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}