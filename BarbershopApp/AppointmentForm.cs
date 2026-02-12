using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

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
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Новая запись";
            this.Size = new System.Drawing.Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            SetupUI();
            LoadComboBoxes();
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            int yPos = 20;
            int labelWidth = 120;
            int controlWidth = 250;

            // Клиент
            var lblClient = new Label();
            lblClient.Text = "Клиент:*";
            lblClient.Location = new System.Drawing.Point(20, yPos);
            lblClient.Width = labelWidth;

            cmbClient = new ComboBox();
            cmbClient.Location = new System.Drawing.Point(150, yPos);
            cmbClient.Width = controlWidth;
            cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
            yPos += 35;

            // Сотрудник
            var lblEmployee = new Label();
            lblEmployee.Text = "Мастер:*";
            lblEmployee.Location = new System.Drawing.Point(20, yPos);
            lblEmployee.Width = labelWidth;

            cmbEmployee = new ComboBox();
            cmbEmployee.Location = new System.Drawing.Point(150, yPos);
            cmbEmployee.Width = controlWidth;
            cmbEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmployee.SelectedIndexChanged += CmbEmployee_SelectedIndexChanged;
            yPos += 35;

            // Услуга
            var lblService = new Label();
            lblService.Text = "Услуга:*";
            lblService.Location = new System.Drawing.Point(20, yPos);
            lblService.Width = labelWidth;

            cmbService = new ComboBox();
            cmbService.Location = new System.Drawing.Point(150, yPos);
            cmbService.Width = controlWidth;
            cmbService.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbService.SelectedIndexChanged += CmbService_SelectedIndexChanged;
            yPos += 35;

            // Стоимость
            var lblPriceText = new Label();
            lblPriceText.Text = "Стоимость:";
            lblPriceText.Location = new System.Drawing.Point(20, yPos);
            lblPriceText.Width = labelWidth;

            lblPrice = new Label();
            lblPrice.Text = "0 ₽";
            lblPrice.Location = new System.Drawing.Point(150, yPos);
            lblPrice.AutoSize = true;
            yPos += 35;

            // Дата
            var lblDate = new Label();
            lblDate.Text = "Дата:*";
            lblDate.Location = new System.Drawing.Point(20, yPos);
            lblDate.Width = labelWidth;

            dtpDate = new DateTimePicker();
            dtpDate.Location = new System.Drawing.Point(150, yPos);
            dtpDate.Width = 150;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.ValueChanged += DtpDate_ValueChanged;
            yPos += 35;

            // Время
            var lblTime = new Label();
            lblTime.Text = "Время:*";
            lblTime.Location = new System.Drawing.Point(20, yPos);
            lblTime.Width = labelWidth;

            cmbTime = new ComboBox();
            cmbTime.Location = new System.Drawing.Point(150, yPos);
            cmbTime.Width = 100;
            cmbTime.DropDownStyle = ComboBoxStyle.DropDownList;
            yPos += 35;

            // Примечания
            var lblNotes = new Label();
            lblNotes.Text = "Примечания:";
            lblNotes.Location = new System.Drawing.Point(20, yPos);
            lblNotes.Width = labelWidth;

            txtNotes = new TextBox();
            txtNotes.Location = new System.Drawing.Point(150, yPos);
            txtNotes.Width = controlWidth;
            txtNotes.Height = 60;
            txtNotes.Multiline = true;
            yPos += 80;

            // Кнопки
            btnSave = new Button();
            btnSave.Text = "Записать";
            btnSave.Location = new System.Drawing.Point(150, yPos);
            btnSave.Width = 100;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(260, yPos);
            btnCancel.Width = 100;
            btnCancel.Click += (s, e) => this.Close();

            mainPanel.Controls.AddRange(new Control[] {
                lblClient, cmbClient,
                lblEmployee, cmbEmployee,
                lblService, cmbService,
                lblPriceText, lblPrice,
                lblDate, dtpDate,
                lblTime, cmbTime,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadComboBoxes()
        {
            // Загружаем клиентов
            string clientsQuery = "SELECT Id, FullName FROM Clients ORDER BY FullName";
            var clientsData = dbHelper.ExecuteQuery(clientsQuery);
            cmbClient.DataSource = clientsData;
            cmbClient.DisplayMember = "FullName";
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

        private void CmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbService.SelectedItem != null)
            {
                DataRowView row = cmbService.SelectedItem as DataRowView;
                if (row != null && row["Price"] != DBNull.Value)
                {
                    decimal price = Convert.ToDecimal(row["Price"]);
                    lblPrice.Text = $"{price:C}";
                }
            }
        }

        private void CmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Обновляем доступное время при смене мастера
            LoadAvailableTimes();
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            // Обновляем доступное время при смене даты
            LoadAvailableTimes();
        }

        private void LoadAvailableTimes()
        {
            if (cmbEmployee.SelectedValue == null) return;

            cmbTime.Items.Clear();

            // Рабочее время с 9:00 до 20:00
            for (int hour = 9; hour <= 19; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    string time = $"{hour:D2}:{minute:D2}";

                    // Проверяем, свободно ли это время
                    if (IsTimeAvailable(Convert.ToInt32(cmbEmployee.SelectedValue), dtpDate.Value, time))
                    {
                        cmbTime.Items.Add(time);
                    }
                }
            }

            if (cmbTime.Items.Count > 0)
                cmbTime.SelectedIndex = 0;
        }

        private bool IsTimeAvailable(int employeeId, DateTime date, string time)
        {
            string query = @"SELECT COUNT(*) FROM Appointments 
                           WHERE EmployeeId = @employeeId 
                           AND AppointmentDate = @date 
                           AND AppointmentTime = @time 
                           AND Status != 'Отменен'";

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@employeeId", employeeId),
                new SQLiteParameter("@date", date.Date),
                new SQLiteParameter("@time", time)
            };

            var result = dbHelper.ExecuteQuery(query, parameters);
            int count = Convert.ToInt32(result.Rows[0][0]);

            return count == 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (cmbClient.SelectedValue == null ||
                cmbEmployee.SelectedValue == null ||
                cmbService.SelectedValue == null ||
                cmbTime.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            string query = @"INSERT INTO Appointments 
                           (ClientId, EmployeeId, ServiceId, AppointmentDate, AppointmentTime, Notes, Status) 
                           VALUES 
                           (@clientId, @employeeId, @serviceId, @date, @time, @notes, 'Запланирован')";

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@clientId", Convert.ToInt32(cmbClient.SelectedValue)),
                new SQLiteParameter("@employeeId", Convert.ToInt32(cmbEmployee.SelectedValue)),
                new SQLiteParameter("@serviceId", Convert.ToInt32(cmbService.SelectedValue)),
                new SQLiteParameter("@date", dtpDate.Value.Date),
                new SQLiteParameter("@time", cmbTime.SelectedItem.ToString()),
                new SQLiteParameter("@notes", txtNotes.Text)
            };

            try
            {
                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Запись успешно создана");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании записи: {ex.Message}");
            }
        }
    }
}