using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MySqlConnector;

namespace SKAProject
{
    public partial class AddWorkerWindow : Window
    {
        public AddWorkerWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnNext(object sender, RoutedEventArgs e)
        {
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

        private void BtnAdd(object sender, RoutedEventArgs e)
        {
            string email = TBoxWorkEmail.Text.Trim();
            string department = TBoxWorkDep.Text.Trim();
            string post = TBoxWorkPost.Text.Trim();
            string status = TBoxWorkStatus.Text.Trim();

            try
            {
                using (MySqlConnection conn = DataBase.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(@"INSERT INTO Workers(Email, Department, Post, Status) VALUES(@em, @dep, @post, @st)", conn);
                    cmd.Parameters.AddWithValue("@em", email);
                    cmd.Parameters.AddWithValue("@dep", department);
                    cmd.Parameters.AddWithValue("@post", post);
                    cmd.Parameters.AddWithValue("@st", status);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Работник успешно добавлен!");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении работника. Попробуйте снова.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
