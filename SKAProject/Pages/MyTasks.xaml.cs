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
    /// Логика взаимодействия для MyTasks.xaml
    /// </summary>
    public partial class MyTasks : Page
    {
        public MyTasks()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
        }

        private void ReportCreate_Click(object sender, RoutedEventArgs e)
        {
            // new CreateReportWindow().Show();
        }
    }
}
