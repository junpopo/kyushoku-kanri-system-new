using System;
using System.Windows.Forms;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム.Models.Forms.Calendar
{
    public partial class DayDetailForm : Form
    {
        private string _date; // yyyy/M/dd
        private SQLiteUtil _sqlite;
        private int _district;

        public bool IsSaved { get; private set; } = false;

        public DayDetailForm(int district, string date)
        {
            InitializeComponent();
            _date = date;
            _district = district;
            label1.Text = DateTime.Parse(date).ToString("MM月d日");
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            LoadData();
        }

        private void LoadData()
        {
            var sql = $@"SELECT * FROM {SystemData.baseCalendarTable} WHERE dates = '{_date}'";
            var dt = _sqlite.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                textBox1.Text = r["holiday"].ToString();
                checkBox1.Checked = r["dayoff"].ToString() == "true" ? true : false;
                checkBox2.Checked = r["lunch"].ToString() == "true" ? true : false;
                textBox2.Text = r["remark"].ToString();
            }
            else
            {
                // 空のときは何もしない（新規）
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var holiday = textBox1.Text;
                var dayoff = checkBox1.Checked ? "true" : "false";
                var lunch = checkBox2.Checked ? "true" : "false";
                var remark = textBox2.Text;

                if (_district == 0)//全員
                {
                    var exists = _sqlite.GetData($"SELECT COUNT(1) cnt FROM {SystemData.baseCalendarTable} WHERE dates = '{_date}'");
                    int cnt = Convert.ToInt32(exists.Rows[0]["cnt"]);
                    if (cnt > 0)
                    {
                        //baseCalendarTableの更新
                        var sql = $"UPDATE {SystemData.baseCalendarTable} SET holiday = '{holiday}', dayoff = '{dayoff}', lunch = '{lunch}', remark = '{remark}' WHERE dates = '{_date}'";
                        _sqlite.ExecuteNoneQuery(sql);
                        //personalCalendarTalbeの更新
                        sql = $"UPDATE {SystemData.personalCalendarTable} SET holiday = '{holiday}', dayoff = '{dayoff}', lunch = '{lunch}', remark = '{remark}' WHERE dates = '{_date}'";
                        _sqlite.ExecuteNoneQuery(sql);
                    }
                    else
                    {
                        // day_name
                        var dow = DateTime.Parse(_date).DayOfWeek.ToString();
                        var sql = $"INSERT INTO {SystemData.baseCalendarTable} (dates, day_name, holiday, dayoff, lunch, remark) VALUES ('{_date}', '{dow}', '{holiday}', '{dayoff}', '{lunch}', '{remark}')";
                        _sqlite.ExecuteNoneQuery(sql);
                        //personalCalendarTalbeの更新
                        sql = $"UPDATE {SystemData.personalCalendarTable} SET holiday = '{holiday}', dayoff = '{dayoff}', lunch = '{lunch}', remark = '{remark}' WHERE dates = '{_date}'";
                        _sqlite.ExecuteNoneQuery(sql);
                    }
                }
                else if (_district == 1) { }
                else if (_district == 2) { }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }
    }
}

