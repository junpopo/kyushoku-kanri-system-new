using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム.Models.Forms.Calendar
{
    public partial class ManagerHoliday : Form
    {
        private static SQLiteUtil? _sqlite;
        public ManagerHoliday()
        {
            InitializeComponent();
        }

        private void ManagerHoliday_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetHolidayForm setHolidayForm = new SetHolidayForm();
            setHolidayForm.ShowDialog();
            setHolidayForm.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateYear updateYear = new UpdateYear();
            updateYear.ShowDialog();
            updateYear.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            //マスターカレンダー有無
            var dt = _sqlite.Exists(SystemData.calendarTable);
            if (dt == false)
            {
                MessageBox.Show("祝日設定でカレンダーを作成してください。");
            }
            else
            {
                ManageOderForm manageOderForm = new ManageOderForm();
                manageOderForm.ShowDialog();
                manageOderForm.Dispose();
            }
                
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
