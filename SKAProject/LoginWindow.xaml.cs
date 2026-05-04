using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySqlConnector;

namespace SKAProject
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OpenRegWindow(object sender, RoutedEventArgs e)
        {
            RegistrationWindow reg = new RegistrationWindow();
            reg.Show();
            this.Close();
        }

        private async void BtnEnter(object sender, RoutedEventArgs e)
        {
            string login = TBoxLogin.Text.Trim();
            string password = TboxPassword.Text.Trim();
            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    // Изменён запрос – теперь выбираем и UserID, и Role
                    var cmd = new MySqlCommand(
                        @"SELECT UserID, Role FROM Users WHERE Login=@lg AND Password=@ps",
                        conn
                    );

                    cmd.Parameters.AddWithValue("@lg", login);
                    cmd.Parameters.AddWithValue("@ps", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int userId = reader.GetInt32("UserID");
                            string role = reader.GetString("Role");

                            // Сохраняем в сессию
                            Session.UserID = userId;
                            Session.Role = role;

                            // Логирование входа
                            await Logger.LogAsync(
                                userId,
                                "Вход в систему",
                                "Авторизация",
                                "Успешный вход"
                            );

                            new MainWindow().Show();
                            this.Close();
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

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnRollup(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
