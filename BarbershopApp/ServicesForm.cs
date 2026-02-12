using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class ServicesForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataGridView dgvServices;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public ServicesForm(DatabaseHelper helper)
        {
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Управление услугами";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            // Панель поиска
            var searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.Padding = new Padding(10);

            var lblSearch = new Label();
            lblSearch.Text = "Поиск:";
            lblSearch.Location = new System.Drawing.Point(10, 15);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new System.Drawing.Point(70, 12);
            txtSearch.Width = 300;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);

            // Панель кнопок
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Padding(10);

            btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Location = new System.Drawing.Point(10, 10);
            btnAdd.Width = 100;
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "Редактировать";
            btnEdit.Location = new System.Drawing.Point(120, 10);
            btnEdit.Width = 120;
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Location = new System.Drawing.Point(250, 10);
            btnDelete.Width = 100;
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(360, 10);
            btnRefresh.Width = 100;
            btnRefresh.Click += BtnRefresh_Click;

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnRefresh);

            // Таблица услуг
            dgvServices = new DataGridView();
            dgvServices.Dock = DockStyle.Fill;
            dgvServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvServices.MultiSelect = false;
            dgvServices.AllowUserToAddRows = false;
            dgvServices.ReadOnly = true;

            this.Controls.Add(dgvServices);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(searchPanel);
        }

        private void LoadData(string searchText = "")
        {
            string query = @"SELECT Id, Name, Price, DurationMinutes, Description,
                           CASE WHEN IsActive = 1 THEN 'Активна' ELSE 'Не активна' END as Status 
                           FROM Services";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " WHERE Name LIKE @search OR Description LIKE @search";
            }

            query += " ORDER BY IsActive DESC, Name";

            SQLiteParameter[] parameters = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@search", $"%{searchText}%")
                };
            }

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            dgvServices.DataSource = dataTable;

            // Настройка заголовков
            dgvServices.Columns["Id"].HeaderText = "ID";
            dgvServices.Columns["Name"].HeaderText = "Название";
            dgvServices.Columns["Price"].HeaderText = "Цена";
            dgvServices.Columns["DurationMinutes"].HeaderText = "Длительность (мин)";
            dgvServices.Columns["Description"].HeaderText = "Описание";
            dgvServices.Columns["Status"].HeaderText = "Статус";

            // Формат цены
            dgvServices.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearch.Text);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var serviceForm = new ServiceEditForm(dbHelper);
            if (serviceForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvServices.CurrentRow == null)
            {
                MessageBox.Show("Выберите услугу для редактирования");
                return;
            }

            int serviceId = Convert.ToInt32(dgvServices.CurrentRow.Cells["Id"].Value);
            var serviceForm = new ServiceEditForm(dbHelper, serviceId);
            if (serviceForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvServices.CurrentRow == null)
            {
                MessageBox.Show("Выберите услугу");
                return;
            }

            if (MessageBox.Show("Деактивировать услугу?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int serviceId = Convert.ToInt32(dgvServices.CurrentRow.Cells["Id"].Value);
                string query = "UPDATE Services SET IsActive = 0 WHERE Id = @id";

                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@id", serviceId)
                };

                dbHelper.ExecuteNonQuery(query, parameters);
                LoadData(txtSearch.Text);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadData();
        }
    }
}
