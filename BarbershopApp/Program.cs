namespace BarbershopApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Проверяем и создаем базу данных при запуске
                var dbHelper = new DatabaseHelper();

                // ЗАПУСКАЕМ MAINFORM, А НЕ FORM1
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}