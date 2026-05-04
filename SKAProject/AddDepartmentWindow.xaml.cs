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

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для AddDepartmentWindow.xaml
    /// </summary>
    public partial class AddDepartmentWindow : Window
    {
        public AddDepartmentWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private async void BtnAdd(object sender, RoutedEventArgs e)
        {
            string department = TBoxDepName.Text.Trim();

            try
            {
                // Подключение к бд
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlTransaction tx = conn.BeginTransaction())
                    {
                        MySqlCommand cmdUser = new MySqlCommand(
                            @"INSERT INTO Departments(DepName) VALUES(@dep); SELECT LAST_INSERT_ID();",
                            conn,
                            tx
                        );

                        // Передаём параметры в запрос
                        cmdUser.Parameters.AddWithValue("@dep", department);

                        // Выполняем запрос
                        await cmdUser.ExecuteScalarAsync();

                        // Подтверждение транзакции
                        tx.Commit();
                    }
                }

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(department))
                {
                    MessageBox.Show("Заполните все поля");
                    return;
                }

                MessageBox.Show("Отдел добавлен");
                await Logger.LogAsync(
                    Session.UserID,
                    "Добавление отдела",
                    "Управление организацией",
                    $"Добавлен отдел: {department}"
                );
                Close();
            }
            // При каких то ошибках связанных с БД
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления отдела: " + ex.Message);
            }
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
