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
    public partial class WorkersPage : Page
    {
        public WorkersPage()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Логика поиска сотрудников
            string searchText = SearchBox.Text.ToLower();
            // Здесь будет фильтрация списка
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика фильтрации по отделу и статусу
        }

        private void WorkerCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                string workerId = border.Tag?.ToString();
                // Открыть карточку сотрудника
                MainWindow.MainFrameStatic.Navigate(new ProfilePage());
            }
        }

        private void EditWorker_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string workerId = button.Tag?.ToString();
                MessageBox.Show($"Редактировать сотрудника #{workerId}");
            }
        }

        private void MoreWorker_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string workerId = button.Tag?.ToString();
                MessageBox.Show($"Дополнительные действия для #{workerId}");
            }
        }

        private void AddWorker_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавить нового сотрудника");
        }
    }
}
