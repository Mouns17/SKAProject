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
            string firstName = FirstNameBox.Text.Trim();
            string lastName = LastNameBox.Text.Trim();
            string group = GroupBox.Text.Trim();

            int course;
            if (!int.TryParse(CourseBox.Text, out course))
            {
                MessageBox.Show("Курс должен быть числом");
                return;
            }

            try
            {
                using (MySqlConnection conn = Db.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        // Users
                        MySqlCommand cmdUser = new MySqlCommand(@"
                            INSERT INTO Users(Login, PasswordHash, Email, Phone, Role)
                            VALUES(@l,@p,@e,@ph,'Student');
                            SELECT LAST_INSERT_ID();", conn, tx);

                        cmdUser.Parameters.AddWithValue("@l", _login);
                        cmdUser.Parameters.AddWithValue("@p", _password); // Без хэширования
                        cmdUser.Parameters.AddWithValue("@e", _email);
                        cmdUser.Parameters.AddWithValue("@ph", _phone);

                        int userId = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());

                        // Profiles
                        MySqlCommand cmdProfile = new MySqlCommand(@"
                            INSERT INTO Profiles(UserID, FirstName, LastName)
                            VALUES(@id,@f,@l)", conn, tx);

                        cmdProfile.Parameters.AddWithValue("@id", userId);
                        cmdProfile.Parameters.AddWithValue("@f", firstName);
                        cmdProfile.Parameters.AddWithValue("@l", lastName);

                        await cmdProfile.ExecuteNonQueryAsync();

                        // Students
                        MySqlCommand cmdStudent = new MySqlCommand(@"
                            INSERT INTO Students(UserID, GroupName, CourseNumber)
                            VALUES(@id,@g,@c)", conn, tx);

                        cmdStudent.Parameters.AddWithValue("@id", userId);
                        cmdStudent.Parameters.AddWithValue("@g", group);
                        cmdStudent.Parameters.AddWithValue("@c", course);

                        await cmdStudent.ExecuteNonQueryAsync();

                        tx.Commit();
                    }
                }

                MessageBox.Show("Регистрация завершена");
                new LogginWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
    }
}
