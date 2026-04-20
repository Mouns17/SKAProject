using MySqlConnector;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SKAProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для MyOrganizationPage.xaml
    /// </summary>
    public partial class MyOrganizationPage : Page
    {
        Models.OrganizationModel organization = new Models.OrganizationModel();
        public MyOrganizationPage()
        {
            InitializeComponent();
        }

        private void BtnAddDepartment(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAddPost(object sender, RoutedEventArgs e)
        {

        }

        private async void LoadOrganization()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"SELECT u.Login, u.FirstName, u.LastName, u.MiddleName, u.Email, u.Phone, u.Role, w.Post, w.Status, d.DepName, o.OrgName FROM Users u LEFT JOIN Workers w  ON u.UserID = w.UserID LEFT JOIN Departments d ON w.DepID = d.DepID LEFT JOIN Organizations o ON w.OrgID = o.OrgID WHERE u.UserID = @id";

                    var cmd = new MySqlCommand(query, conn);

                    var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        TBlockOrgName.Text = " " + reader["OrgName"];
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
