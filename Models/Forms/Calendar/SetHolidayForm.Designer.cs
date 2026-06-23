namespace 給食管理システム.Models.Forms.Calendar
{
    partial class SetHolidayForm
    {
        /// <suMary>
        /// Required designer variable.
        /// </suMary>
        private System.ComponentModel.IContainer components = null;

        /// <suMary>
        /// Clean up any resources being used.
        /// </suMary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <suMary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </suMary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            button2 = new Button();
            button1 = new Button();
            textBox1 = new TextBox();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            dataGridView1 = new DataGridView();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(4, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(345, 213);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "祝日登録";
            // 
            // button2
            // 
            button2.Location = new Point(32, 168);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 9;
            button2.Text = "終　了";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(33, 127);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 8;
            button1.Text = "取　込";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.ImeMode = ImeMode.Alpha;
            textBox1.Location = new Point(129, 92);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(60, 23);
            textBox1.TabIndex = 7;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(32, 95);
            label7.Name = "label7";
            label7.Size = new Size(91, 15);
            label7.TabIndex = 6;
            label7.Text = "対象西暦年度：";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(32, 63);
            label6.Name = "label6";
            label6.Size = new Size(278, 15);
            label6.TabIndex = 5;
            label6.Text = "○年から○年国民の祝日.csvを右クリしてダウンロードする";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 172);
            label5.Name = "label5";
            label5.Size = new Size(19, 15);
            label5.TabIndex = 4;
            label5.Text = "④";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 131);
            label4.Name = "label4";
            label4.Size = new Size(19, 15);
            label4.TabIndex = 3;
            label4.Text = "③";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 95);
            label3.Name = "label3";
            label3.Size = new Size(19, 15);
            label3.TabIndex = 2;
            label3.Text = "➁";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(32, 31);
            label2.Name = "label2";
            label2.Size = new Size(301, 15);
            label2.TabIndex = 1;
            label2.Text = "内閣府 > 内閣府の再生 > 制度 > 国民の祝日について 参照\r\n";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 31);
            label1.Name = "label1";
            label1.Size = new Size(19, 15);
            label1.TabIndex = 0;
            label1.Text = "①";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 224);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(325, 438);
            dataGridView1.TabIndex = 1;
            // 
            // SetHolidayForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(361, 677);
            ControlBox = false;
            Controls.Add(dataGridView1);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            Name = "SetHolidayForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "祝日登録・更新";
            Load += SetHolidayForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Button button2;
        private Button button1;
        private TextBox textBox1;
        private Label label7;
        private Label label6;
        private DataGridView dataGridView1;
    }
}