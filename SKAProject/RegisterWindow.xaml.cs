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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void OpenLoggin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow skebob = new LoginWindow();
            skebob.Show();
            this.Close();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // Получем данные и удаляем пробелы в них
            string login = LoginBox.Text.Trim();
            string password = PassBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();

            RegisterNextWindow next = new RegisterNextWindow(login, password, email, phone);
            next.Show();
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

    }
}
