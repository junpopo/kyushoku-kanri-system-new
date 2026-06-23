using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Account;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム
{
    public partial class AccountForm : Form
    {
        private SQLiteUtil _sqlite;
        private string _sql;
        public AccountForm()
        {
            InitializeComponent();

            SystemData systemData = new SystemData();
            systemData.Init();


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 新規登録
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            UserManager userManager = new UserManager();

            userManager.Manage(sender);
        }

        private void AccountForm_Load(object sender, EventArgs e)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.ReadOnly = false;
            dataGridView1.DataSource = "";


            string tables =
            $" LEFT OUTER JOIN {SystemData.userTable}" +
            $" ON {SystemData.specailTaskTable}.users_id = {SystemData.userTable}.id" +
            $" LEFT OUTER JOIN {SystemData.guidances_specialTask_historiesTable}" +
            $" ON {SystemData.guidances_specialTask_historiesTable}.specialTask_histories_id = {SystemData.specailTaskTable}.id" +
            $" LEFT OUTER JOIN {SystemData.guidancesTable}" +
            $" ON {SystemData.guidancesTable}.id = {SystemData.guidances_specialTask_historiesTable}.guidances_id " +
            $" LEFT OUTER JOIN {SystemData.venues_specialTask_historiesTable}" +
            $" ON {SystemData.venues_specialTask_historiesTable}.specialTask_histories_id = {SystemData.specailTaskTable}.id" +
            $" LEFT OUTER JOIN {SystemData.deadlines_specialTaskTable}" +
            $" ON {SystemData.deadlines_specialTaskTable}.specialTask_histories_id = {SystemData.specailTaskTable}.id" +
            $" LEFT OUTER JOIN {SystemData.transportsTable}" +
            $" ON {SystemData.transportsTable}.specialTask_histories_id = {SystemData.specailTaskTable}.id" +
            $"{app}";


            string sql =
            $"SELECT {SystemData.specailTaskTable}.id AS '番号'," +
            $"{SystemData.userTable}.name AS '氏名'," +
            $"{SystemData.specailTaskTable}.applyDay AS '申請日'," +
            $"{SystemData.specailTaskTable}.startTime AS '開始時刻'," +
            $"{SystemData.specailTaskTable}.endTime AS '終了時刻'," +
            $"{SystemData.guidancesTable}.guide AS '指導内容'," +
            $"{SystemData.venues_specialTask_historiesTable}.venue AS '会場'," +
            $"{SystemData.transportsTable}.isBool AS '校外行事実施届'," +
            $"{fields}" +
            $"{SystemData.deadlines_specialTaskTable}.deadline AS '締め'" +
            $" FROM {SystemData.specailTaskTable}" +
            $"{tables}";

            var dt = _sqlite.GetData(sql);
            dataGridView1.DataSource = dt;


        }
    }
}
