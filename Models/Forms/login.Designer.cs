namespace 給食管理システム
{
    partial class Login
    {
        /// <suMary>
        ///  Required designer variable.
        /// </suMary>
        private System.ComponentModel.IContainer components = null;

        /// <suMary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </suMary>
        private void InitializeComponent()
        {
            button3 = new Button();
            button2 = new Button();
            button4 = new Button();
            groupBox1 = new GroupBox();
            label1 = new Label();
            textBox1 = new TextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button3
            // 
            button3.Location = new Point(35, 145);
            button3.Name = "button3";
            button3.Size = new Size(107, 23);
            button3.TabIndex = 400;
            button3.Text = "終　了";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(35, 116);
            button2.Name = "button2";
            button2.Size = new Size(107, 23);
            button2.TabIndex = 300;
            button2.Text = "ゲスト";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button4
            // 
            button4.Location = new Point(23, 60);
            button4.Name = "button4";
            button4.Size = new Size(107, 23);
            button4.TabIndex = 2;
            button4.Text = "管理者";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(button4);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(151, 98);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "管理者";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 7;
            label1.Text = "Password:";
            // 
            // textBox1
            // 
            textBox1.ImeMode = ImeMode.Alpha;
            textBox1.Location = new Point(72, 22);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(68, 23);
            textBox1.TabIndex = 0;
            textBox1.Tag = "0";
            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gainsboro;
            ClientSize = new Size(177, 194);
            ControlBox = false;
            Controls.Add(groupBox1);
            Controls.Add(button2);
            Controls.Add(button3);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Login";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ログイン画面";
            Load += login_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button button3;
        private Button button2;
        private Button button4;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private Label label1;
    }
}
