using System.Text.RegularExpressions;
using 給食管理システム.Dto;
using 給食管理システム.Sql;

namespace 給食管理システム
{
    public partial class Login : Form
    {
        private SQLiteUtil _sqlite;
        public Login()
        {
            InitializeComponent();
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);

            //スペルチェック
            if (IsCheck() == false)
            {
                return;
            }
            else
            {
                Program.DoContinue = true;
                Program.admin = true;
                this.Close();
            }



        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void login_Load(object sender, EventArgs e)
        {
            
        }
        private bool IsCheck()
        {
            if (textBox1.Text == "admin")
            {
                return true;
            }
            else
            {
                MessageBox.Show("IDに、4桁の数字を入力してください");
                return false;
            }

            
        }
        /// <suMary>
        /// ゲストログイン
        /// </suMary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Program.DoContinue = true;
            Program.admin = false;
            this.Close();
        }
    }
}
