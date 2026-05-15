using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnClose(object sender, RoutedEventArgs e) => Close();
        private void BtnRollup(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnBack(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void BtnNextReg(object sender, RoutedEventArgs e)
        {
            TBoxLogin.Visibility = Visibility.Collapsed;
            TBoxPassword.Visibility = Visibility.Collapsed;
            TBoxRePassword.Visibility = Visibility.Collapsed;

            TBoxFirstName.Visibility = Visibility.Visible;
            TBoxLastName.Visibility = Visibility.Visible;
            TBoxMiddleName.Visibility = Visibility.Visible;

            BtnNextRegVis.Visibility = Visibility.Collapsed;
            BtnRegisterVis.Visibility = Visibility.Visible;
        }

        private async void BtnRegister(object sender, RoutedEventArgs e)
        {
            string login = TBoxLogin.Text.Trim();
            string password = TBoxPassword.Text.Trim();
            string firstname = TBoxFirstName.Text.Trim();
            string lastname = TBoxLastName.Text.Trim();
            string middlename = TBoxMiddleName.Text.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все обязательные поля (логин и пароль).");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    string countQuery = "SELECT COUNT(*) FROM users";
                    string role;
                    using (var cmd = new MySqlCommand(countQuery, conn))
                    {
                        long userCount = (long)await cmd.ExecuteScalarAsync();
                        role = userCount == 0 ? "Owner" : "User";
                    }

                    string checkLogin = "SELECT COUNT(*) FROM users WHERE Login = @lg";
                    using (var cmd = new MySqlCommand(checkLogin, conn))
                    {
                        cmd.Parameters.AddWithValue("@lg", login);
                        long exists = (long)await cmd.ExecuteScalarAsync();
                        if (exists > 0)
                        {
                            MessageBox.Show("Этот логин уже используется. Пожалуйста, выберите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string insert = @"INSERT INTO users (Login, Password, FirstName, LastName, MiddleName, Role, Activated)
                                      VALUES (@lg, @ps, @fn, @ln, @mn, @role, 1)";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@lg", login);
                        cmd.Parameters.AddWithValue("@ps", password);
                        cmd.Parameters.AddWithValue("@fn", firstname);
                        cmd.Parameters.AddWithValue("@ln", lastname);
                        cmd.Parameters.AddWithValue("@mn", middlename);
                        cmd.Parameters.AddWithValue("@role", role);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await Logger.LogAsync(null, "Регистрация нового пользователя", "Пользователи", $"Логин: {login}, Роль: {role}");
                    MessageBox.Show($"Регистрация успешно завершена! Ваша роль: {role}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    new LoginWindow().Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}