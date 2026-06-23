using Microsoft.WindowsAPICodePack.Shell.Interop;
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

namespace 給食管理システム
{
    public partial class Setting : Form
    {
        string _sql;
        private SQLiteUtil _sqlite;

        private List<string> dataBaseListClassNameFisrtGrade;
        private List<string> dataBaseListClassNameSecondGrade;
        private List<string> dataBaseListClassNameThirdGrade;
        public Setting()
        {
            InitializeComponent();

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.DrawItem += tabControl1_DrawItem;

            SystemData systemData = new SystemData();
            systemData.Init();

            if (SystemData.schoolSetDataTable != null)
            {
                textBox19.Text = SystemData.schoolSetDataTable.Rows[0].Field<string>("year");
                textBox1.Text = SystemData.schoolSetDataTable.Rows[0].Field<string>("schoolName");
                textBox2.Text = SystemData.schoolSetDataTable.Rows[0].Field<string>("charger");
                textBox3.Text = SystemData.schoolSetDataTable.Rows[0].Field<string>("cost");
            }

            if (SystemData.termDataTable != null)
            {

                var distinctRows = SystemData.termDataTable.AsEnumerable()
                    .GroupBy(r => new
                    {
                        Grade = r.Field<string>("grade"),
                        ClassName = r.Field<string>("className")
                    })
                     .Select(g => g.First());

                DataTable resultTable = SystemData.termDataTable.Clone();

                foreach (var row in distinctRows)
                {
                    resultTable.ImportRow(row);
                }


                dataBaseListClassNameFisrtGrade = new List<string>();
                dataBaseListClassNameSecondGrade = new List<string>();
                dataBaseListClassNameThirdGrade = new List<string>();
                foreach (DataRow row in resultTable.Rows)
                {
                    if (int.Parse(row["grade"].ToString()) == 1)
                    {
                        listBox1.Items.Add(row["className"].ToString());
                        dataBaseListClassNameFisrtGrade.Add(row["className"].ToString());
                    }
                    if (int.Parse(row["grade"].ToString()) == 2) 
                    {
                        listBox2.Items.Add(row["className"].ToString());
                        dataBaseListClassNameSecondGrade.Add(row["className"].ToString());
                    }
                    if (int.Parse(row["grade"].ToString()) == 3)
                    {
                        listBox3.Items.Add(row["className"].ToString());
                        dataBaseListClassNameThirdGrade.Add(row["className"].ToString());
                    } 

                }

                //日付参照
                distinctRows = SystemData.termDataTable.AsEnumerable();

                resultTable = SystemData.termDataTable.Clone();
                foreach (var row in distinctRows)
                {
                    resultTable.ImportRow(row);
                }

                foreach (DataRow row in resultTable.Rows)
                {
                    //1年
                    if (int.Parse(row["grade"].ToString()) == 1 && int.Parse(row["term"].ToString()) == 1)
                    { textBox4.Text = row["start"].ToString(); textBox5.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 1 && int.Parse(row["term"].ToString()) == 2)
                    { textBox7.Text = row["start"].ToString(); textBox6.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 1 && int.Parse(row["term"].ToString()) == 3)
                    { textBox21.Text = row["start"].ToString(); textBox20.Text = row["end"].ToString(); }
                    //2年
                    if (int.Parse(row["grade"].ToString()) == 2 && int.Parse(row["term"].ToString()) == 1)
                    { textBox12.Text = row["start"].ToString(); textBox13.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 2 && int.Parse(row["term"].ToString()) == 2)
                    { textBox11.Text = row["start"].ToString(); textBox10.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 2 && int.Parse(row["term"].ToString()) == 3)
                    { textBox9.Text = row["start"].ToString(); textBox8.Text = row["end"].ToString(); }
                    //3年
                    if (int.Parse(row["grade"].ToString()) == 3 && int.Parse(row["term"].ToString()) == 1)
                    { textBox24.Text = row["start"].ToString(); textBox25.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 3 && int.Parse(row["term"].ToString()) == 2)
                    { textBox23.Text = row["start"].ToString(); textBox22.Text = row["end"].ToString(); }
                    if (int.Parse(row["grade"].ToString()) == 3 && int.Parse(row["term"].ToString()) == 3)
                    { textBox15.Text = row["start"].ToString(); textBox14.Text = row["end"].ToString(); }


                }

            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            textBox4.Text = dateTimePicker1.Text.ToString();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabctrl = (TabControl)sender;
            TabPage tabPage = tabctrl.TabPages[e.Index];

            Rectangle bounds = e.Bounds;
            Brush backBrush;
            Brush foreBrush;

            if (e.Index == tabctrl.SelectedIndex)
            {
                //選択時：背景
                backBrush = new SolidBrush(Color.Blue);
                foreBrush = new SolidBrush(Color.White);
            }
            else
            {
                backBrush = new SolidBrush(Color.LightGray);
                foreBrush = new SolidBrush(Color.Black);
            }

            e.Graphics.FillRectangle(backBrush, bounds);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(
                tabPage.Text, tabctrl.Font, foreBrush, bounds, stringFormat
                );

            backBrush.Dispose();
            foreBrush.Dispose();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            textBox5.Text = dateTimePicker2.Text.ToString();
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            textBox7.Text = dateTimePicker4.Text.ToString();
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            textBox6.Text = dateTimePicker3.Text.ToString();
        }

        private void dateTimePicker14_ValueChanged(object sender, EventArgs e)
        {
            textBox21.Text = dateTimePicker14.Text.ToString();
        }

        private void dateTimePicker13_ValueChanged(object sender, EventArgs e)
        {
            textBox20.Text = dateTimePicker13.Text.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox16.Text);
            textBox16.Text = "";
        }


        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                _sqlite = new SQLiteUtil(SystemData.SQLITE_DB);
                //schoolSetの更新or新規登録
                _sql = $"SELECT COUNT(*) FROM {SystemData.schoolTable}";
                var cnt = _sqlite.ExecuteScalar(_sql);

