namespace ProjectG.PresentationLayer
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblNtfyNotifyUrl = new Label();
            txtNtfyNotifyUrl = new TextBox();
            lblMailboxLocateHotkey = new Label();
            txtMailboxLocateHotkey = new TextBox();
            lblGuildBankLocateHotkey = new Label();
            txtGuildBankLocateHotkey = new TextBox();
            lblRestockMaxNotificationCount = new Label();
            txtRestockMaxNotificationCount = new TextBox();
            lblRestockThresholdPercent = new Label();
            txtRestockThresholdPercent = new TextBox();
            lblGuildBankMinIntervalMinutes = new Label();
            txtGuildBankMinIntervalMinutes = new TextBox();
            lblGuildBankIntervalTo = new Label();
            txtGuildBankMaxIntervalMinutes = new TextBox();
            lblGuildBankAfterNotifyDeltaSec = new Label();
            txtGuildBankAfterNotifyDeltaSec = new TextBox();
            lblCancelingLoadedExtraThresholdSeconds = new Label();
            txtCancelingLoadedExtraThresholdSeconds = new TextBox();
            lblCancelingLoadedMaxStaySeconds = new Label();
            txtCancelingLoadedMaxStayMinSeconds = new TextBox();
            lblCancelingLoadedMaxStayTo = new Label();
            txtCancelingLoadedMaxStayMaxSeconds = new TextBox();
            lblDynamicShortMinTMult = new Label();
            txtDynamicShortMinTMult = new TextBox();
            lblDynamicShortMaxTMult = new Label();
            txtDynamicShortMaxTMult = new TextBox();
            lblDynamicShortMaxExtraSec = new Label();
            txtDynamicShortMaxExtraSec = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblNtfyNotifyUrl
            // 
            lblNtfyNotifyUrl.AutoSize = true;
            lblNtfyNotifyUrl.Location = new Point(12, 15);
            lblNtfyNotifyUrl.Name = "lblNtfyNotifyUrl";
            lblNtfyNotifyUrl.Size = new Size(60, 15);
            lblNtfyNotifyUrl.TabIndex = 0;
            lblNtfyNotifyUrl.Text = "NTFY URL";
            // 
            // txtNtfyNotifyUrl
            // 
            txtNtfyNotifyUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNtfyNotifyUrl.Location = new Point(12, 33);
            txtNtfyNotifyUrl.Name = "txtNtfyNotifyUrl";
            txtNtfyNotifyUrl.Size = new Size(383, 23);
            txtNtfyNotifyUrl.TabIndex = 1;
            // 
            // lblMailboxLocateHotkey
            // 
            lblMailboxLocateHotkey.AutoSize = true;
            lblMailboxLocateHotkey.Location = new Point(12, 64);
            lblMailboxLocateHotkey.Name = "lblMailboxLocateHotkey";
            lblMailboxLocateHotkey.Size = new Size(146, 15);
            lblMailboxLocateHotkey.TabIndex = 2;
            lblMailboxLocateHotkey.Text = "Mailbox Locate Key (Z vb.)";
            // 
            // txtMailboxLocateHotkey
            // 
            txtMailboxLocateHotkey.Location = new Point(12, 82);
            txtMailboxLocateHotkey.Name = "txtMailboxLocateHotkey";
            txtMailboxLocateHotkey.Size = new Size(100, 23);
            txtMailboxLocateHotkey.TabIndex = 3;
            // 
            // lblGuildBankLocateHotkey
            // 
            lblGuildBankLocateHotkey.AutoSize = true;
            lblGuildBankLocateHotkey.Location = new Point(12, 108);
            lblGuildBankLocateHotkey.Name = "lblGuildBankLocateHotkey";
            lblGuildBankLocateHotkey.Size = new Size(161, 15);
            lblGuildBankLocateHotkey.TabIndex = 4;
            lblGuildBankLocateHotkey.Text = "Guild Bank Locate Key (X vb.)";
            // 
            // txtGuildBankLocateHotkey
            // 
            txtGuildBankLocateHotkey.Location = new Point(12, 126);
            txtGuildBankLocateHotkey.Name = "txtGuildBankLocateHotkey";
            txtGuildBankLocateHotkey.Size = new Size(100, 23);
            txtGuildBankLocateHotkey.TabIndex = 5;
            // 
            // lblRestockMaxNotificationCount
            // 
            lblRestockMaxNotificationCount.AutoSize = true;
            lblRestockMaxNotificationCount.Location = new Point(12, 157);
            lblRestockMaxNotificationCount.Name = "lblRestockMaxNotificationCount";
            lblRestockMaxNotificationCount.Size = new Size(175, 15);
            lblRestockMaxNotificationCount.TabIndex = 6;
            lblRestockMaxNotificationCount.Text = "Restock Max Notification Count";
            // 
            // txtRestockMaxNotificationCount
            // 
            txtRestockMaxNotificationCount.Location = new Point(12, 175);
            txtRestockMaxNotificationCount.Name = "txtRestockMaxNotificationCount";
            txtRestockMaxNotificationCount.Size = new Size(100, 23);
            txtRestockMaxNotificationCount.TabIndex = 7;
            // 
            // lblRestockThresholdPercent
            // 
            lblRestockThresholdPercent.AutoSize = true;
            lblRestockThresholdPercent.Location = new Point(12, 206);
            lblRestockThresholdPercent.Name = "lblRestockThresholdPercent";
            lblRestockThresholdPercent.Size = new Size(125, 15);
            lblRestockThresholdPercent.TabIndex = 8;
            lblRestockThresholdPercent.Text = "Restock Threshold (%)";
            // 
            // txtRestockThresholdPercent
            // 
            txtRestockThresholdPercent.Location = new Point(12, 224);
            txtRestockThresholdPercent.Name = "txtRestockThresholdPercent";
            txtRestockThresholdPercent.Size = new Size(100, 23);
            txtRestockThresholdPercent.TabIndex = 9;
            // 
            // lblGuildBankMinIntervalMinutes
            // 
            lblGuildBankMinIntervalMinutes.AutoSize = true;
            lblGuildBankMinIntervalMinutes.Location = new Point(12, 252);
            lblGuildBankMinIntervalMinutes.Name = "lblGuildBankMinIntervalMinutes";
            lblGuildBankMinIntervalMinutes.Size = new Size(289, 15);
            lblGuildBankMinIntervalMinutes.TabIndex = 21;
            lblGuildBankMinIntervalMinutes.Text = "Guild bank: son beklenen aralik (dk, min — max) [OR]";
            // 
            // txtGuildBankMinIntervalMinutes
            // 
            txtGuildBankMinIntervalMinutes.Location = new Point(12, 270);
            txtGuildBankMinIntervalMinutes.Name = "txtGuildBankMinIntervalMinutes";
            txtGuildBankMinIntervalMinutes.Size = new Size(56, 23);
            txtGuildBankMinIntervalMinutes.TabIndex = 22;
            // 
            // lblGuildBankIntervalTo
            // 
            lblGuildBankIntervalTo.AutoSize = true;
            lblGuildBankIntervalTo.Location = new Point(74, 273);
            lblGuildBankIntervalTo.Name = "lblGuildBankIntervalTo";
            lblGuildBankIntervalTo.Size = new Size(19, 15);
            lblGuildBankIntervalTo.TabIndex = 32;
            lblGuildBankIntervalTo.Text = "—";
            // 
            // txtGuildBankMaxIntervalMinutes
            // 
            txtGuildBankMaxIntervalMinutes.Location = new Point(95, 270);
            txtGuildBankMaxIntervalMinutes.Name = "txtGuildBankMaxIntervalMinutes";
            txtGuildBankMaxIntervalMinutes.Size = new Size(56, 23);
            txtGuildBankMaxIntervalMinutes.TabIndex = 33;
            // 
            // lblGuildBankAfterNotifyDeltaSec
            // 
            lblGuildBankAfterNotifyDeltaSec.AutoSize = true;
            lblGuildBankAfterNotifyDeltaSec.Location = new Point(12, 298);
            lblGuildBankAfterNotifyDeltaSec.Name = "lblGuildBankAfterNotifyDeltaSec";
            lblGuildBankAfterNotifyDeltaSec.Size = new Size(384, 15);
            lblGuildBankAfterNotifyDeltaSec.TabIndex = 23;
            lblGuildBankAfterNotifyDeltaSec.Text = "Restock bildirimi sonrasi: (ilk posting sn) - (son posting sn) >= [sn] [OR]";
            // 
            // txtGuildBankAfterNotifyDeltaSec
            // 
            txtGuildBankAfterNotifyDeltaSec.Location = new Point(12, 316);
            txtGuildBankAfterNotifyDeltaSec.Name = "txtGuildBankAfterNotifyDeltaSec";
            txtGuildBankAfterNotifyDeltaSec.Size = new Size(72, 23);
            txtGuildBankAfterNotifyDeltaSec.TabIndex = 24;
            // 
            // lblCancelingLoadedExtraThresholdSeconds
            // 
            lblCancelingLoadedExtraThresholdSeconds.AutoSize = true;
            lblCancelingLoadedExtraThresholdSeconds.Location = new Point(12, 347);
            lblCancelingLoadedExtraThresholdSeconds.Name = "lblCancelingLoadedExtraThresholdSeconds";
            lblCancelingLoadedExtraThresholdSeconds.Size = new Size(211, 15);
            lblCancelingLoadedExtraThresholdSeconds.TabIndex = 10;
            lblCancelingLoadedExtraThresholdSeconds.Text = "CancelingLoaded Extra Threshold (sec)";
            // 
            // txtCancelingLoadedExtraThresholdSeconds
            // 
            txtCancelingLoadedExtraThresholdSeconds.Location = new Point(12, 365);
            txtCancelingLoadedExtraThresholdSeconds.Name = "txtCancelingLoadedExtraThresholdSeconds";
            txtCancelingLoadedExtraThresholdSeconds.Size = new Size(100, 23);
            txtCancelingLoadedExtraThresholdSeconds.TabIndex = 11;
            // 
            // lblCancelingLoadedMaxStaySeconds
            // 
            lblCancelingLoadedMaxStaySeconds.AutoSize = true;
            lblCancelingLoadedMaxStaySeconds.Location = new Point(12, 392);
            lblCancelingLoadedMaxStaySeconds.Name = "lblCancelingLoadedMaxStaySeconds";
            lblCancelingLoadedMaxStaySeconds.Size = new Size(273, 15);
            lblCancelingLoadedMaxStaySeconds.TabIndex = 34;
            lblCancelingLoadedMaxStaySeconds.Text = "CancelingLoaded max kalis (sn, min — max) → CancelingDone";
            // 
            // txtCancelingLoadedMaxStayMinSeconds
            // 
            txtCancelingLoadedMaxStayMinSeconds.Location = new Point(12, 410);
            txtCancelingLoadedMaxStayMinSeconds.Name = "txtCancelingLoadedMaxStayMinSeconds";
            txtCancelingLoadedMaxStayMinSeconds.Size = new Size(56, 23);
            txtCancelingLoadedMaxStayMinSeconds.TabIndex = 35;
            // 
            // lblCancelingLoadedMaxStayTo
            // 
            lblCancelingLoadedMaxStayTo.AutoSize = true;
            lblCancelingLoadedMaxStayTo.Location = new Point(74, 413);
            lblCancelingLoadedMaxStayTo.Name = "lblCancelingLoadedMaxStayTo";
            lblCancelingLoadedMaxStayTo.Size = new Size(19, 15);
            lblCancelingLoadedMaxStayTo.TabIndex = 36;
            lblCancelingLoadedMaxStayTo.Text = "—";
            // 
            // txtCancelingLoadedMaxStayMaxSeconds
            // 
            txtCancelingLoadedMaxStayMaxSeconds.Location = new Point(95, 410);
            txtCancelingLoadedMaxStayMaxSeconds.Name = "txtCancelingLoadedMaxStayMaxSeconds";
            txtCancelingLoadedMaxStayMaxSeconds.Size = new Size(56, 23);
            txtCancelingLoadedMaxStayMaxSeconds.TabIndex = 37;
            // 
            // lblDynamicShortMinTMult
            // 
            lblDynamicShortMinTMult.AutoSize = true;
            lblDynamicShortMinTMult.Location = new Point(12, 448);
            lblDynamicShortMinTMult.Name = "lblDynamicShortMinTMult";
            lblDynamicShortMinTMult.Size = new Size(100, 15);
            lblDynamicShortMinTMult.TabIndex = 13;
            lblDynamicShortMinTMult.Text = "Min bekleme: T ×";
            // 
            // txtDynamicShortMinTMult
            // 
            txtDynamicShortMinTMult.Location = new Point(117, 448);
            txtDynamicShortMinTMult.Name = "txtDynamicShortMinTMult";
            txtDynamicShortMinTMult.Size = new Size(72, 23);
            txtDynamicShortMinTMult.TabIndex = 14;
            // 
            // lblDynamicShortMaxTMult
            // 
            lblDynamicShortMaxTMult.AutoSize = true;
            lblDynamicShortMaxTMult.Location = new Point(12, 476);
            lblDynamicShortMaxTMult.Name = "lblDynamicShortMaxTMult";
            lblDynamicShortMaxTMult.Size = new Size(101, 15);
            lblDynamicShortMaxTMult.TabIndex = 15;
            lblDynamicShortMaxTMult.Text = "Max bekleme: T ×";
            // 
            // txtDynamicShortMaxTMult
            // 
            txtDynamicShortMaxTMult.Location = new Point(117, 476);
            txtDynamicShortMaxTMult.Name = "txtDynamicShortMaxTMult";
            txtDynamicShortMaxTMult.Size = new Size(72, 23);
            txtDynamicShortMaxTMult.TabIndex = 16;
            // 
            // lblDynamicShortMaxExtraSec
            // 
            lblDynamicShortMaxExtraSec.AutoSize = true;
            lblDynamicShortMaxExtraSec.Location = new Point(196, 479);
            lblDynamicShortMaxExtraSec.Name = "lblDynamicShortMaxExtraSec";
            lblDynamicShortMaxExtraSec.Size = new Size(53, 15);
            lblDynamicShortMaxExtraSec.TabIndex = 17;
            lblDynamicShortMaxExtraSec.Text = "+ ek (sn)";
            // 
            // txtDynamicShortMaxExtraSec
            // 
            txtDynamicShortMaxExtraSec.Location = new Point(255, 476);
            txtDynamicShortMaxExtraSec.Name = "txtDynamicShortMaxExtraSec";
            txtDynamicShortMaxExtraSec.Size = new Size(72, 23);
            txtDynamicShortMaxExtraSec.TabIndex = 18;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(147, 522);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 28);
            btnSave.TabIndex = 19;
            btnSave.Text = "Kaydet";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(228, 522);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 20;
            btnCancel.Text = "İptal";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(407, 562);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(txtDynamicShortMaxExtraSec);
            Controls.Add(lblDynamicShortMaxExtraSec);
            Controls.Add(txtDynamicShortMaxTMult);
            Controls.Add(lblDynamicShortMaxTMult);
            Controls.Add(txtDynamicShortMinTMult);
            Controls.Add(lblDynamicShortMinTMult);
            Controls.Add(txtCancelingLoadedMaxStayMaxSeconds);
            Controls.Add(lblCancelingLoadedMaxStayTo);
            Controls.Add(txtCancelingLoadedMaxStayMinSeconds);
            Controls.Add(lblCancelingLoadedMaxStaySeconds);
            Controls.Add(txtCancelingLoadedExtraThresholdSeconds);
            Controls.Add(lblCancelingLoadedExtraThresholdSeconds);
            Controls.Add(txtGuildBankAfterNotifyDeltaSec);
            Controls.Add(lblGuildBankAfterNotifyDeltaSec);
            Controls.Add(txtGuildBankMaxIntervalMinutes);
            Controls.Add(lblGuildBankIntervalTo);
            Controls.Add(txtGuildBankMinIntervalMinutes);
            Controls.Add(lblGuildBankMinIntervalMinutes);
            Controls.Add(txtRestockThresholdPercent);
            Controls.Add(lblRestockThresholdPercent);
            Controls.Add(txtRestockMaxNotificationCount);
            Controls.Add(lblRestockMaxNotificationCount);
            Controls.Add(txtMailboxLocateHotkey);
            Controls.Add(lblMailboxLocateHotkey);
            Controls.Add(txtGuildBankLocateHotkey);
            Controls.Add(lblGuildBankLocateHotkey);
            Controls.Add(txtNtfyNotifyUrl);
            Controls.Add(lblNtfyNotifyUrl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Ayarlar";
            Load += SettingsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNtfyNotifyUrl;
        private TextBox txtNtfyNotifyUrl;
        private Label lblMailboxLocateHotkey;
        private TextBox txtMailboxLocateHotkey;
        private Label lblGuildBankLocateHotkey;
        private TextBox txtGuildBankLocateHotkey;
        private Label lblRestockMaxNotificationCount;
        private TextBox txtRestockMaxNotificationCount;
        private Label lblRestockThresholdPercent;
        private TextBox txtRestockThresholdPercent;
        private Label lblGuildBankMinIntervalMinutes;
        private TextBox txtGuildBankMinIntervalMinutes;
        private Label lblGuildBankIntervalTo;
        private TextBox txtGuildBankMaxIntervalMinutes;
        private Label lblGuildBankAfterNotifyDeltaSec;
        private TextBox txtGuildBankAfterNotifyDeltaSec;
        private Label lblCancelingLoadedExtraThresholdSeconds;
        private TextBox txtCancelingLoadedExtraThresholdSeconds;
        private Label lblCancelingLoadedMaxStaySeconds;
        private TextBox txtCancelingLoadedMaxStayMinSeconds;
        private Label lblCancelingLoadedMaxStayTo;
        private TextBox txtCancelingLoadedMaxStayMaxSeconds;
        private Label lblDynamicShortMinTMult;
        private TextBox txtDynamicShortMinTMult;
        private Label lblDynamicShortMaxTMult;
        private TextBox txtDynamicShortMaxTMult;
        private Label lblDynamicShortMaxExtraSec;
        private TextBox txtDynamicShortMaxExtraSec;
        private Button btnSave;
        private Button btnCancel;
    }
}
