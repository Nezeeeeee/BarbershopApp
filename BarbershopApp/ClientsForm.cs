using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using BarbershopApp.Models;

namespace BarbershopApp
{
    public partial class ClientsForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataGridView dgvClients;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public ClientsForm(DatabaseHelper helper)
        {
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Управление клиентами";
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

            // Таблица клиентов
            dgvClients = new DataGridView();
            dgvClients.Dock = DockStyle.Fill;
            dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.MultiSelect = false;
            dgvClients.AllowUserToAddRows = false;
            dgvClients.ReadOnly = true;

            // Добавляем все на форму
            this.Controls.Add(dgvClients);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(searchPanel);
        }

        private void LoadData(string searchText = "")
        {
            string query = "SELECT Id, FullName, Phone, Email, BirthDate, RegistrationDate, Notes FROM Clients";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " WHERE FullName LIKE @search OR Phone LIKE @search";
            }

            query += " ORDER BY FullName";

            SQLiteParameter[] parameters = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@search", $"%{searchText}%")
                };
            }

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            dgvClients.DataSource = dataTable;

            // Настройка заголовков столбцов
            dgvClients.Columns["Id"].HeaderText = "ID";
            dgvClients.Columns["FullName"].HeaderText = "ФИО";
            dgvClients.Columns["Phone"].HeaderText = "Телефон";
            dgvClients.Columns["Email"].HeaderText = "Email";
            dgvClients.Columns["BirthDate"].HeaderText = "Дата рождения";
            dgvClients.Columns["RegistrationDate"].HeaderText = "Дата регистрации";
            dgvClients.Columns["Notes"].HeaderText = "Примечания";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearch.Text);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var clientForm = new ClientEditForm(dbHelper);
            if (clientForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow == null)
            {
                MessageBox.Show("Выберите клиента для редактирования");
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.CurrentRow.Cells["Id"].Value);
            var clientForm = new ClientEditForm(dbHelper, clientId);
            if (clientForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow == null)
            {
                MessageBox.Show("Выберите клиента для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int clientId = Convert.ToInt32(dgvClients.CurrentRow.Cells["Id"].Value);
                string query = "DELETE FROM Clients WHERE Id = @id";

                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@id", clientId)
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
