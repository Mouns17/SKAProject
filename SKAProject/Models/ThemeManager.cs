using System;
using System.Windows;

namespace SKAProject
{
    public static class ThemeManager
    {
        private static ResourceDictionary _currentThemeDictionary;

        public static void ApplyTheme(string themeName)
        {
            // Удаляем предыдущую тему, если была
            if (_currentThemeDictionary != null)
                Application.Current.Resources.MergedDictionaries.Remove(_currentThemeDictionary);

            // Загружаем выбранную тему
            string uri = $"/Resources/{themeName}Theme.xaml";
            _currentThemeDictionary = new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Relative),
            };

            Application.Current.Resources.MergedDictionaries.Add(_currentThemeDictionary);
        }

        public static void LoadSavedTheme()
        {
            string theme = Properties.Settings.Default.Theme;
            if (string.IsNullOrEmpty(theme))
                theme = "Light";
            ApplyTheme(theme);
        }
    }
}
