using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class UsersPage : Page
    {
        // Список доступных ролей (статический, чтобы привязка работала)
        public List<string> AvailableRoles { get; } = new List<string>
        {
            "Owner", "Admin", "Director", "HR", "Accountant", "DepartmentHead", "User"
        };

        private ObservableCollection<UserEntry> _users = new ObservableCollection<UserEntry>();

        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        public class UserEntry : INotifyPropertyChanged
        {
            private string _selectedRole;
            public int UserID { get; set; }
            public string Login { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public bool Activated { get; set; }
            public string ActivatedText => Activated ? "✅ Да" : "❌ Нет";
            public string ActivatedColor => Activated ? "Green" : "Red";

            public string OriginalRole { get; set; }
            public string SelectedRole
            {
                get => _selectedRole;
                set
                {
                    if (_selectedRole != value)
                    {
                        _selectedRole = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRole)));
                    }
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadUsers()
        {
            try
            {
                _users.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT UserID, Login, FirstName, LastName, MiddleName, Email, Role, Activated
                                     FROM users
                                     ORDER BY UserID";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new UserEntry
                            {
                                UserID = reader.GetInt32("UserID"),
                                Login = reader.GetString("Login"),
                                FullName = $"{reader["LastName"]} {reader["FirstName"]} {reader["MiddleName"]}",
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                                Activated = reader.GetBoolean("Activated"),
                                OriginalRole = reader.GetString("Role")
                            };
                            user.SelectedRole = user.OriginalRole; // начальное значение
                            _users.Add(user);
                        }
                    }
                }
                UsersItemsControl.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message);
            }
        }

        private async void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox comboBox) || comboBox.Tag == null) return;
            if (comboBox.SelectedItem == null) return;

            int userId = Convert.ToInt32(comboBox.Tag);
            string newRole = comboBox.SelectedItem.ToString();

            // Проверяем, действительно ли роль изменилась
            var user = _users.FirstOrDefault(u => u.UserID == userId);
            if (user == null || user.OriginalRole == newRole) return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string upd = "UPDATE users SET Role = @role WHERE UserID = @id";
                    using (var cmd = new MySqlCommand(upd, conn))
                    {
                        cmd.Parameters.AddWithValue("@role", newRole);
                        cmd.Parameters.AddWithValue("@id", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await Logger.LogAsync(Session.UserID, "Изменение роли пользователя", "Пользователи",
                    $"Роль пользователя UserID={userId} изменена на {newRole}");

                user.OriginalRole = newRole;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения роли: " + ex.Message);
                // Откатываем ComboBox к предыдущему значению
                user.SelectedRole = user.OriginalRole;
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int userId = Convert.ToInt32(button.CommandParameter);

            if (userId == Session.UserID)
            {
                MessageBox.Show("Вы не можете удалить сами себя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить пользователя с ID {userId}? Это действие необратимо.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string del = "DELETE FROM users WHERE UserID = @id";
                    using (var cmd = new MySqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await Logger.LogAsync(Session.UserID, "Удаление пользователя", "Пользователи",
                    $"Удалён пользователь UserID={userId}");

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}