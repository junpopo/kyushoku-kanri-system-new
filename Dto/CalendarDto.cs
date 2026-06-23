using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    class CalendarDto
    {
        string _sql;
        private SQLiteUtil _sqlite;
        DataTable _DT;
        public static int Id { get; set; }
        public static string Dates { get; set; } = string.Empty;
        public static string dayOfWeek { get; set; } = string.Empty;
        public static string Holiday { get; set; } = string.Empty;
        public static string DayOff { get; set; } = string.Empty;
        public static string Remark { get; set; } = string.Empty;

        public static string IniCalendarSet =
            $"'id' INTEGER NOT NULL," +
            $"'dates' TEXT NOT NULL," +
            $"'dayOfWeek' TEXT NOT NULL," +
            $"'dayOff' TEXT";

        public static string IniHolidaysSet =
            $"'id' INTEGER NOT NULL," +
            $"'day' TEXT NOT NULL," +
            $"'holiday' TEXT NOT NULL";

        public static string IniBaseCalenderSet =
            $"'id' INTEGER NOT NULL," +
            $"'dates' TEXT NOT NULL," +
            $"'dayOfWeek' TEXT NOT NULL," +
            $"'holiday' TEXT," +
            $"'dayOff' TEXT NOT NULL," +
            $"'lunch' TEXT NOT NULL," +
            $"'remark' TEXT";

        public static string IniPersonalCalenderSet =
            $"'id' INTEGER NOT NULL," +
            $"'users_id' INTEGER NOT NULL," +
            $"'dates' TEXT NOT NULL," +
            $"'dayOfWeek' TEXT NOT NULL," +
            $"'holiday' TEXT," +
            $"'dayOff' TEXT NOT NULL," +
            $"'lunch' TEXT NOT NULL," +
            $"'milk' TEXT NOT NULL," +
            $"'remark' TEXT";

        public string InsertDateOnMasterCalender()
        {
            int[] month = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };

            string thisYear = (schoolDto.Year).ToString();
            string nextYear = (schoolDto.Year + 1).ToString();
            _sql =
                    $"INSERT INTO {SystemData.calendarTable}" +
                    $"(dates,dayOfWeek,dayOff) VALUES";

            for (int i = 0; i < month.Length; i++)
            {
                string mounth = month[i].ToString();

                if (month[i] == 1 || month[i] == 2 || month[i] == 3)
                {
                    thisYear = nextYear;
                }

                int day = 0;
                if (month[i] == 4) { day = 30; }
                else if (month[i] == 5) { day = 31; }
                else if (month[i] == 6) { day = 30; }
                else if (month[i] == 7) { day = 31; }
                else if (month[i] == 8) { day = 31; }
                else if (month[i] == 9) { day = 30; }
                else if (month[i] == 10) { day = 31; }
                else if (month[i] == 11) { day = 30; }
                else if (month[i] == 12) { day = 31; }
                else if (month[i] == 1) { day = 31; }
                else if (month[i] == 3) { day = 31; }

                if (month[i] == 2)
                {
                    if (DateTime.IsLeapYear(int.Parse(thisYear)))
                    {
                        day = 29;
                    }
                    else
                    {
                        day = 28;
                    }
                }

                for (int x = 1; x <= day; x++)
                {
                    var oneDay = DateTime.Parse(thisYear + "/" + month[i] + "/" + x);

                    var oneDayWeek = oneDay.DayOfWeek.ToString();

                    string dayOff;
                    if (oneDay.DayOfWeek == DayOfWeek.Saturday || oneDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dayOff = "true";
                    }
                    else
                    {
                        dayOff = "false";
                    }

                    string oneDay_string = oneDay.ToString("yyyy/M/d");

                    if (i == month.Length - 1 && x == day)
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}')";
                    }
                    else
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}'),";
                    }

                }

            }
            return _sql;
        }

        public string InsertDateOnMasterCalenderPublic(int year)
        {
            int[] month = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };

            string thisYear = (year).ToString();
            string nextYear = (year + 1).ToString();
            _sql =
                    $"INSERT INTO {SystemData.calendarTable}" +
                    $"(dates,dayOfWeek,dayOff) VALUES";

            for (int i = 0; i < month.Length; i++)
            {
                string mounth = month[i].ToString();

                if (month[i] == 1 || month[i] == 2 || month[i] == 3)
                {
                    thisYear = nextYear;
                }

                int day = 0;
                if (month[i] == 4) { day = 30; }
                else if (month[i] == 5) { day = 31; }
                else if (month[i] == 6) { day = 30; }
                else if (month[i] == 7) { day = 31; }
                else if (month[i] == 8) { day = 31; }
                else if (month[i] == 9) { day = 30; }
                else if (month[i] == 10) { day = 31; }
                else if (month[i] == 11) { day = 30; }
                else if (month[i] == 12) { day = 31; }
                else if (month[i] == 1) { day = 31; }
                else if (month[i] == 3) { day = 31; }

                if (month[i] == 2)
                {
                    if (DateTime.IsLeapYear(int.Parse(thisYear)))
                    {
                        day = 29;
                    }
                    else
                    {
                        day = 28;
                    }
                }

                for (int x = 1; x <= day; x++)
                {
                    var oneDay = DateTime.Parse(thisYear + "/" + month[i] + "/" + x);

                    var oneDayWeek = oneDay.DayOfWeek.ToString();

                    string dayOff;
                    if (oneDay.DayOfWeek == DayOfWeek.Saturday || oneDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dayOff = "true";
                    }
                    else
                    {
                        dayOff = "false";
                    }

                    string oneDay_string = oneDay.ToString("yyyy/M/d");

                    if (i == month.Length - 1 && x == day)
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}')";
                    }
                    else
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}'),";
                    }

                }

            }
            return _sql;
        }
        public string InsertUpDateOnDaseCalender(int year)
        {
            int[] month = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };

            string thisYear = year.ToString();
            string nextYear = (year + 1).ToString();
            _sql =
                    $"INSERT INTO {SystemData.baseCalendarTable}" +
                    $"(dates,dayOfWeek,dayOff,lunch) VALUES";

            for (int i = 0; i < month.Length; i++)
            {
                string mounth = month[i].ToString();

                if (month[i] == 1 || month[i] == 2 || month[i] == 3)
                {
                    thisYear = nextYear;
                }

                int day = 0;
                if (month[i] == 4) { day = 30; }
                else if (month[i] == 5) { day = 31; }
                else if (month[i] == 6) { day = 30; }
                else if (month[i] == 7) { day = 31; }
                else if (month[i] == 8) { day = 31; }
                else if (month[i] == 9) { day = 30; }
                else if (month[i] == 10) { day = 31; }
                else if (month[i] == 11) { day = 30; }
                else if (month[i] == 12) { day = 31; }
                else if (month[i] == 1) { day = 31; }
                else if (month[i] == 3) { day = 31; }

                if (month[i] == 2)
                {
                    if (DateTime.IsLeapYear(int.Parse(thisYear)))
                    {
                        day = 29;
                    }
                    else
                    {
                        day = 28;
                    }
                }

                for (int x = 1; x <= day; x++)
                {
                    var oneDay = DateTime.Parse(thisYear + "-" + month[i] + "-" + x);

                    var oneDayWeek = oneDay.DayOfWeek.ToString();

                    var lunch = "true";

                    string dayOff;
                    if (oneDay.DayOfWeek == DayOfWeek.Saturday || oneDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dayOff = "true";
                        lunch = "false";
                    }
                    else
                    {
                        dayOff = "false";
                        
                    }

                    string oneDay_string = oneDay.ToString("yyyy/M/d");

                    if (i == month.Length - 1 && x == day)
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}','{lunch}')";
                    }
                    else
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}','{lunch}'),";
                    }

                }

            }
            return _sql;
        }

        public string InsertDateOnDaseCalender(string table)
        {
            int[] month = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };

            string thisYear = schoolDto.Year.ToString();
            string nextYear = schoolDto.Year + 1.ToString();
            _sql =
                    $"INSERT INTO {table}" +
                    $"(dates,dayOfWeek,dayOff) VALUES";

            for (int i = 0; i < month.Length; i++)
            {
                string mounth = month[i].ToString();

                if (month[i] == 1 || month[i] == 2 || month[i] == 3)
                {
                    thisYear = nextYear;
                }

                int day = 0;
                if (month[i] == 4) { day = 30; }
                else if (month[i] == 5) { day = 31; }
                else if (month[i] == 6) { day = 30; }
                else if (month[i] == 7) { day = 31; }
                else if (month[i] == 8) { day = 31; }
                else if (month[i] == 9) { day = 30; }
                else if (month[i] == 10) { day = 31; }
                else if (month[i] == 11) { day = 30; }
                else if (month[i] == 12) { day = 31; }
                else if (month[i] == 1) { day = 31; }
                else if (month[i] == 3) { day = 31; }

                if (month[i] == 2)
                {
                    if (DateTime.IsLeapYear(int.Parse(thisYear)))
                    {
                        day = 29;
                    }
                    else
                    {
                        day = 28;
                    }
                }

                for (int x = 1; x <= day; x++)
                {
                    var oneDay = DateTime.Parse(thisYear + "-" + month[i] + "-" + x);

                    var oneDayWeek = oneDay.DayOfWeek.ToString();

                    string dayOff;
                    if (oneDay.DayOfWeek == DayOfWeek.Saturday || oneDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dayOff = "true";
                    }
                    else
                    {
                        dayOff = "false";
                    }

                    string oneDay_string = oneDay.ToString("yyyy/M/d");

                    if (i == month.Length - 1 && x == day)
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}')";
                    }
                    else
                    {
                        _sql += $"('{oneDay_string}','{oneDayWeek}','{dayOff}'),";
                    }

                }

            }
            return _sql;
        }

        public static string SelectBaseCalenderJointHolidaysSql(string table)
        {
            string sql =
                $"SELECT {table}.id,{table}.dates,{table}.dayOfWeek, {SystemData.holidaysTable}.holiday,{table}.dayOff,{table}.lunch,{table}.remark" +
                $" FROM {table}" +                
                $" LEFT OUTER JOIN {SystemData.holidaysTable}" +
                $" ON {SystemData.holidaysTable}.day = {table}.dates";

            return sql;
        }

        public static string UpdateBaseCalenderInsertHolidays()
        {
            string sql =
                $"UPDATE {SystemData.baseCalendarTable} SET" +
                $" holiday = (SELECT {SystemData.holidaysTable}.holiday" +
                $" FROM {SystemData.holidaysTable}" +
                $" WHERE {SystemData.baseCalendarTable}.dates = {SystemData.holidaysTable}.day)";

            return sql;
        }
        
        public static string UpdateBaseCalender(string table)
        {
            string sql =
                $"UPDATE {table} SET" +
                $" dayOff = 'true'," +
                $" lunch = 'false'" +
                $" WHERE holiday != ''";
            return sql;
        }
        public static string SelectSql(string table)
        {
            string sql =
                $"SELECT *" +
                $" FROM {table}";

            return sql;
        }

        public string LoadMonth(DateTime month, TableLayoutPanel _table)
        {
            ClearAllDayButtons(_table);

            SQLiteUtil _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            // DBから base_calendar を読み込み（当月分）
            var sql = $"SELECT * FROM {SystemData.baseCalendarTable} WHERE dates LIKE '{month:yyyy/M}%' ORDER BY dates";
            var dt = _sqlite.GetData(sql);

            // カレンダーの初日の曜日オフセット（例：月曜日=0）
            DateTime firstDay = new DateTime(month.Year, month.Month, 1);
            int offset = (((int)firstDay.DayOfWeek + 6) % 7); // .NET: Sunday=0 -> convert so Monday=0, Sunday=6

            // dt にない日（未登録）も含めて 1..daysInMonth を表示
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                int pos = offset + (day - 1); // 0-based position in 0..41
                int index = pos + 1;          // DayBtn index

                var btn = _table.Controls.OfType<Button>().FirstOrDefault(b => b.Name == $"DayBtn{index}");
                if (btn == null) continue;

                string dateStr = new DateTime(month.Year, month.Month, day).ToString("yyyy/M/d");

                // DB ロー取得
                var row = dt.AsEnumerable().FirstOrDefault(r => r["dates"].ToString() == dateStr);

                string holiday = "";
                string remark = "";
                string dayoff = "false";
                string lunch = "";

                if (row != null)
                {
                    holiday = row["holiday"].ToString();
                    remark = row["remark"].ToString();
                    dayoff = row["dayoff"].ToString();
                    lunch = row["lunch"].ToString();
                }
                else
                {
                    // 祝日テーブルチェック（あるなら表示）
                    var h = _sqlite.GetData($"SELECT holiday FROM {SystemData.holidaysTable} WHERE day = '{dateStr}'");
                    if (h.Rows.Count > 0) holiday = h.Rows[0]["holiday"].ToString();
                }

                // 色分け：休日・土日
                var dow = new DateTime(month.Year, month.Month, day).DayOfWeek;
                if (dayoff == "true")
                {
                    btn.BackColor = Color.FromArgb(192, 192, 220);
                }
                else if (dow == DayOfWeek.Saturday)
                {
                    btn.BackColor = Color.LightCoral;
                }
                else if (dow == DayOfWeek.Sunday)
                {
                    btn.BackColor = Color.LightCoral;
                }
                else
                {
                    btn.BackColor = SystemColors.Control;
                }

                // テキスト：日 + 絵文字アイコン
                string icon = "";
                if(lunch == "true") icon += "〇";
                else icon += "✕";

                //if (!string.IsNullOrEmpty(holiday)) icon += "祝";


                btn.Text = $"{day}\n{icon}\n{holiday}";
                btn.Tag = dateStr; // クリック時に日付が分かるように置く

                
            }
            return month.ToString("yyyy年M月");
        }

        private void ClearAllDayButtons(TableLayoutPanel _table)
        {
            foreach (var b in _table.Controls.OfType<Button>().Where(x => x.Name.StartsWith("DayBtn")))
            {
                b.Text = "";
                b.BackColor = SystemColors.Control;
                // Tag を残す必要ない場合はクリア
                b.Tag = null;
            }
        }
        /// <summary>
        /// １人毎PersonalCalender登録
        /// </summary>
        public void InsertPersonalCalender()
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            _sql = $"SELECT {SystemData.userTable}.id FROM {SystemData.userTable}" +
                   $" WHERE " +
                   $" NOT EXISTS( SELECT {SystemData.personalCalendarTable}.* FROM {SystemData.personalCalendarTable}" +
                   $" WHERE" +
                   $" {SystemData.personalCalendarTable}.users_id = {SystemData.userTable}.id)";

            _DT = _sqlite.GetData(_sql);

            _sql = CalendarDto.SelectSql(SystemData.baseCalendarTable);


            if (_DT.Rows.Count > 0)
            {
                var baseDataTable = _sqlite.GetData(_sql);
                baseDataTable.Columns.Add("users_id", typeof(int)).SetOrdinal(1);

                foreach (var row in _DT.Rows)
                {
                    var oneRow = row as DataRow;

                    _sql = $"INSERT INTO {SystemData.personalCalendarTable}" +
                           $"(users_id, dates, dayOfWeek, holiday, dayOff, lunch, milk, remark) VALUES";

                    foreach (var Row in baseDataTable.AsEnumerable())
                    {
                        Row["users_id"] = oneRow.ItemArray[0];

                        var users_id = Row.ItemArray[1];
                        var dates = Row.ItemArray[2];
                        var dayOfWeek = Row.ItemArray[3];
                        var holiday = Row.ItemArray[4];
                        var dayOff = Row.ItemArray[5];
                        var lunch = Row.ItemArray[6];
                        var milk = Row.ItemArray[7];
                        var remark = Row.ItemArray[8];

                        if (Row == baseDataTable.AsEnumerable().Last())
                        {
                            _sql += $"({users_id},'{dates}','{dayOfWeek}','{holiday}','{dayOff}','{lunch}','{milk}','{remark}')";
                        }
                        else
                        {
                            _sql += $"({users_id},'{dates}','{dayOfWeek}','{holiday}','{dayOff}','{lunch}','{milk}','{remark}'),";
                        }
                    }

                    _sqlite.ExecuteNoneQuery(_sql);

                }
            }
        }

        //public void InsertIniPersonalCalender(string id,)
        //{

        //}
    }
}
