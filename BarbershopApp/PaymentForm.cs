using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class PaymentForm : Form
    {
        private DatabaseHelper dbHelper;
        private int appointmentId;
        private decimal remainingAmount;
        private NumericUpDown numAmount;
        private ComboBox cmbMethod;
        private Button btnPay, btnCancel;

        public PaymentForm(DatabaseHelper helper, int appId, decimal remaining)
        {
            //InitializeComponent();
            dbHelper = helper;
            appointmentId = appId;
            remainingAmount = remaining;
            this.Text = "Принять оплату";
            this.Size = new System.Drawing.Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();
        }

        private void SetupUI()
        {
            var mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            int yPos = 20;

            var lblAmount = new Label();
            lblAmount.Text = "Сумма оплаты:";
            lblAmount.Location = new System.Drawing.Point(20, yPos);
            lblAmount.Width = 100;

            numAmount = new NumericUpDown();
            numAmount.Location = new System.Drawing.Point(130, yPos);
            numAmount.Width = 150;
            numAmount.Minimum = 1;
            numAmount.Maximum = remainingAmount;
            numAmount.Value = remainingAmount;
            numAmount.DecimalPlaces = 2;
            yPos += 35;

            var lblMethod = new Label();
            lblMethod.Text = "Способ оплаты:";
            lblMethod.Location = new System.Drawing.Point(20, yPos);
            lblMethod.Width = 100;

            cmbMethod = new ComboBox();
            cmbMethod.Location = new System.Drawing.Point(130, yPos);
            cmbMethod.Width = 150;
            cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMethod.Items.AddRange(new object[] { "Наличные", "Карта", "Перевод" });
            cmbMethod.SelectedIndex = 0;
            yPos += 45;

            btnPay = new Button();
            btnPay.Text = "Оплатить";
            btnPay.Location = new System.Drawing.Point(130, yPos);
            btnPay.Width = 100;
            btnPay.Click += BtnPay_Click;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(240, yPos);
            btnCancel.Width = 100;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            mainPanel.Controls.Add(lblAmount);
            mainPanel.Controls.Add(numAmount);
            mainPanel.Controls.Add(lblMethod);
            mainPanel.Controls.Add(cmbMethod);
            mainPanel.Controls.Add(btnPay);
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            string query = @"INSERT INTO Payments (AppointmentId, Amount, PaymentMethod) 
                           VALUES (@appointmentId, @amount, @method)";

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@appointmentId", appointmentId),
                new SQLiteParameter("@amount", numAmount.Value),
                new SQLiteParameter("@method", cmbMethod.SelectedItem.ToString())
            };

            try
            {
                dbHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Оплата принята");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оплате: {ex.Message}");
            }
        }
    }
}