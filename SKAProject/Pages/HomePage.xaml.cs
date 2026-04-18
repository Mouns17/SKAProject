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
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private RegOrgWindow _regOrgWindow;
        public HomePage()
        {
            InitializeComponent();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRegOrg(object sender, RoutedEventArgs e)
        {
            if (_regOrgWindow == null)
            {
                _regOrgWindow = new RegOrgWindow();
                _regOrgWindow.Closed += (s, args) => _regOrgWindow = null;
                _regOrgWindow.Show();
            }
            else
            {
                _regOrgWindow.Activate(); // Активируем окно, если оно уже открыто
            }
        }
    }
}
