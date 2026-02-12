using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class ServiceEditForm : Form
    {
        private DatabaseHelper dbHelper;
        private int? serviceId;
        private TextBox txtName, txtDescription;
        private NumericUpDown numPrice, numDuration;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public ServiceEditForm(DatabaseHelper helper, int? existingServiceId = null)
        {
            //InitializeComponent();
            dbHelper = helper;
            serviceId = existingServiceId;
            this.Text = serviceId.HasValue ? "Редактирование услуги" : "Добавление услуги";
            this.Size = new System.Drawing.Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();

            if (serviceId.HasValue)
            {
                LoadServiceData();
            }
            else
            {
                numPrice.Value = 0;
                numDuration.Value = 30;
                chkIsActive.Checked = true;
                chkIsActive.Enabled = false;
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

            // Название
            var lblName = new Label();
            lblName.Text = "Название:*";
            lblName.Location = new System.Drawing.Point(20, yPos);
            lblName.Width = labelWidth;

            txtName = new TextBox();
            txtName.Location = new System.Drawing.Point(150, yPos);
            txtName.Width = controlWidth;
            yPos += 35;

            // Цена
            var lblPrice = new Label();
            lblPrice.Text = "Цена:*";
            lblPrice.Location = new System.Drawing.Point(20, yPos);
            lblPrice.Width = labelWidth;

            numPrice = new NumericUpDown();
            numPrice.Location = new System.Drawing.Point(150, yPos);
            numPrice.Width = 150;
            numPrice.Minimum = 0;
            numPrice.Maximum = 100000;
            numPrice.DecimalPlaces = 2;
            yPos += 35;

            // Длительность
            var lblDuration = new Label();
            lblDuration.Text = "Длительность (мин):*";
            lblDuration.Location = new System.Drawing.Point(20, yPos);
            lblDuration.Width = labelWidth;

            numDuration = new NumericUpDown();
            numDuration.Location = new System.Drawing.Point(150, yPos);
            numDuration.Width = 150;
            numDuration.Minimum = 5;
            numDuration.Maximum = 480;
            yPos += 35;

            // Описание
            var lblDescription = new Label();
            lblDescription.Text = "Описание:";
            lblDescription.Location = new System.Drawing.Point(20, yPos);
            lblDescription.Width = labelWidth;

            txtDescription = new TextBox();
            txtDescription.Location = new System.Drawing.Point(150, yPos);
            txtDescription.Width = controlWidth;
            txtDescription.Height = 60;
            txtDescription.Multiline = true;
            yPos += 80;

            // Статус
            var lblIsActive = new Label();
            lblIsActive.Text = "Статус:";
            lblIsActive.Location = new System.Drawing.Point(20, yPos);
            lblIsActive.Width = labelWidth;

            chkIsActive = new CheckBox();
            chkIsActive.Text = "Активна";
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
                lblName, txtName,
                lblPrice, numPrice,
                lblDuration, numDuration,
                lblDescription, txtDescription,
                lblIsActive, chkIsActive,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadServiceData()
        {
            string query = "SELECT Name, Price, DurationMinutes, Description, IsActive FROM Services WHERE Id = @id";
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@id", serviceId.Value)
            };

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                txtName.Text = row["Name"].ToString();
                numPrice.Value = Convert.ToDecimal(row["Price"]);
                numDuration.Value = Convert.ToInt32(row["DurationMinutes"]);
                txtDescription.Text = row["Description"].ToString();
                chkIsActive.Checked = Convert.ToBoolean(row["IsActive"]);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название услуги");
                return;
            }

            if (numPrice.Value <= 0)
            {
                MessageBox.Show("Введите корректную цену");
                return;
            }

            string query;
            SQLiteParameter[] parameters;

            if (serviceId.HasValue)
            {
                query = @"UPDATE Services SET 
                         Name = @name, 
                         Price = @price, 
                         DurationMinutes = @duration,
                         Description = @description,
                         IsActive = @isActive 
                         WHERE Id = @id";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@price", numPrice.Value),
                    new SQLiteParameter("@duration", (int)numDuration.Value),
                    new SQLiteParameter("@description", txtDescription.Text),
                    new SQLiteParameter("@isActive", chkIsActive.Checked ? 1 : 0),
                    new SQLiteParameter("@id", serviceId.Value)
                };
            }
            else
            {
                query = @"INSERT INTO Services (Name, Price, DurationMinutes, Description, IsActive) 
                         VALUES (@name, @price, @duration, @description, 1)";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@price", numPrice.Value),
                    new SQLiteParameter("@duration", (int)numDuration.Value),
                    new SQLiteParameter("@description", txtDescription.Text)
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