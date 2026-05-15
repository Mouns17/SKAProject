using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SKAProject.Pages
{
    public partial class ProfilePage : Page
    {
        // Храним исходные значения для возможности отката и для полей, не участвующих в редактировании
        private string _originalFirstName, _originalLastName, _originalMiddleName,
                       _originalEmail, _originalPhone;

        public ProfilePage()
        {
            InitializeComponent();
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
                        SELECT u.FirstName, u.LastName, u.MiddleName, u.Email, u.Phone,
                               u.Role, u.CreatedAt,
                               w.WrkID, w.Status, d.DepName, p.PosName
                        FROM users u
                        LEFT JOIN workers w ON u.UserID = w.UserID
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
                                // Простые строки
                                _originalFirstName = reader["FirstName"]?.ToString() ?? "";
                                _originalLastName = reader["LastName"]?.ToString() ?? "";
                                _originalMiddleName = reader["MiddleName"]?.ToString() ?? "";
                                _originalEmail = reader["Email"]?.ToString() ?? "";
                                _originalPhone = reader["Phone"]?.ToString() ?? "";
                                string role = reader["Role"]?.ToString() ?? "User";


                                // Отдел и должность
                                string dep = GetNullableString(reader, "DepName");
                                if (string.IsNullOrWhiteSpace(dep)) dep = "Не указан";
                                string pos = GetNullableString(reader, "PosName");
                                if (string.IsNullOrWhiteSpace(pos)) pos = "Не указана";

                                // Статус
                                string status = GetNullableString(reader, "Status");
                                if (string.IsNullOrWhiteSpace(status)) status = "Работает";

                                // Табельный номер (WrkID)
                                int? workerId = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("WrkID")))
                                    workerId = reader.GetInt32(reader.GetOrdinal("WrkID"));
                                string employeeId = workerId.HasValue ? workerId.ToString() : "—";

                                // Заполняем текстовые блоки
                                TBlockNames.Text = $"ФИО: {_originalLastName} {_originalFirstName} {_originalMiddleName}";
                                TBlockRole.Text = $"Роль: {role}";
                                TBlockEmployeeId.Text = $"Таб. №: {employeeId}";
                                TxtStatus.Text = status;
                                TBlockPhone.Text = $"Телефон: {_originalPhone}";
                                TBlockEmail.Text = $"Email: {_originalEmail}";
                                TBlockDep.Text = $"Отдел: {dep}";
                                TBlockPost.Text = $"Должность: {pos}";

                                // Цвет статуса
                                SolidColorBrush statusColor;
                                switch (status)
                                {
                                    case "Работает":
                                        statusColor = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                                        break;
                                    case "В отпуске":
                                        statusColor = new SolidColorBrush(Color.FromRgb(245, 158, 11));
                                        break;
                                    case "Уволен":
                                        statusColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                                        break;
                                    default:
                                        statusColor = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                                        break;
                                }
                                StatusBadge.Background = statusColor;

                                // Заполняем скрытые поля ввода актуальными значениями
                                TBoxLastName.Text = _originalLastName;
                                TBoxFirstName.Text = _originalFirstName;
                                TBoxMiddleName.Text = _originalMiddleName;
                                TBoxPhone.Text = _originalPhone;
                                TBoxEmail.Text = _originalEmail;
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

        /// <summary>Безопасно читает строковое поле из data reader.</summary>
        private static string GetNullableString(MySqlDataReader reader, string columnName)
        {
            int idx = reader.GetOrdinal(columnName);
            return reader.IsDBNull(idx) ? "" : reader.GetString(idx);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Показать поля ввода, скрыть текстовые блоки
            TBlockNames.Visibility = Visibility.Collapsed;
            TBlockPhone.Visibility = Visibility.Collapsed;
            TBlockEmail.Visibility = Visibility.Collapsed;
            TBlockSnils.Visibility = Visibility.Collapsed;
            TBlockAddress.Visibility = Visibility.Collapsed;

            PanelEditFullName.Visibility = Visibility.Visible;
            TBoxPhone.Visibility = Visibility.Visible;
            TBoxEmail.Visibility = Visibility.Visible;

            BtnEdit.Visibility = Visibility.Collapsed;
            BtnSave.Visibility = Visibility.Visible;
            BtnCancel.Visibility = Visibility.Visible;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Вернуть исходные значения в поля ввода и скрыть их
            TBoxLastName.Text = _originalLastName;
            TBoxFirstName.Text = _originalFirstName;
            TBoxMiddleName.Text = _originalMiddleName;
            TBoxPhone.Text = _originalPhone;
            TBoxEmail.Text = _originalEmail;

            ToggleViewMode();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fn = TBoxFirstName.Text.Trim();
            string ln = TBoxLastName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            {
                MessageBox.Show("Имя и фамилия обязательны.");
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string upd = @"UPDATE users SET 
                                    FirstName = @fn, LastName = @ln, MiddleName = @mn,
                                    Email = @email, Phone = @phone
                                   WHERE UserID = @id";
                    using (var cmd = new MySqlCommand(upd, conn))
                    {
                        cmd.Parameters.AddWithValue("@fn", fn);
                        cmd.Parameters.AddWithValue("@ln", ln);
                        cmd.Parameters.AddWithValue("@mn", TBoxMiddleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", TBoxEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", TBoxPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", Session.UserID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await Logger.LogAsync(Session.UserID, "Редактирование профиля", "Пользователи",
                    "Обновлены личные данные");

                // Обновить отображаемые тексты и исходные переменные
                _originalFirstName = fn;
                _originalLastName = ln;
                _originalMiddleName = TBoxMiddleName.Text.Trim();
                _originalEmail = TBoxEmail.Text.Trim();
                _originalPhone = TBoxPhone.Text.Trim();

                TBlockNames.Text = $"ФИО: {ln} {fn} {_originalMiddleName}";
                TBlockPhone.Text = $"Телефон: {_originalPhone}";
                TBlockEmail.Text = $"Email: {_originalEmail}";

                MessageBox.Show("Данные сохранены.");
                ToggleViewMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void ToggleViewMode()
        {
            // Скрыть поля ввода, показать текстовые блоки
            TBlockNames.Visibility = Visibility.Visible;
            TBlockPhone.Visibility = Visibility.Visible;
            TBlockEmail.Visibility = Visibility.Visible;
            TBlockSnils.Visibility = Visibility.Visible;
            TBlockAddress.Visibility = Visibility.Visible;

            PanelEditFullName.Visibility = Visibility.Collapsed;
            TBoxPhone.Visibility = Visibility.Collapsed;
            TBoxEmail.Visibility = Visibility.Collapsed;

            BtnEdit.Visibility = Visibility.Visible;
            BtnSave.Visibility = Visibility.Collapsed;
            BtnCancel.Visibility = Visibility.Collapsed;
        }
    }
}