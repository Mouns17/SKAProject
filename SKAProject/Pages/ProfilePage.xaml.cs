using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
using static System.Collections.Specialized.BitVector32;

namespace SKAProject.Pages
{
    public partial class ProfilePage : Page
    {
        Models.ProfileModel profile = new Models.ProfileModel();
        public ProfilePage()
        {
            InitializeComponent();

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { TBlockTime.Text = DateTime.Now.ToString("HH mm"); };
            timer.Start();

            LoadProfile();
        }

        private string GetValueOrEmpty(object value)
        {
            if (value == DBNull.Value || value == null)
                return "Пусто";

            string text = value.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return "Пусто";

            return text;
        }
        private async void LoadProfile()
        {
            try
            {
                int userId = Session.UserID;

                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"SELECT u.Login, u.FirstName, u.LastName, u.MiddleName, u.Email, u.Phone, u.Role, w.Post, w.Status, d.DepName, o.OrgName FROM Users u LEFT JOIN Workers w  ON u.UserID = w.UserID LEFT JOIN Departments d ON w.DepID = d.DepID LEFT JOIN Organizations o ON w.OrgID = o.OrgID WHERE u.UserID = @id";

                    var cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@id", userId);

                    var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        TBlockNames.Text = "ФИО: " + GetValueOrEmpty(reader["LastName"]) + " " + GetValueOrEmpty(reader["FirstName"]) + " " + GetValueOrEmpty(reader["MiddleName"]);
                        TBlockRole.Text = "Роль: " + GetValueOrEmpty(reader["Role"]);
                        TBlockLogin.Text = "Логин: " + GetValueOrEmpty(reader["Login"]);
                        TBlockEmail.Text = "Email: " + GetValueOrEmpty(reader["Email"]);
                        TBlockPhone.Text = "Телефон: " + GetValueOrEmpty(reader["Phone"]);
                        TBlockOrg.Text = "Организация: " + GetValueOrEmpty(reader["OrgName"]);
                        TBlockDep.Text = "Отдел: " + GetValueOrEmpty(reader["DepName"]);
                        TBlockPost.Text = "Должность: " + GetValueOrEmpty(reader["Post"]);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Пользователь не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}