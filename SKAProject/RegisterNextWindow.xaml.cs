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
using System.Windows.Shapes;
using MySqlConnector;
using System.Security.Cryptography;

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для RegisterNextWindow.xaml
    /// </summary>
    public partial class RegisterNextWindow : Window
    {
        private string _login;
        private string _password;
        private string _email;
        private string _phone;

        public RegisterNextWindow(string login, string password, string email, string phone)
        {
            InitializeComponent();

            _login = login;
            _password = password;
            _email = email;
            _phone = phone;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            // Получем данные и удаляем пробелы в них
            string firstName = FirstNameBox.Text.Trim();
            string lastName = LastNameBox.Text.Trim();
            string group = GroupBox.Text.Trim();

            int course;

            // Проверка что введенное значение число
            if (!int.TryParse(CourseBox.Text, out course))
            {
                MessageBox.Show("Курс должен быть числом");
                return;
            }

            try
            {
                // Подключение к бд
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        // Users
                        MySqlCommand cmdUser = new MySqlCommand(@"INSERT INTO Users(Login, Password, Email, Phone, Role) VALUES(@l,@p,@e,@ph,'Student'); SELECT LAST_INSERT_ID();", conn, tx);

                        // Передаём параметры в запрос
                        cmdUser.Parameters.AddWithValue("@l", _login);
                        cmdUser.Parameters.AddWithValue("@p", _password); 
                        cmdUser.Parameters.AddWithValue("@e", _email);
                        cmdUser.Parameters.AddWithValue("@ph", _phone);

                        // Выполняем запрос
                        int userId = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());

                        // Profiles
                        MySqlCommand cmdProfile = new MySqlCommand(@"INSERT INTO Profiles(UserID, FirstName, LastName) VALUES(@id,@f,@l)", conn, tx);

                        // Передаём параметры в запрос
                        cmdProfile.Parameters.AddWithValue("@id", userId);
                        cmdProfile.Parameters.AddWithValue("@f", firstName);
                        cmdProfile.Parameters.AddWithValue("@l", lastName);

                        // Выполняем запрос
                        await cmdProfile.ExecuteNonQueryAsync();

                        // Students
                        MySqlCommand cmdStudent = new MySqlCommand(@"INSERT INTO Students(UserID, GroupName, CourseNumber) VALUES(@id,@g,@c)", conn, tx);

                        // Передаём параметры в запрос
                        cmdStudent.Parameters.AddWithValue("@id", userId);
                        cmdStudent.Parameters.AddWithValue("@g", group);
                        cmdStudent.Parameters.AddWithValue("@c", course);

                        // Выполняем запрос
                        await cmdStudent.ExecuteNonQueryAsync();

                        // Проверка на существующий логин
                        MySqlCommand checkUser = new MySqlCommand(@"SELECT COUNT(*) FROM Users WHERE Login=@login", conn);

                        checkUser.Parameters.AddWithValue("@login", _login);

                        int exists = Convert.ToInt32(await checkUser.ExecuteScalarAsync());

                        if (exists > 0)
                        {
                            MessageBox.Show("Такой логин уже существует");
                            return;
                        }

                        // Подтверждение транзакции
                        tx.Commit();
                    }
                } 

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(group))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                MessageBox.Show("Регистрация завершена");
                new LogginWindow().Show();
                Close();
            }

            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }

        private void OpenLoggin_Click(object sender, RoutedEventArgs e)
        {
            LogginWindow shpionirogolubiro = new LogginWindow();
            shpionirogolubiro.Show();
            this.Close();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow shpionirogolubiro = new RegisterWindow();
            shpionirogolubiro.Show();
            this.Close();
        }
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
