using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;

namespace BarbershopApp
{
    public partial class AppointmentForm : Form
    {
        private DatabaseHelper dbHelper;
        private ComboBox cmbClient, cmbEmployee, cmbService;
        private DateTimePicker dtpDate;
        private ComboBox cmbTime;
        private TextBox txtNotes;
        private Button btnSave, btnCancel;
        private Label lblPrice;

        public AppointmentForm(DatabaseHelper helper)
        {
            dbHelper = helper;
            this.Text = "Новая запись";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();
            LoadComboBoxes();

            // Загружаем доступное время после загрузки всех комбобоксов
            this.Shown += (s, e) => {
                LoadAvailableTimes();
            };
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);
            mainPanel.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 120;
            int controlWidth = 250;

            // Клиент
            var lblClient = new Label();
            lblClient.Text = "Клиент:*";
            lblClient.Location = new Point(20, yPos);
            lblClient.Width = labelWidth;
            lblClient.TextAlign = ContentAlignment.MiddleRight;
            lblClient.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            cmbClient = new ComboBox();
            cmbClient.Location = new Point(150, yPos - 2);
            cmbClient.Width = controlWidth;
            cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClient.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            yPos += 35;

            // Сотрудник
            var lblEmployee = new Label();
            lblEmployee.Text = "Мастер:*";
            lblEmployee.Location = new Point(20, yPos);
            lblEmployee.Width = labelWidth;
            lblEmployee.TextAlign = ContentAlignment.MiddleRight;
            lblEmployee.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            cmbEmployee = new ComboBox();
            cmbEmployee.Location = new Point(150, yPos - 2);
            cmbEmployee.Width = controlWidth;
            cmbEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmployee.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            cmbEmployee.SelectedIndexChanged += CmbEmployee_SelectedIndexChanged;
            yPos += 35;

            // Услуга
            var lblService = new Label();
            lblService.Text = "Услуга:*";
            lblService.Location = new Point(20, yPos);
            lblService.Width = labelWidth;
            lblService.TextAlign = ContentAlignment.MiddleRight;
            lblService.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            cmbService = new ComboBox();
            cmbService.Location = new Point(150, yPos - 2);
            cmbService.Width = controlWidth;
            cmbService.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbService.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            cmbService.SelectedIndexChanged += CmbService_SelectedIndexChanged;
            yPos += 35;

            // Стоимость
            var lblPriceText = new Label();
            lblPriceText.Text = "Стоимость:";
            lblPriceText.Location = new Point(20, yPos);
            lblPriceText.Width = labelWidth;
            lblPriceText.TextAlign = ContentAlignment.MiddleRight;
            lblPriceText.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            lblPrice = new Label();
            lblPrice.Text = "0 ₽";
            lblPrice.Location = new Point(150, yPos);
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            lblPrice.ForeColor = Color.Green;
            yPos += 35;

            // Дата
            var lblDate = new Label();
            lblDate.Text = "Дата:*";
            lblDate.Location = new Point(20, yPos);
            lblDate.Width = labelWidth;
            lblDate.TextAlign = ContentAlignment.MiddleRight;
            lblDate.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(150, yPos - 2);
            dtpDate.Width = 150;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.MinDate = DateTime.Today;
            dtpDate.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            dtpDate.ValueChanged += DtpDate_ValueChanged;
            yPos += 35;

            // Время
            var lblTime = new Label();
            lblTime.Text = "Время:*";
            lblTime.Location = new Point(20, yPos);
            lblTime.Width = labelWidth;
            lblTime.TextAlign = ContentAlignment.MiddleRight;
            lblTime.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            cmbTime = new ComboBox();
            cmbTime.Location = new Point(150, yPos - 2);
            cmbTime.Width = 100;
            cmbTime.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTime.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            yPos += 35;

            // Примечания
            var lblNotes = new Label();
            lblNotes.Text = "Примечания:";
            lblNotes.Location = new Point(20, yPos);
            lblNotes.Width = labelWidth;
            lblNotes.TextAlign = ContentAlignment.MiddleRight;
            lblNotes.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);

