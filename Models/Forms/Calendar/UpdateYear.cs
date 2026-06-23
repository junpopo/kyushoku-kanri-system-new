using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Dto;
using 給食管理システム.Models.Forms.Setting;
using 給食管理システム.Sql;

namespace 給食管理システム.Models.Forms.Calendar
{
    public partial class UpdateYear : Form
    {
        string _sql;
        private static SQLiteUtil _sqlite;
        List<string> arrList = null;
        string[] arr;
        public UpdateYear()
        {
            InitializeComponent();
        }

        private void UpdateYear_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text !="")
            {
                DialogResult result = MessageBox.Show($"{textBox1.Text}年にカレンダーを更新しますか？", "重要", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
                        //削除
                        Reset reset = new Reset();
                        reset.ReSet();

                        //更新
                         CalendarDto calendarDto = new CalendarDto();
                        _sql = calendarDto.InsertDateOnMasterCalenderPublic(int.Parse(textBox1.Text));
                        _sqlite.ExecuteNoneQuery(_sql);
                        
                        _sql = calendarDto.InsertUpDateOnDaseCalender(int.Parse(textBox1.Text));                        
                        _sqlite.ExecuteNoneQuery(_sql);

                        MessageBox.Show("更新しました");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }



                }
            }
            else
            {
                MessageBox.Show("対象西暦年度を入力してください");
            }
                
        }
    }
}
