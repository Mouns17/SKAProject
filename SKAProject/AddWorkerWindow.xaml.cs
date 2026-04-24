using MySqlConnector;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SKAProject
{
    public partial class AddWorkerWindow : Window
    {
        private int? foundUserId = null;

        public AddWorkerWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnClose(object sender, RoutedEventArgs e) => Close();

        // Шаг 1: поиск пользователя по ФИО и телефону
        private void BtnNext(object sender, RoutedEventArgs e)
        {
            string lastName = TBoxWorkLastName.Text.Trim();
            string firstName = TBoxWorkFirstName.Text.Trim();
            string middleName = TBoxWorkMiddleName.Text.Trim();
            string phone = TBoxWorkPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Фамилия и имя обязательны.");
                return;
            }

            // Поиск в базе
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT UserID FROM Users
                        WHERE LastName = @ln AND FirstName = @fn
                          AND (MiddleName = @mn OR (@mn = '' AND MiddleName IS NULL))
                        LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ln", lastName);
                        cmd.Parameters.AddWithValue("@fn", firstName);
                        cmd.Parameters.AddWithValue("@mn", middleName);
                        var res = cmd.ExecuteScalar();
                        if (res == null)
                        {
                            MessageBox.Show("Пользователь с таким ФИО не найден.");
                            return;
                        }
                        foundUserId = Convert.ToInt32(res);
                    }
                }

                // Переключение на второй шаг
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
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        // Шаг 2: добавление сотрудника
        private async void BtnAdd(object sender, RoutedEventArgs e)
        {
            if (foundUserId == null)
            {
                MessageBox.Show("Сначала выполните поиск пользователя.");
                return;
            }

            string email = TBoxWorkEmail.Text.Trim();
            string depName = TBoxWorkDep.Text.Trim();
            string posName = TBoxWorkPost.Text.Trim();
            string status = TBoxWorkStatus.Text.Trim();

            if (string.IsNullOrWhiteSpace(posName))
            {
                MessageBox.Show("Должность обязательна.");
                return;
            }
            if (string.IsNullOrWhiteSpace(status))
                status = "Работает"; // значение по умолчанию

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        // 1. Получить или создать отдел
                        int? depId = null;
                        if (!string.IsNullOrWhiteSpace(depName))
                        {
                            // Ищем отдел
                            string depQuery = "SELECT DepID FROM departments WHERE DepName = @name";
                            using (var cmd = new MySqlCommand(depQuery, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@name", depName);
                                var res = cmd.ExecuteScalar();
                                if (res != null)
                                {
                                    depId = Convert.ToInt32(res);
                                }
                                else
                                {
                                    // Создаём отдел
                                    string insDep = @"INSERT INTO departments (DepName) VALUES (@name);
                                                      SELECT LAST_INSERT_ID();";
                                    using (var cmdIns = new MySqlCommand(insDep, conn, tx))
                                    {
                                        cmdIns.Parameters.AddWithValue("@name", depName);
                                        depId = Convert.ToInt32(cmdIns.ExecuteScalar());
                                    }
                                }
                            }
                        }

                        // 2. Получить или создать должность
                        int? posId = null;
                        if (!string.IsNullOrWhiteSpace(posName))
                        {
                            string posQuery = "SELECT PosID FROM positions WHERE PosName = @name";
                            using (var cmd = new MySqlCommand(posQuery, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@name", posName);
                                var res = cmd.ExecuteScalar();
                                if (res != null)
                                {
                                    posId = Convert.ToInt32(res);
                                }
                                else
                                {
                                    string insPos = @"INSERT INTO positions (PosName) VALUES (@name);
                                                      SELECT LAST_INSERT_ID();";
                                    using (var cmdIns = new MySqlCommand(insPos, conn, tx))
                                    {
                                        cmdIns.Parameters.AddWithValue("@name", posName);
                                        posId = Convert.ToInt32(cmdIns.ExecuteScalar());
                                    }
                                }
                            }
                        }

                        // 3. Вставить запись в workers
                        string workerQuery = @"
                            INSERT INTO workers (UserID, DepID, PosID, Status)
                            VALUES (@uid, @did, @pid, @st)";
                        using (var cmd = new MySqlCommand(workerQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@uid", foundUserId.Value);
                            cmd.Parameters.AddWithValue("@did", depId);
                            cmd.Parameters.AddWithValue("@pid", posId);
                            cmd.Parameters.AddWithValue("@st", status);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 4. Обновить email пользователя, если указан
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            string upd = "UPDATE users SET Email = @email WHERE UserID = @uid";
                            using (var cmd = new MySqlCommand(upd, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@email", email);
                                cmd.Parameters.AddWithValue("@uid", foundUserId.Value);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                    }
                }

                MessageBox.Show("Сотрудник успешно добавлен.");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления сотрудника: " + ex.Message);
            }
        }
    }
}