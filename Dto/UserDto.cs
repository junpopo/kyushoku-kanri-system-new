using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    public class UserDto
    {
        public  int Id { get; set; } // 主キー
        public  int OrderIn { get; set; }　//並び順
        public  int Category { get; set; }  // 分類　public static readonly string[] CategoryArry
        public  string FirstName { get; set; } = ""; // 氏
        public string SecondName { get; set; } = ""; // 名
        public  int Grade { get; set; } //年
        public  string Class { get; set; } = "";　//組
        public  int Number { get; set; }　//番号
        public  string LocationFirst { get; set; } = ""; // 配膳場所1
        public DateTime LocationFirstMoveIn { get; set; }//配膳場所1開始日
        public DateTime LocationFirstMoveOut { get; set; }//配膳場所1終了日
        public string LocationSecond { get; set; } = ""; // 配膳場所2
        public DateTime LocationSecondMoveIn { get; set; }//配膳場所2開始日
        public DateTime LocationSecondMoveOut { get; set; }//配膳場所2終了日        
        public int StopLunch { get; set; }//給食停止
        public DateTime StopLunchMoveIn { get; set; }//給食停止開始日
        public DateTime StopLunchMoveOut { get; set; }//給食停止終了日
        public int Mon { get; set; } //月曜日
        public int Tue { get; set; } //火曜日
        public int Wed { get; set; } //水曜日
        public int Thu { get; set; } //木曜日
        public int Fri { get; set; } //金曜日
        public int Milk { get; set; } //牛乳
        public DateTime MilkMoveIn { get; set; }//牛乳停止開始日
        public DateTime MilkMoveOut { get; set; }//牛乳停止終了日
        public int Allergy { get; set; } //アレルギー対応
        public  DateTime MoveIn { get; set; }//給食開始日
        public  DateTime MoveOut { get; set; }//給食終了日
        public  string Remark { get; set; } = "";//備考        
        public int Absent { get; set; } //不登校
        public int Deleted { get; set; } //削除フラグ

        public static readonly string[] CategoryArry = {"職員", "生徒", "ALT", "教育実習生", "給食試食会", "ゲスト" };

        
         
        private SQLiteUtil? _sqlite;

        public static string IniUserSet =
            "'id' INTEGER NOT NULL," +
            "'order' INTEGER," +
            "'category' INTEGER NOT NULL," +
            "'firstName' TEXT NOT NULL," +
            "'secondName' TEXT NOT NULL," +
            "'grade' INTEGER," +
            "'class' TEXT," +
            "'number' INTEGER," +
            "'LocationFirst' INTEGER NOT NULL," +
            "'LocationFirstMoveIn' TEXT," +
            "'LocationFirstMoveOut' TEXT," +
            "'LocationSecond' TEXT," +
            "'LocationSecondMoveIn' TEXT," +
            "'LocationSecondMoveOut' TEXT," +            
            "'stopLunch' INTEGER NOT NULL," +
            "'StopLunchMoveIn' TEXT," +
            "'StopLunchMoveOut' TEXT," +
            "'mon' INTEGER NOT NULL," +
            "'tue' INTEGER NOT NULL," +
            "'wed' INTEGER NOT NULL," +
            "'thu' INTEGER NOT NULL," +
            "'fri' INTEGER NOT NULL," +
            "'milk' INTEGER NOT NULL," +
            "'MilkMoveIn' TEXT," +
            "'MilkMoveOut' TEXT," +
            "'allergy' INTEGER NOT NULL," +
            "'moveIn' TEXT," +
            "'moveOut' TEXT," +
            "'remark' TEXT," +
            "'absent' INTEGER NOT NULL," +
            "'deleted' INTEGER NOT NULL";

        //public static string InsertUserSql =
        // $"INSERT INTO {SystemData.userTable} VALUES" +
        // "(1,1,1,'給食','太郎',,'',,'職員室',1,1,0,0,0,0,0,0,1,'','','',0,1))";

        public static string IniInsertUser(int category,string firstName, string secondName,
            int grade,string classes,int number,
            int LocationFirst, string LocationFirstMoveIn,string LocationFirstMoveOut,
            int stopLunch,string StopLunchMoveIn,string StopLunchMoveOut,
            int mon, int tue, int wed, int thu, int fri, 
            int milk,string MilkMoveIn,string MilkMoveOut,int allergy,
            string moveIn,string moveOut,string remark,int absent)
        {
            var sql = $"INSERT INTO {SystemData.userTable}(" +                
                $"category," +
                $"firstName," +
                $"secondName," +                
                $"grade," +
                $"class," +
                $"number," +
                $"LocationFirst," +
                $"LocationFirstMoveIn," +
                $"LocationFirstMoveOut," +
                $"stopLunch," +
                $"StopLunchMoveIn," +
                $"StopLunchMoveOut," +
                $"mon," +
                $"tue," +
                $"wed," +
                $"thu," +
                $"fri," +
                $"milk," +
                $"MilkMoveIn," +
                $"MilkMoveOut," +
                $"allergy," +
                $"moveIn," +
                $"moveOut," +
                $"remark," +
                $"absent," +
                $"deleted)" +
                $" VALUES(" +
                $"'{category}','{firstName}','{secondName}',{grade},'{classes}',{number}," +
                $"{LocationFirst},'{LocationFirstMoveIn}','{LocationFirstMoveOut}'," +                
                $"{stopLunch},'{StopLunchMoveIn}','{StopLunchMoveOut}'," +
                $"{mon},{tue},{wed},{thu},{fri}," +
                $"{milk},'{MilkMoveIn}','{MilkMoveOut}'," +
                $"{allergy}," +
                $"'{moveIn}','{moveOut}','{remark}',{absent},0)";
                
            return sql;
        }

        public  DataTable  SelectSql()
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            var sql = $"SELECT {SystemData.userTable}.*";

            return  _sqlite.GetData(sql);


        }
    }
}
