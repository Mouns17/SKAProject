using System;
using System.Threading.Tasks;
using System.Windows;
using MySqlConnector;

namespace SKAProject
{
    public partial class EditWorkerWindow : Window
    {
        private readonly int _wrkId;
        private readonly string _originalDep;
        private readonly string _originalPos;
        private readonly string _originalStatus;

        public EditWorkerWindow(
            int wrkId,
            string fullName,
            string department,
            string position,
            string status
        )
        {
            InitializeComponent();

            _wrkId = wrkId;
            _originalDep = department;
            _originalPos = position;
            _originalStatus = status;

            TxtWorkerName.Text = fullName;

            // Устанавливаем текущие значения
            CmbDepartment.SelectedItem = department;
            CmbPosition.SelectedItem = position;
            CmbStatus.SelectedItem = status;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newDep = CmbDepartment.SelectedItem?.ToString();
            string newPos = CmbPosition.SelectedItem?.ToString();
            string newStatus = CmbStatus.SelectedItem?.ToString();

            if (
                string.IsNullOrWhiteSpace(newDep)
                || string.IsNullOrWhiteSpace(newPos)
                || string.IsNullOrWhiteSpace(newStatus)
            )
            {
                MessageBox.Show(
                    "Заполните все поля.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            try
            {
                using (var conn = DataBase.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var tx = conn.BeginTransaction())
                    {
                        // Получаем/создаём отдел
                        int? depId = await GetOrCreateId(
                            conn,
                            tx,
                            "departments",
                            "DepID",
                            "DepName",
                            newDep
                        );
                        // Получаем/создаём должность
                        int? posId = await GetOrCreateId(
                            conn,
                            tx,
                            "positions",
                            "PosID",
                            "PosName",
                            newPos
                        );

                        // Обновляем workers
                        string update =
                            "UPDATE workers SET DepID = @did, PosID = @pid, Status = @st WHERE WrkID = @wid";
                        using (var cmd = new MySqlCommand(update, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@did", (object)depId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@pid", (object)posId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@st", newStatus);
                            cmd.Parameters.AddWithValue("@wid", _wrkId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                    }
                }

                await Logger.LogAsync(
                    Session.UserID,
                    "Редактирование сотрудника",
                    "Управление персоналом",
                    $"Обновлён сотрудник ID {_wrkId}: отдел={newDep}, должность={newPos}, статус={newStatus}"
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка сохранения: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task<int?> GetOrCreateId(
            MySqlConnection conn,
            MySqlTransaction tx,
            string table,
            string idCol,
            string nameCol,
            string name
        )
        {
            // Ищем существующий
            string sel = $"SELECT {idCol} FROM {table} WHERE {nameCol} = @name";
            using (var cmd = new MySqlCommand(sel, conn, tx))
            {
                cmd.Parameters.AddWithValue("@name", name);
                var res = await cmd.ExecuteScalarAsync();
                if (res != null)
                    return Convert.ToInt32(res);
            }

            // Создаём новый
            string ins =
                $"INSERT INTO {table} ({nameCol}) VALUES (@name); SELECT LAST_INSERT_ID();";
            using (var cmd = new MySqlCommand(ins, conn, tx))
            {
                cmd.Parameters.AddWithValue("@name", name);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }
    }
}
