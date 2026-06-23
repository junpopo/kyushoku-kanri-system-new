using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    public class systemDto
    {

        private static SQLiteUtil _sqlite;

        public static string SelectSql()
        {
            return
                $"" +
                $"" +
                $"";
        }
        public static void MakeTables()
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            //管理関係

            //学校テーブル
            _sqlite.CreateTable(SystemData.schoolTable, schoolDto.IniSchoolSet, SystemData.primayKey, SystemData.autoincrement);
            ////配膳場所テーブル
            _sqlite.CreateTable(SystemData.localTable, LocalDto.IniLocalSet, SystemData.primayKey, SystemData.autoincrement);
            ////学期テーブル
            _sqlite.CreateTable(SystemData.TermTable, TermDto.IniTermSet, SystemData.primayKey, SystemData.autoincrement);
            //ユーザーテーブル
            _sqlite.CreateTable(SystemData.userTable, UserDto.IniUserSet, SystemData.primayKey, SystemData.autoincrement);
            //マスターカレンダーテーブル　※日付のみのカレンダー
            _sqlite.CreateTable(SystemData.calendarTable, CalendarDto.IniCalendarSet, SystemData.primayKey, SystemData.autoincrement);
            ////休日カレンダーテーブル
            _sqlite.CreateTable(SystemData.holidaysTable, CalendarDto.IniHolidaysSet, SystemData.primayKey, SystemData.autoincrement);
            ////baseカレンダーテーブル　※マスターカレンダーに休日を加えたカレンダー　これが基本になる
            _sqlite.CreateTable(SystemData.baseCalendarTable, CalendarDto.IniBaseCalenderSet, SystemData.primayKey, SystemData.autoincrement);
            ////ユーザーカレンダーテーブル
            _sqlite.CreateTable(SystemData.personalCalendarTable, CalendarDto.IniPersonalCalenderSet, SystemData.primayKey, SystemData.autoincrement);

        }

        public void InsertTableData()
        {
            string sql;
            //学校テーブル
            sql = schoolDto.InsertSchoolSql;
            _sqlite.ExecuteNoneQuery(sql);
            //配膳場所テーブル
            sql = LocalDto.InsertLocalSql;
            _sqlite.ExecuteNoneQuery(sql);
            //学期テーブル
            sql = TermDto.InsertTermSql;
            _sqlite.ExecuteNoneQuery(sql);
            //ユーザーテーブル
            //sql = UserDto.InsertUserSql;
            //_sqlite.ExecuteNoneQuery(sql);

            
            ///マスターカレンダーテーブルに日付挿入
            var calendarMaster = new CalendarDto();
            sql = calendarMaster.InsertDateOnMasterCalender();
            _sqlite.ExecuteNoneQuery(sql);
            ///ベースカレンダーに日付挿入         
            sql = calendarMaster.InsertDateOnDaseCalender(SystemData.baseCalendarTable);
            _sqlite.ExecuteNoneQuery(sql);
        }

        public void Init()
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            var result = _sqlite.Exists(SystemData.userTable);

            try
            {
                if (result) {
                    var sql = systemDto.SelectSql();

                    var dataTable = _sqlite.GetData(sql);
                }
            }
            catch { }
        }


    }

}


