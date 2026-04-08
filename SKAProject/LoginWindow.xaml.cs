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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
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
            // Получем данные и удаляем пробелы в них
            string login = TBoxLogin.Text.Trim();
            string password = TboxPassword.Text.Trim();

            // Проверка на пустые поля
            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                // Подключение к БД
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    // Проверка есть ли пользователь с таким логином и паролем
                    var cmd = new MySqlCommand(@"SELECT UserID FROM Users WHERE Login=@lg AND Password=@ps", conn);

                    // Передаем параметры в запрос
                    cmd.Parameters.AddWithValue("@lg", login);
                    cmd.Parameters.AddWithValue("@ps", password);

                    // Выполняем запрос
                    var result = await cmd.ExecuteScalarAsync();

                    // Тут думаю ничего не надо обьяснять
                    if (result != null)
                    {
                        MessageBox.Show("Вход выполнен");

                        new MainWindow().Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль");
                    }
                }
            }

            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка авторизации: " + ex.Message);
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