                schoolDto schooldto = new schoolDto();
                if (int.Parse(cnt.ToString()) == 0)
                {
                    schooldto.Insert(int.Parse(textBox19.Text), textBox1.Text, textBox2.Text, int.Parse(textBox3.Text));
                }
                else
                {
                    schooldto.Update(int.Parse(textBox19.Text), textBox1.Text, textBox2.Text, int.Parse(textBox3.Text));
                }

                //クラス名の更新or新規登録

                TermDto termDto = new TermDto();
                int num = 0;
                string[,] termArry = {
                                    { textBox4.Text, textBox5.Text },
                                    { textBox7.Text, textBox6.Text },
                                    { textBox21.Text,textBox20.Text } };

                switch (tabControl1.SelectedIndex)
                {
                    case 0://１学年を選択                            

                        foreach (var item in listBox1.Items)
                        {
                            var className = item.ToString();

                            var terms = new[]
                            {
                                        new{Term = 1,start = textBox4.Text, End = textBox5.Text},
                                        new{Term = 2,start = textBox7.Text, End = textBox6.Text},
                                        new{Term = 3,start = textBox21.Text, End = textBox20.Text}

                                    };

                            foreach (var t in terms)
                            {
                                num = int.Parse(termDto.SelectTermGrade(1, className, t.Term).ToString());
                                if (num == 0)
                                {
                                    termDto.Insert(1, className, t.Term, t.start, t.End);
                                }
                                else
                                {
                                    //削除の場合　データベースのクラス数 > listBoxのクラス数
                                    if(dataBaseListClassNameFisrtGrade.Count > listBox1.Items.Count)
                                    {
                                        var listB = listBox1.Items.Cast<string>().ToList();
                                        var result = dataBaseListClassNameFisrtGrade.Except(listB).ToList();
                                        if(result.Count > 0)
                                        {
                                            foreach(var row in result)
                                            {
                                                termDto.Delete(1,row);
                                            }
                                        }                                        

                                    }
                                    else if(dataBaseListClassNameFisrtGrade.Count == listBox1.Items.Count)
                                    {
                                        //変更の場合
                                        termDto.TermUpdate(1, className, t.Term, t.start, t.End);
                                    }
                                        
                                }

                            }

                        }
                        MessageBox.Show("1年の新規登録又は変更が終わりました");
                        break;

                    case 1://２学年を選択
                        foreach (var item in listBox2.Items)
                        {
                            var className = item.ToString();

                            var terms = new[]
                            {
                                        new{Term = 1,start = textBox12.Text, End = textBox13.Text},
                                        new{Term = 2,start = textBox11.Text, End = textBox10.Text},
                                        new{Term = 3,start = textBox9.Text, End = textBox8.Text}

                                    };

                            foreach (var t in terms)
                            {

                                num = int.Parse(termDto.SelectTermGrade(2, className, t.Term).ToString());
                                if (num == 0)
                                {
                                    termDto.Insert(2, className, t.Term, t.start, t.End);
                                }
                                else
                                {
                                    // 削除の場合 データベースのクラス数 > listBoxのクラス数
                                    if (dataBaseListClassNameSecondGrade.Count > listBox2.Items.Count)
                                    {
                                        var listB = listBox2.Items.Cast<string>().ToList();
                                        var result = dataBaseListClassNameSecondGrade.Except(listB).ToList();
                                        if (result.Count > 0)
                                        {
                                            foreach (var row in result)
                                            {
                                                termDto.Delete(2, row);
                                            }
                                        }

                                    }
                                    else if (dataBaseListClassNameSecondGrade.Count == listBox2.Items.Count)
                                    {
                                        //変更の場合
                                        termDto.TermUpdate(2, className, t.Term, t.start, t.End);
                                    }
                                }

                            }

                        }
                        MessageBox.Show("2年の新規登録又は変更が終わりました");
                        break;

                    case 2://３学年を選択
                        foreach (var item in listBox3.Items)
                        {
                            var className = item.ToString();

                            var terms = new[]
                            {
                                        new{Term = 1,start = textBox24.Text, End = textBox25.Text},
                                        new{Term = 2,start = textBox23.Text, End = textBox22.Text},
                                        new{Term = 3,start = textBox15.Text, End = textBox14.Text}

                                    };

                            foreach (var t in terms)
                            {

                                num = int.Parse(termDto.SelectTermGrade(3, className, t.Term).ToString());
                                if (num == 0)
                                {
                                    termDto.Insert(3, className, t.Term, t.start, t.End);
                                }
                                else
                                {
                                    // 削除の場合 データベースのクラス数 > listBoxのクラス数
                                    if (dataBaseListClassNameThirdGrade.Count > listBox3.Items.Count)
                                    {
                                        var listB = listBox3.Items.Cast<string>().ToList();
                                        var result = dataBaseListClassNameThirdGrade.Except(listB).ToList();
                                        if (result.Count > 0)
                                        {
                                            foreach (var row in result)
                                            {
                                                termDto.Delete(3, row);
                                            }
                                        }

                                    }
                                    else if (dataBaseListClassNameThirdGrade.Count == listBox3.Items.Count)
                                    {
                                        //変更の場合
                                        termDto.TermUpdate(3, className, t.Term, t.start, t.End);
                                    }
                                }

                            }

                        }
                        MessageBox.Show("3年の新規登録又は変更が終わりました");
                        break;

                }

            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }



        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items[listBox1.SelectedIndex] = textBox18.Text;

