namespace ProjectG
{
    partial class PG
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PG));
            gBoxCycleDowntime = new GroupBox();
            RadioShortMedium = new RadioButton();
            RadioLong = new RadioButton();
            RadioMedium = new RadioButton();
            RadioShort = new RadioButton();
            btnStart = new Button();
            LblDowntime = new Label();
            TimerDowntime = new System.Windows.Forms.Timer(components);
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            LblState = new Label();
            gBoxCycleDowntime.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // gBoxCycleDowntime
            // 
            gBoxCycleDowntime.Controls.Add(RadioShortMedium);
            gBoxCycleDowntime.Controls.Add(RadioLong);
            gBoxCycleDowntime.Controls.Add(RadioMedium);
            gBoxCycleDowntime.Controls.Add(RadioShort);
            gBoxCycleDowntime.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            gBoxCycleDowntime.ForeColor = Color.Maroon;
            gBoxCycleDowntime.Location = new Point(5, 8);
            gBoxCycleDowntime.Name = "gBoxCycleDowntime";
            gBoxCycleDowntime.Size = new Size(139, 86);
            gBoxCycleDowntime.TabIndex = 2;
            gBoxCycleDowntime.TabStop = false;
            gBoxCycleDowntime.Text = "Cycle Downtime";
            // 
            // RadioShortMedium
            // 
            RadioShortMedium.AutoSize = true;
            RadioShortMedium.Font = new Font("Verdana", 8.25F);
            RadioShortMedium.ForeColor = Color.FromArgb(64, 64, 64);
            RadioShortMedium.Location = new Point(66, 27);
            RadioShortMedium.Name = "RadioShortMedium";
            RadioShortMedium.Size = new Size(58, 17);
            RadioShortMedium.TabIndex = 8;
            RadioShortMedium.Text = "35-65";
            RadioShortMedium.UseVisualStyleBackColor = true;
            // 
            // RadioLong
            // 
            RadioLong.AutoSize = true;
            RadioLong.Font = new Font("Verdana", 8.25F);
            RadioLong.ForeColor = Color.FromArgb(64, 64, 64);
            RadioLong.Location = new Point(66, 50);
            RadioLong.Name = "RadioLong";
            RadioLong.Size = new Size(72, 17);
            RadioLong.TabIndex = 7;
            RadioLong.Text = "120-180";
            RadioLong.UseVisualStyleBackColor = true;
            // 
            // RadioMedium
            // 
            RadioMedium.AutoSize = true;
            RadioMedium.Font = new Font("Verdana", 8.25F);
            RadioMedium.ForeColor = Color.FromArgb(64, 64, 64);
            RadioMedium.Location = new Point(6, 50);
            RadioMedium.Name = "RadioMedium";
            RadioMedium.Size = new Size(58, 17);
            RadioMedium.TabIndex = 6;
            RadioMedium.Text = "55-90";
            RadioMedium.UseVisualStyleBackColor = true;
            // 
            // RadioShort
            // 
            RadioShort.AutoSize = true;
            RadioShort.Checked = true;
            RadioShort.Font = new Font("Verdana", 8.25F);
            RadioShort.ForeColor = Color.FromArgb(64, 64, 64);
            RadioShort.Location = new Point(6, 27);
            RadioShort.Name = "RadioShort";
            RadioShort.Size = new Size(58, 17);
            RadioShort.TabIndex = 5;
            RadioShort.TabStop = true;
            RadioShort.Text = "15-25";
            RadioShort.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            btnStart.Enabled = false;
            btnStart.Font = new Font("Verdana", 9F);
            btnStart.ForeColor = Color.FromArgb(64, 64, 64);
            btnStart.Location = new Point(327, 14);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(69, 80);
            btnStart.TabIndex = 5;
            btnStart.Text = "Locate Mailbox";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // LblDowntime
            // 
            LblDowntime.Font = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            LblDowntime.ForeColor = Color.FromArgb(64, 64, 64);
            LblDowntime.Location = new Point(6, 17);
            LblDowntime.Name = "LblDowntime";
            LblDowntime.Size = new Size(155, 13);
            LblDowntime.TabIndex = 6;
            LblDowntime.Text = "Downtime";
            LblDowntime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TimerDowntime
            // 
            TimerDowntime.Interval = 1000;
            TimerDowntime.Tick += TimerDowntime_Tick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(LblDowntime);
            groupBox1.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            groupBox1.ForeColor = Color.Maroon;
            groupBox1.Location = new Point(154, 55);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(167, 39);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Status";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(LblState);
            groupBox2.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            groupBox2.ForeColor = Color.Maroon;
            groupBox2.Location = new Point(154, 8);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(167, 39);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "State";
            // 
            // LblState
            // 
            LblState.Font = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            LblState.ForeColor = Color.FromArgb(64, 64, 64);
            LblState.Location = new Point(6, 17);
            LblState.Name = "LblState";
            LblState.Size = new Size(155, 13);
            LblState.TabIndex = 6;
            LblState.Text = "State";
            LblState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // PG
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(403, 102);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(btnStart);
            Controls.Add(gBoxCycleDowntime);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PG";
            StartPosition = FormStartPosition.Manual;
            Text = "Project G";
            TopMost = true;
            FormClosing += PG_FormClosing;
            KeyDown += PG_KeyDown;
            gBoxCycleDowntime.ResumeLayout(false);
            gBoxCycleDowntime.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private GroupBox gBoxCycleDowntime;
        private RadioButton RadioShort;
        private RadioButton RadioLong;
        private RadioButton RadioMedium;
        private Button btnStart;
        private RadioButton RadioShortMedium;
        private Label LblDowntime;
        private System.Windows.Forms.Timer TimerDowntime;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label LblState;
    }
}