            txtNotes = new TextBox();
            txtNotes.Location = new Point(150, yPos - 2);
            txtNotes.Width = controlWidth;
            txtNotes.Height = 60;
            txtNotes.Multiline = true;
            txtNotes.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            yPos += 80;

            // Линия разделитель
            var separator = new Label();
            separator.BorderStyle = BorderStyle.Fixed3D;
            separator.Location = new Point(20, yPos - 10);
            separator.Size = new Size(430, 2);
            yPos += 10;

            // Кнопки
            btnSave = new Button();
            btnSave.Text = "Записать";
            btnSave.Location = new Point(150, yPos);
            btnSave.Size = new Size(100, 35);
            btnSave.BackColor = Color.FromArgb(52, 152, 219);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(260, yPos);
            btnCancel.Size = new Size(100, 35);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            btnCancel.Click += (s, e) => this.Close();

            mainPanel.Controls.AddRange(new Control[] {
                lblClient, cmbClient,
                lblEmployee, cmbEmployee,
                lblService, cmbService,
                lblPriceText, lblPrice,
                lblDate, dtpDate,
                lblTime, cmbTime,
                lblNotes, txtNotes,
                separator,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Загружаем клиентов
                string clientsQuery = "SELECT Id, FullName, Phone FROM Clients ORDER BY FullName";
                var clientsData = dbHelper.ExecuteQuery(clientsQuery);

                // Создаем колонку для отображения
                clientsData.Columns.Add("DisplayName", typeof(string), "FullName + ' (' + Phone + ')'");

                cmbClient.DataSource = clientsData;
                cmbClient.DisplayMember = "DisplayName";
                cmbClient.ValueMember = "Id";

                // Загружаем сотрудников
                string employeesQuery = "SELECT Id, FullName FROM Employees WHERE IsActive = 1 ORDER BY FullName";
                var employeesData = dbHelper.ExecuteQuery(employeesQuery);
                cmbEmployee.DataSource = employeesData;
                cmbEmployee.DisplayMember = "FullName";
                cmbEmployee.ValueMember = "Id";

                // Загружаем услуги
                string servicesQuery = "SELECT Id, Name, Price FROM Services WHERE IsActive = 1 ORDER BY Name";
                var servicesData = dbHelper.ExecuteQuery(servicesQuery);
                cmbService.DataSource = servicesData;
                cmbService.DisplayMember = "Name";
                cmbService.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedEmployeeId()
        {
            try
            {
                if (cmbEmployee.SelectedItem is DataRowView row)
                {
                    object idValue = row["Id"];
                    if (idValue != null && idValue != DBNull.Value)
                    {
                        return Convert.ToInt32(idValue);
                    }
                }
                else if (cmbEmployee.SelectedValue != null)
                {
                    return Convert.ToInt32(cmbEmployee.SelectedValue);
                }
            }
            catch
            {
                // Игнорируем ошибки конвертации
            }

            return -1;
        }

        private int GetSelectedServiceId()
        {
            try
            {
                if (cmbService.SelectedItem is DataRowView row)
                {
                    object idValue = row["Id"];
                    if (idValue != null && idValue != DBNull.Value)
                    {
                        return Convert.ToInt32(idValue);
                    }
                }
                else if (cmbService.SelectedValue != null)
                {
                    return Convert.ToInt32(cmbService.SelectedValue);
                }
            }
            catch
            {
                // Игнорируем ошибки конвертации
            }

            return -1;
        }

        private int GetSelectedClientId()
        {
            try
            {
                if (cmbClient.SelectedItem is DataRowView row)
                {
                    object idValue = row["Id"];
                    if (idValue != null && idValue != DBNull.Value)
                    {
                        return Convert.ToInt32(idValue);
                    }
                }
                else if (cmbClient.SelectedValue != null)
                {
                    return Convert.ToInt32(cmbClient.SelectedValue);
                }
            }
            catch
            {
                // Игнорируем ошибки конвертации
            }

            return -1;
        }

        private void CmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbService.SelectedItem != null)
                {
                    if (cmbService.SelectedItem is DataRowView row)
                    {
                        if (row["Price"] != DBNull.Value)
                        {
                            decimal price = Convert.ToDecimal(row["Price"]);
                            lblPrice.Text = $"{price:C}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления цены: {ex.Message}");
            }
        }

        private void CmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAvailableTimes();
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            LoadAvailableTimes();
        }

