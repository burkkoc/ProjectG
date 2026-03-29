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
            lblRestockMaxNotificationCount = new Label();
            txtRestockMaxNotificationCount = new TextBox();
            lblRestockThresholdPercent = new Label();
            txtRestockThresholdPercent = new TextBox();
            lblCancelingLoadedExtraThresholdSeconds = new Label();
            txtCancelingLoadedExtraThresholdSeconds = new TextBox();
            lblDynamicShortSection = new Label();
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
            lblNtfyNotifyUrl.Size = new Size(120, 15);
            lblNtfyNotifyUrl.TabIndex = 0;
            lblNtfyNotifyUrl.Text = "NTFY URL";
            //
            // txtNtfyNotifyUrl
            //
            txtNtfyNotifyUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNtfyNotifyUrl.Location = new Point(12, 33);
            txtNtfyNotifyUrl.Name = "txtNtfyNotifyUrl";
            txtNtfyNotifyUrl.Size = new Size(476, 23);
            txtNtfyNotifyUrl.TabIndex = 1;
            //
            // lblMailboxLocateHotkey
            //
            lblMailboxLocateHotkey.AutoSize = true;
            lblMailboxLocateHotkey.Location = new Point(12, 64);
            lblMailboxLocateHotkey.Name = "lblMailboxLocateHotkey";
            lblMailboxLocateHotkey.Size = new Size(147, 15);
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
            // lblRestockMaxNotificationCount
            //
            lblRestockMaxNotificationCount.AutoSize = true;
            lblRestockMaxNotificationCount.Location = new Point(12, 113);
            lblRestockMaxNotificationCount.Name = "lblRestockMaxNotificationCount";
            lblRestockMaxNotificationCount.Size = new Size(168, 15);
            lblRestockMaxNotificationCount.TabIndex = 4;
            lblRestockMaxNotificationCount.Text = "Restock Max Notification Count";
            //
            // txtRestockMaxNotificationCount
            //
            txtRestockMaxNotificationCount.Location = new Point(12, 131);
            txtRestockMaxNotificationCount.Name = "txtRestockMaxNotificationCount";
            txtRestockMaxNotificationCount.Size = new Size(100, 23);
            txtRestockMaxNotificationCount.TabIndex = 5;
            //
            // lblRestockThresholdPercent
            //
            lblRestockThresholdPercent.AutoSize = true;
            lblRestockThresholdPercent.Location = new Point(12, 162);
            lblRestockThresholdPercent.Name = "lblRestockThresholdPercent";
            lblRestockThresholdPercent.Size = new Size(123, 15);
            lblRestockThresholdPercent.TabIndex = 6;
            lblRestockThresholdPercent.Text = "Restock Threshold (%)";
            //
            // txtRestockThresholdPercent
            //
            txtRestockThresholdPercent.Location = new Point(12, 180);
            txtRestockThresholdPercent.Name = "txtRestockThresholdPercent";
            txtRestockThresholdPercent.Size = new Size(100, 23);
            txtRestockThresholdPercent.TabIndex = 7;
            //
            // lblCancelingLoadedExtraThresholdSeconds
            //
            lblCancelingLoadedExtraThresholdSeconds.AutoSize = true;
            lblCancelingLoadedExtraThresholdSeconds.Location = new Point(12, 211);
            lblCancelingLoadedExtraThresholdSeconds.Name = "lblCancelingLoadedExtraThresholdSeconds";
            lblCancelingLoadedExtraThresholdSeconds.Size = new Size(210, 15);
            lblCancelingLoadedExtraThresholdSeconds.TabIndex = 8;
            lblCancelingLoadedExtraThresholdSeconds.Text = "CancelingLoaded Extra Threshold (sec)";
            //
            // txtCancelingLoadedExtraThresholdSeconds
            //
            txtCancelingLoadedExtraThresholdSeconds.Location = new Point(12, 229);
            txtCancelingLoadedExtraThresholdSeconds.Name = "txtCancelingLoadedExtraThresholdSeconds";
            txtCancelingLoadedExtraThresholdSeconds.Size = new Size(100, 23);
            txtCancelingLoadedExtraThresholdSeconds.TabIndex = 9;
            //
            // lblDynamicShortSection
            //
            lblDynamicShortSection.AutoSize = true;
            lblDynamicShortSection.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            lblDynamicShortSection.Location = new Point(12, 258);
            lblDynamicShortSection.Name = "lblDynamicShortSection";
            lblDynamicShortSection.Size = new Size(420, 13);
            lblDynamicShortSection.TabIndex = 12;
            lblDynamicShortSection.Text = "Short dinamik downtime (T = cancel yukleme suresi, sn)";
            //
            // lblDynamicShortMinTMult
            //
            lblDynamicShortMinTMult.AutoSize = true;
            lblDynamicShortMinTMult.Location = new Point(12, 278);
            lblDynamicShortMinTMult.Name = "lblDynamicShortMinTMult";
            lblDynamicShortMinTMult.Size = new Size(200, 15);
            lblDynamicShortMinTMult.TabIndex = 13;
            lblDynamicShortMinTMult.Text = "Min bekleme: T ×";
            //
            // txtDynamicShortMinTMult
            //
            txtDynamicShortMinTMult.Location = new Point(200, 275);
            txtDynamicShortMinTMult.Name = "txtDynamicShortMinTMult";
            txtDynamicShortMinTMult.Size = new Size(72, 23);
            txtDynamicShortMinTMult.TabIndex = 14;
            //
            // lblDynamicShortMaxTMult
            //
            lblDynamicShortMaxTMult.AutoSize = true;
            lblDynamicShortMaxTMult.Location = new Point(12, 306);
            lblDynamicShortMaxTMult.Name = "lblDynamicShortMaxTMult";
            lblDynamicShortMaxTMult.Size = new Size(200, 15);
            lblDynamicShortMaxTMult.TabIndex = 15;
            lblDynamicShortMaxTMult.Text = "Max bekleme: T ×";
            //
            // txtDynamicShortMaxTMult
            //
            txtDynamicShortMaxTMult.Location = new Point(200, 303);
            txtDynamicShortMaxTMult.Name = "txtDynamicShortMaxTMult";
            txtDynamicShortMaxTMult.Size = new Size(72, 23);
            txtDynamicShortMaxTMult.TabIndex = 16;
            //
            // lblDynamicShortMaxExtraSec
            //
            lblDynamicShortMaxExtraSec.AutoSize = true;
            lblDynamicShortMaxExtraSec.Location = new Point(290, 306);
            lblDynamicShortMaxExtraSec.Name = "lblDynamicShortMaxExtraSec";
            lblDynamicShortMaxExtraSec.Size = new Size(95, 15);
            lblDynamicShortMaxExtraSec.TabIndex = 17;
            lblDynamicShortMaxExtraSec.Text = "+ ek (sn)";
            //
            // txtDynamicShortMaxExtraSec
            //
            txtDynamicShortMaxExtraSec.Location = new Point(385, 303);
            txtDynamicShortMaxExtraSec.Name = "txtDynamicShortMaxExtraSec";
            txtDynamicShortMaxExtraSec.Size = new Size(72, 23);
            txtDynamicShortMaxExtraSec.TabIndex = 18;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(332, 348);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 28);
            btnSave.TabIndex = 10;
            btnSave.Text = "Kaydet";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(413, 348);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "İptal";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // SettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 392);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(txtDynamicShortMaxExtraSec);
            Controls.Add(lblDynamicShortMaxExtraSec);
            Controls.Add(txtDynamicShortMaxTMult);
            Controls.Add(lblDynamicShortMaxTMult);
            Controls.Add(txtDynamicShortMinTMult);
            Controls.Add(lblDynamicShortMinTMult);
            Controls.Add(lblDynamicShortSection);
            Controls.Add(txtCancelingLoadedExtraThresholdSeconds);
            Controls.Add(lblCancelingLoadedExtraThresholdSeconds);
            Controls.Add(txtRestockThresholdPercent);
            Controls.Add(lblRestockThresholdPercent);
            Controls.Add(txtRestockMaxNotificationCount);
            Controls.Add(lblRestockMaxNotificationCount);
            Controls.Add(txtMailboxLocateHotkey);
            Controls.Add(lblMailboxLocateHotkey);
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
        private Label lblRestockMaxNotificationCount;
        private TextBox txtRestockMaxNotificationCount;
        private Label lblRestockThresholdPercent;
        private TextBox txtRestockThresholdPercent;
        private Label lblCancelingLoadedExtraThresholdSeconds;
        private TextBox txtCancelingLoadedExtraThresholdSeconds;
        private Label lblDynamicShortSection;
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
