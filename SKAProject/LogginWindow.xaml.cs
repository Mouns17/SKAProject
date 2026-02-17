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

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LogginWindow : Window
    {
        public LogginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {


            MainWindow tralalelotralala = new MainWindow();
            tralalelotralala.Show();
            this.Close();
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow tungtungtungsahur = new RegisterWindow();
            tungtungtungsahur.Show();
            this.Close();
        }

        private void OpenOtherAutorization_Click(object sender, RoutedEventArgs e)
        {
            PhoneLogginWindow brbrpatapim = new PhoneLogginWindow();
            brbrpatapim.Show();
            this.Close();
        }
    }
}