        private void LoadAvailableTimes()
        {
            try
            {
                cmbTime.Items.Clear();

                // Проверяем, выбран ли сотрудник
                if (cmbEmployee.SelectedItem == null)
                {
                    cmbTime.Items.Add("Сначала выберите мастера");
                    cmbTime.SelectedIndex = 0;
                    cmbTime.Enabled = false;
                    return;
                }

                cmbTime.Enabled = true;

                int employeeId = GetSelectedEmployeeId();
                if (employeeId == -1)
                {
                    cmbTime.Items.Add("Ошибка получения ID мастера");
                    cmbTime.SelectedIndex = 0;
                    return;
                }

                DateTime selectedDate = dtpDate.Value.Date;

                // Рабочее время с 9:00 до 20:00
                for (int hour = 9; hour <= 19; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 30)
                    {
                        string time = $"{hour:D2}:{minute:D2}";

                        // Проверяем, свободно ли это время
                        if (IsTimeAvailable(employeeId, selectedDate, time))
                        {
                            cmbTime.Items.Add(time);
                        }
                    }
                }

                if (cmbTime.Items.Count > 0)
                {
                    cmbTime.SelectedIndex = 0;
                }
                else
                {
                    cmbTime.Items.Add("Нет свободного времени");
                    cmbTime.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке времени: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsTimeAvailable(int employeeId, DateTime date, string time)
        {
            try
            {
                string query = @"SELECT COUNT(*) FROM Appointments 
                               WHERE EmployeeId = @employeeId 
                               AND AppointmentDate = @date 
                               AND AppointmentTime = @time 
                               AND Status != 'Отменен'";

                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@employeeId", employeeId),
                    new SQLiteParameter("@date", date.Date.ToString("yyyy-MM-dd")),
                    new SQLiteParameter("@time", time)
                };

                var result = dbHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0 && result.Rows[0][0] != DBNull.Value)
                {
                    int count = Convert.ToInt32(result.Rows[0][0]);
                    return count == 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки времени: {ex.Message}");
                return true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем ID через специальные методы
                int clientId = GetSelectedClientId();
                int employeeId = GetSelectedEmployeeId();
                int serviceId = GetSelectedServiceId();

                // Валидация
                if (clientId == -1)
                {
                    MessageBox.Show("Ошибка: не удалось получить ID клиента", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (employeeId == -1)
                {
                    MessageBox.Show("Ошибка: не удалось получить ID мастера", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (serviceId == -1)
                {
                    MessageBox.Show("Ошибка: не удалось получить ID услуги", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbTime.SelectedItem == null ||
                    cmbTime.SelectedItem.ToString() == "Нет свободного времени" ||
                    cmbTime.SelectedItem.ToString() == "Сначала выберите мастера" ||
                    cmbTime.SelectedItem.ToString() == "Ошибка получения ID мастера")
                {
                    MessageBox.Show("Выберите доступное время", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = @"INSERT INTO Appointments 
                               (ClientId, EmployeeId, ServiceId, AppointmentDate, AppointmentTime, Notes, Status) 
                               VALUES 
                               (@clientId, @employeeId, @serviceId, @date, @time, @notes, 'Запланирован')";

                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@clientId", clientId),
                    new SQLiteParameter("@employeeId", employeeId),
                    new SQLiteParameter("@serviceId", serviceId),
                    new SQLiteParameter("@date", dtpDate.Value.Date.ToString("yyyy-MM-dd")),
                    new SQLiteParameter("@time", cmbTime.SelectedItem.ToString()),
                    new SQLiteParameter("@notes", txtNotes.Text)
                };

                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Запись успешно создана", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}