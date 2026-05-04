using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MySqlConnector;

namespace SKAProject
{
    public partial class SendReportWindow : Window
    {
        private string _attachedFilePath = null;
        private readonly int _authorId;

        public SendReportWindow(int authorId)
        {
            InitializeComponent();
            _authorId = authorId;
            LoadRecipients();
        }

        private void LoadRecipients()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    conn.Open();
                    string query =
                        @"
                        SELECT u.UserID, CONCAT(u.LastName, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS FullName
                        FROM workers w
                        JOIN users u ON w.UserID = u.UserID
                        WHERE u.UserID != @me
                        ORDER BY u.LastName, u.FirstName";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@me", _authorId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var recipients = new ObservableCollection<Recipient>();
                            while (reader.Read())
                            {
                                recipients.Add(
                                    new Recipient
                                    {
                                        UserID = reader.GetInt32("UserID"),
                                        FullName = reader.GetString("FullName"),
                                    }
                                );
                            }
                            CmbRecipient.ItemsSource = recipients;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки получателей: " + ex.Message);
            }
        }

        private void BtnAttach_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter =
                "Все файлы (*.*)|*.*|Документы Word (*.docx)|*.docx|Таблицы Excel (*.xlsx)|*.xlsx";
            if (dlg.ShowDialog() == true)
            {
                _attachedFilePath = dlg.FileName;
                TxtAttachedFile.Text = System.IO.Path.GetFileName(_attachedFilePath);
            }
        }

        private void BtnClose(object sender, RoutedEventArgs e) => Close();

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            DragMove();

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (CmbRecipient.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите получателя.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            int recipientId = (int)CmbRecipient.SelectedValue;
            DateTime? reportDate = DpReportDate.SelectedDate;
            string description = TBoxDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(description) && _attachedFilePath == null)
            {
                MessageBox.Show(
                    "Введите описание или прикрепите файл.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            string savedFilePath = null;
            if (!string.IsNullOrEmpty(_attachedFilePath))
            {
                try
                {
                    string reportsDir = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Reports"
                    );
                    if (!Directory.Exists(reportsDir))
                        Directory.CreateDirectory(reportsDir);

                    string uniqueName =
                        Guid.NewGuid().ToString() + System.IO.Path.GetExtension(_attachedFilePath);
                    string destPath = System.IO.Path.Combine(reportsDir, uniqueName);
                    File.Copy(_attachedFilePath, destPath, true);
                    savedFilePath = destPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла: " + ex.Message);
                    return;
                }
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string insert =
                        @"
                        INSERT INTO reports (AuthorID, RecipientID, ReportDate, Description, FilePath, Status, CreatedAt)
                        VALUES (@auth, @rec, @date, @desc, @file, 'На проверке', NOW())";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@auth", _authorId);
                        cmd.Parameters.AddWithValue("@rec", recipientId);
                        cmd.Parameters.AddWithValue("@date", (object)reportDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@file", (object)savedFilePath ?? DBNull.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await Logger.LogAsync(
                        _authorId,
                        "Отправка отчёта",
                        "Отчёты",
                        $"Отчёт отправлен пользователю ID={recipientId}"
                    );
                }

                MessageBox.Show(
                    "Отчёт успешно отправлен!",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при сохранении отчёта: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }

    public class Recipient
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
    }
}
