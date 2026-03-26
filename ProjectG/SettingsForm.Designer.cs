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
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(332, 263);
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
            btnCancel.Location = new Point(413, 263);
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
            ClientSize = new Size(500, 307);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
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
        private Button btnSave;
        private Button btnCancel;
    }
}
