using MySqlConnector;

namespace SKAProject
{
    public static class DataBase
    {
        private static string connStr =
            "server=localhost;database=SKADataBase;user=root;password=12345;SslMode=None";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}