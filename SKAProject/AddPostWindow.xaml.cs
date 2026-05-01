using MySqlConnector;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class AddPostWindow : Window
    {
        public AddPostWindow() => InitializeComponent();
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnClose(object sender, RoutedEventArgs e) => Close();

        private async void BtnAdd(object sender, RoutedEventArgs e)
        {
            string post = TBoxPostName.Text.Trim();
            if (string.IsNullOrWhiteSpace(post))
            {
                MessageBox.Show("Введите название должности.");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        // Проверить, есть ли уже такая должность
                        string check = "SELECT COUNT(*) FROM positions WHERE PosName = @name";
                        using (var cmd = new MySqlCommand(check, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", post);
                            long count = (long)await cmd.ExecuteScalarAsync();
                            if (count > 0)
                            {
                                MessageBox.Show("Такая должность уже существует.");
                                tx.Rollback();
                                return;
                            }
                        }

                        string ins = "INSERT INTO positions (PosName) VALUES (@name)";
                        using (var cmd = new MySqlCommand(ins, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", post);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                    }
                }

                MessageBox.Show("Должность добавлена.");
                await Logger.LogAsync(Session.UserID, "Создание должности", "Управление организацией", $"Должность: {post}");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}