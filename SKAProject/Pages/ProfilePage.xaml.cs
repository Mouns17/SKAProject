using MySqlConnector;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SKAProject.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();

            // Часы
            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            timer.Tick += (o, t) => { TBlockTime.Text = DateTime.Now.ToString("HH:mm"); };
            timer.Start();

            LoadProfile();
        }

        private async void LoadProfile()
        {
            try
            {
                int userId = Session.UserID;
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT u.FirstName, u.LastName, u.MiddleName,
                               u.Email, u.Phone, u.Role,
                               d.DepName, p.PosName
                        FROM Users u
                        LEFT JOIN Workers w ON u.UserID = w.UserID
                        LEFT JOIN departments d ON w.DepID = d.DepID
                        LEFT JOIN positions p ON w.PosID = p.PosID
                        WHERE u.UserID = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string firstName = reader["FirstName"]?.ToString() ?? "";
                                string lastName = reader["LastName"]?.ToString() ?? "";
                                string middleName = reader["MiddleName"]?.ToString() ?? "";
                                string email = reader["Email"]?.ToString() ?? "";
                                string phone = reader["Phone"]?.ToString() ?? "";
                                string role = reader["Role"]?.ToString() ?? "";
                                string dep = reader["DepName"]?.ToString() ?? "Не указан";
                                string post = reader["PosName"]?.ToString() ?? "Не указана";

                                // Отображение
                                TBlockNames.Text = $"ФИО: {lastName} {firstName} {middleName}";
                                TBlockRole.Text = $"Роль: {role}";
                                TBlockEmail.Text = $"Email: {email}";
                                TBlockPhone.Text = $"Телефон: {phone}";
                                TBlockDep.Text = $"Отдел: {dep}";
                                TBlockPost.Text = $"Должность: {post}";

                                // Сохраняем текущие значения для редактирования
                                TBoxFirstName.Text = firstName;
                                TBoxLastName.Text = lastName;
                                TBoxMiddleName.Text = middleName;
                                TBoxEmail.Text = email;
                                TBoxPhone.Text = phone;
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки профиля: " + ex.Message);
            }
        }

        // Вход в режим редактирования
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем текстовые блоки
            TBlockNames.Visibility = Visibility.Collapsed;
            TBlockEmail.Visibility = Visibility.Collapsed;
            TBlockPhone.Visibility = Visibility.Collapsed;

            // Показываем TextBox'ы
            TBoxFirstName.Visibility = Visibility.Visible;
            TBoxLastName.Visibility = Visibility.Visible;
            TBoxMiddleName.Visibility = Visibility.Visible;
            TBoxEmail.Visibility = Visibility.Visible;
            TBoxPhone.Visibility = Visibility.Visible;

            // Меняем кнопки
            BtnEdit.Visibility = Visibility.Collapsed;
            BtnSave.Visibility = Visibility.Visible;
            BtnCancel.Visibility = Visibility.Visible;
        }

        // Отмена редактирования (возврат к просмотру)
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ToggleViewMode();
        }

        // Сохранение изменений
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBoxFirstName.Text) || string.IsNullOrWhiteSpace(TBoxLastName.Text))
            {
                MessageBox.Show("Имя и фамилия обязательны.");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE Users 
                        SET FirstName = @fn, LastName = @ln, MiddleName = @mn,
                            Email = @email, Phone = @phone
                        WHERE UserID = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fn", TBoxFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@ln", TBoxLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@mn", TBoxMiddleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", TBoxEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", TBoxPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", Session.UserID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await Logger.LogAsync(Session.UserID, "Редактирование профиля", "Пользователи", "Обновлены личные данные");

                // Обновляем отображение
                TBlockNames.Text = $"ФИО: {TBoxLastName.Text} {TBoxFirstName.Text} {TBoxMiddleName.Text}";
                TBlockEmail.Text = $"Email: {TBoxEmail.Text}";
                TBlockPhone.Text = $"Телефон: {TBoxPhone.Text}";

                MessageBox.Show("Данные сохранены.");
                ToggleViewMode(); // возврат в режим просмотра
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void ToggleViewMode()
        {
            TBlockNames.Visibility = Visibility.Visible;
            TBlockEmail.Visibility = Visibility.Visible;
            TBlockPhone.Visibility = Visibility.Visible;

            TBoxFirstName.Visibility = Visibility.Collapsed;
            TBoxLastName.Visibility = Visibility.Collapsed;
            TBoxMiddleName.Visibility = Visibility.Collapsed;
            TBoxEmail.Visibility = Visibility.Collapsed;
            TBoxPhone.Visibility = Visibility.Collapsed;

            BtnEdit.Visibility = Visibility.Visible;
            BtnSave.Visibility = Visibility.Collapsed;
            BtnCancel.Visibility = Visibility.Collapsed;
        }
    }
}