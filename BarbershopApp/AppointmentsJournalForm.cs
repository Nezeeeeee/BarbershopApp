using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class AppointmentsJournalForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataGridView dgvAppointments;
        private DateTimePicker dtpDate;
        private ComboBox cmbStatus;
        private Button btnComplete, btnCancel, btnRefresh, btnPayment;

        public AppointmentsJournalForm(DatabaseHelper helper)
        {
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Журнал записей";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            // Панель фильтров
            var filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 60;
            filterPanel.Padding = new Padding(10);

            var lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Location = new System.Drawing.Point(10, 15);
            lblDate.AutoSize = true;

            dtpDate = new DateTimePicker();
            dtpDate.Location = new System.Drawing.Point(60, 12);
            dtpDate.Width = 150;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.ValueChanged += DtpDate_ValueChanged;

            var lblStatus = new Label();
            lblStatus.Text = "Статус:";
            lblStatus.Location = new System.Drawing.Point(230, 15);
            lblStatus.AutoSize = true;

            cmbStatus = new ComboBox();
            cmbStatus.Location = new System.Drawing.Point(290, 12);
            cmbStatus.Width = 150;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "Все", "Запланирован", "Выполнен", "Отменен", "Не пришел" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;

            filterPanel.Controls.Add(lblDate);
            filterPanel.Controls.Add(dtpDate);
            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cmbStatus);

            // Панель кнопок
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10);

            btnComplete = new Button();
            btnComplete.Text = "Отметить выполненным";
            btnComplete.Location = new System.Drawing.Point(10, 10);
            btnComplete.Width = 180;
            btnComplete.Click += BtnComplete_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отменить запись";
            btnCancel.Location = new System.Drawing.Point(200, 10);
            btnCancel.Width = 150;
            btnCancel.Click += BtnCancel_Click;

            btnPayment = new Button();
            btnPayment.Text = "Принять оплату";
            btnPayment.Location = new System.Drawing.Point(360, 10);
            btnPayment.Width = 150;
            btnPayment.Click += BtnPayment_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(520, 10);
            btnRefresh.Width = 100;
            btnRefresh.Click += BtnRefresh_Click;

            buttonPanel.Controls.Add(btnComplete);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnPayment);
            buttonPanel.Controls.Add(btnRefresh);

            // Таблица записей
            dgvAppointments = new DataGridView();
            dgvAppointments.Dock = DockStyle.Fill;
            dgvAppointments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.MultiSelect = false;
            dgvAppointments.AllowUserToAddRows = false;
            dgvAppointments.ReadOnly = true;

            this.Controls.Add(dgvAppointments);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(filterPanel);
        }

        private void LoadData()
        {
            string query = @"
                SELECT 
                    a.Id,
                    c.FullName as ClientName,
                    e.FullName as EmployeeName,
                    s.Name as ServiceName,
                    s.Price,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    a.Status,
                    a.Notes,
                    (SELECT SUM(Amount) FROM Payments WHERE AppointmentId = a.Id) as PaidAmount
                FROM Appointments a
                JOIN Clients c ON a.ClientId = c.Id
                JOIN Employees e ON a.EmployeeId = e.Id
                JOIN Services s ON a.ServiceId = s.Id
                WHERE a.AppointmentDate = @date";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@date", dtpDate.Value.Date)
            };

            if (cmbStatus.SelectedIndex > 0)
            {
                query += " AND a.Status = @status";
                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@date", dtpDate.Value.Date),
                    new SQLiteParameter("@status", cmbStatus.SelectedItem.ToString())
                };
            }

            query += " ORDER BY a.AppointmentTime";

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            dgvAppointments.DataSource = dataTable;

            // Настройка заголовков
            dgvAppointments.Columns["Id"].HeaderText = "ID";
            dgvAppointments.Columns["ClientName"].HeaderText = "Клиент";
            dgvAppointments.Columns["EmployeeName"].HeaderText = "Мастер";
            dgvAppointments.Columns["ServiceName"].HeaderText = "Услуга";
            dgvAppointments.Columns["Price"].HeaderText = "Цена";
            dgvAppointments.Columns["AppointmentDate"].HeaderText = "Дата";
            dgvAppointments.Columns["AppointmentTime"].HeaderText = "Время";
            dgvAppointments.Columns["Status"].HeaderText = "Статус";
            dgvAppointments.Columns["Notes"].HeaderText = "Примечания";
            dgvAppointments.Columns["PaidAmount"].HeaderText = "Оплачено";

            dgvAppointments.Columns["Price"].DefaultCellStyle.Format = "C2";
            dgvAppointments.Columns["PaidAmount"].DefaultCellStyle.Format = "C2";

            // Цветовая индикация статусов
            dgvAppointments.CellFormatting += DgvAppointments_CellFormatting;
        }

        private void DgvAppointments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvAppointments.Columns[e.ColumnIndex].Name == "Status")
            {
                string status = e.Value?.ToString();
                switch (status)
                {
                    case "Запланирован":
                        e.CellStyle.BackColor = System.Drawing.Color.LightYellow;
                        break;
                    case "Выполнен":
                        e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                        break;
                    case "Отменен":
                        e.CellStyle.BackColor = System.Drawing.Color.LightCoral;
                        break;
                    case "Не пришел":
                        e.CellStyle.BackColor = System.Drawing.Color.LightGray;
                        break;
                }
            }
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);
            string status = dgvAppointments.CurrentRow.Cells["Status"].Value.ToString();

            if (status == "Выполнен")
            {
                MessageBox.Show("Запись уже отмечена как выполненная");
                return;
            }

            string query = "UPDATE Appointments SET Status = 'Выполнен' WHERE Id = @id";
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@id", appointmentId)
            };

            dbHelper.ExecuteNonQuery(query, parameters);
            LoadData();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);
            string status = dgvAppointments.CurrentRow.Cells["Status"].Value.ToString();

            if (status == "Отменен")
            {
                MessageBox.Show("Запись уже отменена");
                return;
            }

            if (MessageBox.Show("Отменить запись?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string query = "UPDATE Appointments SET Status = 'Отменен' WHERE Id = @id";
                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@id", appointmentId)
                };

                dbHelper.ExecuteNonQuery(query, parameters);
                LoadData();
            }
        }

        private void BtnPayment_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);
            decimal price = Convert.ToDecimal(dgvAppointments.CurrentRow.Cells["Price"].Value);
            object paidObj = dgvAppointments.CurrentRow.Cells["PaidAmount"].Value;
            decimal paid = paidObj == DBNull.Value ? 0 : Convert.ToDecimal(paidObj);

            if (paid >= price)
            {
                MessageBox.Show("Услуга уже оплачена полностью");
                return;
            }

            var paymentForm = new PaymentForm(dbHelper, appointmentId, price - paid);
            if (paymentForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}