using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class ClientEditForm : Form
    {
        private DatabaseHelper dbHelper;
        private int? clientId;
        private TextBox txtFullName, txtPhone, txtEmail, txtNotes;
        private DateTimePicker dtpBirthDate;
        private Button btnSave, btnCancel;

        public ClientEditForm(DatabaseHelper helper, int? existingClientId = null)
        {
            //InitializeComponent();
            dbHelper = helper;
            clientId = existingClientId;
            this.Text = clientId.HasValue ? "Редактирование клиента" : "Добавление клиента";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();

            if (clientId.HasValue)
            {
                LoadClientData();
            }
        }

        private void SetupUI()
        {
            // Создаем и настраиваем все контролы
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
            lblPhone.Text = "Телефон:*";
            lblPhone.Location = new System.Drawing.Point(20, yPos);
            lblPhone.Width = labelWidth;

            txtPhone = new TextBox();
            txtPhone.Location = new System.Drawing.Point(150, yPos);
            txtPhone.Width = controlWidth;
            yPos += 35;

            // Email
            var lblEmail = new Label();
            lblEmail.Text = "Email:";
            lblEmail.Location = new System.Drawing.Point(20, yPos);
            lblEmail.Width = labelWidth;

            txtEmail = new TextBox();
            txtEmail.Location = new System.Drawing.Point(150, yPos);
            txtEmail.Width = controlWidth;
            yPos += 35;

            // Дата рождения
            var lblBirthDate = new Label();
            lblBirthDate.Text = "Дата рождения:";
            lblBirthDate.Location = new System.Drawing.Point(20, yPos);
            lblBirthDate.Width = labelWidth;

            dtpBirthDate = new DateTimePicker();
            dtpBirthDate.Location = new System.Drawing.Point(150, yPos);
            dtpBirthDate.Width = controlWidth;
            dtpBirthDate.Format = DateTimePickerFormat.Short;
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
            btnSave.Text = "Сохранить";
            btnSave.Location = new System.Drawing.Point(150, yPos);
            btnSave.Width = 100;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(260, yPos);
            btnCancel.Width = 100;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Добавляем все на форму
            mainPanel.Controls.AddRange(new Control[] {
                lblFullName, txtFullName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblBirthDate, dtpBirthDate,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void LoadClientData()
        {
            string query = "SELECT FullName, Phone, Email, BirthDate, Notes FROM Clients WHERE Id = @id";
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@id", clientId.Value)
            };

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                txtFullName.Text = row["FullName"].ToString();
                txtPhone.Text = row["Phone"].ToString();
                txtEmail.Text = row["Email"].ToString();

                if (row["BirthDate"] != DBNull.Value)
                {
                    dtpBirthDate.Value = Convert.ToDateTime(row["BirthDate"]);
                }

                txtNotes.Text = row["Notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО клиента");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Введите телефон клиента");
                return;
            }

            string query;
            SQLiteParameter[] parameters;

            if (clientId.HasValue)
            {
                // Обновление существующего клиента
                query = @"UPDATE Clients SET 
                         FullName = @fullName, 
                         Phone = @phone, 
                         Email = @email, 
                         BirthDate = @birthDate, 
                         Notes = @notes 
                         WHERE Id = @id";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@fullName", txtFullName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@birthDate", dtpBirthDate.Checked ? dtpBirthDate.Value : (object)DBNull.Value),
                    new SQLiteParameter("@notes", txtNotes.Text),
                    new SQLiteParameter("@id", clientId.Value)
                };
            }
            else
            {
                // Добавление нового клиента
                query = @"INSERT INTO Clients (FullName, Phone, Email, BirthDate, Notes) 
                         VALUES (@fullName, @phone, @email, @birthDate, @notes)";

                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@fullName", txtFullName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@birthDate", dtpBirthDate.Checked ? dtpBirthDate.Value : (object)DBNull.Value),
                    new SQLiteParameter("@notes", txtNotes.Text)
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