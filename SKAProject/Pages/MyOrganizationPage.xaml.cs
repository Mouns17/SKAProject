using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class MyOrganizationPage : Page
    {
        public MyOrganizationPage()
        {
            InitializeComponent();
            LoadOrganization();
        }

        private async void LoadOrganization()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    // Данные компании
                    string query = "SELECT Address, Phone, Email FROM company_info WHERE CompanyID = 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            TBlockOrgAddress.Text = "Адрес: " + (reader["Address"]?.ToString() ?? "");
                            TBlockOrgPhone.Text = "Телефон: " + (reader["Phone"]?.ToString() ?? "");
                            TBlockOrgEmail.Text = "Email: " + (reader["Email"]?.ToString() ?? "");
                        }
                    }

                    // Количество сотрудников (работающих)
                    string cntQuery = "SELECT COUNT(*) FROM workers WHERE Status = 'Paботает'";
                    using (var cmd = new MySqlCommand(cntQuery, conn))
                    {
                        long count = (long)await cmd.ExecuteScalarAsync();
                        TBlockOrgCountWorkers.Text = $"{count}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных организации: " + ex.Message);
            }
        }

        private void BtnAddDepartment(object sender, RoutedEventArgs e) => new AddDepartmentWindow().Show();
        private void BtnAddPost(object sender, RoutedEventArgs e) => new AddPostWindow().Show();
    }
}