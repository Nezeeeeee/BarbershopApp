using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;

namespace BarbershopApp
{
    public partial class RevenueReportForm : Form
    {
        private DatabaseHelper dbHelper;
        private DateTimePicker dtpStart, dtpEnd;
        private Button btnGenerate;
        private DataGridView dgvReport;
        private Label lblTotal;

        public RevenueReportForm(DatabaseHelper helper)
        {
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Отчет по выручке";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            SetupUI();
        }

        private void SetupUI()
        {
            // Панель параметров
            var paramPanel = new Panel();
            paramPanel.Dock = DockStyle.Top;
            paramPanel.Height = 80;
            paramPanel.Padding = new Padding(10);

            var lblStart = new Label();
            lblStart.Text = "Начало периода:";
            lblStart.Location = new System.Drawing.Point(10, 20);
            lblStart.AutoSize = true;

            dtpStart = new DateTimePicker();
            dtpStart.Location = new System.Drawing.Point(120, 17);
            dtpStart.Width = 150;
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Value = DateTime.Now.AddMonths(-1);

            var lblEnd = new Label();
            lblEnd.Text = "Конец периода:";
            lblEnd.Location = new System.Drawing.Point(300, 20);
            lblEnd.AutoSize = true;

            dtpEnd = new DateTimePicker();
            dtpEnd.Location = new System.Drawing.Point(410, 17);
            dtpEnd.Width = 150;
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Value = DateTime.Now;

            btnGenerate = new Button();
            btnGenerate.Text = "Сформировать отчет";
            btnGenerate.Location = new System.Drawing.Point(600, 15);
            btnGenerate.Width = 180;
            btnGenerate.Height = 30;
            btnGenerate.Click += BtnGenerate_Click;

            paramPanel.Controls.Add(lblStart);
            paramPanel.Controls.Add(dtpStart);
            paramPanel.Controls.Add(lblEnd);
            paramPanel.Controls.Add(dtpEnd);
            paramPanel.Controls.Add(btnGenerate);

            // Таблица отчета
            dgvReport = new DataGridView();
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReport.ReadOnly = true;
            dgvReport.AllowUserToAddRows = false;

            // Панель итогов
            var totalPanel = new Panel();
            totalPanel.Dock = DockStyle.Bottom;
            totalPanel.Height = 40;
            totalPanel.BackColor = Color.LightGray;
            totalPanel.Padding = new Padding(10);

            lblTotal = new Label();
            lblTotal.Text = "Общая выручка: 0 ₽";
            lblTotal.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTotal.Location = new System.Drawing.Point(10, 10);
            lblTotal.AutoSize = true;

            totalPanel.Controls.Add(lblTotal);

            this.Controls.Add(dgvReport);
            this.Controls.Add(totalPanel);
            this.Controls.Add(paramPanel);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            string query = @"
                SELECT 
                    date(p.PaymentDate) as PaymentDate,
                    c.FullName as ClientName,
                    e.FullName as EmployeeName,
                    s.Name as ServiceName,
                    p.Amount,
                    p.PaymentMethod
                FROM Payments p
                JOIN Appointments a ON p.AppointmentId = a.Id
                JOIN Clients c ON a.ClientId = c.Id
                JOIN Employees e ON a.EmployeeId = e.Id
                JOIN Services s ON a.ServiceId = s.Id
                WHERE date(p.PaymentDate) BETWEEN @start AND @end
                ORDER BY p.PaymentDate DESC";

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@start", dtpStart.Value.Date),
                new SQLiteParameter("@end", dtpEnd.Value.Date)
            };

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            dgvReport.DataSource = dataTable;

            // Настройка заголовков
            dgvReport.Columns["PaymentDate"].HeaderText = "Дата оплаты";
            dgvReport.Columns["ClientName"].HeaderText = "Клиент";
            dgvReport.Columns["EmployeeName"].HeaderText = "Мастер";
            dgvReport.Columns["ServiceName"].HeaderText = "Услуга";
            dgvReport.Columns["Amount"].HeaderText = "Сумма";
            dgvReport.Columns["PaymentMethod"].HeaderText = "Способ оплаты";

            dgvReport.Columns["Amount"].DefaultCellStyle.Format = "C2";

            // Подсчет итога
            decimal total = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                total += Convert.ToDecimal(row["Amount"]);
            }

            lblTotal.Text = $"Общая выручка: {total:C2}";
        }
    }
}