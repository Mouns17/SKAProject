using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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

            try
            {
                // Подключение к бд
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        MySqlCommand cmdUser = new MySqlCommand(@"INSERT INTO Users(Login, Password, FirstName, LastName, MiddleName) VALUES(@lg,@ps, @fn, @ln, @mn); SELECT LAST_INSERT_ID();", conn, tx);

                        // Передаём параметры в запрос
                        cmdUser.Parameters.AddWithValue("@lg", login);
                        cmdUser.Parameters.AddWithValue("@ps", password);
                        cmdUser.Parameters.AddWithValue("@fn", firstname);
                        cmdUser.Parameters.AddWithValue("@ln", lastname);
                        cmdUser.Parameters.AddWithValue("@mn", middlename);

                        // Выполняем запрос
                        await cmdUser.ExecuteScalarAsync();

                        // Подтверждение транзакции
                        tx.Commit();
                    }
                }

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                MessageBox.Show("Регистрация завершена");
                new LoginWindow().Show();
                Close();
            }

            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }

        }

        private void BtnBack(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
