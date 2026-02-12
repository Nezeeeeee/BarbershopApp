using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class EmployeeEditForm : Form
    {
        private DatabaseHelper dbHelper;
        private int? employeeId;
        private TextBox txtFullName, txtPhone;
        private DateTimePicker dtpHireDate;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public EmployeeEditForm(DatabaseHelper helper, int? existingEmployeeId = null)
        {
            //InitializeComponent();
            dbHelper = helper;
            employeeId = existingEmployeeId;
            this.Text = employeeId.HasValue ? "Редактирование сотрудника" : "Добавление сотрудника";
            this.Size = new System.Drawing.Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();

            if (employeeId.HasValue)
            {
                LoadEmployeeData();
            }
            else
            {
                dtpHireDate.Value = DateTime.Now;
                chkIsActive.Checked = true;
                chkIsActive.Enabled = false; // Новый сотрудник сразу активен
            }
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            int yPos = 20;
            int labelWidth = 120;
            int controlWidth = 300;

            // ФИО
            var lblFullName = new Label();
            lblFullName.Text = "ФИО:*";
            lblFullName.Location = new System.Drawing.Point(20, yPos);
            lblFullName.Width = labelWidth;

            txtFullName = new TextBox();
            txtFullName.Location = new System.Drawing.Point(150, yPos);
            txtFullName.Width = controlWidth;
            yPos += 35;

            // Телефон
            var lblPhone = new Label();
            lblPhone.Text = "Телефон:";
            lblPhone.Location = new System.Drawing.Point(20, yPos);
            lblPhone.Width = labelWidth;

            txtPhone = new TextBox();
            txtPhone.Location = new System.Drawing.Point(150, yPos);
            txtPhone.Width = controlWidth;
            yPos += 35;

            // Дата приема
            var lblHireDate = new Label();
            lblHireDate.Text = "Дата приема:";
            lblHireDate.Location = new System.Drawing.Point(20, yPos);
            lblHireDate.Width = labelWidth;

            dtpHireDate = new DateTimePicker();
            dtpHireDate.Location = new System.Drawing.Point(150, yPos);
            dtpHireDate.Width = controlWidth;
            dtpHireDate.Format = DateTimePickerFormat.Short;
            yPos += 35;

            // Статус
            var lblIsActive = new Label();
            lblIsActive.Text = "Статус:";
            lblIsActive.Location = new System.Drawing.Point(20, yPos);
            lblIsActive.Width = labelWidth;

            chkIsActive = new CheckBox();
            chkIsActive.Text = "Работает";
            chkIsActive.Location = new System.Drawing.Point(150, yPos);
            chkIsActive.Width = 100;
            yPos += 45;

            // Кнопки
            btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Location = new System.Drawing.Point(150, yPos);
            btnSave.Width = 100;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(260, yPos);
            btnCancel.Width = 100;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            mainPanel.Controls.AddRange(new Control[] {
                lblFullName, txtFullName,
                lblPhone, txtPhone,
                lblHireDate, dtpHireDate,
                lblIsActive, chkIsActive,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadEmployeeData()
        {
            string query = "SELECT FullName, Phone, HireDate, IsActive FROM Employees WHERE Id = @id";
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@id", employeeId.Value)
            };

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                txtFullName.Text = row["FullName"].ToString();
                txtPhone.Text = row["Phone"].ToString();

                if (row["HireDate"] != DBNull.Value)
                {
                    dtpHireDate.Value = Convert.ToDateTime(row["HireDate"]);
                }

                chkIsActive.Checked = Convert.ToBoolean(row["IsActive"]);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО сотрудника");
                return;
            }

            string query;
            SQLiteParameter[] parameters;

            if (employeeId.HasValue)
            {
                query = @"UPDATE Employees SET 
                         FullName = @fullName, 
                         Phone = @phone, 
                         HireDate = @hireDate, 
                         IsActive = @isActive 
                         WHERE Id = @id";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@fullName", txtFullName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@hireDate", dtpHireDate.Value.Date),
                    new SQLiteParameter("@isActive", chkIsActive.Checked ? 1 : 0),
                    new SQLiteParameter("@id", employeeId.Value)
                };
            }
            else
            {
                query = @"INSERT INTO Employees (FullName, Phone, HireDate, IsActive) 
                         VALUES (@fullName, @phone, @hireDate, 1)";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@fullName", txtFullName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@hireDate", dtpHireDate.Value.Date)
                };
            }

            try
            {
                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Данные сохранены успешно");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}