using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace SKAProject
{  
    public static class DataBase
    {
        private static string connStr = "server=localhost;database=SKADataBase;user=root;password=123456;SslMode=None";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}
