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

        private void OpenNextWin(object sender, RoutedEventArgs e)
        {

        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /*
         private async void Login_Click(object sender, RoutedEventArgs e)
         {
             // Получем данные и удаляем пробелы в них
             string login = LoginBox.Text.Trim();
             string password = PassBox.Text.Trim();

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
                     var cmd = new MySqlCommand(@"SELECT UserID FROM Users WHERE Login=@l AND Password=@p", conn);

                     // Передаем параметры в запрос
                     cmd.Parameters.AddWithValue("@l", login);
                     cmd.Parameters.AddWithValue("@p", password);

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

         private void OpenRegister_Click(object sender, RoutedEventArgs e)
         {
             new RegisterWindow().Show();
             this.Close();
         }

         private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             DragMove();
         }

         private void BtnClose1_Click(object sender, RoutedEventArgs e)
         {
             Close();
         }

         private void BtnClose2_Click(object sender, RoutedEventArgs e)
         {
             this.WindowState = WindowState.Minimized;
         }
        */
    }
}
