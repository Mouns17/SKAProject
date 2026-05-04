using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SKAProject
{
    public partial class CreateTaskWindow : Window
    {
        private readonly List<int> _userIds;  // теперь храним UserId

        public CreateTaskWindow(List<int> userIds)
        {
            InitializeComponent();
            _userIds = userIds;
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (_userIds == null || _userIds.Count == 0)
            {
                MessageBox.Show("Не выбраны сотрудники.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string title = TBoxTitle.Text.Trim();
            string description = TBoxDescription.Text.Trim();
            string priority = ((ComboBoxItem)CmbPriority.SelectedItem).Content.ToString();
            DateTime? deadline = DpDeadline.SelectedDate;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        foreach (int userId in _userIds)  // используем userId
                        {
                            string insert = @"
                            INSERT INTO tasks (Title, Description, CreatedBy, AssignedTo, Status, Priority, Deadline, CreatedAt)
                            VALUES (@title, @desc, @createdBy, @assignedTo, 'Новая', @priority, @deadline, NOW())";
                            using (var cmd = new MySqlCommand(insert, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@title", title);
                                cmd.Parameters.AddWithValue("@desc", description ?? "");
                                cmd.Parameters.AddWithValue("@createdBy", Session.UserID);
                                cmd.Parameters.AddWithValue("@assignedTo", userId);
                                cmd.Parameters.AddWithValue("@priority", priority);
                                cmd.Parameters.AddWithValue("@deadline", (object)deadline ?? DBNull.Value);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            await Logger.LogAsync(Session.UserID, "Создание задачи", "Задачи",
                                $"Создана задача для пользователя (UserID={userId}): {title}");
                        }
                        tx.Commit();
                    }
                }

                MessageBox.Show($"Создано задач: {_userIds.Count}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}