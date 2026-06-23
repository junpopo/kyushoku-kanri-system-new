using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Dto;
using 給食管理システム.Sql;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;


namespace 給食管理システム.Models.Forms.Calendar
{
    public partial class ManageOderForm : Form
    {
        private SQLiteUtil _sqlite;
        private string _sql;
        bool _check;
        DataTable _DT;
        private TableLayoutPanel _table; // カレンダー本体を保持
        private DateTime _displayMonth; // 表示している年月（1日を保持）
        private const int DAYS_MAX = 42;
        private CalendarDto calendarDto;

        public ManageOderForm()
        {
            InitializeComponent();
            string[] main = { "全員", "学年(生徒)", "所属(職員)", "個人(生徒)", "個人(職員)" };
            comboBox1.Items.AddRange(main);
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            CreateCalendarTable();

            
            //button5.Click += BtnRefresh_Click;
            //button6.Click += BtnAggregation_Click;

            // 初期月 → 今月
            _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        //===========================================================
        // ■ カレンダーTableLayoutPanelの作成
        //===========================================================
        private void CreateCalendarTable()
        {
            _table = new TableLayoutPanel();
            _table.Dock = DockStyle.Fill;
            _table.RowCount = 7;
            _table.ColumnCount = 7;
            _table.Margin = new Padding(0);
            _table.Padding = new Padding(0);
            _table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (int i = 0; i < 7; i++)
            {
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 7));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
            }

            //―――曜日ヘッダ（1行目）
            string[] week = { "月", "火", "水", "木", "金", "土", "日" };
            for (int i = 0; i < 7; i++)
            {
                var btn = new System.Windows.Forms.Button();
                btn.Text = week[i];
                btn.Dock = DockStyle.Fill;
                btn.BackColor = (i >= 5) ? Color.Tomato : Color.LightSkyBlue;
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Margin = new Padding(0);
                btn.Enabled = false;  // 曜日はクリックしない
                _table.Controls.Add(btn, i, 0);
            }

            //―――日付ボタン（6行 × 7列 = 最大42日）
            for (int r = 1; r <= 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    int index = (r - 1) * 7 + c + 1;  // 1～42
                    var btn = new System.Windows.Forms.Button();
                    btn.Name = $"DayBtn{index}";
                    btn.Text = "";
                    btn.Dock = DockStyle.Fill;
                    btn.Margin = new Padding(0);
                    _table.Controls.Add(btn, c, r);
                    btn.Click += new EventHandler(DayDetailClick);
                }
            }

            groupBox2.Controls.Add(_table);
        }

        private void DayDetailClick(object sender, EventArgs e)
        {
            try
            {
                var year = label4.Text.Substring(0, 4);
                var month = label4.Text.Replace(year, "");
                month = month.Replace("年", "");
                month = month.Replace("月", "");
                month = month.Replace("日", "");
                var day = ((System.Windows.Forms.Button)sender).Text.ToString().Split("\n")[0];
                var ymd = year + "/" + month + "/" + day;

                DayDetailForm dayDetailForm = new DayDetailForm(comboBox1.SelectedIndex, ymd);
                DialogResult drRet = dayDetailForm.ShowDialog();
                if (drRet == DialogResult.OK)
                {
                    //画面更新
                    ManageOderForm_Load(sender, e);
                    MessageBox.Show("保存しました。");
                }
                else { }//キャンセル何もしない
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //===========================================================
        // フォームロード
        //===========================================================
        private void ManageOderForm_Load(object sender, EventArgs e)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            if (!IsCheck()) return;

            calendarDto = new CalendarDto();

            label4.Text = calendarDto.LoadMonth(_displayMonth, _table);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = comboBox1.SelectedIndex != 0;
        }


        //===========================================================
        // DB存在チェック
        //===========================================================
        private bool IsCheck()
        {
            if (!_sqlite.Exists(SystemData.baseCalendarTable))
            {
                MessageBox.Show("baseCalendarTable を作成してください。");
                return false;
            }

            if (!_sqlite.Exists(SystemData.holidaysTable))
            {
                MessageBox.Show("祝日テーブルを作成してください。");
                return false;
            }

            return true;
        }
        //===========================================================
        // ■ 曜日を数値に変換（カレンダー開始位置）
        //===========================================================
        private static int DayToNumber(string dow)
        {
            return dow switch
            {
                "Monday" => 0,
                "Tuesday" => 1,
                "Wednesday" => 2,
                "Thursday" => 3,
                "Friday" => 4,
                "Saturday" => 5,
                "Sunday" => 6,
                _ => -1,
            };
        }
        //===========================================================
        // ■ 個別ボタン設定
        //===========================================================
        private void SetCalendar(int offset, int number, int dayValue,
                                 string remark, string holiday,
                                 string dayOff, string lunch)
        {
            int pos = offset + (number - 1); // 0〜41
            int index = pos + 1;             // ボタン名は1始まり DayBtn1〜42

            var btn = _table.Controls
                            .OfType<System.Windows.Forms.Button>()
                            .FirstOrDefault(b => b.Name == $"DayBtn{index}");

            if (btn == null) return;

            if (dayOff == "true")
            {
                btn.BackColor = Color.FromArgb(255, 0, 0);
            }
            else { btn.BackColor = Color.LightYellow; }

            btn.Text = $"{dayValue}\n{holiday}\n{remark}\n{lunch}";
        }


        /// <summary>
        /// 月を減らす
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            int[] fiscalMonths = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };            

            int todayMonth = _displayMonth.Month;
            
            int currentIndex = Array.IndexOf(fiscalMonths, todayMonth);

            if (currentIndex == 0)
            {
                currentIndex = 0;
            }
            else
            {
                int currentMonth = fiscalMonths[currentIndex - 1];

                
                if (currentMonth == 12)
                {
                    _displayMonth = DateTime.Parse(_displayMonth.AddYears(-1).Year + "/" + currentMonth);
                }
                else
                {
                    _displayMonth = DateTime.Parse(_displayMonth.Year + "/" + currentMonth);
                }

                calendarDto = new CalendarDto();

                label4.Text = calendarDto.LoadMonth(_displayMonth, _table);
            }            

        }
        /// <summary>
        /// 月を増やす
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            int[] fiscalMonths = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };

            int todayMonth = _displayMonth.Month;

            int currentIndex = Array.IndexOf(fiscalMonths, todayMonth);

            if (currentIndex == 11)
            {
                currentIndex = 11;
            }
            else
            {
                int currentMonth = fiscalMonths[currentIndex + 1];


                if (currentMonth == 1)
                {
                    _displayMonth = DateTime.Parse(_displayMonth.AddYears(+1).Year + "/" + currentMonth);
                }
                else
                {
                    _displayMonth = DateTime.Parse(_displayMonth.Year + "/" + currentMonth);
                }

                calendarDto = new CalendarDto();

                label4.Text = calendarDto.LoadMonth(_displayMonth, _table);
            }
        }
    }
}
