using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using BarbershopApp.Models;

namespace BarbershopApp
{
    public partial class EmployeesForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataGridView dgvEmployees;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public EmployeesForm(DatabaseHelper helper)
        {
            //InitializeComponent();
            dbHelper = helper;
            this.Text = "Управление сотрудниками";
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
            btnDelete.Text = "Уволить";
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

            // Таблица сотрудников
            dgvEmployees = new DataGridView();
            dgvEmployees.Dock = DockStyle.Fill;
            dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployees.MultiSelect = false;
            dgvEmployees.AllowUserToAddRows = false;
            dgvEmployees.ReadOnly = true;

            this.Controls.Add(dgvEmployees);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(searchPanel);
        }

        private void LoadData(string searchText = "")
        {
            string query = @"SELECT Id, FullName, Phone, HireDate, 
                           CASE WHEN IsActive = 1 THEN 'Работает' ELSE 'Уволен' END as Status 
                           FROM Employees";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " WHERE FullName LIKE @search OR Phone LIKE @search";
            }

            query += " ORDER BY IsActive DESC, FullName";

            SQLiteParameter[] parameters = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@search", $"%{searchText}%")
                };
            }

            var dataTable = dbHelper.ExecuteQuery(query, parameters);
            dgvEmployees.DataSource = dataTable;

            // Настройка заголовков
            dgvEmployees.Columns["Id"].HeaderText = "ID";
            dgvEmployees.Columns["FullName"].HeaderText = "ФИО";
            dgvEmployees.Columns["Phone"].HeaderText = "Телефон";
            dgvEmployees.Columns["HireDate"].HeaderText = "Дата приема";
            dgvEmployees.Columns["Status"].HeaderText = "Статус";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearch.Text);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var employeeForm = new EmployeeEditForm(dbHelper);
            if (employeeForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования");
                return;
            }

            int employeeId = Convert.ToInt32(dgvEmployees.CurrentRow.Cells["Id"].Value);
            var employeeForm = new EmployeeEditForm(dbHelper, employeeId);
            if (employeeForm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите сотрудника");
                return;
            }

            if (MessageBox.Show("Отметить сотрудника как уволенного?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int employeeId = Convert.ToInt32(dgvEmployees.CurrentRow.Cells["Id"].Value);
                string query = "UPDATE Employees SET IsActive = 0 WHERE Id = @id";

                SQLiteParameter[] parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@id", employeeId)
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