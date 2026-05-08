using Microsoft.Win32;
using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class SendReportPage : Page
    {
        private string _attachedFilePath;
        private ObservableCollection<Recipient> _recipients = new ObservableCollection<Recipient>();

        public SendReportPage()
        {
            InitializeComponent();
            LoadRecipients();
        }

        public class Recipient
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
        }

        private async void LoadRecipients()
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT u.UserID, CONCAT(u.LastName,' ',u.FirstName,' ',COALESCE(u.MiddleName,'')) AS FullName
                                     FROM users u
                                     JOIN workers w ON u.UserID = w.UserID
                                     WHERE w.Status IN ('Работает','В отпуске')
                                     ORDER BY u.LastName";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _recipients.Add(new Recipient
                            {
                                UserId = reader.GetInt32("UserID"),
                                FullName = reader.GetString("FullName")
                            });
                        }
                    }
                }
                CmbRecipient.ItemsSource = _recipients;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки получателей: " + ex.Message);
            }
        }

        private void BtnAttach_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Все файлы (*.*)|*.*|Документы Word (*.docx)|*.docx|Таблицы Excel (*.xlsx)|*.xlsx";
            if (dlg.ShowDialog() == true)
            {
                _attachedFilePath = dlg.FileName;
                TxtAttachedFile.Text = Path.GetFileName(_attachedFilePath);
            }
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (CmbRecipient.SelectedItem == null)
            {
                MessageBox.Show("Выберите получателя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int recipientId = (int)CmbRecipient.SelectedValue;
            DateTime? reportDate = DpReportDate.SelectedDate;
            string description = TxtDescription.Text.Trim();
            if (string.IsNullOrWhiteSpace(description) && _attachedFilePath == null)
            {
                MessageBox.Show("Введите описание или прикрепите файл.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string savedFilePath = null;
            if (!string.IsNullOrEmpty(_attachedFilePath))
            {
                try
                {
                    string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                    Directory.CreateDirectory(reportsDir);
                    string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(_attachedFilePath);
                    string destPath = Path.Combine(reportsDir, uniqueName);
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
                    string insert = @"INSERT INTO reports (AuthorID, RecipientID, ReportDate, Description, FilePath, Status, CreatedAt)
                                     VALUES (@auth, @rec, @date, @desc, @file, 'На проверке', NOW())";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@auth", Session.UserID);
                        cmd.Parameters.AddWithValue("@rec", recipientId);
                        cmd.Parameters.AddWithValue("@date", (object)reportDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@file", (object)savedFilePath ?? DBNull.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await Logger.LogAsync(Session.UserID, "Отправка отчёта", "Отчёты",
                        $"Отчёт отправлен пользователю ID={recipientId}");
                }
                MessageBox.Show("Отчёт успешно отправлен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}