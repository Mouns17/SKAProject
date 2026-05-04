using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MySqlConnector;

namespace SKAProject
{
    public partial class AddWorkerWindow : Window
    {
        private int? foundUserId = null;

        public AddWorkerWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            DragMove();

        private void BtnClose(object sender, RoutedEventArgs e) => Close();

        private async void BtnAdd(object sender, RoutedEventArgs e)
        {
            // 1. Получаем и проверяем поля
            string fullName = TBoxWorkLFM.Text.Trim();
            string depName = TBoxWorkDep.Text.Trim();
            string posName = TBoxWorkPost.Text.Trim();

            if (
                string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(depName)
                || string.IsNullOrWhiteSpace(posName)
            )
            {
                MessageBox.Show(
                    "Заполните все поля.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Разбиваем ФИО на части (ожидается минимум 2 слова)
            string[] parts = Regex.Split(fullName, @"\s+");
            if (parts.Length < 2)
            {
                MessageBox.Show(
                    "Введите фамилию и имя через пробел.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            string lastName = parts[0];
            string firstName = parts[1];
            string middleName = parts.Length > 2 ? parts[2] : null;

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        // 2. Ищем пользователя
                        string userQuery =
                            middleName == null
                                ? "SELECT UserID FROM users WHERE LastName = @ln AND FirstName = @fn AND (MiddleName IS NULL OR MiddleName = '') LIMIT 1"
                                : "SELECT UserID FROM users WHERE LastName = @ln AND FirstName = @fn AND MiddleName = @mn LIMIT 1";

                        int? userId = null;
                        using (var cmd = new MySqlCommand(userQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ln", lastName);
                            cmd.Parameters.AddWithValue("@fn", firstName);
                            if (middleName != null)
                                cmd.Parameters.AddWithValue("@mn", middleName);

                            var res = await cmd.ExecuteScalarAsync();
                            if (res == null)
                            {
                                tx.Rollback();
                                MessageBox.Show(
                                    "Пользователь с таким ФИО не найден.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error
                                );
                                return;
                            }
                            userId = Convert.ToInt32(res);
                        }

                        // 3. Проверяем, не является ли уже сотрудником
                        string checkWorker = "SELECT COUNT(*) FROM workers WHERE UserID = @uid";
                        using (var cmd = new MySqlCommand(checkWorker, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId.Value);
                            long count = (long)await cmd.ExecuteScalarAsync();
                            if (count > 0)
                            {
                                tx.Rollback();
                                MessageBox.Show(
                                    "Этот пользователь уже добавлен как сотрудник.",
                                    "Информация",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                                return;
                            }
                        }

                        // 4. Получаем или создаём отдел
                        int? depId = null;
                        string depQuery = "SELECT DepID FROM departments WHERE DepName = @name";
                        using (var cmd = new MySqlCommand(depQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", depName);
                            var res = await cmd.ExecuteScalarAsync();
                            if (res != null)
                            {
                                depId = Convert.ToInt32(res);
                            }
                            else
                            {
                                string insDep =
                                    "INSERT INTO departments (DepName) VALUES (@name); SELECT LAST_INSERT_ID();";
                                using (var cmdIns = new MySqlCommand(insDep, conn, tx))
                                {
                                    cmdIns.Parameters.AddWithValue("@name", depName);
                                    depId = Convert.ToInt32(await cmdIns.ExecuteScalarAsync());
                                }
                            }
                        }

                        // 5. Получаем или создаём должность
                        int? posId = null;
                        string posQuery = "SELECT PosID FROM positions WHERE PosName = @name";
                        using (var cmd = new MySqlCommand(posQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@name", posName);
                            var res = await cmd.ExecuteScalarAsync();
                            if (res != null)
                            {
                                posId = Convert.ToInt32(res);
                            }
                            else
                            {
                                string insPos =
                                    "INSERT INTO positions (PosName) VALUES (@name); SELECT LAST_INSERT_ID();";
                                using (var cmdIns = new MySqlCommand(insPos, conn, tx))
                                {
                                    cmdIns.Parameters.AddWithValue("@name", posName);
                                    posId = Convert.ToInt32(await cmdIns.ExecuteScalarAsync());
                                }
                            }
                        }

                        // 6. Добавляем запись в workers
                        string insertWorker =
                            @"
                            INSERT INTO workers (UserID, DepID, PosID, Status)
                            VALUES (@uid, @did, @pid, 'Работает')";
                        using (var cmd = new MySqlCommand(insertWorker, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId.Value);
                            cmd.Parameters.AddWithValue(
                                "@did",
                                depId.HasValue ? (object)depId.Value : DBNull.Value
                            );
                            cmd.Parameters.AddWithValue(
                                "@pid",
                                posId.HasValue ? (object)posId.Value : DBNull.Value
                            );
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                    }
                }

                // Логирование
                await Logger.LogAsync(
                    Session.UserID,
                    "Добавление сотрудника",
                    "Управление персоналом",
                    $"Добавлен сотрудник: {fullName}, отдел: {depName}, должность: {posName}"
                );

                MessageBox.Show(
                    "Сотрудник успешно добавлен.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка добавления сотрудника: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
