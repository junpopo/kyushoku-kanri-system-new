using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 給食管理システム.Models.Forms.Calendar;

namespace 給食管理システム.Models.Forms.Setting
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Reset reset = new Reset();
            reset.ReSet();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            給食管理システム.Setting settingcs = new 給食管理システム.Setting();
            settingcs.ShowDialog();
            settingcs.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateYear updateYear = new UpdateYear();
            updateYear.ShowDialog();
            updateYear.Dispose();
        }
    }
}
