using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 給食管理システム.Sql;

namespace 給食管理システム.Dto
{
    public class SystemData
    {
        private SQLiteUtil _sqlite;
        public static string RootDir { get; set; }
        
        public static string SQLITE_DB { get; set; }

        //databaseの場所
        public static string dataBaseDir { get; set; }        
        public const string UpdateDir = @"\Update\";
        public const string ReferenceDir = @"\reference\";
        public const string BackUpDir = @"\backUp\";
        public const string UpdateExeDir = @"\upExe\";
        public const string requestDir = @"\requestlFile";
        public const string NewAssemblyFileVersionTxt = @"NewAssemblyFile.txt";
        public const string databaseName = @"databaseName.ini";

        public static List<string>? NewAssemblyFileVersion = null;

        public static string newAssemblyFileVersion { get; set; }
        /// <suMary>
        /// アップデート確認用アセンブリファイル
        /// </suMary>
        public const string Root_UpdateFileName = UpdateDir + NewAssemblyFileVersionTxt;
        /// <suMary>
        /// id
        /// </suMary>
        public const string primayKey = "id";
        /// <suMary>
        /// id
        /// </suMary>
        public const string autoincrement = "AUTOINCREMENT";

        ///<suMary>
        ///現在管理者として実行されているか
        ///</suMary>
        public static bool IsAdmin { get; set; }

        /// <suMary>
        /// 学校設定
        /// </suMary>
        public const string schoolTable = "schoolSet";
        public static DataTable? schoolSetDataTable;
        ///
        ///ユーザー
        ///
        public static List<UserDto>? userList = null;
        public const string userTable = "users";
        public static DataTable usersDataTable;
        ///

        /// <suMary>
        /// 配膳場所
        /// </suMary>
        public const string placesTable = "places";
        public static List<string>? placesList = null;
        /// <suMary>
        /// 分類
        /// </suMary>
        public const string categoryTable = "category";
        public static List<string>? categoryList = null;
        /// <suMary>
        /// 配膳場所
        /// </suMary>
        public const string localTable = "local";
        public static List<string>? localList = null;
        ///
        ///学期
        ///
        public const string TermTable = "term";
        public static DataTable termDataTable;

        /// <suMary>
        /// マスターカレンダーテーブル
        /// </suMary>
        public const string calendarTable = "calendarMaster";
        /// <suMary>
        /// baseカレンダーテーブル
        /// </suMary>
        public const string baseCalendarTable = "baseCalendarMaster";
        /// <suMary>
        /// 祝日テーブル
        /// </suMary>
        public const string holidaysTable = "holidays";
        /// <suMary>
        /// 個人用カレンダーテーブル
        /// </suMary>
        public const string personalCalendarTable = "personalCalender";

       public void Init()
        {
            _sqlite = new SQLiteUtil(SQLITE_DB);

            SetLocalList();
            SetTermDataTable();
            SetSchoolSetDataTable();
            SetUsersDataTable();
        } 


        private void SetLocalList()
        {
            string sql = $"SELECT id,name FROM {SystemData.localTable}";

            var dt = _sqlite.ExecuteReader(sql);

            localList = new List<string>();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    localList.Add(dt.Rows[i][1].ToString());
                }
            }
        }

        private void SetTermDataTable()
        {
            string sql = $"SELECT * FROM {SystemData.TermTable}";

            var dt = _sqlite.ExecuteReader(sql);

            termDataTable = new DataTable();

            if (dt.Rows.Count > 0)
            {
                termDataTable = dt as DataTable;
            }
        }

        private void SetSchoolSetDataTable()
        {
            string sql = $"SELECT * FROM {SystemData.schoolTable}";

            var dt = _sqlite.ExecuteReader(sql);

            schoolSetDataTable = new DataTable();

            if (dt.Rows.Count > 0)
            {
                schoolSetDataTable = dt as DataTable;
                
            }
        }

        private void SetUsersDataTable()
        {
            string sql = $"SELECT * FROM {SystemData.userTable}";

            var dt = _sqlite.ExecuteReader(sql);

            usersDataTable = new DataTable();

            if (dt.Rows.Count > 0)
            {
                usersDataTable = dt as DataTable;

            }
        }

    }
}
