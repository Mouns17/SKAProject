using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;

namespace SKAProject.Pages
{
    public partial class WorkersPage : Page
    {
        public WorkersPage()
        {
            InitializeComponent();
        }

        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            new AddWorkerWindow().Show();
        }

        private void EditWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            // new EditWorkerWindow().Show();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            // new ExportExcelWindow().Show();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            // new ImportExcelWindow().Show();
        }
    }  
}