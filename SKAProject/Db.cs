using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace SKAProject
{
    public static class Db
    {
        private static string connStr =
            "server=localhost;database=SKADB;user=root;password=12345;SslMode=None";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}
