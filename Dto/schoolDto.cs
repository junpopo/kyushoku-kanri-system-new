using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    class schoolDto
    {
        public static int Id { get; set; }
        public static int Year { get; set; }
        public static string SchoolName { get; set; } = string.Empty;
        public string Charger { get; set; } = string.Empty;
        public int Cost { get; set; }

        public const string IniSchoolSet =
            "'id' INTEGER NOT NULL," +
            "'year' INTEGER NOT NULL," +
            "'schoolName' TEXT NOT NULL," +
            "'charger' TEXT NOT NULL," +
            "'cost' INTEGER NOT NULL";

        public static　string InsertSchoolSql =
         $"INSERT INTO {SystemData.schoolTable} VALUES" +
         "(1,2025,'給食中学校','給食太郎',342)";

        private SQLiteUtil _sqlite;
        public void SetSchoolDto()
        {
            string sql = $"SELECT {SystemData.schoolTable}.*";            

            var dt = _sqlite.ExecuteReader(sql);

            Year = int.Parse(dt.Rows[0][1].ToString());
            SchoolName = (string)dt.Rows[0][2];
            Charger = (string)dt.Rows[0][3];
            Cost = int.Parse(dt.Rows[0][4].ToString());
        }

        public void  Insert(int year,string schoolName, string charger,int cost)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"INSERT INTO {SystemData.schoolTable} VALUES" +
                   $"(1,{year},'{schoolName}','{charger}',{cost})";
            _sqlite.ExecuteNoneQuery(sql);
        }

        public void Update(int year, string schoolName, string charger, int cost) 
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"UPDATE {SystemData.schoolTable} SET" +
                   $" year = {year}," +
                   $" schoolName = '{schoolName}'," +
                   $" charger = '{charger}'," +
                   $" cost = {cost}" +
                   $" WHERE id = 1";
                   ;
            _sqlite.ExecuteNoneQuery(sql);
        }
            
    }
}
