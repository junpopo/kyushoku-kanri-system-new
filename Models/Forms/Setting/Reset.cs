using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム.Models.Forms.Setting
{
    class Reset
    {
        private SQLiteUtil _sqlite;
        private string _sql;
        List<string> arrList = null;
        string[] arr;
        public void ReSet()
        {
            DialogResult result = MessageBox.Show("カレンダー関係のデータベースをリセットします", "重要", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                try
                {
                    var folder = Path.GetDirectoryName(SystemData.SQLITE_DB);
                    

                    _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
                    arrList = new List<string>();

                    _sql = $"PRAGMA foreign_keys=false;";
                    arrList.Add(_sql);
                    _sql = $"DELETE FROM {SystemData.calendarTable};";
                    arrList.Add(_sql);
                    _sql = $"DELETE FROM sqlite_sequence WHERE name = '{SystemData.calendarTable}';";
                    arrList.Add(_sql);

                    _sql = $"DELETE FROM {SystemData.baseCalendarTable};";
                    arrList.Add(_sql);
                    _sql = $"DELETE FROM sqlite_sequence WHERE name = '{SystemData.baseCalendarTable}';";
                    arrList.Add(_sql);

                    _sql = $"DELETE FROM {SystemData.holidaysTable};";
                    arrList.Add(_sql);
                    _sql = $"DELETE FROM sqlite_sequence WHERE name = '{SystemData.holidaysTable}';";
                    arrList.Add(_sql);

                    _sql = $"DELETE FROM {SystemData.personalCalendarTable};";
                    arrList.Add(_sql);
                    _sql = $"DELETE FROM sqlite_sequence WHERE name = '{SystemData.personalCalendarTable}';";
                    arrList.Add(_sql);                                   
                                       
                    arr = arrList.ToArray();

                    _sqlite.ExecuteNoneQueryWithTransaction(arr);

                    MessageBox.Show($"カレンダー関係のデータベースをリセットしました。");

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }



            }
        }
    }
}
