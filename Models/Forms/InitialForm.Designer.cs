namespace 給食管理システム.Models.Forms
{
    partial class InitialForm
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
            textBox2 = new TextBox();
            button3 = new Button();
            label4 = new Label();
            textBox3 = new TextBox();
            textBox1 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label1 = new Label();
            button1 = new Button();
            button2 = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ScrollBar;
            groupBox1.BackgroundImageLayout = ImageLayout.Stretch;
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(button3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(327, 144);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(110, 89);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(180, 45);
            textBox2.TabIndex = 62;
            // 
            // button3
            // 
            button3.BackColor = SystemColors.ButtonShadow;
            button3.Font = new Font("Yu Gothic UI", 6F);
            button3.Location = new Point(110, 64);
            button3.Name = "button3";
            button3.Size = new Size(67, 23);
            button3.TabIndex = 3;
            button3.Text = "ファイル保存先";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.Red;
            label4.Location = new Point(184, 21);
            label4.Name = "label4";
            label4.Size = new Size(59, 15);
            label4.TabIndex = 61;
            label4.Text = "例)　2025";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(110, 15);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(67, 23);
            textBox3.TabIndex = 60;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(110, 40);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(67, 23);
            textBox1.TabIndex = 59;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ImeMode = ImeMode.NoControl;
            label2.Location = new Point(20, 43);
            label2.Name = "label2";
            label2.Size = new Size(84, 15);
            label2.TabIndex = 58;
            label2.Text = "データベース名：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ImeMode = ImeMode.NoControl;
            label3.Location = new Point(6, 68);
            label3.Name = "label3";
            label3.Size = new Size(99, 15);
            label3.TabIndex = 55;
            label3.Text = "ファイルの保存先：";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(61, 21);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 0;
            label1.Text = "年度：";
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.Cancel;
            button1.Location = new Point(93, 174);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "キャンセル";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.OK;
            button2.Location = new Point(198, 174);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "登　録";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // InitialForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ScrollBar;
            ClientSize = new Size(337, 214);
            ControlBox = false;
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InitialForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "初期ユーザー登録";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label2;
        private Button button1;
        private Button button2;
        private Button button3;
        public TextBox textBox3;
        public TextBox textBox1;
        public TextBox textBox2;
    }
}