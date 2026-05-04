using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class MyOrganizationPage : Page
    {
        // Основные данные
        private string _currentAddress = "";
        private string _currentEmail = "";
        private string _currentPhone = "";

        // Коллекции для списков
        private ObservableCollection<DepartmentItem> _departments = new ObservableCollection<DepartmentItem>();
        private ObservableCollection<PositionItem> _positions = new ObservableCollection<PositionItem>();

        public MyOrganizationPage()
        {
            InitializeComponent();
            LoadOrganizationData();
            LoadDepartments();
            LoadPositions();
            LoadStatistics();
        }

        // ===== Модели для списков =====
        public class DepartmentItem : INotifyPropertyChanged
        {
            public int DepID { get; set; }
            public string DepName { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class PositionItem : INotifyPropertyChanged
        {
            public int PosID { get; set; }
            public string PosName { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        // ===== Загрузка информации об организации =====
        private async void LoadOrganizationData()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT Address, Phone, Email FROM company_info WHERE CompanyID = 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Получаем индексы
                            int addrIdx = reader.GetOrdinal("Address");
                            int phoneIdx = reader.GetOrdinal("Phone");
                            int emailIdx = reader.GetOrdinal("Email");

                            _currentAddress = reader.IsDBNull(addrIdx) ? "" : reader.GetString(addrIdx);
                            _currentEmail = reader.IsDBNull(emailIdx) ? "" : reader.GetString(emailIdx);
                            _currentPhone = reader.IsDBNull(phoneIdx) ? "" : reader.GetString(phoneIdx);
                        }
                    }
                }

                // Обновляем отображение
                TBlockOrgAddress.Text = string.IsNullOrWhiteSpace(_currentAddress) ? "Не указан" : _currentAddress;
                TBlockOrgEmail.Text = string.IsNullOrWhiteSpace(_currentEmail) ? "Не указан" : _currentEmail;
                TBlockOrgPhone.Text = string.IsNullOrWhiteSpace(_currentPhone) ? "Не указан" : _currentPhone;
                TBlockOrgName.Text = "Моя организация";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных организации: " + ex.Message);
            }
        }

        // ===== Загрузка списков =====
        private async void LoadDepartments()
        {
            try
            {
                _departments.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT DepID, DepName FROM departments ORDER BY DepName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _departments.Add(new DepartmentItem
                            {
                                DepID = reader.GetInt32("DepID"),
                                DepName = reader.GetString("DepName")
                            });
                        }
                    }
                }
                DepartmentsItemsControl.ItemsSource = _departments;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки отделов: " + ex.Message);
            }
        }

        private async void LoadPositions()
        {
            try
            {
                _positions.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT PosID, PosName FROM positions ORDER BY PosName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _positions.Add(new PositionItem
                            {
                                PosID = reader.GetInt32("PosID"),
                                PosName = reader.GetString("PosName")
                            });
                        }
                    }
                }
                PositionsItemsControl.ItemsSource = _positions;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки должностей: " + ex.Message);
            }
        }

        private async void LoadStatistics()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    // Сотрудники
                    string workerSql = "SELECT COUNT(*) FROM workers WHERE Status IN ('Работает', 'В отпуске')";
                    using (var cmd = new MySqlCommand(workerSql, conn))
                    {
                        long cnt = (long)await cmd.ExecuteScalarAsync();
                        TxtEmployeeCount.Text = cnt.ToString();
                    }

                    // Отделы
                    string deptSql = "SELECT COUNT(*) FROM departments";
                    using (var cmd = new MySqlCommand(deptSql, conn))
                    {
                        long cnt = (long)await cmd.ExecuteScalarAsync();
                        TxtDepartmentCount.Text = cnt.ToString();
                    }

                    // Должности
                    string posSql = "SELECT COUNT(*) FROM positions";
                    using (var cmd = new MySqlCommand(posSql, conn))
                    {
                        long cnt = (long)await cmd.ExecuteScalarAsync();
                        TxtPositionCount.Text = cnt.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статистики: " + ex.Message);
            }
        }

        // ===== Редактирование организации (как в профиле) =====
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Session.Role == "User")
            {
                BtnEdit.Visibility = Visibility.Collapsed;
            }

            else
            {
                // Переключаем в режим редактирования
                TBlockOrgAddress.Visibility = Visibility.Collapsed;
                TBlockOrgEmail.Visibility = Visibility.Collapsed;
                TBlockOrgPhone.Visibility = Visibility.Collapsed;

                TBoxAddress.Text = _currentAddress;
                TBoxEmail.Text = _currentEmail;
                TBoxPhone.Text = _currentPhone;

                TBoxAddress.Visibility = Visibility.Visible;
                TBoxEmail.Visibility = Visibility.Visible;
                TBoxPhone.Visibility = Visibility.Visible;

                BtnEdit.Visibility = Visibility.Collapsed;
                PanelSaveCancel.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Возврат к режиму просмотра
            TBoxAddress.Visibility = Visibility.Collapsed;
            TBoxEmail.Visibility = Visibility.Collapsed;
            TBoxPhone.Visibility = Visibility.Collapsed;

            TBlockOrgAddress.Visibility = Visibility.Visible;
            TBlockOrgEmail.Visibility = Visibility.Visible;
            TBlockOrgPhone.Visibility = Visibility.Visible;

            BtnEdit.Visibility = Visibility.Visible;
            PanelSaveCancel.Visibility = Visibility.Collapsed;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newAddress = TBoxAddress.Text.Trim();
            string newEmail = TBoxEmail.Text.Trim();
            string newPhone = TBoxPhone.Text.Trim();

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    // Обновляем company_info (предполагается, что CompanyID = 1)
                    string update = @"UPDATE company_info SET Address = @addr, Phone = @phone, Email = @email
                                      WHERE CompanyID = 1";
                    using (var cmd = new MySqlCommand(update, conn))
                    {
                        cmd.Parameters.AddWithValue("@addr", newAddress);
                        cmd.Parameters.AddWithValue("@phone", newPhone);
                        cmd.Parameters.AddWithValue("@email", newEmail);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Логгируем
                await Logger.LogAsync(Session.UserID, "Редактирование организации", "Организация",
                    "Обновлены контактные данные");

                // Обновляем поля и возвращаем просмотр
                _currentAddress = newAddress;
                _currentEmail = newEmail;
                _currentPhone = newPhone;

                TBlockOrgAddress.Text = string.IsNullOrWhiteSpace(newAddress) ? "Не указан" : newAddress;
                TBlockOrgEmail.Text = string.IsNullOrWhiteSpace(newEmail) ? "Не указан" : newEmail;
                TBlockOrgPhone.Text = string.IsNullOrWhiteSpace(newPhone) ? "Не указан" : newPhone;

                BtnCancel_Click(sender, e); // вернём в режим просмотра
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        // ===== Добавление отделов и должностей (используем готовые окна) =====
        private void BtnAddDepartment(object sender, RoutedEventArgs e)
        {
            var win = new AddDepartmentWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                LoadDepartments();
                LoadStatistics();
            }
        }

        private void BtnAddPost(object sender, RoutedEventArgs e)
        {
            var win = new AddPostWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                LoadPositions();
                LoadStatistics();
            }
        }

        // ===== Удаление =====
        private async void DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int depId = Convert.ToInt32(button.CommandParameter);

            if (MessageBox.Show("Удалить отдел и всех сотрудников в нём?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = DataBase.GetConnection())
                    {
                        await conn.OpenAsync();
                        using (var tx = conn.BeginTransaction())
                        {
                            // Сбросить DepID у работников
                            string upd = "UPDATE workers SET DepID = NULL WHERE DepID = @id";
                            using (var cmd = new MySqlCommand(upd, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", depId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                            // Удалить отдел
                            string del = "DELETE FROM departments WHERE DepID = @id";
                            using (var cmd = new MySqlCommand(del, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", depId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                            tx.Commit();
                        }
                    }

                    await Logger.LogAsync(Session.UserID, "Удаление отдела", "Управление организацией",
                        $"Удалён отдел с ID={depId}");
                    LoadDepartments();
                    LoadStatistics();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private async void DeletePosition_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int posId = Convert.ToInt32(button.CommandParameter);

            if (MessageBox.Show("Удалить должность и сбросить её у сотрудников?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = DataBase.GetConnection())
                    {
                        await conn.OpenAsync();
                        using (var tx = conn.BeginTransaction())
                        {
                            // Сбросить PosID у работников
                            string upd = "UPDATE workers SET PosID = NULL WHERE PosID = @id";
                            using (var cmd = new MySqlCommand(upd, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", posId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                            // Удалить должность
                            string del = "DELETE FROM positions WHERE PosID = @id";
                            using (var cmd = new MySqlCommand(del, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", posId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                            tx.Commit();
                        }
                    }

                    await Logger.LogAsync(Session.UserID, "Удаление должности", "Управление организацией",
                        $"Удалена должность с ID={posId}");
                    LoadPositions();
                    LoadStatistics();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
    }
}