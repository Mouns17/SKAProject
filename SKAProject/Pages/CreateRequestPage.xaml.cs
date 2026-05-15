using Microsoft.Win32;
using MySqlConnector;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SKAProject.Pages
{
    public partial class CreateRequestPage : Page
    {
        private string _attachedFilePath;

        public CreateRequestPage()
        {
            InitializeComponent();
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

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Получаем тип заявки
            string requestType = (CmbRequestType.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrWhiteSpace(requestType))
            {
                MessageBox.Show("Выберите тип заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Причина
            string reason = TxtReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Введите причину.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохраняем вложенный файл (если есть)
            string savedFilePath = null;
            if (!string.IsNullOrEmpty(_attachedFilePath))
            {
                try
                {
                    string requestsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requests");
                    Directory.CreateDirectory(requestsDir);
                    string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(_attachedFilePath);
                    string destPath = Path.Combine(requestsDir, uniqueName);
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
                    // Вставляем заявку в таблицу employeerequests
                    string insert = @"INSERT INTO employeerequests (RequestDate, RequestType, Reason, Status, EmployeeUserID)
                                     VALUES (NOW(), @type, @reason, 'На рассмотрении', @uid)";
                    using (var cmd = new MySqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@type", requestType);
                        cmd.Parameters.AddWithValue("@reason", reason);
                        cmd.Parameters.AddWithValue("@uid", Session.UserID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // Логируем
                    await Logger.LogAsync(Session.UserID, "Создание заявки", "Заявки",
                        $"Создана заявка на {requestType}");
                }

                MessageBox.Show("Заявка успешно отправлена на рассмотрение.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                // Очищаем форму
                TxtReason.Text = "";
                TxtAttachedFile.Text = "";
                _attachedFilePath = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке заявки: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}