            textBox18.Text = "";
            textBox17.Text = "";
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textBox18.Text = listBox1.SelectedItem.ToString();
            textBox17.Text = listBox1.SelectedItem.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            textBox18.Text = "";
            textBox17.Text = "";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            listBox2.Items.Add(textBox28.Text);
            textBox28.Text = "";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listBox3.Items.Add(textBox31.Text);
            textBox31.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            listBox2.Items[listBox2.SelectedIndex] = textBox26.Text;

            textBox26.Text = "";
            textBox27.Text = "";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            listBox3.Items[listBox3.SelectedIndex] = textBox29.Text;

            textBox29.Text = "";
            textBox30.Text = "";
        }

        private void dateTimePicker7_ValueChanged(object sender, EventArgs e)
        {
            textBox12.Text = dateTimePicker7.Text.ToString();
        }

        private void dateTimePicker9_ValueChanged(object sender, EventArgs e)
        {
            textBox13.Text = dateTimePicker9.Text.ToString();
        }

        private void dateTimePicker10_ValueChanged(object sender, EventArgs e)
        {
            textBox11.Text = dateTimePicker10.Text.ToString();
        }

        private void dateTimePicker8_ValueChanged(object sender, EventArgs e)
        {
            textBox10.Text = dateTimePicker8.Text.ToString();
        }

        private void dateTimePicker6_ValueChanged(object sender, EventArgs e)
        {
            textBox9.Text = dateTimePicker6.Text.ToString();
        }

        private void dateTimePicker5_ValueChanged(object sender, EventArgs e)
        {
            textBox8.Text = dateTimePicker5.Text.ToString();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textBox26.Text = listBox2.SelectedItem.ToString();
            textBox27.Text = listBox2.SelectedItem.ToString();
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textBox29.Text = listBox3.SelectedItem.ToString();
            textBox30.Text = listBox3.SelectedItem.ToString();
        }

        private void dateTimePicker15_ValueChanged(object sender, EventArgs e)
        {
            textBox24.Text = dateTimePicker15.Text.ToString();
        }

        private void dateTimePicker17_ValueChanged(object sender, EventArgs e)
        {
            textBox25.Text = dateTimePicker17.Text.ToString();
        }

        private void dateTimePicker18_ValueChanged(object sender, EventArgs e)
        {
            textBox23.Text = dateTimePicker18.Text.ToString();
        }

        private void dateTimePicker16_ValueChanged(object sender, EventArgs e)
        {
            textBox22.Text = dateTimePicker16.Text.ToString();
        }

        private void dateTimePicker12_ValueChanged(object sender, EventArgs e)
        {
            textBox15.Text = dateTimePicker12.Text.ToString();
        }

        private void dateTimePicker11_ValueChanged(object sender, EventArgs e)
        {
            textBox14.Text = dateTimePicker11.Text.ToString();
        }
    }
}
