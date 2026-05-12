using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnClose(object sender, RoutedEventArgs e) => Close();
        private void BtnRollup(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void OpenRegWindow(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().Show();
            Close();
        }

        private async void BtnEnter(object sender, RoutedEventArgs e)
        {
            string login = TBoxLogin.Text.Trim();
            string password = TboxPassword.Text.Trim();
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("SELECT UserID, Role FROM users WHERE Login=@lg AND Password=@ps", conn);
                    cmd.Parameters.AddWithValue("@lg", login);
                    cmd.Parameters.AddWithValue("@ps", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int userId = reader.GetInt32("UserID");
                            string role = reader.GetString("Role");
                            Session.UserID = userId;
                            Session.Role = role;
                            await Logger.LogAsync(userId, "Вход в систему", "Авторизация", "Успешный вход");
                            new MainWindow().Show();
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка авторизации: " + ex.Message);
            }
        }
    }
}