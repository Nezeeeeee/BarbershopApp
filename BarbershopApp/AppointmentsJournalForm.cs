using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;

namespace BarbershopApp
{
    public partial class AppointmentsJournalForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataGridView dgvAppointments;
        private DateTimePicker dtpDate;
        private ComboBox cmbStatus;
        private Button btnComplete, btnCancel, btnRefresh, btnPayment;
        private CheckBox chkShowAll;
        private Label lblInfo;

        public AppointmentsJournalForm(DatabaseHelper helper)
        {
            dbHelper = helper;
            this.Text = "Журнал записей";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.IsMdiContainer = false;

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            // Панель фильтров
            var filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 80;
            filterPanel.Padding = new Padding(10);
            filterPanel.BackColor = Color.FromArgb(240, 240, 240);
            filterPanel.BorderStyle = BorderStyle.FixedSingle;

            var lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Location = new Point(10, 15);
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);

            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(60, 12);
            dtpDate.Width = 150;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.ValueChanged += DtpDate_ValueChanged;

            var lblStatus = new Label();
            lblStatus.Text = "Статус:";
            lblStatus.Location = new Point(230, 15);
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);

            cmbStatus = new ComboBox();
            cmbStatus.Location = new Point(290, 12);
            cmbStatus.Width = 150;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "Все", "Запланирован", "Выполнен", "Отменен", "Не пришел" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;

            chkShowAll = new CheckBox();
            chkShowAll.Text = "Показать все записи";
            chkShowAll.Location = new Point(460, 12);
            chkShowAll.AutoSize = true;
            chkShowAll.CheckedChanged += ChkShowAll_CheckedChanged;

            // Информационная надпись
            lblInfo = new Label();
            lblInfo.Text = "Всего записей: 0";
            lblInfo.Location = new Point(600, 12);
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Italic);
            lblInfo.ForeColor = Color.Gray;

            filterPanel.Controls.Add(lblDate);
            filterPanel.Controls.Add(dtpDate);
            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(chkShowAll);
            filterPanel.Controls.Add(lblInfo);

            // Панель кнопок
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10);
            buttonPanel.BackColor = Color.White;

            btnComplete = new Button();
            btnComplete.Text = "✓ Отметить выполненным";
            btnComplete.Location = new Point(10, 10);
            btnComplete.Size = new Size(180, 30);
            btnComplete.BackColor = Color.FromArgb(46, 204, 113);
            btnComplete.ForeColor = Color.White;
            btnComplete.FlatStyle = FlatStyle.Flat;
            btnComplete.FlatAppearance.BorderSize = 0;
            btnComplete.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            btnComplete.Click += BtnComplete_Click;

            btnCancel = new Button();
            btnCancel.Text = "✗ Отменить запись";
            btnCancel.Location = new Point(200, 10);
            btnCancel.Size = new Size(150, 30);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            btnCancel.Click += BtnCancel_Click;

            btnPayment = new Button();
            btnPayment.Text = "💰 Принять оплату";
            btnPayment.Location = new Point(360, 10);
            btnPayment.Size = new Size(150, 30);
            btnPayment.BackColor = Color.FromArgb(52, 152, 219);
            btnPayment.ForeColor = Color.White;
            btnPayment.FlatStyle = FlatStyle.Flat;
            btnPayment.FlatAppearance.BorderSize = 0;
            btnPayment.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            btnPayment.Click += BtnPayment_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new Point(520, 10);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.BackColor = Color.FromArgb(149, 165, 166);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
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
            dgvAppointments.RowHeadersVisible = false;
            dgvAppointments.BackgroundColor = Color.White;
            dgvAppointments.BorderStyle = BorderStyle.Fixed3D;
            dgvAppointments.CellFormatting += DgvAppointments_CellFormatting;

            this.Controls.Add(dgvAppointments);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(filterPanel);
        }

        private void LoadData()
        {
            try
            {
                string query;
                SQLiteParameter[] parameters = null;

                if (chkShowAll.Checked)
                {
                    // Показываем все записи
                    query = @"
                        SELECT 
                            a.Id,
                            c.FullName as ClientName,
                            c.Phone as ClientPhone,
                            e.FullName as EmployeeName,
                            s.Name as ServiceName,
                            s.Price,
                            a.AppointmentDate,
                            a.AppointmentTime,
                            a.Status,
                            a.Notes,
                            (SELECT IFNULL(SUM(Amount), 0) FROM Payments WHERE AppointmentId = a.Id) as PaidAmount
                        FROM Appointments a
                        LEFT JOIN Clients c ON a.ClientId = c.Id
                        LEFT JOIN Employees e ON a.EmployeeId = e.Id
                        LEFT JOIN Services s ON a.ServiceId = s.Id";

                    if (cmbStatus.SelectedIndex > 0)
                    {
                        query += " WHERE a.Status = @status";
                        parameters = new SQLiteParameter[]
                        {
                            new SQLiteParameter("@status", cmbStatus.SelectedItem.ToString())
                        };
                    }

                    query += " ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC";
                }
                else
                {
                    // Показываем записи только за выбранную дату
                    query = @"
                        SELECT 
                            a.Id,
                            c.FullName as ClientName,
                            c.Phone as ClientPhone,
                            e.FullName as EmployeeName,
                            s.Name as ServiceName,
                            s.Price,
                            a.AppointmentDate,
                            a.AppointmentTime,
                            a.Status,
                            a.Notes,
                            (SELECT IFNULL(SUM(Amount), 0) FROM Payments WHERE AppointmentId = a.Id) as PaidAmount
                        FROM Appointments a
                        LEFT JOIN Clients c ON a.ClientId = c.Id
                        LEFT JOIN Employees e ON a.EmployeeId = e.Id
                        LEFT JOIN Services s ON a.ServiceId = s.Id
                        WHERE a.AppointmentDate = @date";

                    if (cmbStatus.SelectedIndex > 0)
                    {
                        query += " AND a.Status = @status";
                        parameters = new SQLiteParameter[]
                        {
                            new SQLiteParameter("@date", dtpDate.Value.Date.ToString("yyyy-MM-dd")),
                            new SQLiteParameter("@status", cmbStatus.SelectedItem.ToString())
                        };
                    }
                    else
                    {
                        parameters = new SQLiteParameter[]
                        {
                            new SQLiteParameter("@date", dtpDate.Value.Date.ToString("yyyy-MM-dd"))
                        };
                    }

                    query += " ORDER BY a.AppointmentTime";
                }

                var dataTable = dbHelper.ExecuteQuery(query, parameters);

                if (dataTable != null)
                {
                    dgvAppointments.DataSource = dataTable;
                    ConfigureDataGridView();
                    UpdateInfoLabel(dataTable.Rows.Count);
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить данные", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridView()
        {
            if (dgvAppointments.Columns == null || dgvAppointments.Columns.Count == 0) return;

            try
            {
                // Скрываем колонку Id
                if (dgvAppointments.Columns.Contains("Id"))
                    dgvAppointments.Columns["Id"].Visible = false;

                // Настройка заголовков
                if (dgvAppointments.Columns.Contains("ClientName"))
                    dgvAppointments.Columns["ClientName"].HeaderText = "Клиент";

                if (dgvAppointments.Columns.Contains("ClientPhone"))
                    dgvAppointments.Columns["ClientPhone"].HeaderText = "Телефон";

                if (dgvAppointments.Columns.Contains("EmployeeName"))
                    dgvAppointments.Columns["EmployeeName"].HeaderText = "Мастер";

                if (dgvAppointments.Columns.Contains("ServiceName"))
                    dgvAppointments.Columns["ServiceName"].HeaderText = "Услуга";

                if (dgvAppointments.Columns.Contains("Price"))
                {
                    dgvAppointments.Columns["Price"].HeaderText = "Цена";
                    dgvAppointments.Columns["Price"].DefaultCellStyle.Format = "C2";
                }

                if (dgvAppointments.Columns.Contains("AppointmentDate"))
                    dgvAppointments.Columns["AppointmentDate"].HeaderText = "Дата";

                if (dgvAppointments.Columns.Contains("AppointmentTime"))
                    dgvAppointments.Columns["AppointmentTime"].HeaderText = "Время";

                if (dgvAppointments.Columns.Contains("Status"))
                    dgvAppointments.Columns["Status"].HeaderText = "Статус";

                if (dgvAppointments.Columns.Contains("Notes"))
                {
                    dgvAppointments.Columns["Notes"].HeaderText = "Примечания";
                    dgvAppointments.Columns["Notes"].Width = 200;
                }

                if (dgvAppointments.Columns.Contains("PaidAmount"))
                {
                    dgvAppointments.Columns["PaidAmount"].HeaderText = "Оплачено";
                    dgvAppointments.Columns["PaidAmount"].DefaultCellStyle.Format = "C2";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при настройке таблицы: {ex.Message}");
            }
        }

        private void UpdateInfoLabel(int count)
        {
            if (lblInfo != null)
            {
                lblInfo.Text = $"Всего записей: {count}";
                if (chkShowAll.Checked)
                    lblInfo.Text += " (все записи)";
                else
                    lblInfo.Text += $" за {dtpDate.Value.ToShortDateString()}";
            }
        }

        private void DgvAppointments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dgvAppointments.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString();
                    switch (status)
                    {
                        case "Запланирован":
                            e.CellStyle.BackColor = Color.LightYellow;
                            e.CellStyle.ForeColor = Color.Black;
                            e.CellStyle.Font = new Font(dgvAppointments.Font, FontStyle.Bold);
                            break;
                        case "Выполнен":
                            e.CellStyle.BackColor = Color.LightGreen;
                            e.CellStyle.ForeColor = Color.Black;
                            break;
                        case "Отменен":
                            e.CellStyle.BackColor = Color.LightCoral;
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        case "Не пришел":
                            e.CellStyle.BackColor = Color.LightGray;
                            e.CellStyle.ForeColor = Color.Black;
                            e.CellStyle.Font = new Font(dgvAppointments.Font, FontStyle.Italic);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка форматирования ячейки: {ex.Message}");
            }
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowAll != null)
                chkShowAll.Checked = false;
            LoadData();
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void ChkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            if (dtpDate != null)
                dtpDate.Enabled = !chkShowAll.Checked;
            LoadData();
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvAppointments.CurrentRow == null)
                {
                    MessageBox.Show("Выберите запись", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Получаем ID из текущей строки
                if (dgvAppointments.CurrentRow.Cells["Id"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Id"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить ID записи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);

                // Получаем статус
                if (dgvAppointments.CurrentRow.Cells["Status"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Status"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить статус записи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string status = dgvAppointments.CurrentRow.Cells["Status"].Value.ToString();

                if (status == "Выполнен")
                {
                    MessageBox.Show("Запись уже отмечена как выполненная", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Отметить запись как выполненную?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string query = "UPDATE Appointments SET Status = 'Выполнен' WHERE Id = @id";
                    SQLiteParameter[] parameters = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@id", appointmentId)
                    };

                    int result = dbHelper.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        LoadData();

                        // Предлагаем принять оплату
                        if (MessageBox.Show("Принять оплату за выполненную услугу?", "Оплата",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            BtnPayment_Click(sender, e);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить статус записи", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvAppointments.CurrentRow == null)
                {
                    MessageBox.Show("Выберите запись", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Получаем ID
                if (dgvAppointments.CurrentRow.Cells["Id"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Id"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить ID записи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);

                // Получаем статус
                if (dgvAppointments.CurrentRow.Cells["Status"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Status"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить статус записи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string status = dgvAppointments.CurrentRow.Cells["Status"].Value.ToString();

                if (status == "Отменен")
                {
                    MessageBox.Show("Запись уже отменена", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (status == "Выполнен")
                {
                    MessageBox.Show("Нельзя отменить выполненную запись", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Простая форма для причины отмены
                string reason = Microsoft.VisualBasic.Interaction.InputBox(
                    "Укажите причину отмены:",
                    "Причина отмены",
                    "Клиент отменил запись",
                    -1, -1);

                if (!string.IsNullOrEmpty(reason))
                {
                    // Получаем текущие примечания
                    string currentNotes = "";
                    if (dgvAppointments.CurrentRow.Cells["Notes"].Value != null &&
                        dgvAppointments.CurrentRow.Cells["Notes"].Value != DBNull.Value)
                    {
                        currentNotes = dgvAppointments.CurrentRow.Cells["Notes"].Value.ToString();
                    }

                    string query = @"UPDATE Appointments 
                                    SET Status = 'Отменен', 
                                        Notes = @notes
                                    WHERE Id = @id";

                    string newNotes = currentNotes;
                    if (!string.IsNullOrEmpty(currentNotes))
                        newNotes += Environment.NewLine;
                    newNotes += $"Отмена: {reason} ({DateTime.Now:dd.MM.yyyy HH:mm})";

                    SQLiteParameter[] parameters = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@id", appointmentId),
                        new SQLiteParameter("@notes", newNotes)
                    };

                    int result = dbHelper.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        MessageBox.Show("Запись отменена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отменить запись", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPayment_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvAppointments.CurrentRow == null)
                {
                    MessageBox.Show("Выберите запись", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Получаем ID
                if (dgvAppointments.CurrentRow.Cells["Id"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Id"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить ID записи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int appointmentId = Convert.ToInt32(dgvAppointments.CurrentRow.Cells["Id"].Value);

                // Получаем цену
                if (dgvAppointments.CurrentRow.Cells["Price"].Value == null ||
                    dgvAppointments.CurrentRow.Cells["Price"].Value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить стоимость услуги", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                decimal price = Convert.ToDecimal(dgvAppointments.CurrentRow.Cells["Price"].Value);

                // Получаем оплаченную сумму
                decimal paid = 0;
                if (dgvAppointments.CurrentRow.Cells["PaidAmount"].Value != null &&
                    dgvAppointments.CurrentRow.Cells["PaidAmount"].Value != DBNull.Value)
                {
                    paid = Convert.ToDecimal(dgvAppointments.CurrentRow.Cells["PaidAmount"].Value);
                }

                if (paid >= price)
                {
                    MessageBox.Show("Услуга уже оплачена полностью", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Создаем форму оплаты
                Form paymentForm = new Form();
                paymentForm.Text = "Принять оплату";
                paymentForm.Size = new Size(400, 200);
                paymentForm.StartPosition = FormStartPosition.CenterParent;
                paymentForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                paymentForm.MaximizeBox = false;
                paymentForm.MinimizeBox = false;

                // Создаем элементы формы оплаты
                Label lblAmount = new Label();
                lblAmount.Text = "Сумма оплаты:";
                lblAmount.Location = new Point(20, 20);
                lblAmount.Size = new Size(100, 20);

                NumericUpDown numAmount = new NumericUpDown();
                numAmount.Location = new Point(130, 18);
                numAmount.Size = new Size(150, 20);
                numAmount.Minimum = 1;
                numAmount.Maximum = price - paid;
                numAmount.Value = price - paid;
                numAmount.DecimalPlaces = 2;

                Label lblMethod = new Label();
                lblMethod.Text = "Способ оплаты:";
                lblMethod.Location = new Point(20, 50);
                lblMethod.Size = new Size(100, 20);

                ComboBox cmbMethod = new ComboBox();
                cmbMethod.Location = new Point(130, 48);
                cmbMethod.Size = new Size(150, 20);
                cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbMethod.Items.AddRange(new object[] { "Наличные", "Карта", "Перевод" });
                cmbMethod.SelectedIndex = 0;

                Button btnPay = new Button();
                btnPay.Text = "Оплатить";
                btnPay.Location = new Point(130, 90);
                btnPay.Size = new Size(100, 30);
                btnPay.DialogResult = DialogResult.OK;

                Button btnCancelPay = new Button();
                btnCancelPay.Text = "Отмена";
                btnCancelPay.Location = new Point(240, 90);
                btnCancelPay.Size = new Size(70, 30);
                btnCancelPay.DialogResult = DialogResult.Cancel;

                paymentForm.Controls.Add(lblAmount);
                paymentForm.Controls.Add(numAmount);
                paymentForm.Controls.Add(lblMethod);
                paymentForm.Controls.Add(cmbMethod);
                paymentForm.Controls.Add(btnPay);
                paymentForm.Controls.Add(btnCancelPay);

                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    string query = @"INSERT INTO Payments (AppointmentId, Amount, PaymentMethod) 
                                   VALUES (@appointmentId, @amount, @method)";

                    SQLiteParameter[] parameters = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@appointmentId", appointmentId),
                        new SQLiteParameter("@amount", numAmount.Value),
                        new SQLiteParameter("@method", cmbMethod.SelectedItem.ToString())
                    };

                    int result = dbHelper.ExecuteNonQuery(query, parameters);

                    if (result > 0)
                    {
                        MessageBox.Show("Оплата принята", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить оплату", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оплате: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}