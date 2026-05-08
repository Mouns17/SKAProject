using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class StaffSchedulePage : Page
    {
        private ObservableCollection<StaffRow> _staff = new ObservableCollection<StaffRow>();

        public StaffSchedulePage()
        {
            InitializeComponent();
            LoadStaff();
        }

        public class StaffRow : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int RowNumber { get; set; }
            public string PositionName { get; set; }
            public string DepartmentName { get; set; }
            public int Rate { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private async void LoadStaff()
        {
            try
            {
                _staff.Clear();
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT ss.Id, ss.PositionName, ss.DepartmentName, ss.Rate
                                     FROM staffschedule ss
                                     ORDER BY ss.DepartmentName, ss.PositionName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int row = 1;
                        while (await reader.ReadAsync())
                        {
                            _staff.Add(new StaffRow
                            {
                                Id = reader.GetInt32("Id"),
                                RowNumber = row++,
                                PositionName = reader.GetString("PositionName"),
                                DepartmentName = reader.GetString("DepartmentName"),
                                Rate = reader.GetInt32("Rate")
                            });
                        }
                    }
                }
                StaffItemsControl.ItemsSource = _staff;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки штатного расписания: " + ex.Message);
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var posDialog = new InputDialog("Введите должность:");
            if (posDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(posDialog.Result)) return;

            var depDialog = new InputDialog("Введите отдел:");
            if (depDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(depDialog.Result)) return;

            var rateDialog = new InputDialog("Количество ставок:", "1");
            if (rateDialog.ShowDialog() != true || !int.TryParse(rateDialog.Result, out int rate) || rate <= 0) return;

            string pos = posDialog.Result.Trim();
            string dep = depDialog.Result.Trim();

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    string insert = @"INSERT INTO staffschedule (PositionName, DepartmentName, Rate) VALUES (@pos, @dep, @rate)";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@pos", pos);
                        cmd.Parameters.AddWithValue("@dep", dep);
                        cmd.Parameters.AddWithValue("@rate", rate);
                        cmd.ExecuteNonQuery();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Добавление в штатное расписание", "Кадры",
                    $"Добавлена должность {pos} ({dep})");
                LoadStaff();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }

        private async void DeleteStaffRow_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || button.CommandParameter == null) return;
            int id = Convert.ToInt32(button.CommandParameter);
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string del = "DELETE FROM staffschedule WHERE Id = @id";
                    using (var cmd = new MySqlCommand(del, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await Logger.LogAsync(Session.UserID, "Удаление из штатного расписания", "Кадры", $"Удалена запись ID={id}");
                LoadStaff();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
}