using System;
using System.IO;
using System.Text;

namespace SKAProject
{
    public static class ConnectionSettings
    {
        private static readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connection.config");

        public class DBSettings
        {
            public string Server { get; set; } = "localhost";
            public int Port { get; set; } = 3306;
            public string Database { get; set; } = "SKADataBase";
            public string User { get; set; } = "root";
            public string Password { get; set; } = "";
        }

        public static DBSettings Load()
        {
            var settings = new DBSettings();
            if (File.Exists(configPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configPath, Encoding.UTF8);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length != 2) continue;
                        switch (parts[0].Trim())
                        {
                            case "Server": settings.Server = parts[1].Trim(); break;
                            case "Port": settings.Port = int.Parse(parts[1].Trim()); break;
                            case "Database": settings.Database = parts[1].Trim(); break;
                            case "User": settings.User = parts[1].Trim(); break;
                            case "Password": settings.Password = Unprotect(parts[1].Trim()); break;
                        }
                    }
                }
                catch { }
            }
            return settings;
        }

        public static void Save(DBSettings settings)
        {
            var lines = new string[]
            {
                $"Server={settings.Server}",
                $"Port={settings.Port}",
                $"Database={settings.Database}",
                $"User={settings.User}",
                $"Password={Protect(settings.Password)}"
            };
            File.WriteAllLines(configPath, lines, Encoding.UTF8);
        }

        private static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(data);
        }

        private static string Unprotect(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            byte[] data = Convert.FromBase64String(cipherText);
            return Encoding.UTF8.GetString(data);
        }
    }
}