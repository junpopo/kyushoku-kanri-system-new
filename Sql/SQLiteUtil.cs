using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 給食管理システム.Sql
{
    public class SQLiteUtil
    {
        //接続文字列
        public string ConnectString { get; set; }
        //データベースファイルのフルパス
        public string SQLitePath { get; set; }

        /// <suMary>
        /// コンストラクタ
        /// </suMary>
        /// <param name="path"></param>
        public SQLiteUtil(string path)
        {
            SQLitePath = path;
            ConnectString = "Data Source = " + SQLitePath;
        }

        /// <suMary>
        /// DBファイルの作成（既に存在する場合、中身はクリアされる）
        /// </suMary>
        public void CreateDatabase()
        {
            SQLiteConnection.CreateFile(SQLitePath);
        }

        /// <suMary>
        /// SQLの実行 データの更新又は削除
        /// </suMary>
        /// <param name="sqls"></param>
        public void ExecuteNoneQuery(string sql)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }


                }
            }
        }

        public List<object[]> ObjectExecuteReader(string sql)
        {
            List<object[]> result = new List<object[]>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    //SQLの設定
                    cmd.CommandText = sql;

                    //検索
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] data = Enumerable.Range(0, reader.FieldCount).Select(i => reader[i]).ToArray();
                            result.Add(data);
                        }
                    }
                }
            }
            return result;
        }

        /// <suMary>
        /// スカラーの実行
        /// </suMary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            object result = null;

            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    result = cmd.ExecuteScalar();
                }
            }

            return result;
        }

        /// <suMary>
        /// SQLの実行
        /// </suMary>
        /// <param name="sqls"></param>
        public void ExecuteNoneQueryWithTransaction(string[] sqls)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();
                SQLiteTransaction trans = connection.BeginTransaction();

                try
                {
                    foreach (string sql in sqls)
                    {
                        using (SQLiteCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <suMary>
        /// DataReaderを使ったデータの取得 1行毎処理
        /// </suMary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ExecuteReader(string sql)
        {
            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    //SQLの設定
                    cmd.CommandText = sql;

                    //検索
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        create_columns(dt, reader);

                        while (reader.Read())
                        {
                            object[] data = Enumerable.Range(0, reader.FieldCount).Select(i => reader[i]).ToArray();
                            dt.Rows.Add(dt.NewRow().ItemArray = data);
                        }
                    }
                }
            }
            return dt;

            //列名が重複した場合、列名に連番を付加した上でDataColumnを追加
            void create_columns(DataTable p_dt, SQLiteDataReader p_reader)
            {
                Dictionary<string, int> l_dic = new Dictionary<string, int>();
                for (int i = 0; i < p_reader.FieldCount; i++)
                {
                    string p_name = p_reader.GetName(i);
                    if (l_dic.ContainsKey(p_name))
                    {
                        int p_cnt = l_dic[p_name]++;
                        p_dt.Columns.Add(p_name + p_cnt.ToString());
                    }
                    else
                    {
                        p_dt.Columns.Add(p_name);
                        l_dic.Add(p_name, 1);
                    }
                }
            }
        }

        /// <suMary>
        /// DataAdapterを使ったデータの取得　まとめてデータを取得
        /// </suMary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetData(string sql)
        {
            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;

                    // DataAdapterの生成
                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);

                    // データベースからデータを取得
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <suMary>
        /// DataTableの内容をデータベースに保存
        /// </suMary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable SetData(string tableName, DataTable dt)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectString))
            {
                connection.Open();
                SQLiteTransaction trans = connection.BeginTransaction();

                try
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        //書き込み先テーブルの列名と型を取得するためのSQLをCoMandに登録
                        cmd.CommandText = "select * from " + tableName;

                        // DataAdapterの生成
                        SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);

                        //Insert、Delete、Update　コマンドの自動生成
                        SQLiteCommandBuilder bulider = new SQLiteCommandBuilder(da);

                        string insert = bulider.GetInsertCommand().CommandText;

                        //DataTableの内容をデータベースに書き込む
                        da.Update(dt);

                        trans.Commit();
                    }
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    MessageBox.Show(e.Message);
                    throw;
                }
            }
            return dt;
        }

        /// <suMary>
        /// カラム一覧の取得
        /// </suMary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string[] GetColumnNames(string tableName)
        {
            var dt = GetData("select * from " + tableName + " where 1=2");
            return dt.Columns.Cast<DataColumn>().Select(i => i.ColumnName).ToArray();
        }

        /// <suMary>
        /// カラム情報の取得
        /// </suMary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetColumnInfo(string tableName)
        {
            return GetData("PRAGMA table_info('" + tableName + "')");
        }

        /// <suMary>
        /// テーブル一覧の取得
        /// </suMary>
        /// <returns></returns>
        public string[] GetTableList()
        {
            var dt = GetData("select tbl_name from sqlite_master where type in ('table','view') ");
            return dt.AsEnumerable().Select(i => i[0].ToString()).ToArray();
        }

        /// <suMary>
        /// テーブル／Viewの存在チェック
        /// </suMary>
        /// <param name="tableName"></param>
        /// <returns></returns>198
        public bool Exists(string tableName)
        {
            object val = ExecuteScalar("select count(*) from sqlite_master where type in ('table','view') and name='" + tableName + "'");
            return (int.Parse(val.ToString()) == 0) ? false : true;
        }

        /// <suMary>
        /// テーブルの削除
        /// </suMary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            ExecuteNoneQuery("drop table if exists " + tableName);
        }

        /// <suMary>
        /// 未使用エリアの開放
        /// </suMary>
        public void Vacuum()
        {
            ExecuteNoneQuery("VACUUM");
        }

        /// <suMary>
        /// テーブルの作成
        /// </suMary>
        /// <param name="tableName"></param>
        /// <param name="fieldList"></param>
        /// <param name="primaryKeyList"></param>

        public void CreateTable(string tableName, string fieldList, string primaryKeyList, string autoincrement)
        {
            var primary = (primaryKeyList == "") ? "" : (",PRIMARY KEY(" + $"'{primaryKeyList}' " + $"{autoincrement})");
            var sql = string.Format($"CREATE TABLE {tableName}({fieldList} {primary})");

            ExecuteNoneQuery(sql);
        }


        /// <suMary>
        /// DataTableからカラムと型を推測する
        /// </suMary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public string[] CreateFieldList(DataTable dt)
        {
            List<string> fields = new List<string>();

            foreach (DataColumn dr in dt.Columns)
            {
                int num = dt.AsEnumerable().Count(i => int.TryParse(i[dr.ColumnName].ToString(), out int val1));
                int dbl = dt.AsEnumerable().Count(i => double.TryParse(i[dr.ColumnName].ToString(), out double val2));
                string type = (num == dt.Rows.Count) ? "intEGER" : (dbl == dt.Rows.Count) ? "REAL" : "TEXT";
                fields.Add(dr.ColumnName + " " + type);
            }

            return fields.ToArray();
        }

        public SQLiteDataAdapter ExecuteQueryAdapter(string sql)
        {
            SQLiteDataAdapter Adapter = new SQLiteDataAdapter(sql, ConnectString);
            return Adapter;
        }
    }
}
