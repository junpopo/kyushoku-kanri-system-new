using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Dto;
using 給食管理システム.Sql;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace 給食管理システム.Models.Forms.Calendar
{
    public partial class SetHolidayForm : Form
    {
        private SQLiteUtil _sqlite;
        DataTable _dt;
        string _sql;
        public SetHolidayForm()
        {
            InitializeComponent();
        }

        private void SetHolidayForm_Load(object sender, EventArgs e)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            var HolidayDB = _sqlite.Exists(SystemData.holidaysTable);

            if (HolidayDB == true)
            {
                string sql = $"SELECT id,day AS '日', holiday AS '休日' From {SystemData.holidaysTable}";
                var dataTable = _sqlite.GetData(sql);

                dataGridView1.DataSource = dataTable;

                dataGridView1.Columns[0].Visible = false;


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <suMary>
        /// 取り込み
        /// </suMary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("祝日データを取り込みしますか？\n既に存在する場合は上書きされます。", "重要", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                //デフォルトでDesktop
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                ofd.Filter = "テキストファイル(*.csv)|*.csv";
                //タイトルを設定する
                ofd.Title = "取りこむcsvファイルを選択してください";
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                ofd.RestoreDirectory = true;

                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        dataGridView1.ReadOnly = false;

                        _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

                        var HolidayDB = _sqlite.Exists(SystemData.holidaysTable);

                        if (HolidayDB == false)
                        {
                            ///Holidayマスタを作る                            
                            _sqlite.CreateTable(SystemData.holidaysTable, CalendarDto.IniHolidaysSet, SystemData.primayKey, SystemData.autoincrement);

                            //選択したcsvを取りこむ
                            string selectedFileName = ofd.FileName;

                            //var List = GetCsv(selectedFileName);
                            //csvファイルを指定年度用に編集
                            var thisYearList = ArrengCsv(selectedFileName, textBox1.Text);

                            var columns = _sqlite.GetColumnNames(SystemData.holidaysTable);

                            _dt = new DataTable();

                            for (int i = 0; i < columns.Length; i++)
                            {
                                _dt.Columns.Add(columns[i]);
                            }

                            foreach (var item in thisYearList)
                            {
                                var dr = _dt.NewRow();
                                dr[columns[1]] = item.Key;
                                dr[columns[2]] = item.Value;
                                _dt.Rows.Add(dr);
                            }

                            _sqlite.SetData(SystemData.holidaysTable, _dt);

                            //祝日があるときはbaseカレンダーテーブルのdayOffをtrueにする,備考に祝日名を挿入
                            _sql = CalendarDto.SelectBaseCalenderJointHolidaysSql(SystemData.baseCalendarTable);
                            _dt = _sqlite.GetData(_sql);
                            _dt = _sqlite.SetData(SystemData.baseCalendarTable, _dt);
                            _sql = CalendarDto.UpdateBaseCalender(SystemData.baseCalendarTable);
                            _sqlite.ExecuteNoneQuery(_sql);

                            MessageBox.Show("祝日が登録されました");

                        }
                        else
                        {
                            //tableをリセット
                            _sql = $"delete from {SystemData.holidaysTable}";

                            _sqlite.ExecuteNoneQuery(_sql);

                            //選択したcsvを取りこむ
                            string selectedFileName = ofd.FileName;

                            //var List = GetCsv(selectedFileName);
                            //csvファイルを指定年度用に編集
                            var thisYearList = ArrengCsv(selectedFileName, textBox1.Text);

                            var columns = _sqlite.GetColumnNames(SystemData.holidaysTable);
                            _dt = new DataTable();
                            _dt.Columns.Add(columns[0]);
                            _dt.Columns.Add(columns[1]);
                            _dt.Columns.Add(columns[2]);

                            foreach (var item in thisYearList)
                            {
                                var dr = _dt.NewRow();
                                dr[columns[1]] = (DateTime.Parse(item.Key).ToString("yyyy/M/d")).ToString();
                                dr[columns[2]] = item.Value;
                                _dt.Rows.Add(dr);
                            }

                            _sqlite.SetData(SystemData.holidaysTable, _dt);

                            _sql = $"SELECT * From {SystemData.holidaysTable}";

                            _dt = _sqlite.GetData(_sql);

                            dataGridView1.DataSource = _dt;

                            int currentRow = dataGridView1.CurrentRow.Index;
                                                        
                            dataGridView1.ReadOnly = true;

                            //祝日があるときはbaseカレンダーテーブルのdayOffをtrueにする
                            //_sql = CalendarDto.SelectBaseCalenderJointHolidaysSql(SystemData.baseCalendarTable);
                            //_dt = _sqlite.GetData(_sql);
                            //_dt = _sqlite.SetData(SystemData.baseCalendarTable, _dt);
                            _sql = CalendarDto.UpdateBaseCalenderInsertHolidays();
                            _sqlite.ExecuteNoneQuery(_sql);
                            _sql = CalendarDto.UpdateBaseCalender(SystemData.baseCalendarTable);
                            _sqlite.ExecuteNoneQuery(_sql);

                            MessageBox.Show("祝日が更新されました");
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    _sql = $"SELECT id, day AS '日', holiday AS '休日' From {SystemData.holidaysTable}";
                    _dt = _sqlite.GetData(_sql);

                    dataGridView1.DataSource = _dt;

                    dataGridView1.Columns[0].Visible = false;
                    
                }

            }
        }

        private Dictionary<string, string> ArrengCsv(string setFile, string thisYear)
        {

            List<string> List = new List<string>();

            string year = "";

            foreach (string line in GetCsv(setFile).Skip(1))
            {
                var date = line.Split(',')[0].ToString();
                var holiday = line.Split(',')[1];
                year = date.Split('/')[0];
                var month = date.Split('/')[1];


                if (month == "1" || month == "2" || month == "3")
                {
                    year = (int.Parse(year) - 1).ToString();
                }

                if (year == thisYear)
                {
                    List.Add(line);

                }

            }

            List.Add($"{thisYear}-12-29,年末休");
            List.Add($"{thisYear}-12-30,年末休");
            List.Add($"{thisYear}-12-31,年末休");
            List.Add($"{int.Parse(thisYear) + 1}-1-2,年始休");
            List.Add($"{int.Parse(thisYear) + 1}-1-3,年始休");

            var sortedDates = List.OrderBy(x => DateTime.Parse(x.Split(',')[0])).ToList();

            var returnList = new List<string>();
            var sampleDic = new Dictionary<string, string>();
            for (int i = 0; i < sortedDates.Count; i++)
            {
                if (!sampleDic.ContainsKey(sortedDates[i].Split(',')[0]))
                {
                    sampleDic.Add(sortedDates[i].Split(',')[0], sortedDates[i].Split(',')[1]);
                }

            }

            return sampleDic;
        }

        private List<string> GetCsv(string getfile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (StreamReader sr = new StreamReader(getfile, Encoding.GetEncoding("shift_jis")))
            {
                List<string> lists = new List<string>();

                while (sr.Peek() > -1)
                {
                    lists.Add(sr.ReadLine());
                }
                //lists.ForEach(Console.WriteLine);

                return lists;
            }
        }
    }
}
