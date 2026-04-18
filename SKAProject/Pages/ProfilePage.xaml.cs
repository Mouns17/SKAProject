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
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        Profile profile = new Profile();
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
        public class Profile
        {
            public string Login { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Role { get; set; }
            public string Post { get; set; }
            public string Status { get; set; }
            public string Department { get; set; }
            public string Organization { get; set; }
        }

        // Загрузка профиля
        private async void LoadProfile()
        {
            try
            {
                int userId = Session.UserID;

                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
SELECT 
    u.Login,
    u.FirstName,
    u.LastName,
    u.MiddleName,
    u.Email,
    u.Phone,
    u.Role,

    w.Post,
    w.Status,

    d.DepName,
    o.OrgName

FROM Users u

LEFT JOIN Workers w 
    ON u.UserID = w.UserID

LEFT JOIN Departments d 
    ON w.DepID = d.DepID

LEFT JOIN Organizations o 
    ON w.OrgID = o.OrgID

WHERE u.UserID = @id";

                    var cmd =
                        new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue(
                        "@id",
                        userId
                    );

                    var reader =
                        await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        // ФИО
                        TBlockNames.Text =
                            "ФИО: " +
                            reader["LastName"] + " " +
                            reader["FirstName"] + " " +
                            reader["MiddleName"];

                        // Роль
                        TBlockRole.Text =
                            "Роль: " +
                            reader["Role"];

                        // Логин
                        TBlockLogin.Text =
                            "Логин: " +
                            reader["Login"];

                        // Email
                        TBlockEmail.Text =
                            "Email: " +
                            reader["Email"];

                        // Телефон
                        TBlockPhone.Text =
                            "Телефон: " +
                            reader["Phone"];

                        // Организация
                        TBlockOrg.Text =
                            "Организация: " +
                            reader["OrgName"];

                        // Отдел
                        TBlockDep.Text =
                            "Отдел: " +
                            reader["DepName"];

                        // Должность
                        TBlockPost.Text =
                            "Должность: " +
                            reader["Post"];
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "Пользователь не найден"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message
                );
            }
        }
    }
}
