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
            lblCustomDowntimeMax = new Label();
            lblCustomDowntimeMin = new Label();
            numericCustomDowntimeMaxSec = new NumericUpDown();
            numericCustomDowntimeMinSec = new NumericUpDown();
            RadioShortMedium = new RadioButton();
            RadioCustom = new RadioButton();
            RadioMedium = new RadioButton();
            RadioShort = new RadioButton();
            btnStart = new Button();
            LblDowntime = new Label();
            TimerDowntime = new System.Windows.Forms.Timer(components);
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            LblState = new Label();
            cBoxDualClient = new CheckBox();
            groupBoxInternet = new GroupBox();
            LblInternetStatus = new Label();
            TimerInternetUi = new System.Windows.Forms.Timer(components);
            gBoxCycleDowntime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericCustomDowntimeMaxSec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericCustomDowntimeMinSec).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBoxInternet.SuspendLayout();
            SuspendLayout();
            // 
            // gBoxCycleDowntime
            // 
            gBoxCycleDowntime.Controls.Add(lblCustomDowntimeMax);
            gBoxCycleDowntime.Controls.Add(lblCustomDowntimeMin);
            gBoxCycleDowntime.Controls.Add(numericCustomDowntimeMaxSec);
            gBoxCycleDowntime.Controls.Add(numericCustomDowntimeMinSec);
            gBoxCycleDowntime.Controls.Add(RadioShortMedium);
            gBoxCycleDowntime.Controls.Add(RadioCustom);
            gBoxCycleDowntime.Controls.Add(RadioMedium);
            gBoxCycleDowntime.Controls.Add(RadioShort);
            gBoxCycleDowntime.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            gBoxCycleDowntime.ForeColor = Color.Maroon;
            gBoxCycleDowntime.Location = new Point(5, 8);
            gBoxCycleDowntime.Name = "gBoxCycleDowntime";
            gBoxCycleDowntime.Size = new Size(200, 108);
            gBoxCycleDowntime.TabIndex = 2;
            gBoxCycleDowntime.TabStop = false;
            gBoxCycleDowntime.Text = "Cycle Downtime";
            // 
            // lblCustomDowntimeMax
            // 
            lblCustomDowntimeMax.AutoSize = true;
            lblCustomDowntimeMax.Font = new Font("Verdana", 7.5F);
            lblCustomDowntimeMax.ForeColor = Color.FromArgb(64, 64, 64);
            lblCustomDowntimeMax.Location = new Point(89, 72);
            lblCustomDowntimeMax.Name = "lblCustomDowntimeMax";
            lblCustomDowntimeMax.Size = new Size(35, 12);
            lblCustomDowntimeMax.TabIndex = 11;
            lblCustomDowntimeMax.Text = "max:";
            // 
            // lblCustomDowntimeMin
            // 
            lblCustomDowntimeMin.AutoSize = true;
            lblCustomDowntimeMin.Font = new Font("Verdana", 7.5F);
            lblCustomDowntimeMin.ForeColor = Color.FromArgb(64, 64, 64);
            lblCustomDowntimeMin.Location = new Point(6, 72);
            lblCustomDowntimeMin.Name = "lblCustomDowntimeMin";
            lblCustomDowntimeMin.Size = new Size(26, 12);
            lblCustomDowntimeMin.TabIndex = 9;
            lblCustomDowntimeMin.Text = "(s):";
            // 
            // numericCustomDowntimeMaxSec
            // 
            numericCustomDowntimeMaxSec.Font = new Font("Verdana", 8.25F);
            numericCustomDowntimeMaxSec.Location = new Point(120, 69);
            numericCustomDowntimeMaxSec.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numericCustomDowntimeMaxSec.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericCustomDowntimeMaxSec.Name = "numericCustomDowntimeMaxSec";
            numericCustomDowntimeMaxSec.Size = new Size(44, 21);
            numericCustomDowntimeMaxSec.TabIndex = 12;
            numericCustomDowntimeMaxSec.TextAlign = HorizontalAlignment.Center;
            numericCustomDowntimeMaxSec.Value = new decimal(new int[] { 999, 0, 0, 0 });
            // 
            // numericCustomDowntimeMinSec
            // 
            numericCustomDowntimeMinSec.Font = new Font("Verdana", 8.25F);
            numericCustomDowntimeMinSec.Location = new Point(38, 69);
            numericCustomDowntimeMinSec.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numericCustomDowntimeMinSec.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericCustomDowntimeMinSec.Name = "numericCustomDowntimeMinSec";
            numericCustomDowntimeMinSec.Size = new Size(48, 21);
            numericCustomDowntimeMinSec.TabIndex = 10;
            numericCustomDowntimeMinSec.TextAlign = HorizontalAlignment.Center;
            numericCustomDowntimeMinSec.Value = new decimal(new int[] { 1, 0, 0, 0 });
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
            // RadioCustom
            // 
            RadioCustom.AutoSize = true;
            RadioCustom.Font = new Font("Verdana", 8.25F);
            RadioCustom.ForeColor = Color.FromArgb(64, 64, 64);
            RadioCustom.Location = new Point(66, 50);
            RadioCustom.Name = "RadioCustom";
            RadioCustom.Size = new Size(69, 17);
            RadioCustom.TabIndex = 7;
            RadioCustom.Text = "Custom";
            RadioCustom.UseVisualStyleBackColor = true;
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
            btnStart.Location = new Point(385, 44);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(69, 50);
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
            groupBox1.Location = new Point(215, 55);
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
            groupBox2.Location = new Point(215, 8);
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
            // cBoxDualClient
            // 
            cBoxDualClient.AutoSize = true;
            cBoxDualClient.Location = new Point(385, 19);
            cBoxDualClient.Name = "cBoxDualClient";
            cBoxDualClient.Size = new Size(81, 19);
            cBoxDualClient.TabIndex = 9;
            cBoxDualClient.Text = "DualClient";
            cBoxDualClient.UseVisualStyleBackColor = true;
            cBoxDualClient.Visible = false;
            // 
            // groupBoxInternet
            // 
            groupBoxInternet.Controls.Add(LblInternetStatus);
            groupBoxInternet.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 162);
            groupBoxInternet.ForeColor = Color.Maroon;
            groupBoxInternet.Location = new Point(215, 96);
            groupBoxInternet.Name = "groupBoxInternet";
            groupBoxInternet.Size = new Size(167, 38);
            groupBoxInternet.TabIndex = 10;
            groupBoxInternet.TabStop = false;
            groupBoxInternet.Text = "Internet";
            // 
            // LblInternetStatus
            // 
            LblInternetStatus.Font = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            LblInternetStatus.ForeColor = Color.FromArgb(64, 64, 64);
            LblInternetStatus.Location = new Point(6, 16);
            LblInternetStatus.Name = "LblInternetStatus";
            LblInternetStatus.Size = new Size(155, 14);
            LblInternetStatus.TabIndex = 0;
            LblInternetStatus.Text = "…";
            LblInternetStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TimerInternetUi
            // 
            TimerInternetUi.Enabled = true;
            TimerInternetUi.Interval = 1000;
            TimerInternetUi.Tick += TimerInternetUi_Tick;
            // 
            // PG
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(465, 138);
            Controls.Add(cBoxDualClient);
            Controls.Add(groupBoxInternet);
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
            ((System.ComponentModel.ISupportInitialize)numericCustomDowntimeMaxSec).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericCustomDowntimeMinSec).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBoxInternet.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox gBoxCycleDowntime;
        private Label lblCustomDowntimeMax;
        private Label lblCustomDowntimeMin;
        private NumericUpDown numericCustomDowntimeMaxSec;
        private NumericUpDown numericCustomDowntimeMinSec;
        private RadioButton RadioShort;
        private RadioButton RadioCustom;
        private RadioButton RadioMedium;
        private Button btnStart;
        private RadioButton RadioShortMedium;
        private Label LblDowntime;
        private System.Windows.Forms.Timer TimerDowntime;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label LblState;
        private CheckBox cBoxDualClient;
        private GroupBox groupBoxInternet;
        private Label LblInternetStatus;
        private System.Windows.Forms.Timer TimerInternetUi;
    }
}
