using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace SKAProject
{
    public static class Logger
    {
        public static async Task LogAsync(
            int? userId,
            string action,
            string eventType,
            string description = ""
        )
        {
            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    string query =
                        @"INSERT INTO logs (UserID, Action, EventType, Description, CreatedAt)
                                     VALUES (@uid, @action, @type, @desc, NOW())";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", (object)userId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@action", action);
                        cmd.Parameters.AddWithValue("@type", eventType);
                        cmd.Parameters.AddWithValue("@desc", description);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Лучше записать в отладку, но не показывать пользователю
                System.Diagnostics.Debug.WriteLine("Ошибка логирования: " + ex.Message);
            }
        }
    }
}
