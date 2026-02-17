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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LogginWindow : Window
    {
        public LogginWindow()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PassBox.Text.Trim();

            if (login == "" || password == "")
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var conn = Db.GetConnection())
                {
                    await conn.OpenAsync();

                    var cmd = new MySqlCommand(@"
                SELECT UserID 
                FROM Users
                WHERE Login=@l AND Password=@p", conn);

                    cmd.Parameters.AddWithValue("@l", login);
                    cmd.Parameters.AddWithValue("@p", password);

                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        // успех
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow tungtungtungsahur = new RegisterWindow();
            tungtungtungsahur.Show();
            this.Close();
        }

        private void OpenOtherAutorization_Click(object sender, RoutedEventArgs e)
        {
            PhoneLogginWindow brbrpatapim = new PhoneLogginWindow();
            brbrpatapim.Show();
            this.Close();
        }
    }
}
