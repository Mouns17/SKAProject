using MySqlConnector;
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

namespace SKAProject
{
    public partial class RegOrgWindow : Window
    {
        public RegOrgWindow()
        {
            InitializeComponent();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnEnter(object sender, RoutedEventArgs e)
        {
            string name = TBoxOrgName.Text.Trim();
            string address = TBoxOrgAdress.Text.Trim();
            string phone = TBoxOrgPhone.Text.Trim();
            string email = TBoxOrgEmail.Text.Trim();

            try
            {
                // Подключение к бд
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        MySqlCommand cmdUser = new MySqlCommand(@"INSERT INTO Organizations(OrgName, Address, Phone, Email) VALUES(@on,@ad, @ph, @em); SELECT LAST_INSERT_ID();", conn, tx);

                        // Передаём параметры в запрос
                        cmdUser.Parameters.AddWithValue("@on", name);
                        cmdUser.Parameters.AddWithValue("@ad", address);
                        cmdUser.Parameters.AddWithValue("@ph", phone);
                        cmdUser.Parameters.AddWithValue("@em", email);

                        // Выполняем запрос
                        await cmdUser.ExecuteScalarAsync();

                        // Подтверждение транзакции
                        tx.Commit();
                    }
                }

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                MessageBox.Show("Регистрация завершена");
                Close();
            }

            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
