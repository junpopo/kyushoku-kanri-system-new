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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Forms.Button;

namespace 給食管理システム.Account
{
    public partial class AccountFixForm : Form
    {
        private readonly object _sender;
        public AccountFixForm(object sender)
        {
            InitializeComponent();
            this._sender = sender;
            this.Text = ((Button)this._sender).Text;
            if (((Button)this._sender).Text == "変更")
            {
                button1.Text = "変更";
            }
            else if (((Button)this._sender).Text == "削除")
            {
                button1.Text = "削除";
            }

            if (((Button)this._sender).Text == "新規登録")
            {
                button1.Text = "新規登録";
            }

            for (int i = 0; i < SystemData.localList.Count; i++)
            {
                comboBox1.Items.Add(SystemData.localList[i]);
            }
            for (int i = 0; i < UserDto.CategoryArry.Length; i++)
            {
                comboBox9.Items.Add(UserDto.CategoryArry[i]);
            }
            //クラス名取り込み

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Text = comboBox1.SelectedItem.ToString();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void AccountFixForm_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0; comboBox3.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0; comboBox4.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox9.SelectedIndex != -1) { textBox9.Text = comboBox9.SelectedItem.ToString(); }

            if (textBox9.Text != "生徒")
            {
                comboBox10.Enabled = false; comboBox11.Enabled = false; comboBox12.Enabled = false;
            }
            if (textBox9.Text == "生徒")
            {
                comboBox10.Enabled = true; comboBox11.Enabled = true; comboBox12.Enabled = true;
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                comboBox2.SelectedIndex = 1; comboBox3.SelectedIndex = 1; comboBox4.SelectedIndex = 1; comboBox5.SelectedIndex = 1; comboBox6.SelectedIndex = 1;
                comboBox7.SelectedIndex = 1;
            }
            else
            {
                comboBox2.SelectedIndex = 0; comboBox3.SelectedIndex = 0; comboBox4.SelectedIndex = 0; comboBox5.SelectedIndex = 0; comboBox6.SelectedIndex = 0;
                comboBox7.SelectedIndex = 0;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsCheck() == false)
            {
                MessageBox.Show("入力or選択されていない項目があります");
                return;
            }

            DialogResult result = MessageBox.Show($"{((Button)this._sender).Text}しますか?", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                if (((Button)this._sender).Text == "新規登録")
                {
                    this.DialogResult = DialogResult.OK;
                }
                else if (((Button)this._sender).Text == "変更")
                {
                    this.DialogResult = DialogResult.Yes;
                }
                else if (((Button)this._sender).Text == "削除")
                {
                    this.DialogResult = DialogResult.No;
                }
            }
        }

        private bool IsCheck()
        {
            var check = true;

            if (comboBox9.SelectedIndex == -1) check = false;//分類 
            if (textBox9.Text == "生徒")
            {
                if (textBox23.Text == "" || textBox2.Text == "" || textBox25.Text == "") check = false;

            }
            if (textBox11.Text == "") check = false;//名前
            if (textBox2.Text == "") check = false;//苗字
            if (textBox3.Text == "") check = false;//配膳場所１
            if (textBox19.Text == "" || textBox18.Text == "") check = false;//配膳場所１開始・終了日
            if (textBox22.Text != "")//配膳場所2が選択されているなら、開始・終了日が空白ならエラー
            {
                if (textBox21.Text == "" || textBox20.Text == "")
                {
                    check = false;
                }
            }
            if (checkBox2.Checked)//給食停止を選択している場合
            {
                if (textBox17.Text == "" || textBox14.Text == "")
                {
                    check = false;
                }
            }
            if (comboBox1.SelectedIndex == -1) check = false;//喫食曜日
            if (comboBox2.SelectedIndex == -1) check = false;//喫食曜日
            if (comboBox3.SelectedIndex == -1) check = false;//喫食曜日
            if (comboBox4.SelectedIndex == -1) check = false;//喫食曜日
            if (comboBox5.SelectedIndex == -1) check = false;//喫食曜日
            if (comboBox6.SelectedIndex == -1) check = false;//喫食曜日
            if (checkBox2.Checked)//牛乳停止を選択している場合
            {
                if (textBox15.Text == "" || textBox16.Text == "")
                {
                    check = false;
                }
            }
            if (textBox12.Text == "" || textBox13.Text == "")//喫食開始日
            {
                check = false;
            }


            return check;
        }


        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox23.Text = comboBox10.SelectedItem.ToString();
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox24.Text = comboBox11.SelectedItem.ToString();
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox25.Text = comboBox12.SelectedItem.ToString();
        }

        private void dateTimePicker8_ValueChanged(object sender, EventArgs e)
        {
            textBox19.Text = dateTimePicker8.Value.ToString("MM/dd");
        }

        private void dateTimePicker7_ValueChanged(object sender, EventArgs e)
        {
            textBox18.Text = dateTimePicker8.Value.ToString("MM/dd");
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            textBox12.Text = dateTimePicker1.Value.ToString("MM/dd");
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            textBox13.Text = dateTimePicker2.Value.ToString("MM/dd");
        }
    }
}
