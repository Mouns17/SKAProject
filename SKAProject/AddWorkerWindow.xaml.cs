using MySqlConnector;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace SKAProject
{
    public partial class AddWorkerWindow : Window
    {
        public AddWorkerWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnNext(object sender, RoutedEventArgs e)
        {
            TBoxWorkLastName.Visibility = Visibility.Collapsed;
            TBoxWorkFirstName.Visibility = Visibility.Collapsed;
            TBoxWorkMiddleName.Visibility = Visibility.Collapsed;
            TBoxWorkPhone.Visibility = Visibility.Collapsed;

            TBoxWorkEmail.Visibility = Visibility.Visible;
            TBoxWorkDep.Visibility = Visibility.Visible;
            TBoxWorkPost.Visibility = Visibility.Visible;
            TBoxWorkStatus.Visibility = Visibility.Visible;

            BtnNextVis.Visibility = Visibility.Collapsed;
            BtnAddVis.Visibility = Visibility.Visible;
        }

        private async void BtnAdd(object sender, RoutedEventArgs e)
        {
            string department = TBoxWorkDep.Text.Trim();
            string post = TBoxWorkPost.Text.Trim();
            string status = TBoxWorkStatus.Text.Trim();

            try
            {
                // Подключение к бд
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        MySqlCommand cmdUser = new MySqlCommand(@"INSERT INTO Workers(Departament, Post, Status) VALUES(@dep, @p, @s); SELECT LAST_INSERT_ID();", conn, tx);

                        // Передаём параметры в запрос
                        cmdUser.Parameters.AddWithValue("@dep", department);
                        cmdUser.Parameters.AddWithValue("@p", post);
                        cmdUser.Parameters.AddWithValue("@s", status);

                        // Выполняем запрос
                        await cmdUser.ExecuteScalarAsync();

                        // Подтверждение транзакции
                        tx.Commit();
                    }
                }

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(post) || string.IsNullOrWhiteSpace(status))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                MessageBox.Show("Сотрудник добавлен");
                Close();
            }

            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }
    }
}