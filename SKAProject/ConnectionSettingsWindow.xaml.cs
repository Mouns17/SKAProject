using System;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class ConnectionSettingsWindow : Window
    {
        public bool SettingsSaved { get; private set; } = false;

        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            var cfg = ConnectionSettings.Load();
            TxtServer.Text = cfg.Server;
            TxtPort.Text = cfg.Port.ToString();
            TxtDatabase.Text = cfg.Database;
            TxtUser.Text = cfg.User;
            PwdPassword.Password = cfg.Password;
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtServer.Text) ||
                string.IsNullOrWhiteSpace(TxtPort.Text) ||
                string.IsNullOrWhiteSpace(TxtDatabase.Text) ||
                string.IsNullOrWhiteSpace(TxtUser.Text))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtPort.Text, out int port) || port <= 0)
            {
                MessageBox.Show("Неверный порт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var settings = new ConnectionSettings.DBSettings
            {
                Server = TxtServer.Text.Trim(),
                Port = port,
                Database = TxtDatabase.Text.Trim(),
                User = TxtUser.Text.Trim(),
                Password = PwdPassword.Password
            };
            ConnectionSettings.Save(settings);
            DataBase.ResetConnectionString();

            MessageBox.Show("Настройки сохранены.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            SettingsSaved = true;
            this.Close();
            LoginWindow log = new LoginWindow();
            log.Show();
        }
    }
}