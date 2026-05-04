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
using MySqlConnector;

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

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все обязательные поля (логин и пароль).");
                return;
            }

            try
            {
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    // Проверяем, не занят ли логин
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE Login = @lg";
                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@lg", login);
                        long exists = (long)await checkCmd.ExecuteScalarAsync();
                        if (exists > 0)
                        {
                            MessageBox.Show(
                                "Этот логин уже используется. Пожалуйста, выберите другой.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                            return;
                        }
                    }

                    // Начинаем транзакцию только после проверки
                    using (var tx = conn.BeginTransaction())
                    {
                        MySqlCommand cmdUser = new MySqlCommand(
                            @"INSERT INTO users (Login, Password, FirstName, LastName, MiddleName) 
                      VALUES (@lg, @ps, @fn, @ln, @mn); 
                      SELECT LAST_INSERT_ID();",
                            conn,
                            tx
                        );

                        cmdUser.Parameters.AddWithValue("@lg", login);
                        cmdUser.Parameters.AddWithValue("@ps", password); // пароль лучше хэшировать, но пока оставим так
                        cmdUser.Parameters.AddWithValue("@fn", firstname);
                        cmdUser.Parameters.AddWithValue("@ln", lastname);
                        cmdUser.Parameters.AddWithValue("@mn", middlename);

                        int newUserId = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());
                        tx.Commit();

                        // Запись в лог
                        await Logger.LogAsync(
                            newUserId,
                            "Регистрация нового пользователя",
                            "Пользователи",
                            $"Логин: {login}"
                        );
                    }
                }

                MessageBox.Show("Регистрация успешно завершена!");
                new LoginWindow().Show();
                this.Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062) // дубликат ключа
            {
                MessageBox.Show(
                    "Этот логин уже занят. Выберите другой.",
                    "Ошибка регистрации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
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

        private void BtnRollup(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
