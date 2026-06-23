using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム.Account
{
    class UserManager
    {
        string _sql;
        private SQLiteUtil _sqlite;
        List<string> arrList = null;
        string[] arr;

        public void Manage(object title)
        {
            AccountFixForm accountFixForm = new AccountFixForm(title);

            DialogResult drRet = accountFixForm.ShowDialog();

            try
            {
                _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
                arrList = new List<string>();
                if (drRet == DialogResult.OK)//新規登録
                {
                    //user数をカウント                    
                    _sql = $"SELECT COUNT(*) FROM {SystemData.userTable}";
                    var i = _sqlite.ExecuteScalar(_sql);
                    var cnt = int.Parse(i.ToString());
                    //アカウント新規登録
                    int grade = 0;//学年
                    if(accountFixForm.comboBox10.SelectedIndex != -1)
                    {
                        grade = int.Parse(accountFixForm.textBox23.Text);
                    }
                    string className = string.Empty;
                    if (accountFixForm.comboBox11.SelectedIndex != -1)//クラス
                    {
                        className = accountFixForm.textBox24.Text;
                    }
                    int number = 0;
                    if (accountFixForm.comboBox12.SelectedIndex != -1)//番号
                    {
                        number = int.Parse(accountFixForm.textBox25.Text);
                    }

                    
                    var stopLunch = accountFixForm.checkBox2.Checked ? 0 : 1;
                    var mon = accountFixForm.comboBox2.SelectedIndex == 1 ? 0 : 1;
                    var tue = accountFixForm.comboBox3.SelectedIndex == 1 ? 0 : 1;
                    var wed = accountFixForm.comboBox5.SelectedIndex == 1 ? 0 : 1;
                    var thu = accountFixForm.comboBox4.SelectedIndex == 1 ? 0 : 1;
                    var fri = accountFixForm.comboBox6.SelectedIndex == 1 ? 0 : 1;
                    var milk = accountFixForm.comboBox7.SelectedIndex == 1 ? 0 : 1;
                    var allergy = accountFixForm.checkBox5.Checked ? 0 : 1;
                    
                    var absent = accountFixForm.checkBox1.Checked ? 0 : 1;

                    _sql = UserDto.IniInsertUser(
                        accountFixForm.comboBox9.SelectedIndex,//category
                        accountFixForm.textBox11.Text,accountFixForm.textBox2.Text,//firstName,secondName                                                                                    
                        grade, className, number,//grade,class,number                                       
                        accountFixForm.comboBox1.SelectedIndex,accountFixForm.textBox19.Text, accountFixForm.textBox18.Text,//LocationFirstMoveIn,LocationFirstMoveOut
                        stopLunch,accountFixForm.textBox15.Text, accountFixForm.textBox14.Text,//StopLunchMoveInStopLunchMoveOut
                        mon, tue, wed, thu, fri,                        
                        milk, accountFixForm.textBox17.Text, accountFixForm.textBox17.Text,//milk
                        allergy,//allergy
                        accountFixForm.textBox12.Text,//moveIn
                        accountFixForm.textBox13.Text,//moveOut
                        accountFixForm.textBox10.Text,//remark
                        absent);//欠席

                    
                    _sqlite.ExecuteScalar(_sql);

                    //personalCalendarに登録
                    CalendarDto calendarDto = new CalendarDto();
                    calendarDto.InsertPersonalCalender();

                }
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}
