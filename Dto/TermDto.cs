using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    
    class TermDto
    {
        public static int Id { get; set; }
        public static int Grade { get; set; }
        public static string ClassName { get; set; } = string.Empty;
        public static int Term { get; set; } 
        public static string Start { get; set; } = string.Empty;
        public static string End { get; set; } = string.Empty;

        private string _sql = "";
        private SQLiteUtil _sqlite;

        public static string IniTermSet =           
            "'id' INTEGER NOT NULL," +
            "'grade' INTEGER NOT NULL," +
            "'className' TEXT NOT NULL," +
            "'term' INTEGER NOT NULL," +
            "'start' TEXT NOT NULL," +
            "'end' TEXT NOT NULL";

        public static string InsertTermSql =
         $"INSERT INTO {SystemData.schoolTable} VALUES" +
         "(1,1,'A',1,'04-08','07-18')" +
            $"INSERT INTO {SystemData.schoolTable} VALUES" +
         "(2,2,'A',1,'04-08','07-18')" +
            $"INSERT INTO {SystemData.schoolTable} VALUES" +
         "(3,3,'A',1,'04-08','07-18')";

        public object SelectTermGrade(int grade,string className,int term)
        {
            _sql = $"SELECT COUNT(*) FROM {SystemData.TermTable} WHERE grade = {grade} AND className = '{className}' AND term = {term}";
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            return _sqlite.ExecuteScalar(_sql);            
        }

        public void Insert(int grade, string className, int term, string start,string end)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"INSERT INTO {SystemData.TermTable}(grade,className,term,start,end) VALUES" +
                   $"({grade}," +
                   $"'{className}'," +
                   $" {term}," +
                   $"'{start}'," +
                   $"'{end}')";
            _sqlite.ExecuteNoneQuery(sql);
        }

        public void Update(int year, string school, string charger, int cost)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"UPDATE {SystemData.schoolTable} SET" +
                   $" year = {year}," +
                   $" shcool = '{school}'," +
                   $" charger = '{charger}'," +
                   $" cost = {cost}" +
                   $" WHERE id = 1";
            ;
            _sqlite.ExecuteNoneQuery(sql);
        }

        public void TermUpdate(int grade, string className, int term, string start, string end)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"UPDATE {SystemData.TermTable} SET" +
                   $" grade = {grade}," +
                   $" className = '{className}'," +
                   $" term = {term}," +
                   $" start = '{start}'," +
                   $" end = '{end}'" +
                   $" WHERE grade = {grade} AND className = '{className}' AND term = {term}";
            ;
            _sqlite.ExecuteNoneQuery(sql);
        }

        public void Delete(int grade, string className)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
            string sql = $"DELETE FROM {SystemData.TermTable} " +                   
                         $" WHERE grade = {grade} AND className = '{className}'";
            _sqlite.ExecuteNoneQuery(sql);
        }


    }
}
