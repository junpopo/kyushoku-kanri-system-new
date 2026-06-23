using Microsoft.VisualBasic.Logging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using 給食管理システム.Account;
using 給食管理システム.Dto;
using 給食管理システム.Models.Forms;
using 給食管理システム.Sql;

namespace 給食管理システム
{
    internal static class Program
    {
        public static bool DoContinue = false;
        private static SQLiteUtil _sqlite;
        public static bool admin = false;
        /// <suMary>
        ///  The main entry point for the application.
        /// </suMary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.


            Mutex app_mutex = new Mutex(false, "MYSOFTWARW_001");

            if (app_mutex.WaitOne(0, false) == false)
            {
                MessageBox.Show("このアプリケーションは複数起動ができません");
                Application.Exit();
                return;
            }


            try
            {
                string appDomain = AppDomain.CurrentDomain.BaseDirectory + "config.ini";
                StreamReader sr;
                if (File.Exists(appDomain))
                {
                    // config.ini の内容を読み込んで、ルートディレクトリ（大元のディレクトリ）の位置を格納する変数に代入
                    sr = new StreamReader(appDomain);
                    SystemData.RootDir = sr.ReadLine();
                }
                else
                {
                    MessageBox.Show("ルートディレクトリ設定がありません。config.iniに、ルートを設定ください。");
                    Application.Exit();
                    return;
                }


                sr.Close();


                //ネットワークor端末にデータベースがあるか判別
                var datbaseAppDomain = AppDomain.CurrentDomain.BaseDirectory + @"\NetworkDatabaseName.ini";
                if (File.Exists(datbaseAppDomain))
                {
                    sr = new StreamReader(datbaseAppDomain);
                    SystemData.SQLITE_DB = sr.ReadLine();

                }
                else if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\NoNetworkDatabaseName.ini"))
                {
                    sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\NoNetworkDatabaseName.ini");
                    SystemData.SQLITE_DB = sr.ReadLine();
                    
                }
                else
                {
                    InitialForm InitialForm = new InitialForm();
                    DialogResult result = InitialForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        //チェック
                        if (InitialForm.textBox1.Text != "")
                        {
                            MessageBox.Show("データベース名が未入力です。");
                            return;
                        }
                        if (InitialForm.textBox2.Text != "")
                        {
                            MessageBox.Show("保存先が選択できていません。");
                            return;
                        }
                        if (InitialForm.textBox3.Text != "")
                        {
                            MessageBox.Show("年度が未入力です。");
                            return;
                        }


                        //InitialForm.textBox2.Textの先頭に\\があるなら（ネットワーク先）ならNetworkDatabaseName.iniを作成

                        var target = InitialForm.textBox2.Text;

                        var check = Regex.Match(target, @"^\{1}.*");
                        string path;
                        string text = SystemData.RootDir + @"\" + InitialForm.textBox1.Text;
                        if (check.Success)
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + @"\NetworkDatabaseName.ini";
                            using (StreamWriter file = new StreamWriter(path, false, Encoding.GetEncoding("shift-jis")))
                            { file.WriteLine(text); }
                        }
                        else
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + @"\NoNetworkDatabaseName.ini";
                            using (StreamWriter file = new StreamWriter(path, false, Encoding.GetEncoding("shift-jis")))
                            { file.WriteLine(text); }
                        }


                        //データベースをつくる
                        _sqlite = new SQLiteUtil(InitialForm.textBox2.Text + @$"\{InitialForm.textBox3.Text} _ {InitialForm.textBox1.Text}");
                        _sqlite.CreateDatabase();

                        //using (
                                
                        //    //StreamReader(path);
                        //)

                        //テーブルを作る
                        systemDto.MakeTables();
                        //テーブルにデータを挿入
                        systemDto systemdto = new systemDto();
                        systemdto.InsertTableData();
                    }
                    else
                    {
                        Application.Exit();
                    }


                }
                sr.Close();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Login login = new Login();

                login.ShowDialog();

                if (DoContinue)
                {
                    if (admin)
                    {
                        
                        AdminForm adminForm = new AdminForm();
                        adminForm.ShowDialog();
                        adminForm.Dispose();
                    }
                    else {
                        GuestForm guestForm = new GuestForm();
                        guestForm.ShowDialog();
                        guestForm.Dispose();
                    }                   
                }                
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

           
        }



       
    }
}