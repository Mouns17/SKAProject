using MySqlConnector;
using System;
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
                               d.DepName, p.PosName, w.Status
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
                                TBlockNames.Text = $"ФИО: {reader["LastName"]} {reader["FirstName"]} {reader["MiddleName"]}";
                                TBlockRole.Text = $"Роль: {reader["Role"]}";
                                TBlockEmail.Text = $"Email: {reader["Email"]}";
                                TBlockPhone.Text = $"Телефон: {reader["Phone"]}";
                                TBlockDep.Text = $"Отдел: {GetOrDefault(reader["DepName"])}";
                                TBlockPost.Text = $"Должность: {GetOrDefault(reader["PosName"])}";
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
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private string GetOrDefault(object val) =>
            val == null || val == DBNull.Value ? "Не указано" : val.ToString();
    }
}