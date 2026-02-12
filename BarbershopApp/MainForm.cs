using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarbershopApp
{
    public partial class MainForm : Form
    {
        private DatabaseHelper dbHelper;

        public MainForm()
        {
            // НЕ вызывайте InitializeComponent() если нет файла дизайнера
            dbHelper = new DatabaseHelper();

            // Настройки формы
            this.Text = "Управление парикмахерской";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true; // Включаем MDI контейнер

            // Загружаем меню и дашборд
            CreateMenuStrip();
            ShowDashboard();
        }

        private void CreateMenuStrip()
        {
            var menuStrip = new MenuStrip();

            // Справочники
            var справочникиMenuItem = new ToolStripMenuItem("Справочники");
            справочникиMenuItem.DropDownItems.Add("Клиенты", null, ClientsMenuItem_Click);
            справочникиMenuItem.DropDownItems.Add("Сотрудники", null, EmployeesMenuItem_Click);
            справочникиMenuItem.DropDownItems.Add("Услуги", null, ServicesMenuItem_Click);

            // Записи
            var записиMenuItem = new ToolStripMenuItem("Записи");
            записиMenuItem.DropDownItems.Add("Новая запись", null, NewAppointmentMenuItem_Click);
            записиMenuItem.DropDownItems.Add("Журнал записей", null, AppointmentsMenuItem_Click);

            // Отчеты
            var отчетыMenuItem = new ToolStripMenuItem("Отчеты");
            отчетыMenuItem.DropDownItems.Add("Выручка за период", null, RevenueReportMenuItem_Click);

            menuStrip.Items.Add(справочникиMenuItem);
            menuStrip.Items.Add(записиMenuItem);
            menuStrip.Items.Add(отчетыMenuItem);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void ShowDashboard()
        {
            Label lblWelcome = new Label();
            lblWelcome.Text = "Добро пожаловать в систему управления парикмахерской!";
            lblWelcome.Font = new Font("Arial", 16, FontStyle.Bold);
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(50, 50);

            this.Controls.Add(lblWelcome);
        }

        // Обработчики меню
        private void ClientsMenuItem_Click(object sender, EventArgs e)
        {
            ClientsForm clientsForm = new ClientsForm(dbHelper);
            clientsForm.MdiParent = this;
            clientsForm.Show();
        }

        private void EmployeesMenuItem_Click(object sender, EventArgs e)
        {
            EmployeesForm employeesForm = new EmployeesForm(dbHelper);
            employeesForm.MdiParent = this;
            employeesForm.Show();
        }

        private void ServicesMenuItem_Click(object sender, EventArgs e)
        {
            ServicesForm servicesForm = new ServicesForm(dbHelper);
            servicesForm.MdiParent = this;
            servicesForm.Show();
        }

        private void NewAppointmentMenuItem_Click(object sender, EventArgs e)
        {
            AppointmentForm appointmentForm = new AppointmentForm(dbHelper);
            appointmentForm.MdiParent = this;
            appointmentForm.Show();
        }

        private void AppointmentsMenuItem_Click(object sender, EventArgs e)
        {
            AppointmentsJournalForm appointmentsForm = new AppointmentsJournalForm(dbHelper);
            appointmentsForm.MdiParent = this;
            appointmentsForm.Show();
        }

        private void RevenueReportMenuItem_Click(object sender, EventArgs e)
        {
            RevenueReportForm reportForm = new RevenueReportForm(dbHelper);
            reportForm.MdiParent = this;
            reportForm.Show();
        }
    }
}