using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace SKAProject
{
    public partial class ExportExcelWindow : Window
    {
        private readonly List<Pages.WorkersPage.WorkerModel> _workers;

        public ExportExcelWindow(List<Pages.WorkersPage.WorkerModel> workers)
        {
            InitializeComponent();
            _workers = workers;
        }

        private void MainBorder_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            DragMove();
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Сотрудники.xlsx",
            };
            if (sfd.ShowDialog() == true)
            {
                TBoxPath.Text = sfd.FileName;
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            string path = TBoxPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(
                    "Укажите путь для сохранения файла.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Сотрудники");
                    // Заголовки
                    ws.Cell(1, 1).Value = "ID";
                    ws.Cell(1, 2).Value = "ФИО";
                    ws.Cell(1, 3).Value = "Отдел";
                    ws.Cell(1, 4).Value = "Должность";
                    ws.Cell(1, 5).Value = "Телефон";
                    ws.Cell(1, 6).Value = "Email";
                    ws.Cell(1, 7).Value = "Статус";

                    int row = 2;
                    foreach (var worker in _workers)
                    {
                        ws.Cell(row, 1).Value = worker.WrkID;
                        ws.Cell(row, 2).Value = worker.FullName;
                        ws.Cell(row, 3).Value = worker.Department;
                        ws.Cell(row, 4).Value = worker.Position;
                        ws.Cell(row, 5).Value = worker.Phone;
                        ws.Cell(row, 6).Value = worker.Email;
                        ws.Cell(row, 7).Value = worker.Status;
                        row++;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(path);
                }

                MessageBox.Show(
                    "Экспорт успешно завершён!",
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
                    "Ошибка экспорта: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
