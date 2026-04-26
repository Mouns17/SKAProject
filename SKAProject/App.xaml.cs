using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SKAProject
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UserSession CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.LoadSavedTheme();
        }
    }

    public class UserSession
    {
        public int UserID { get; set; }
        public int OrgID { get; set; }
        public string Role { get; set; }
    }
}
